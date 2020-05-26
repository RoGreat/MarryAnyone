using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PlayerEncounter), "LeaveEncounter", MethodType.Setter)]
    internal class LeaveEncounterPatch
    {
        private static bool Prefix()
        {
            if (Hero.OneToOneConversationHero != null && (Hero.OneToOneConversationHero.IsWanderer || Hero.OneToOneConversationHero.IsPlayerCompanion))
            {
                return false;
            }
            return true;
        }
    }
}