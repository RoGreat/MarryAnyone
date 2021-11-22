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

        public static bool IsCoupleSuitableForMarriageStatic(Hero firstHero, Hero secondHero, bool canCheat)
        {
            ISettingsProvider settings = MAHelper.MASettings; // new MASettings();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.Incest && isMainHero;
            bool discoverAncestors = DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

            if (!isMainHero)
            {
                if (firstHero.Clan == null || secondHero.Clan == null)
                {
#if TRACEROMANCEISSUITABLE
                    MAHelper.Print(string.Format("SuitableForMarriage:: entre {0} et {1} Echoue car il manque au moins un clan"
                                    , firstHero.Name.ToString(), secondHero.Name.ToString())
                                    , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif
                    return false;
                }
                if ((firstHero.Spouse != null && !firstHero.Spouse.IsDead)
                    || (secondHero.Spouse != null && !secondHero.Spouse.IsDead))
                {
#if TRACEROMANCEISSUITABLE
                    MAHelper.Print(string.Format("SuitableForMarriage:: entre {0} et {1} Echoue car un des héros est déjà marié"
                                    , firstHero.Name.ToString(), secondHero.Name.ToString())
                                    , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif

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
#if TRACEROMANCEISSUITABLE
                    MAHelper.Print(string.Format("SuitableForMarriage:: entre {0} et {1} Ancetres en commun {2}"
                                    , firstHero.Name.ToString(), secondHero.Name.ToString()
                                    , string.Join(", ", ancetresEnCommun.Select<Hero, string>(x => x.Name.ToString())))
                                    , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif
                    return false;
                }
            }
            if (isHomosexual)
            {
#if TRACEROMANCEISSUITABLE
                MAHelper.Print(string.Format("SuitableForMarriage::Homo entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (firstHero.IsFemale == secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat)))
                        , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif
                return firstHero.IsFemale == secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat);
            }
            if (isBisexual)
            {
#if TRACEROMANCEISSUITABLE
                MAHelper.Print(string.Format("SuitableForMarriage::Bi entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                        , (IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat)))
                        , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif
                return IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat);
            }
#if TRACEROMANCEISSUITABLE
            MAHelper.Print(string.Format("SuitableForMarriage::Hétéro entre {0} et {1} répond {2}", firstHero.Name.ToString(), secondHero.Name.ToString()
                    , (firstHero.IsFemale != secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat)))
                    , MAHelper.PRINT_TRACE_ROMANCE_IS_SUITABLE);
#endif
            return firstHero.IsFemale != secondHero.IsFemale && IsSuitableForMarriageStatic(firstHero, canCheat) && IsSuitableForMarriageStatic(secondHero, canCheat);
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            return IsCoupleSuitableForMarriageStatic(firstHero, secondHero, false);
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


        public static bool IsSuitableForCheatingStatic(Hero maidenOrSuitor)
        {

            if (!MAHelper.IsSuitableForMarriagePathMA(maidenOrSuitor))
                return false;

            if (!MAHelper.MASettings.Cheating) return false;

            if (maidenOrSuitor.Spouse != null
                || (maidenOrSuitor.ExSpouses != null &&
                    maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)))
            {
                if (maidenOrSuitor.IsFemale)
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }

            return false;

        }

        public static bool IsSuitableForMarriageStatic(Hero maidenOrSuitor, bool canCheat = false)
        {
            if (!MAHelper.IsSuitableForMarriagePathMA(maidenOrSuitor))
                return false;

            bool inConversation, isCheating, isPolygamous;
            inConversation = isCheating = isPolygamous = false;
            if (maidenOrSuitor == Hero.MainHero)
            {
                isCheating = MAHelper.MASettings.Cheating;  
                isPolygamous = MAHelper.MASettings.Polygamy; 
            }
            else if (canCheat)
                isCheating = MAHelper.MASettings.Cheating;  

            if (isPolygamous || isCheating || (maidenOrSuitor.Spouse is null && !maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)))
            {
                if (maidenOrSuitor.IsFemale)
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
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