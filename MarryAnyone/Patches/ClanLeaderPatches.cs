using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_finalize_courtship_for_hero_on_condition")]
    class conversation_finalize_courtship_for_hero_on_condition_patch
    {
        static bool Prefix(ref bool __result)
        {
            if (Hero.OneToOneConversationHero.Clan is null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
