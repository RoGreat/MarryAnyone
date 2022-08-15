using HarmonyLib;
using SandBox.CampaignBehaviors;
using System;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
    internal class LordConversationsCampaignBehaviorPatches
    {
        // Condition Delegate
        [HarmonyReversePatch]
        [HarmonyPatch("conversation_lord_agrees_to_discussion_on_condition")]
        private static bool conversation_lord_agrees_to_discussion_on_condition_patch(object instance)
        {
            throw new NotImplementedException();
        }
        public static bool conversation_lord_agrees_to_discussion_on_condition()
        {
            return conversation_lord_agrees_to_discussion_on_condition_patch(SubModule.LordConversationsCampaignBehaviorInstance!);
        }
    }
}