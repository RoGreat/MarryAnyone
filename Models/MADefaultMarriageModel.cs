using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Models
{
    public class MADefaultMarriageModel : DefaultMarriageModel
    {

        //public bool accepteSansClan = false;

        public static bool IsCoupleSuitableForMarriageStatic(Hero firstHero, Hero secondHero)
        {
            ISettingsProvider settings = new MASettings();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.Incest && isMainHero;
            bool discoverAncestors = DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

            if (!isMainHero)
            {
                if (firstHero.Clan == null || secondHero.Clan == null)
                {
                    MAHelper.Print(string.Format("SuitableForMarriage:: entre {0} et {1} Echoue car il manque au moins un clan"
                                    , firstHero.Name.ToString(), secondHero.Name.ToString())
                                    , MAHelper.PRINT_TEST_ROMANCE);
                    return false;
                }
            }

            Clan clan = firstHero.Clan;
            if (clan?.Leader == firstHero && !isMainHero)
            {
                Clan clan2 = secondHero.Clan;
                if (clan2?.Leader == secondHero)
                {
                    return false;
                }
            }
            if (!isIncestuous)
            {
                if (discoverAncestors)
                {
                    List<Hero> ancetresEnCommun = DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).ToList<Hero>();
                    MAHelper.Print(string.Format("SuitableForMarriage:: entre {0} et {1} Ancetres en commun {2}"
                                    , firstHero.Name.ToString(), secondHero.Name.ToString()
                                    , string.Join(", ", ancetresEnCommun.Select<Hero, string>(x => x.Name.ToString())))
                                    , MAHelper.PRINT_TEST_ROMANCE);
                    return false;
                }
            }
            if (isHomosexual)
            {
                MAHelper.Print(string.Format("SuitableForMarriage::Homo entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (firstHero.IsFemale == secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero)))
                        , MAHelper.PRINT_TEST_ROMANCE);
                return firstHero.IsFemale == secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero);
            }
            if (isBisexual)
            {
                MAHelper.Print(string.Format("SuitableForMarriage::Bi entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero)))
                        , MAHelper.PRINT_TEST_ROMANCE);
                return IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero);
            }
            MAHelper.Print(string.Format("SuitableForMarriage::Hétéro entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                    , (firstHero.IsFemale != secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero)))
                    , MAHelper.PRINT_TEST_ROMANCE);
            return firstHero.IsFemale != secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero) && IsSuitableForMarriageStatic(secondHero);
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            return IsCoupleSuitableForMarriageStatic(firstHero, secondHero);
        }

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

        public static bool IsSuitableForMarriageStatic(Hero maidenOrSuitor)
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
                return false;
            }
            if ((maidenOrSuitor.Spouse is null && !maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)) || isPolygamous || isCheating)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }
            return false;
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            return IsSuitableForMarriageStatic(maidenOrSuitor);
        }
    }
}