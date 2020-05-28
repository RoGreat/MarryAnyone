using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PlayerEncounter), "LeaveEncounter", MethodType.Setter)]
    internal class LeaveEncounterPatch
    {
        private static bool Prefix()
        {
            if (PlayerEncounter.IsActive)
            {
                return true;
            }
            return false;
        }
    }
}