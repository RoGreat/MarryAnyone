using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    class ConversationRomanceAtStagesOneAndTwoDiscussionOnConditionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            if (Hero.OneToOneConversationHero.IsPlayerCompanion || Hero.OneToOneConversationHero.IsNotable)
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
            if (Hero.OneToOneConversationHero.IsPlayerCompanion || Hero.OneToOneConversationHero.IsNotable)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}