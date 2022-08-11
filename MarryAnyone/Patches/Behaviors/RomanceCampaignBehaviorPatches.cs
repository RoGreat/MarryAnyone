using HarmonyLib;
using System;
using MarryAnyone.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Localization;
using MarryAnyone.Settings;
using MarryAnyone.Behaviors;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class RomanceCampaignBehaviorPatches
    {
        // Not a delegate
        [HarmonyReversePatch]
        [HarmonyPatch("MarriageCourtshipPossibility")]
        private static bool MarriageCourtshipPossibility(object instance, Hero person1, Hero person2)
        {
            throw new NotImplementedException();
        }


        // Romance Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_finalize_courtship_for_other_on_condition")]
        private static bool conversation_finalize_courtship_for_other_on_condition_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static bool conversation_finalize_courtship_for_other_on_condition()
        {
            return conversation_finalize_courtship_for_other_on_condition_patch(MarryAnyoneCampaignBehavior.RomanceCampaignBehaviorInstance);
        }

        // Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_marriage_barter_successful_on_condition")]
        private static bool conversation_marriage_barter_successful_on_condition_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static bool conversation_marriage_barter_successful_on_condition()
        {
            return conversation_marriage_barter_successful_on_condition_patch(MarryAnyoneCampaignBehavior.RomanceCampaignBehaviorInstance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
        private static void Postfix1(ref bool __result, object __instance)
        {
            __result = Hero.OneToOneConversationHero is not null && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) is null && MarriageCourtshipPossibility(__instance, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_hero_rltv_on_condition")]
        private static void Postfix2(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero is not null;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RomanceCourtshipAttemptCooldown", MethodType.Getter)]
        private static void Postfix3(ref CampaignTime __result)
        {
            IMASettingsProvider settings = new MASettings();
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
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool Prefix2(ref bool __result)
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