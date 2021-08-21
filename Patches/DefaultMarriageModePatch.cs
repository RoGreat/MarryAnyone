using HarmonyLib;
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
            ISettingsProvider settings = new MASettings();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.Incest && isMainHero;
            bool discoverAncestors = DefaultMarriageModelHelp.DiscoverAncestors(firstHero, 3).Intersect(DefaultMarriageModelHelp.DiscoverAncestors(secondHero, 3)).Any();

            //if (!accepteSansClan && (firstHero.Clan == null || secondHero.Clan == null))
            //    return false;

            Clan clan = firstHero.Clan;
            if (clan?.Leader == firstHero && !isMainHero)
            {
                Clan clan2 = secondHero.Clan;
                if (clan2?.Leader == secondHero)
                {
                    goto returnFalse;
                }
            }
            if (!isIncestuous)
            {
                if (discoverAncestors)
                {
                    List<Hero> ancetresEnCommun = DefaultMarriageModelHelp.DiscoverAncestors(firstHero, 3).Intersect(DefaultMarriageModelHelp.DiscoverAncestors(secondHero, 3)).ToList<Hero>();
                    MAHelper.Print(string.Format("SuitableForMarriage:: Ancetres en commun {0}", string.Join(", ", ancetresEnCommun.Select<Hero, string>(x => x.Name.ToString()))));
                    goto returnFalse;
                }
            }
            if (isHomosexual)
            {
                MAHelper.Print(string.Format("SuitableForMarriage::Homo entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (firstHero.IsFemale == secondHero.IsFemale && __instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))));
                if (firstHero.IsFemale == secondHero.IsFemale && __instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))
                    goto returnTrue;
                goto returnFalse;
            }
            if (isBisexual)
            {
                MAHelper.Print(string.Format("SuitableForMarriage::Bi entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (__instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))));
                if (__instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))
                    goto returnTrue;
                goto returnFalse;
            }
            MAHelper.Print(string.Format("SuitableForMarriage::Hétéro entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                    , (firstHero.IsFemale != secondHero.IsFemale && __instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))));
            if (firstHero.IsFemale != secondHero.IsFemale && __instance.IsSuitableForMarriage(firstHero) && __instance.IsSuitableForMarriage(secondHero))
                goto returnTrue;
            goto returnFalse;

        returnTrue:
            __result = true;
            goto avantRetour;

        returnFalse:
            __result = false;
            goto avantRetour;
        avantRetour:

            return false;

        }
    }

    [HarmonyPatch(typeof(DefaultMarriageModel), "IsSuitableForMarriage", new Type[] { typeof(Hero) })]
    public class DefaultMarriageModel_IsSuitableForMarriage_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(DefaultMarriageModel __instance, Hero maidenOrSuitor, ref bool __result)
        {
            ISettingsProvider settings = new MASettings();
            bool inConversation, isCheating, isPolygamous;
            inConversation = isCheating = isPolygamous = false;
            if (Hero.OneToOneConversationHero is not null)
            {
                inConversation = maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero;
                isCheating = settings.Cheating && inConversation;
                isPolygamous = settings.Polygamy && inConversation;
            }
            if (!maidenOrSuitor.IsAlive || maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate)
            {
                goto returnFalse;
            }
            if ((maidenOrSuitor.Spouse is null && !maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)) || isPolygamous || isCheating)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    if (maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale)
                        goto returnTrue;
                    goto returnFalse;
                }
                if (maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale)
                    goto returnTrue;
                goto returnFalse;
            }
            goto returnFalse;

        returnTrue:
            __result = true;
            goto avantRetour;

        returnFalse:
            __result = false;
            goto avantRetour;
        avantRetour:

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
