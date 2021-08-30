using HarmonyLib;
using MarryAnyone.Models;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(DefaultMarriageModel), "IsCoupleSuitableForMarriage", new Type[] {typeof(Hero), typeof(Hero) })]
    public class DefaultMarriageModel_IsCoupleSuitableForMarriage_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(DefaultMarriageModel __instance, Hero firstHero, Hero secondHero, ref bool __result)
        {
            __result = MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(firstHero, secondHero);
            return false;

        }
    }

    [HarmonyPatch(typeof(DefaultMarriageModel), "IsSuitableForMarriage", new Type[] { typeof(Hero) })]
    public class DefaultMarriageModel_IsSuitableForMarriage_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(DefaultMarriageModel __instance, Hero maidenOrSuitor, ref bool __result)
        {
            __result = MADefaultMarriageModel.IsSuitableForMarriageStatic(maidenOrSuitor);

            return false;
        }
    }

    internal static class DefaultMarriageModelHelp
    {
        public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            if (hero is not null)
            {
                yield return hero;
                if (n > 0)
                {
                    foreach (Hero hero2 in DiscoverAncestors(hero.Mother, n - 1))
                    {
                        yield return hero2;
                    }
                    foreach (Hero hero3 in DiscoverAncestors(hero.Father, n - 1))
                    {
                        yield return hero3;
                    }
                }
            }
            yield break;
        }
    }
}
