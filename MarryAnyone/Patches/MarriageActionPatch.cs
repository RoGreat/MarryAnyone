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
            if (firstHero.Clan is null && secondHero.Clan is not null)
            {
                firstHero.Clan = secondHero.Clan;
            }
            if (secondHero.Clan is null && firstHero.Clan is not null)
            {
                secondHero.Clan = firstHero.Clan;
            }
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
