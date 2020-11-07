using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

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
                    Trace.WriteLine("Made It");
                    return false;
                }
            }
            Trace.WriteLine("Didn't Make It");
            return true;
        }
    }
}