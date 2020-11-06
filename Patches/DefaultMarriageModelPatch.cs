using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(DefaultMarriageModel))]
    internal class DefaultMarriageModelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("IsSuitableForMarriage")]
        private static bool Prefix1(ref bool __result, Hero maidenOrSuitor)
        {
            __result = IsSuitableForMarriage(maidenOrSuitor);
            return false;
        }

        public static bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            bool isPolygamous = MASubModule.Polygamy && (maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero);

            if (!maidenOrSuitor.IsAlive || Hero.MainHero.ExSpouses.Contains(maidenOrSuitor) || maidenOrSuitor.IsTemplate)
            {
                return false;
            }
            if (maidenOrSuitor.Spouse == null || isPolygamous)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsCoupleSuitableForMarriage")]
        private static bool Prefix2(ref bool __result, Hero firstHero, Hero secondHero)
        {
            __result = IsCoupleSuitableForMarriage(firstHero, secondHero);
            return false;
        }

        public static bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = MASubModule.Orientation == "Homosexual" && isMainHero;
            bool isBisexual = MASubModule.Orientation == "Bisexual" && isMainHero;
            bool isIncestual = MASubModule.Incest && isMainHero;
            bool discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

            if (isIncestual)
            {
                discoverAncestors = true;
            }
            if (isHomosexual)
            {
                return firstHero.IsFemale == secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            if (isBisexual)
            {
                return discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            return firstHero.IsFemale != secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            if (hero != null)
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