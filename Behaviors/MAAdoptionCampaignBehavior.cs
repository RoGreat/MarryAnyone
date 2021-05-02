using HarmonyLib;
using Helpers;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.Behaviors
{
    internal class MAAdoptionCampaignBehavior : CampaignBehaviorBase
    {
        private static void RefreshClanVM(Hero hero)
        {
            // In ClanLordItemVM
            // this.IsFamilyMember = Hero.MainHero.Clan.Lords.Contains(this._hero);
            // In the class "Clan" the field "_lords" is now "_lordsCache"
            List<Hero> _lordsCache = (List<Hero>)AccessTools.Field(typeof(Clan), "_lordsCache").GetValue(Clan.PlayerClan);
            if (!_lordsCache.Contains(hero))
            {
                _lordsCache.Add(hero);
            }
        }


        protected void AddDialogs(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.All.ToList())
            {
                if (Hero.MainHero.Children.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
                    RefreshClanVM(hero);
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
                MASubModule.Print("Cannot Adopt");
                return false;
            }
            if (_adoptableAgents.Contains(_agent))
            {
                MASubModule.Print("Can Adopt");
                return true;
            }
            if (Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                MASubModule.Print("Adoption: " + settings.Adoption);
                if (!settings.Adoption)
                {
                    return false;
                }
                MASubModule.Print("Adoption Chance: " + settings.AdoptionChance);
                // You only roll once!
                float random = MBRandom.RandomFloat;
                MASubModule.Print("Random Number: " + random);
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

        private static CharacterObject? _childTemplate;

        private void conversation_adopt_child_on_consequence()
        {
            // Same system as Recruit Everyone pretty much
            // Add in Deliver Offspring into the mix
            if (_notAdoptableAgents is not null)
            {
                _notAdoptableAgents.Add(_agent);
            }

            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject character = CharacterObject.OneToOneConversationCharacter;

            Hero hero = HeroCreator.CreateSpecialHero(character, Settlement.CurrentSettlement, Clan.PlayerClan, null, (int)agent.Age);

            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            _childTemplate = CharacterObject.ChildTemplates.FirstOrDefault((CharacterObject t) => t.Culture == character.Culture && t.Age <= becomeChildAge && t.IsFemale == character.IsFemale && t.Occupation == Occupation.Lord);
            if (_childTemplate is not null)
            {
                Equipment equipment = _childTemplate.FirstCivilianEquipment.Clone(false);
                Equipment equipment2 = new(false);
                equipment2.FillFrom(equipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment2);
            }
            MAHelper.OccupationToLord(hero.CharacterObject);
            AccessTools.Method(typeof(HeroDeveloper), "CheckInitialLevel").Invoke(hero.HeroDeveloper, null);
            BodyProperties bodyPropertiesValue = agent.BodyPropertiesValue;
            AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(hero, bodyPropertiesValue.StaticProperties);
            hero.HasMet = true;
            if (Hero.MainHero.IsFemale)
            {
                hero.Mother = Hero.MainHero;
            }
            else
            {
                hero.Father = Hero.MainHero;
            }
            hero.IsNoble = true;
            RefreshClanVM(hero);

            if (hero.Issue is not null)
            {
                hero.Issue.CompleteIssueWithCancel();
            }
            // Conflicts since this is already synced up and already exists inside the create a hero function
            // This probably did not work this way until e1.5.9 so I am concerned about this not being the case on e1.5.8
            // CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
            MASubModule.Print(Hero.MainHero.Name + " adopted " + hero.Name, true);
        }

        // Looked at DecideBornSettlement from HeroCreator

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
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