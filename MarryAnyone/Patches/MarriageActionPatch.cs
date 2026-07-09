using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(MarriageAction), "Apply")]
    class MarriageActionPatch
    {
        static void Prefix(Hero firstHero, Hero secondHero, bool showNotification = true)
        {
            if (firstHero.Occupation != Occupation.Lord)
            {
                firstHero.SetNewOccupation(Occupation.Lord);
            }
            if (secondHero.Occupation != Occupation.Lord)
            {
                secondHero.SetNewOccupation(Occupation.Lord);
            }
        }
    }
}
