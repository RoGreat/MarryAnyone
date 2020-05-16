using TaleWorlds.CampaignSystem;
using HarmonyLib;
using System;

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