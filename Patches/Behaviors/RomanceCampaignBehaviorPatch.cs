using HarmonyLib;
using MarryAnyone.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class RomanceCampaignBehaviorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_on_condition")]
        private static void Postfix1(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero is not null && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) is null && Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero);
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
            ISettingsProvider settings = new MASettings();
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }
            bool flag = Hero.MainHero.IsFemale && settings.SexualOrientation == "Heterosexual" || !Hero.MainHero.IsFemale && settings.SexualOrientation == "Homosexual" || !Hero.OneToOneConversationHero.IsFemale && settings.SexualOrientation == "Bisexual";
            Romance.RomanceLevelEnum romanceLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero); // better to use a local variable
            MAHelper.Print("Courtship Possible: " + Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
            MAHelper.Print("Romantic Level: " + romanceLevel.ToString());
            MAHelper.Print("Retry Courtship: " + settings.RetryCourtship.ToString());

            if (Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanceLevel == Romance.RomanceLevelEnum.Untested)
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

            bool areMarried = Util.Util.AreMarried(Hero.MainHero, Hero.OneToOneConversationHero);
            if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility 
                || romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                || (romanceLevel == Romance.RomanceLevelEnum.Ended && settings.RetryCourtship && !areMarried)
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
                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanceLevel == Romance.RomanceLevelEnum.Ended)
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
                }
                return true;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix2(ref bool __result)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.Difficulty == "Very Easy" || (settings.Difficulty == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_2_discussions_on_condition")]
        private static bool Prefix3(ref bool __result)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.Difficulty == "Very Easy" || (settings.Difficulty == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool Prefix4(ref bool __result)
        {
            if (CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && !Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}