using HarmonyLib;
using Helpers;
using MarryAnyone.Patches;
using MarryAnyone.Settings;
using MountAndBlade.CampaignBehaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace MarryAnyone.Behaviors
{
    internal class MAAdoptionCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.All)
            {
                if (Hero.MainHero.Children.Contains(hero))
                {
                    CharacterObject character = hero.CharacterObject;
                    int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
                    _childTemplate = CharacterObject.ChildTemplates.FirstOrDefault((CharacterObject t) => t.Culture == character.Culture && t.Age <= becomeChildAge && t.IsFemale == character.IsFemale && t.Occupation == Occupation.Lord);
                    MAHelper.OccupationToLord(hero.CharacterObject, _childTemplate);
                }
            }

            // Children
            starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_children_player_no_rhyme", "adoption_child_MA", "{=adoption_offer_child}The look on your face tells me you have no parents to go back to. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if you want one.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response_MA", "adoption_child_MA", "close_window", "{=adoption_response_child}Really? Well I guess that makes you my {?PLAYER.GENDER}Ma{?}Pa{\\?} now!", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
            // Teens
            starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_player", "adoption_teen_MA", "{=adoption_offer_teen}It would be a shame to see such a promising young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?} go without a family. Wish to join mine?", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_adoption_response_MA", "adoption_teen_MA", "close_window", "{=adoption_response_teen}Yes, thanks for letting me be a part of your family {?PLAYER.GENDER}milady{?}sir{\\?}.", null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, null);
        }

        private static int _agent;

        private static List<int> _adoptableAgents;

        private static List<int> _notAdoptableAgents;

        private bool conversation_adopt_child_on_condition()
        {
            StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", CharacterObject.OneToOneConversationCharacter, null, null, false);
            ISettingsProvider settings = new MASettings();
            _agent = Math.Abs(Campaign.Current.ConversationManager.OneToOneConversationAgent.GetHashCode());
            if (_notAdoptableAgents.Contains(_agent))
            {
                return false;
            }
            if (_adoptableAgents.Contains(_agent))
            {
                return true;
            }
            if (Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                // You only roll once!
                if (MBRandom.RandomFloat <= settings.AdoptionChance)
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

        private static CharacterObject _childTemplate;

        private void conversation_adopt_child_on_consequence()
        {
            // Same system as Recruit Everyone pretty much
            // Add in Deliver Offspring into the mix
            _notAdoptableAgents.Add(_agent);

            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject character = CharacterObject.OneToOneConversationCharacter;

            Hero hero = HeroCreator.CreateSpecialHero(character, Settlement.CurrentSettlement, Clan.PlayerClan, null, (int)agent.Age);

            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            _childTemplate = CharacterObject.ChildTemplates.FirstOrDefault((CharacterObject t) => t.Culture == character.Culture && t.Age <= becomeChildAge && t.IsFemale == character.IsFemale && t.Occupation == Occupation.Lord);
            if (_childTemplate != null)
            {
                Equipment equipment = _childTemplate.FirstCivilianEquipment.Clone(false);
                Equipment equipment2 = new Equipment(false);
                equipment2.FillFrom(equipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment2);
            }

            MAHelper.OccupationToLord(hero.CharacterObject, _childTemplate);
            BodyProperties bodyPropertiesValue = agent.BodyPropertiesValue;
            AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(hero, bodyPropertiesValue.StaticProperties);

            hero.HasMet = true;
            hero.ChangeState(Hero.CharacterStates.Active);

            // Fundamental difference
            if (Hero.MainHero.IsFemale)
            {
                hero.Mother = Hero.MainHero;
            }
            else
            {
                hero.Father = Hero.MainHero;
            }
            hero.UpdateHomeSettlement();
            hero.IsNoble = true;

            // Notable fixes for the most part
            // In case these issues apply to a child for some reason
            foreach (PartyBase party in hero.OwnedParties.ToList())
            {
                MobileParty mobileParty = party.MobileParty;
                if (mobileParty != null)
                {
                    mobileParty.CurrentSettlement = mobileParty.HomeSettlement;
                    DisbandPartyAction.ApplyDisband(mobileParty);
                }
            }
            if (hero.Issue != null)
            {
                hero.Issue.CompleteIssueWithCancel();
            }
            CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            _adoptableAgents = new List<int>();
            _notAdoptableAgents = new List<int>();
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