using MarryAnyone;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.CampaignBehaviors
{
    internal class CommonerRomanceCampaignBehavior : CampaignBehaviorBase
    {
        public CommonerRomanceCampaignBehavior() { }

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

        // butr.github.io/documentation/advanced/switching-from-membertinfo-to-accesstools2/
        private delegate void SetHeroObjectDelegate(CharacterObject instance, Hero @value);
        private static readonly SetHeroObjectDelegate? SetHeroObject = AccessTools2.GetPropertySetterDelegate<SetHeroObjectDelegate>(typeof(CharacterObject), "HeroObject");

        private bool conversation_player_can_open_courtship_on_condition()
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignCheats.CreateRandomClan

            IAgent conversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;
            if (conversationAgent is not null && Hero.OneToOneConversationHero is null)
            {
                Log.Debug("Hero Creation");
                Settlement settlement = Hero.MainHero.CurrentSettlement;
                Hero conversationHero = HeroCreator.CreateSpecialHero((CharacterObject)((Agent)conversationAgent).Character, settlement, null, null, (int)conversationAgent.Age);
                conversationHero.StaticBodyProperties = ((Agent)conversationAgent).BodyPropertiesValue.StaticProperties;
                conversationHero.Weight = ((Agent)conversationAgent).BodyPropertiesValue.DynamicProperties.Weight;
                conversationHero.Build = ((Agent)conversationAgent).BodyPropertiesValue.DynamicProperties.Build;
                SetHeroObject((CharacterObject)((Agent)conversationAgent).Character, conversationHero);
                Log.Debug("Hero Set");
            }

            if (Hero.OneToOneConversationHero is not null)
            {
                Log.Debug("Hero OneToOne");
                if (MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
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
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
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
            if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) != Romance.RomanceLevelEnum.FailedInCompatibility && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) != Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
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

            if (Hero.OneToOneConversationHero is null)
            {
                return list;
            }

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
            int attractionValuePercentage = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(Hero.OneToOneConversationHero, Hero.MainHero);
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
            float unmodifiedClanLeaderRelationshipWithPlayer = Hero.OneToOneConversationHero.GetUnmodifiedClanLeaderRelationshipWithPlayer();
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
            if (Hero.OneToOneConversationHero is not null)
            {
                IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(Hero.OneToOneConversationHero, Hero.MainHero);
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
                {
                    return false;
                }
                MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou) ? "{=WEkjz9tg}Ah! Yes... We are considering offers... Did you have someone in mind?" : "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
                return true;
            }
            return false;
        }

        private bool conversation_courtship_decline_reaction_to_player_on_condition()
        {
            if (Hero.OneToOneConversationHero is not null)
            {
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
                {
                    MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=emLBsWj6}I am terribly sorry. It is practically not possible for us to be married.", false);
                    return true;
                }
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
                {
                    MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=s7idfhBO}I am terribly sorry. We are not really compatible with each other.", false);
                    return true;
                }
                return false;
            }
            return false;
        }

        private bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            return Hero.MainHero.Spouse == null && Hero.OneToOneConversationHero != null && MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private bool conversation_courtship_reaction_to_player_on_condition()
        {
            IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(Hero.OneToOneConversationHero, Hero.MainHero);
            bool flag = Hero.OneToOneConversationHero.GetTraitLevel(DefaultTraits.Generosity) + Hero.OneToOneConversationHero.GetTraitLevel(DefaultTraits.Mercy) > 0;
            TraitObject persona = Hero.OneToOneConversationHero.CharacterObject.GetPersona();
            bool flag2 = ConversationTagHelper.UsesHighRegister(Hero.OneToOneConversationHero.CharacterObject);
            if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou))
            {
                if (persona == DefaultTraits.PersonaIronic && flag2)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=5ao0RdRT}Well, I do not deny that there is something about you to which I am drawn.", false);
                }
                if (persona == DefaultTraits.PersonaIronic && !flag2)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=r77ZrSUJ}You're straightforward. I like that.", false);
                }
                else if (persona == DefaultTraits.PersonaCurt)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", Hero.MainHero.IsFemale ? "{=YXCGUSYd}Mm. Well, you'd make a very unusual match. But, well, I won't rule it out." : "{=iKYSgoZx}You're a handsome devil, I'll give you that.", false);
                }
                else if (persona == DefaultTraits.PersonaEarnest)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=UCjFAPnk}I am flattered, {?PLAYER.GENDER}my lady{?}sir{\\?}.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=8PwNj5tR}Yes... Yes. We should, em, discuss this.", false);
                }
            }
            else if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyHowCanIMarryAnAdventuress))
            {
                MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=YRN4RBeI}Very well, madame, but I would have you know.... I intend to marry someone of my own rank.", false);
            }
            else
            {
                if (!flag)
                {
                    if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyIWantRealWealth || x == RomanceReservationDescription.PropertyWeNeedToBeComfortable))
                    {
                        MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=P407baEa}I think you would need to rise considerably in the world before I could consider such a thing...", false);
                        return true;
                    }
                }
                if (flag)
                {
                    if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyIWantRealWealth))
                    {
                        MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=gS1noLvf}I do not know whether to find that charming or impertinent...", false);
                        return true;
                    }
                }
                if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionYoureNotMyType))
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=ltXu3DbR}Em... Yes, well, I suppose I can consider your offer.", false);
                }
                else if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.FamilyApprovalIAmGladYouAreFriendsWithOurFamily))
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=UQtXV3kf}Certainly, you have always been close to our family.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=VYmQmqIv}We are considering many offers. You may certainly add your name to the list.", false);
                }
            }
            return true;
        }

        private void courtship_conversation_leave_on_consequence()
        {
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            // Modules\SandBox\bin\...\SandBox.dll -> CampaignBehaviors
            // AlleyCampaignBehavior
            OccupationRomanceDialog(starter, "alley_talk_start");
            // ArenaMasterCampaignBehavior
            // BarberCampaignBehavior
            // BoardGameCampaignBehavior
            // CheckpointCampaignBehavior
            // ClanMemberRolesCampaignBehavior
            // CommonTownsfolkCampaignBehavior
            // CommonVillagersCampaignBehavior
            // CompanionDismissCampaignBehavior
            // ConversationAnimationToolCampaignBehavior
            // DefaultCutscenesCampaignBehavior
            // DefaultNotificationsCampaignBehavior
            // DumpIntegrityCampaignBehavior
            // GuardsCampaignBehavior
            // HeirSelectionCampaignBehavior
            // HideoutConversationsCampaignBehavior
            // PrisonBreakCampaignBehavior
            // RecruitmentAgentSpawnBehavior
            // RetirementCampaignBehavior
            // SettlementMusiciansCampaignBehavior
            // StatisticsCampaignBehavior
            // StealthCharactersCampaignBehavior
            // TavernEmployeesCampaignBehavior
            OccupationRomanceDialog(starter, "tavernmaid_talk");
            // TownMerchantsCampaignBehavior
            // TradersCampaignBehavior
        }

        private void OccupationRomanceDialog(CampaignGameStarter starter, string input, string output = "close_window")
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.RomanceCampaignBehavior
            starter.AddPlayerLine(input + "lord_special_request_flirt", input, input + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response", input + "lord_start_courtship_response", input + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_decline", input + "lord_start_courtship_response", output, "{=!}{COURTSHIP_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_decline_reaction_to_player_on_condition), null, 100, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_2", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_nevermind", input + "lord_start_courtship_response_player_offer", output, "{=D33fIGQe}Never mind.", null, null, 120, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_3", "{=!}{INITIAL_COURTSHIP_REACTION_TO_PLAYER}", new ConversationSentence.OnConditionDelegate(conversation_courtship_reaction_to_player_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_3", input + "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(courtship_conversation_leave_on_consequence), 100, null);
        }
    }
}
