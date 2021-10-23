using HarmonyLib;
using Helpers;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using SandBox;
using System.Linq;

namespace MarryAnyone.Behaviors
{
    internal class MAAdoptionCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.AllAliveHeroes.ToList())
            {
                if (Hero.MainHero.Children.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
                    if (hero.Clan != Clan.PlayerClan) // Else lost the town govenor post on hero.Clan = null !!
                    {
                        hero.Clan = null;
                        hero.Clan = Clan.PlayerClan;
                    }
                }
            }
            
            // Children
            starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_children_player_no_rhyme", "adoption_child_MA", "{=adoption_offer_child}I can tell you have no parents to go back to child. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if that is the case.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response_MA", "adoption_child_MA", "close_window", "{=adoption_response_child}You want to be my {?PLAYER.GENDER}Ma{?}Pa{\\?}? Okay then![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
            // Teens
            starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_player", "adoption_teen_MA", "{=adoption_offer_teen}Do you not have any parents to take care of you young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?}? You are welcome to be a part of my family.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response_MA", "adoption_teen_MA", "close_window", "{=adoption_response_teen}Thanks for allowing me to be a part of your family {?PLAYER.GENDER}milady{?}sir{\\?}. I gratefully accept![rf:happy][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
        }

        private static int _agent;

        private static List<int>? _adoptableAgents;

        private static List<int>? _notAdoptableAgents;

        private bool conversation_adopt_child_on_condition()
        {
            ISettingsProvider settings = new MASettings();
            StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", CharacterObject.OneToOneConversationCharacter);
            _agent = Math.Abs(Campaign.Current.ConversationManager.OneToOneConversationAgent.GetHashCode());
            if (_adoptableAgents is null || _notAdoptableAgents is null)
            {
                _adoptableAgents = new List<int>();
                _notAdoptableAgents = new List<int>();
            }
            if (_notAdoptableAgents.Contains(_agent))
            {
                MAHelper.Print("Cannot Adopt");
                return false;
            }
            if (_adoptableAgents.Contains(_agent))
            {
                MAHelper.Print("Can Adopt");
                return true;
            }
            if (Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                MAHelper.Print("Adoption: " + settings.Adoption);
                if (!settings.Adoption)
                {
                    return false;
                }
                MAHelper.Print("Adoption Chance: " + settings.AdoptionChance);
                // You only roll once!
                float random = MBRandom.RandomFloat;
                MAHelper.Print("Random Number: " + random);
                if (random < settings.AdoptionChance)
                {
                    _adoptableAgents.Add(_agent);
                    return true;
                }
                else
                {
                    _notAdoptableAgents.Add(_agent);
                }
            }
            return false;
        }

        private void conversation_adopt_child_on_consequence()
        {
            // Similar system from Recruit Everyone
            if (_notAdoptableAgents is not null)
            {
                _notAdoptableAgents.Add(_agent);
            }
            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject character = CharacterObject.OneToOneConversationCharacter;

            // Add a bit of the DeliverOffspring method into the mix
            Hero hero = HeroCreator.CreateSpecialHero(character, Settlement.CurrentSettlement, null, null, (int)agent.Age);
            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            CharacterObject characterObject = CharacterObject.ChildTemplates.FirstOrDefault((CharacterObject t) => t.Culture == character.Culture && t.Age <= becomeChildAge && t.IsFemale == character.IsFemale && t.Occupation == Occupation.Lord);
            if (characterObject is not null)
            {
                Equipment equipment = characterObject.FirstCivilianEquipment.Clone(false);
                Equipment equipment2 = new(false);
                equipment2.FillFrom(equipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment2);
            }
            MAHelper.OccupationToLord(hero.CharacterObject);
            hero.Clan = Clan.PlayerClan;
            AccessTools.Method(typeof(HeroDeveloper), "CheckInitialLevel").Invoke(hero.HeroDeveloper, null);
            hero.CharacterObject.IsFemale = character.IsFemale;
            BodyProperties bodyPropertiesValue = agent.BodyPropertiesValue;
            AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(hero, bodyPropertiesValue.StaticProperties);
            // Selects player as the parent
            if (Hero.MainHero.IsFemale)
            {
                hero.Mother = Hero.MainHero;
            }
            else
            {
                hero.Father = Hero.MainHero;
            }
            hero.IsNoble = true;
            hero.HasMet = true;

            // Too much work to try and implement the log
            /*
            CharacterAdoptedLogEntry characterAdoptedLogEntry = new(hero, Hero.MainHero);
            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new AdoptionMapNotification(hero, Hero.MainHero, characterAdoptedLogEntry.GetEncyclopediaText()));
            */

            // Cool idea. Might put this into Recruit Everyone, too!
            AccessTools.Field(typeof(Agent), "_name").SetValue(agent, hero.Name);
            OnHeroAdopted(Hero.MainHero, hero);
            // Follows you! I like this feature :3
            Campaign.Current.ConversationManager.ConversationEndOneShot += FollowMainAgent;

            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            var instance = Traverse.Create<CampaignEventDispatcher>().Property("Instance").GetValue<CampaignEventDispatcher>();
            if (hero.Age > becomeChildAge || (hero.Age == becomeChildAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
            {
                // CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
                Traverse.Create(instance).Method("OnHeroGrowsOutOfInfancy", new Type[] { typeof(Hero) }).GetValue(new object[] { hero });
            }
            if (hero.Age > heroComesOfAge || (hero.Age == heroComesOfAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
            {
                // CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
                Traverse.Create(instance).Method("OnHeroComesOfAge", new Type[] { typeof(Hero) }).GetValue(new object[] { hero });
            }
        }

        private static void FollowMainAgent()
        {
            DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(Agent.Main);
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
        }

        private void OnHeroAdopted(Hero adopter, Hero adoptedHero)
        {
            TextObject textObject = new("{=adopted}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.", null);
            StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject, textObject);
            StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject, textObject);
            InformationManager.AddQuickInformation(textObject, 0, null, "event:/ui/notification/child_born");
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            foreach (Hero hero in Hero.AllAliveHeroes.ToList())
            {
                if (Hero.MainHero.Children.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
                    if (hero.Clan != Clan.PlayerClan) // Else lost the town govenor post on hero.Clan = null !!
                    {
                        hero.Clan = null;
                        hero.Clan = Clan.PlayerClan;
                    }
                }
            }

            AddDialogs(campaignGameStarter);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}