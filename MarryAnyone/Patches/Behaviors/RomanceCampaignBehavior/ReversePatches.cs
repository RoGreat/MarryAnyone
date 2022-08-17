using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal partial class RomanceCampaignBehaviorPatches
    {
        // Private Method
        [HarmonyReversePatch]
        [HarmonyPatch("MarriageCourtshipPossibility")]
        public static bool MarriageCourtshipPossibility(object instance, Hero person1, Hero person2)
        {
            throw new NotImplementedException();
        }

        // Condition Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        public static bool conversation_finalize_courtship_for_hero_on_condition(object instance)
        {
            throw new NotImplementedException();
        }

        // Condition Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_courtship_decline_reaction_to_player_on_condition")]
        public static bool conversation_courtship_decline_reaction_to_player_on_condition(object instance)
        {
            throw new NotImplementedException();
        }

        // Condition Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_marriage_barter_successful_on_condition")]
        public static bool conversation_marriage_barter_successful_on_condition(object instance)
        {
            throw new NotImplementedException();
        }

        // Consequence Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("courtship_conversation_leave_on_consequence")]
        public static void courtship_conversation_leave_on_consequence(object instance)
        {
            throw new NotImplementedException();
        }

        // Consequence Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_player_opens_courtship_on_consequence")]
        public static void conversation_player_opens_courtship_on_consequence(object instance)
        {
            throw new NotImplementedException();
        }

        // Consequence Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_courtship_stage_2_success_on_consequence")]
        public static void conversation_courtship_stage_2_success_on_consequence(object instance)
        {
            throw new NotImplementedException();
        }
    }
}