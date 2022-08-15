using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using MarryAnyone.Behaviors;

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
        [HarmonyPatch("conversation_finalize_courtship_for_other_on_condition")]
        private static bool conversation_finalize_courtship_for_other_on_condition_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static bool conversation_finalize_courtship_for_other_on_condition()
        {
            return conversation_finalize_courtship_for_other_on_condition_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        // Condition Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_marriage_barter_successful_on_condition")]
        private static bool conversation_marriage_barter_successful_on_condition_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static bool conversation_marriage_barter_successful_on_condition()
        {
            return conversation_marriage_barter_successful_on_condition_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        // Consequence Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("courtship_conversation_leave_on_consequence")]
        private static void courtship_conversation_leave_on_consequence_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static void courtship_conversation_leave_on_consequence()
        {
            MarryAnyoneCampaignBehavior.Instance!.RemoveHeroObjectFromCharacter();

            courtship_conversation_leave_on_consequence_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        // Consequence Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_player_opens_courtship_on_consequence")]
        private static void conversation_player_opens_courtship_on_consequence_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static void conversation_player_opens_courtship_on_consequence()
        {
            MarryAnyoneCampaignBehavior.Instance!.RemoveHeroObjectFromCharacter();

            conversation_player_opens_courtship_on_consequence_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }
    }
}