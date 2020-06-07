using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class ConversationRomanceAtStagesOneAndTwoDiscussionOnConditionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            if (MASettings.Instance.IsVeryEasy() || MASettings.Instance.IsEasy() && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero)
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_2_discussions_on_condition")]
        private static bool Prefix2(ref bool __result)
        {
            if (MASettings.Instance.IsVeryEasy() || MASettings.Instance.IsEasy() && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}