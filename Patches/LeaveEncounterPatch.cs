using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PlayerEncounter), "LeaveEncounter", MethodType.Setter)]
    internal class LeaveEncounterPatch
    {
        private static bool Prefix()
        {
            if (Hero.OneToOneConversationHero != null)
            {
                if (Hero.OneToOneConversationHero.PartyBelongedTo == MobileParty.MainParty)
                {
                    return false;
                }
            }
            return true;
        }
    }
}