using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using System;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal class ConversationRomanceAtStagesOneAndTwoDiscussionOnConditionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.VeryEasy || (config.Difficulty == Difficulty.Easy && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
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
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.VeryEasy || (config.Difficulty == Difficulty.Easy && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}