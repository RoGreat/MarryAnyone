using HarmonyLib;
using MarryAnyone.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal partial class RomanceCampaignBehaviorPatches
    {
        /* Prefixes */
        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            if (CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && !Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                __result = false;
                return false;
            }
            return true;
        }

        /* Postfixes */
        // Player eligible for marriage with Lord patch
        // How it was from the original mod just about
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
        private static void Postfix1(ref bool __result, object __instance)
        {
            __result = Hero.OneToOneConversationHero is not null 
                && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) is null 
                && MarriageCourtshipPossibility(__instance, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        // Original method but take out the spouse part
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_hero_rltv_on_condition")]
        private static void Postfix2(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero is not null;
        }

        // Would really like to make this better...
        [HarmonyPostfix]
        [HarmonyPatch("RomanceCourtshipAttemptCooldown", MethodType.Getter)]
        private static void Postfix3(ref CampaignTime __result)
        {
            MASettings settings = new();
            if (settings.RetryCourtship)
            {
                __result = CampaignTime.DaysFromNow(1f);
            }
        }

        /* conversation_player_can_open_courtship_on_condition */
        [HarmonyPrefix]
        [HarmonyPatch("conversation_player_can_open_courtship_on_condition")]
        private static bool Prefix2(ref bool __result)
        {
            __result = conversation_player_can_open_courtship_on_condition();
            return false;
        }

        private static bool conversation_player_can_open_courtship_on_condition()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }

            MASettings settings = new();

            bool flag = Hero.MainHero.IsFemale && settings.SexualOrientation == "Heterosexual"
                || !Hero.MainHero.IsFemale && settings.SexualOrientation == "Homosexual"
                || !Hero.OneToOneConversationHero.IsFemale && settings.SexualOrientation == "Bisexual";

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            bool courtshipPossible = MarriageCourtshipPossibility(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero);

            MADebug.Print("Romantic Level: " + romanticLevel);
            MADebug.Print("Courtship Possible: " + courtshipPossible);
            MADebug.Print("Retry Courtship: " + settings.RetryCourtship);

            if (courtshipPossible && romanticLevel == Romance.RomanceLevelEnum.Untested)
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE",
                    flag
                        // Makes it simpler, and even if player is homosexual, this still makes sense.
                        ? "{=bjJs0eeB}My lord, I note that you have not yet taken a wife."
                        : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                return true;
            }

            if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility
                || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                || (romanticLevel == Romance.RomanceLevelEnum.Ended && settings.RetryCourtship))
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE",
                    flag
                        ? "{=2WnhUBMM}My lord, may you give me another chance to prove myself?"
                        : "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);

                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanticLevel == Romance.RomanceLevelEnum.Ended)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
                    }
                    else if (romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                    }
                }
                return true;
            }
            return false;
        }
    }
}