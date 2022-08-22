using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Localization;
using static MarryAnyone.Debug;

namespace MarryAnyone.Patches
{
    // Patches for Lords romance dialogs
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal sealed class RomanceCampaignBehaviorPatches
    {
        /* Reverse Patches */
        [HarmonyReversePatch]
        [HarmonyPatch("MarriageCourtshipPossibility")]
        public static bool MarriageCourtshipPossibility(object instance, Hero person1, Hero person2)
        {
            throw new NotImplementedException();
        }

        /* Prefixes */
        // This one is very important to avoid crashing with those that do not have clans...
        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            if (Hero.OneToOneConversationHero.Clan is null)
            {
                __result = false;
                return false;
            }
            return true;
        }

        // Completely replaces Lords
        [HarmonyPrefix]
        [HarmonyPatch("conversation_player_can_open_courtship_on_condition")]
        private static bool Prefix2(ref bool __result, object __instance)
        {
            __result = conversation_player_can_open_courtship_on_condition(__instance);
            return false;
        }

        /* Postfixes */
        // Player eligible for marriage with Lord patch
        // How it was from the original mod just about
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
        private static void Postfix1(ref bool __result, object __instance)
        {
            __result = Hero.OneToOneConversationHero is not null && MarriageCourtshipPossibility(__instance, Hero.MainHero, Hero.OneToOneConversationHero);
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

        /* Methods */
        private static bool conversation_player_can_open_courtship_on_condition(object __instance)
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }

            MASettings settings = new();

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            bool courtshipPossible = MarriageCourtshipPossibility(__instance, Hero.MainHero, Hero.OneToOneConversationHero);

            Print("Romantic Level: " + romanticLevel);
            Print("Skip Courtship: " + settings.SkipCourtship);
            Print("Retry Courtship: " + settings.RetryCourtship);
            Print("Courtship Possible: " + courtshipPossible);

            if (courtshipPossible && romanticLevel == Romance.RomanceLevelEnum.Untested
                || (romanticLevel == Romance.RomanceLevelEnum.Ended && (settings.Cheating || settings.Polygamy)))
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer."
                        // Makes it simpler, and even if player is homosexual, this still makes some sense.
                        : "{=bjJs0eeB}My lord, I note that you have not yet taken a wife.", false);
                return true;
            }

            if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=4iTaEZKg}My lady, may you give me another chance to prove myself?"
                        : "{=2WnhUBMM}My lord, may you give me another chance to prove myself?", false);

                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility)
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