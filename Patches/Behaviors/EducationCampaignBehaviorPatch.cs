using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(EducationCampaignBehavior), "GetHighestThreeAttributes")]
    internal class EducationCampaignBehaviorPatch
    {
        // Crash when there is a parent left out of education
        // Resort to Main Hero (adoption) when that happens
        private static void Prefix(ref Hero hero)
        {
            if (hero is null)
            {
                hero = Hero.MainHero;
            }
        }
    }
}