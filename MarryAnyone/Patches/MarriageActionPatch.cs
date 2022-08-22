using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static MarryAnyone.Helpers;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(MarriageAction), "Apply")]
    internal sealed class MarriageActionPatch
    {
        private static void Postfix()
        {
            MASettings settings = new();
            Hero spouseHero = Hero.OneToOneConversationHero;
            // Do NOT break off marriages if polygamy is on...
            if (settings.Cheating && !settings.Polygamy)
            {
                CheatOnSpouse();
            }
            RemoveExSpouses(Hero.MainHero);
            RemoveExSpouses(spouseHero);
        }
    }
}