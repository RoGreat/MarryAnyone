using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PlayerEncounter), "LeaveEncounter", MethodType.Setter)]
    internal class LeaveEncounterPatch
    {
        private static bool Prefix()
        {
            if (Hero.OneToOneConversationHero is not null)
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