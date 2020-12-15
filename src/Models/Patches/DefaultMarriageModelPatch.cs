using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Models.Patches
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
            ISettingsProvider settings = new MASettings();
            bool inConversation, isCheating, isPolygamous;
            inConversation = isCheating = isPolygamous = false;
            if (Hero.OneToOneConversationHero != null)
            {
                inConversation = maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero;
                isCheating = settings.Cheating && inConversation && Hero.OneToOneConversationHero.Spouse != null;
                isPolygamous = settings.Polygamy && inConversation && Hero.OneToOneConversationHero.Spouse == null;
            }
            if (!maidenOrSuitor.IsAlive ||  maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate)
            {
                return false;
            }
            if (maidenOrSuitor.Spouse == null && !maidenOrSuitor.ExSpouses.Where(exSpouse => exSpouse.IsAlive).Any() || isPolygamous || isCheating)
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
            ISettingsProvider settings = new MASettings();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.Incest && isMainHero;
            bool discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();
            
            Clan clan = firstHero.Clan;
            if (clan?.Leader == firstHero && !isMainHero)
            {
                Clan clan2 = secondHero.Clan;
                if (clan2?.Leader == secondHero)
                {
                    return false;
                }
            }
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