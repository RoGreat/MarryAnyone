using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone
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
            __result = RomanceCampaignBehaviorPatch.conversation_player_can_open_courtship_on_condition();
            return false;
        }

        public static bool conversation_player_can_open_courtship_on_condition()
        {
            bool flag = Hero.MainHero.IsFemale && MASubModule.Orientation == "Heterosexual" || !Hero.MainHero.IsFemale && MASubModule.Orientation == "Homosexual" || !Hero.OneToOneConversationHero.IsFemale && MASubModule.Orientation == "Bisexual";

            MASubModule.Debug("condition 1a");
            if (Hero.OneToOneConversationHero == null)
            {
                return false;
            }
            MASubModule.Debug("Courtship Possible: " + Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
            MASubModule.Debug("Romantic Level: " + (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested).ToString());
            if (Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
            {
                MASubModule.Debug("condition 1b");
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
                            ? "{=MA1hqoFgKW}Goodman, I note that you have not yet taken a wife."
                            : "{=MAbX3veGa2}Goodwife, I wish to profess myself your most ardent admirer.", false);
                }
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MASubModule.Debug("condition 1c");
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
                            ? "{=MAgkRbOqsP}Goodman, may you give me another chance to prove myself?"
                            : "{=MAgVM0EyGL}Goodwife, may you give me another chance to prove myself?", false);
                }
                // ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);   // Also needs work
                return true;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix2(ref bool __result)
        {
            if (MASubModule.Difficulty == "Very Easy" || MASubModule.Difficulty == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero)
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
            if (MASubModule.Difficulty == "Very Easy" || MASubModule.Difficulty == "Easy" && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}