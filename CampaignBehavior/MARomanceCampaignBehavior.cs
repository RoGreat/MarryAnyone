using HarmonyLib;
using MCM.Abstractions.Settings.Base.PerCharacter;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        private static bool activatedEncounter = false;

        protected void AddDialogs(CampaignGameStarter starter)
        {
            if (MASettings.Instance.Difficulty.SelectedValue == "Realistic")
            {
                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);
            }
            else
            {
                starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null);

                starter.AddPlayerLine("hero_romance_task", "hero_main_options", "lord_start_courtship_response_3", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 140, null, null);

                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);
            }
        }

        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            ActivateEncounter();
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (MASettings.Instance.Difficulty.SelectedValue == "Realistic")
            {
                MASubModule.MADebug("Realistic Mode");
                if (MADefaultMarriageModel.DiscoverAncestors(Hero.MainHero, 3).Intersect(MADefaultMarriageModel.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any<Hero>() && MASettings.Instance.IsIncestual)
                {
                    MASubModule.MADebug("Realistic: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                }
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MASubModule.MADebug("Realistic: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            }
            else
            {
                MASubModule.MADebug("Very Easy Mode or Easy Mode");
                if (MADefaultMarriageModel.DiscoverAncestors(Hero.MainHero, 3).Intersect(MADefaultMarriageModel.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any<Hero>() && MASettings.Instance.IsIncestual)
                {
                    if (MASettings.Instance.Difficulty.SelectedValue == "Easy")
                    {
                        MASubModule.MADebug("Easy: Incest");
                        return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    }
                    MASubModule.MADebug("Very Easy: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                }
                if (MASettings.Instance.Difficulty.SelectedValue == "Easy" && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
                {
                    MASubModule.MADebug("Easy: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
            }
        }

        private void conversation_courtship_success_on_consequence()
        {
            if (Hero.OneToOneConversationHero.IsNotable)
            {
                LeaveSettlementAction.ApplyForCharacterOnly(Hero.OneToOneConversationHero);
                AddHeroToPartyAction.Apply(Hero.OneToOneConversationHero, MobileParty.MainParty, true);
                MASubModule.MADebug("Notable joined your party");
            }
            if (Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                Hero.OneToOneConversationHero.CompanionOf = null;
                MASubModule.MADebug("Companion is no longer a companion");
            }
            if (Hero.OneToOneConversationHero.CharacterObject.Occupation != Occupation.Lord)
            {
                AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(Hero.OneToOneConversationHero.CharacterObject, Occupation.Lord, null);
                Hero.OneToOneConversationHero.IsNoble = true;
                MASubModule.MADebug("Spouse is now a lord");
            }
            if (Hero.OneToOneConversationHero.IsFactionLeader && !Hero.OneToOneConversationHero.IsMinorFactionHero)
            {
                if (Hero.MainHero.Clan.Kingdom != Hero.OneToOneConversationHero.Clan.Kingdom)
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(Hero.MainHero.Clan, Hero.OneToOneConversationHero.Clan.Kingdom, true);
                    MASubModule.MADebug("Joined spouse's kingdom");
                }
            }
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
            MASubModule.MADebug("Marriage action applied");
            PlayerEncounter.LeaveEncounter = true;
        }

        private static void ActivateEncounter()
        {
            if (!PlayerEncounter.IsActive)
            {
                PlayerEncounter.Start();
                PlayerEncounter.Current.SetupFields(PartyBase.MainParty, PartyBase.MainParty);
                activatedEncounter = true;
                MASubModule.MADebug("Activated");
            }
        }

        public static void DeactivateEncounter()
        {
            if (activatedEncounter)
            {
                if (Hero.OneToOneConversationHero == null)
                {
                    PlayerEncounter.Finish(false);
                    activatedEncounter = false;
                    MASubModule.MADebug("Deactivated");
                }
            }
        }

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