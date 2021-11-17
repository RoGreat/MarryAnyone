using HarmonyLib;
using MarryAnyone.Models;
using MarryAnyone.Settings;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class RomanceCampaignBehaviorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
        private static void Postfix1(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero != null 
                && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) == null 
                && Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, Hero.OneToOneConversationHero);
        }

        [HarmonyPostfix]
        [HarmonyPatch("RomanceCourtshipAttemptCooldown", MethodType.Getter)]
        private static void Postfix2(ref CampaignTime __result)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.RetryCourtship)
            {
                __result = CampaignTime.DaysFromNow(1f);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_player_can_open_courtship_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            __result = conversation_player_can_open_courtship_on_condition();
            return false;
        }

        public static bool conversation_player_can_open_courtship_on_condition()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }
            bool flag = (Hero.MainHero.IsFemale && MAHelper.MASettings.SexualOrientation == "Heterosexual")
                    || (!Hero.MainHero.IsFemale && MAHelper.MASettings.SexualOrientation == "Homosexual") 
                    || (!Hero.OneToOneConversationHero.IsFemale && MAHelper.MASettings.SexualOrientation == "Bisexual");

#if TESTROMANCE && TRACELOAD
            MAHelper.Print(string.Format("Output {0}", MAHelper.LogPath), MAHelper.PRINT_TEST_ROMANCE);
#endif
            bool areMarried = Util.Util.AreMarried(Hero.MainHero, Hero.OneToOneConversationHero);
            Romance.RomanceLevelEnum romanceLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);

#if TESTROMANCE
            MAHelper.Print("Courtship Possible: " + Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, Hero.OneToOneConversationHero).ToString(), MAHelper.PRINT_TEST_ROMANCE);
            MAHelper.Print("Romantic Level: " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString(), MAHelper.PRINT_TEST_ROMANCE);
            MAHelper.Print("Retry Courtship: " + MAHelper.MASettings.RetryCourtship.ToString(), MAHelper.PRINT_TEST_ROMANCE);
            MAHelper.Print("romanceLevel: " + romanceLevel.ToString(), MAHelper.PRINT_TEST_ROMANCE);
#endif

            if (Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, Hero.OneToOneConversationHero)){

                if (romanceLevel == Romance.RomanceLevelEnum.Untested)
                {
                    if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                    {
                        if (Hero.OneToOneConversationHero.Spouse is null)
                        {
                            MBTextManager.SetTextVariable("FLIRTATION_LINE",
                                flag
                                    ? "{=lord_flirt}My lord, I note that you have not yet taken a spouse."
                                    : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                        }
                        else
                        {
                            MBTextManager.SetTextVariable("FLIRTATION_LINE",
                                flag
                                    ? "{=lord_cheating_flirt}My lord, I note that you might wish for a new spouse."
                                    : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                        }
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE",
                            flag
                                ? "{=goodman_flirt}Goodman, I note that you have not yet taken a spouse."
                                : "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer.", false);
                    }
                    return true;
                }
            }
            else
            {
                if (MAHelper.MASettings.RetryCourtship)
                {
                    if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility
                        || romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                        || (romanceLevel == Romance.RomanceLevelEnum.Ended && !areMarried)
                        )
                    {
                        if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                        {
                            MBTextManager.SetTextVariable("FLIRTATION_LINE",
                                flag
                                    ? "{=2WnhUBMM}My lord, may you give me another chance to prove myself?"
                                    : "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);
                        }
                        else
                        {
                            MBTextManager.SetTextVariable("FLIRTATION_LINE",
                                flag
                                    ? "{=goodman_chance}Goodman, may you give me another chance to prove myself?"
                                    : "{=goodwife_chance}Goodwife, may you give me another chance to prove myself?", false);
                        }
                        if (romanceLevel == Romance.RomanceLevelEnum.Ended)
                            // OnNeNousDitPasTout/GrandesMaree Patch
                            // Patch we must have only have one romance status for each relation
                            Util.Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero);

                        if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanceLevel == Romance.RomanceLevelEnum.Ended)
                        {
                            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
                        }
                        else if (romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
                        {
                            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                        }
                        return true;
                    }
                }
            }
#if TESTROMANCE
            MAHelper.Print("conversation_player_can_open_courtship_on_condition Repond FALSE", MAHelper.PRINT_TEST_ROMANCE);
#endif
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool conversation_romance_at_stage_1_discussions_on_conditionPrefix(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null)
            {
                __result = false;
                return false;
            }

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
#if TESTROMANCE
            MAHelper.Print(string.Format("RomanceCampaignBehaviorPatch::conversation_romance_at_stage_1_discussions_on_condition with {0} Difficulty ?= {1} RomanticLevel ?= {2}"
                            , Hero.OneToOneConversationHero.Name.ToString()
                            , MAHelper.MASettings.Difficulty
                            , romanticLevel.ToString()), MAHelper.PRINT_TEST_ROMANCE);
#endif
            if (MAHelper.MASettings.Difficulty == MASettings.DIFFICULTY_VERY_EASY
               || (MAHelper.MASettings.Difficulty == MASettings.DIFFICULTY_EASY && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                if (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted)
                    ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);

                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_2_discussions_on_condition")]
        private static bool conversation_romance_at_stage_2_discussions_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null)
            {
                __result = false;
                return false;
            }

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
#if TESTROMANCE
            MAHelper.Print(string.Format("conversation_romance_at_stage_1_discussions_on_condition with {0} Difficulty ?= {1} Romantilevle ?= {2}"
                    , Hero.OneToOneConversationHero.Name.ToString()
                    , MAHelper.MASettings.Difficulty
                    , romanticLevel.ToString()), MAHelper.PRINT_TEST_ROMANCE);
#endif
            if (MAHelper.MASettings.Difficulty == MASettings.DIFFICULTY_VERY_EASY)
                //|| (settings.Difficulty == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                if (romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible)
                    ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleAgreedOnMarriage);
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool conversation_finalize_courtship_for_hero_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null)
            {
                __result = false;
                return false;
            }

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            Romance.RomanticState romanticState = Romance.GetRomanticState(Hero.MainHero, Hero.OneToOneConversationHero);
            if (romanticState != null && romanticState.ScoreFromPersuasion == 0)
                romanticState.ScoreFromPersuasion = 60;

            __result = MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(Hero.MainHero, Hero.OneToOneConversationHero) 
                && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero) 
                && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;

            if (__result && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan))
            {
#if TESTROMANCE
                MAHelper.Print("RomanceCampaignBehaviorPatch:: conversation_finalize_courtship_for_hero_on_conditionPatch::FAIL car pas de clan (MARomanceCampaignBehavior work)", MAHelper.PRINT_TEST_ROMANCE);
#endif
                __result = false;
            }

#if TESTROMANCE
            MAHelper.Print(string.Format("RomanceCampaignBehaviorPatch:: conversation_finalize_courtship_for_hero_on_conditionPatch:: with {0} Difficulty ?= {1} répond {2} romanticState Score ?= {3}"
                    , Hero.OneToOneConversationHero.Name.ToString()
                    , MAHelper.MASettings.Difficulty
                    , __result
                    , (romanticState != null ? romanticState.ScoreFromPersuasion.ToString() : "NULL")), MAHelper.PRINT_TEST_ROMANCE);
#endif

            return false;
        }

        [HarmonyPatch("conversation_finalize_marriage_barter_consequence")]
        [HarmonyPrefix]
        private static bool conversation_finalize_marriage_barter_consequencePatch(RomanceCampaignBehavior __instance)
        {
            Hero heroBeingProposedTo = Hero.OneToOneConversationHero;
            if (Hero.OneToOneConversationHero.Clan != null)
            {
                foreach (Hero hero in Hero.OneToOneConversationHero.Clan.Lords)
                {
                    if (Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
                    {
                        heroBeingProposedTo = hero;
                        break;
                    }
                }
            }

            BarterManager bmInstance = BarterManager.Instance;
            Hero mainHero = Hero.MainHero;
            Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
            int score = 0;
            if (Romance.GetRomanticState(Hero.MainHero, heroBeingProposedTo) != null)
            {
                score = (int) Romance.GetRomanticState(Hero.MainHero, heroBeingProposedTo).ScoreFromPersuasion;
            }
#if TESTROMANCE
            MAHelper.Print(string.Format("RomanceCampaignBehaviorPatch:: conversation_finalize_marriage_barter_consequence between {0}\r\n\t and {1}\r\n\t BarterManager ?= {2}"
                                , mainHero.Name.ToString()
                                , MAHelper.TraceHero(heroBeingProposedTo)
                                , (bmInstance != null ? "Existe" : "NULL"))
                        , MAHelper.PrintHow.PrintToLogAndWriteAndForceDisplay);
#endif
            PartyBase mainParty = PartyBase.MainParty;
            MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;

            if (heroBeingProposedTo.Clan != null && heroBeingProposedTo.Clan.Leader != heroBeingProposedTo && heroBeingProposedTo.Spouse != mainHero)
            {
#if TESTROMANCE
                MAHelper.Print("StartBarterOffer", MAHelper.PRINT_TEST_ROMANCE);
#endif
#if V1640MORE
                MarriageBarterable marriageBarterable = new MarriageBarterable(Hero.MainHero, PartyBase.MainParty, heroBeingProposedTo, Hero.MainHero);
                bmInstance.StartBarterOffer(mainHero, oneToOneConversationHero, mainParty, (partyBelongedTo != null) ? partyBelongedTo.Party : null, null, (Barterable barterable, BarterData _args, object obj)
                            => BarterManager.Instance.InitializeMarriageBarterContext(barterable, _args
                                                    , new Tuple<Hero, Hero>(heroBeingProposedTo, Hero.MainHero))
                                                    , score, false, new Barterable[] { marriageBarterable  });
#else
                bmInstance.StartBarterOffer(mainHero, oneToOneConversationHero, mainParty, (partyBelongedTo != null) ? partyBelongedTo.Party : null, null, (Barterable barterable, BarterData _args, object obj)
                            => BarterManager.Instance.InitializeMarriageBarterContext(barterable, _args
                                                    , new Tuple<Hero, Hero>(heroBeingProposedTo, Hero.MainHero))
                                                    , score, false, null);
#endif
            }
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
            return false;
        }

    }
}