using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PlayerEncounter), "set_LeaveEncounter")]
    class LeaveEncounterPatch
    {
        private static bool Prefix()
        {
            if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                return false;
            }
            return true;
        }
    }
}