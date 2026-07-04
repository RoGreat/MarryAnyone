using MarryAnyone;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.CampaignBehaviors
{
    // 1. Create a hero from villager character object.
    //  - Refer to previous recipe and improve.
    // 2. Gate dialog to work for occupations that are not lords.
    //  - This might take some thought. Try not to over-patch.
    internal class VillagerRomanceCampaignBehavior : CampaignBehaviorBase
    {
        public VillagerRomanceCampaignBehavior() { }

        public override void SyncData(IDataStore dataStore) { }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        private bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        {
            return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2) && !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction);
        }

        private Hero? _createdHero = null;

        private bool conversation_player_can_open_courtship_on_condition()
        {
            // Ad hoc hero creation
            // Refer to CampaignCheats.CreateRandomClan
            IAgent conversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;
            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            if (conversationAgent is not null && conversationCharacter is not null && Hero.OneToOneConversationHero is null)
            {
                Settlement settlement = Hero.MainHero.CurrentSettlement;
                _createdHero = HeroCreator.CreateSpecialHero(conversationCharacter, settlement, null, null, (int)conversationAgent.Age);
                _createdHero.StaticBodyProperties = ((Agent)conversationAgent).BodyPropertiesValue.StaticProperties;
                _createdHero.Weight = ((Agent)conversationAgent).BodyPropertiesValue.DynamicProperties.Weight;
                _createdHero.Build = ((Agent)conversationAgent).BodyPropertiesValue.DynamicProperties.Build;
            }

            if (_createdHero is not null)
            {
                if (MarriageCourtshipPossibility(Hero.MainHero, _createdHero) && Romance.GetRomanticLevel(Hero.MainHero, _createdHero) == Romance.RomanceLevelEnum.Untested)
                {
                    if (Hero.MainHero.IsFemale)
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=bjJs0eeB}My lord, I note that you have not yet taken a wife.", false);
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                    }
                    return true;
                }
                if (Romance.GetRomanticLevel(Hero.MainHero, _createdHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, _createdHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
                {
                    if (Hero.MainHero.IsFemale)
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=2WnhUBMM}My lord, may you give me another chance to prove myself?", false);
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private void conversation_player_opens_courtship_on_consequence()
        {
            if (Romance.GetRomanticLevel(Hero.MainHero, _createdHero) != Romance.RomanceLevelEnum.FailedInCompatibility && Romance.GetRomanticLevel(Hero.MainHero, _createdHero) != Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                ChangeRomanticStateAction.Apply(Hero.MainHero, _createdHero, Romance.RomanceLevelEnum.CourtshipStarted);
            }
        }

        private enum RomanceReservationDescription
        {
            CompatibilityINeedSomeoneUpright,
            CompatibilityNeedSomethingInCommon,
            CompatibiliyINeedSomeoneDangerous,
            CompatibilityStrongPoliticalBeliefs,
            AttractionYoureNotMyType,
            AttractionYoureGoodEnough,
            AttractionIAmDrawnToYou,
            PropertyYouSeemRichEnough,
            PropertyWeNeedToBeComfortable,
            PropertyIWantRealWealth,
            PropertyHowCanIMarryAnAdventuress,
            FamilyApprovalIAmGladYouAreFriendsWithOurFamily,
            FamilyApprovalYouNeedToBeFriendsWithOurFamily,
            FamilyApprovalHowCanYouBeEnemiesWithOurFamily,
            FamilyApprovalItWouldBeBestToBefriendOurFamily
        }

        private IEnumerable<RomanceReservationDescription> GetRomanceReservations(Hero wooed, Hero wooer)
        {
            List<RomanceReservationDescription> list = new();
            bool flag = wooed.GetTraitLevel(DefaultTraits.Honor) + wooed.GetTraitLevel(DefaultTraits.Mercy) > 0;
            bool flag2 = wooed.GetTraitLevel(DefaultTraits.Honor) < 1 && wooed.GetTraitLevel(DefaultTraits.Valor) < 1 && wooed.GetTraitLevel(DefaultTraits.Calculating) < 1;
            bool flag3 = wooed.GetTraitLevel(DefaultTraits.Calculating) - wooed.GetTraitLevel(DefaultTraits.Mercy) >= 0;
            bool flag4 = wooed.GetTraitLevel(DefaultTraits.Valor) - wooed.GetTraitLevel(DefaultTraits.Calculating) > 0 && wooed.GetTraitLevel(DefaultTraits.Mercy) <= 0;

            if (_createdHero is null)
                return list;

            if (flag)
            {
                list.Add(RomanceReservationDescription.CompatibilityINeedSomeoneUpright);
            }
            else if (flag4 && wooed.IsFemale)
            {
                list.Add(RomanceReservationDescription.CompatibiliyINeedSomeoneDangerous);
            }
            else
            {
                list.Add(RomanceReservationDescription.CompatibilityNeedSomethingInCommon);
            }
            int attractionValuePercentage = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(_createdHero, Hero.MainHero);
            if (attractionValuePercentage > 70)
            {
                list.Add(RomanceReservationDescription.AttractionIAmDrawnToYou);
            }
            else if (attractionValuePercentage > 40)
            {
                list.Add(RomanceReservationDescription.AttractionYoureGoodEnough);
            }
            else
            {
                list.Add(RomanceReservationDescription.AttractionYoureNotMyType);
            }
            List<Settlement> list2 = Enumerable.ToList(Enumerable.Where(Settlement.All, x => x.OwnerClan == wooer.Clan));
            if (flag3 && wooer.IsFemale && list2.Count < 1)
            {
                list.Add(RomanceReservationDescription.PropertyHowCanIMarryAnAdventuress);
            }
            else if (flag3 && list2.Count < 3)
            {
                list.Add(RomanceReservationDescription.PropertyIWantRealWealth);
            }
            else if (list2.Count < 1)
            {
                list.Add(RomanceReservationDescription.PropertyWeNeedToBeComfortable);
            }
            else
            {
                list.Add(RomanceReservationDescription.PropertyYouSeemRichEnough);
            }
            float unmodifiedClanLeaderRelationshipWithPlayer = _createdHero.GetUnmodifiedClanLeaderRelationshipWithPlayer();
            if (unmodifiedClanLeaderRelationshipWithPlayer < -10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalHowCanYouBeEnemiesWithOurFamily);
            }
            else if (!flag2 && unmodifiedClanLeaderRelationshipWithPlayer < 10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalItWouldBeBestToBefriendOurFamily);
            }
            else if (flag2 && unmodifiedClanLeaderRelationshipWithPlayer < 10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalYouNeedToBeFriendsWithOurFamily);
            }
            else
            {
                list.Add(RomanceReservationDescription.FamilyApprovalIAmGladYouAreFriendsWithOurFamily);
            }
            return list;
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            if (_createdHero is not null)
            {
                IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(_createdHero, Hero.MainHero);
                if (Romance.GetRomanticLevel(Hero.MainHero, _createdHero) == Romance.RomanceLevelEnum.FailedInPracticalities || Romance.GetRomanticLevel(Hero.MainHero, _createdHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
                {
                    return false;
                }
                MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou) ? "{=WEkjz9tg}Ah! Yes... We are considering offers... Did you have someone in mind?" : "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
                return true;
            }
            return false;
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("villager_special_request_flirt", "villager_talk_speak_diplomacy_2", "villager_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            starter.AddDialogLine("villager_start_courtship_response", "villager_start_courtship_response", "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
        }
    }
}
