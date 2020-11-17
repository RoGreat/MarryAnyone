using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Patches
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
            ICustomSettingsProvider settings = new MASettings();
            bool isPolygamous = settings.IsPolygamous && (maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero);

            if (!maidenOrSuitor.IsAlive || Hero.MainHero.ExSpouses.Contains(maidenOrSuitor) || maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate)
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
            ICustomSettingsProvider settings = new MASettings();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.IsIncestuous && isMainHero;
            bool discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

            if (isIncestuous)
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