﻿using HarmonyLib;
using MarryAnyone.Settings;
using MCM.Abstractions.Settings.Base.PerSave;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class RomanceCampaignBehaviorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_on_condition")]
        private static void Postfix1(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero != null && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) == null && Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero);
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
            if (MASettings.Instance == null || Hero.OneToOneConversationHero == null)
            {
                return false;
            }
            bool flag = Hero.MainHero.IsFemale && MASettings.Instance.SexualOrientation.SelectedValue == "Heterosexual" || !Hero.MainHero.IsFemale && MASettings.Instance.SexualOrientation.SelectedValue == "Homosexual" || !Hero.OneToOneConversationHero.IsFemale && MASettings.Instance.SexualOrientation.SelectedValue == "Bisexual";
            MASubModule.Debug("condition a");
            MASubModule.Debug("Courtship Possible: " + Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
            MASubModule.Debug("Romantic Level: " + (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested).ToString());
            if (Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
            {
                MASubModule.Debug("condition b");
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=bjJs0eeB}My lord, I note that you have not yet taken a wife."
                            : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=goodman_flirt}Goodman, I note that you have not yet taken a wife."
                            : "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer.", false);
                }
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MASubModule.Debug("condition c");
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
                return true;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix2(ref bool __result)
        {
            if (MASettings.Instance == null)
            {
                __result = false;
                return false;
            }
            if (MASettings.Instance.Difficulty.SelectedValue == "Very Easy" || (MASettings.Instance.Difficulty.SelectedValue == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
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
            if (MASettings.Instance == null)
            {
                __result = false;
                return false;
            }
            if (MASettings.Instance.Difficulty.SelectedValue == "Very Easy" || (PerSaveSettings < MASettings>.Instance.Difficulty.SelectedValue == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}