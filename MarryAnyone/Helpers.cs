using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using static MarryAnyone.Debug;

namespace MarryAnyone
{
    internal static class Helpers
    {
        private static readonly AccessTools.FieldRef<Hero, MBReadOnlyList<Hero>>? ExSpouses = AccessTools2.FieldRefAccess<Hero, MBReadOnlyList<Hero>>("ExSpouses");

        private static readonly AccessTools.FieldRef<Hero, List<Hero>>? _exSpouses = AccessTools2.FieldRefAccess<Hero, List<Hero>>("_exSpouses");

        public enum RemoveExSpousesEnum
        {
            Duplicates,
            Self,
            All
        }

        public static void ResetEndedCourtships()
        {
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
            {
                if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                {
                    if (romanticState.Level == Romance.RomanceLevelEnum.Ended)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Untested;
                    }
                }
            }
        }

        public static void RemoveExSpouses(Hero hero, RemoveExSpousesEnum removalMode = RemoveExSpousesEnum.Duplicates)
        {
            List<Hero> _exSpousesList = _exSpouses!(hero);

            if (removalMode == RemoveExSpousesEnum.Duplicates)
            {
                // Standard remove duplicates spouse
                // Get exspouse list without duplicates
                _exSpousesList = _exSpousesList.Distinct().ToList();
                // If exspouse is already a spouse, then remove from exspouses
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                    Print($"Removed duplicate spouse {hero.Spouse.Name}");
                }
            }
            else
            {
                // Remove all exspouses
                _exSpousesList = _exSpousesList.ToList();
                List<Hero> exSpouses = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();
                foreach (Hero exSpouse in exSpouses)
                {
                    if (removalMode == RemoveExSpousesEnum.Self || removalMode == RemoveExSpousesEnum.All)
                    {
                        // Remove exspouse from list
                        _exSpousesList.Remove(exSpouse);
                    }
                    if (removalMode == RemoveExSpousesEnum.All)
                    {
                        // Look into your exspouse's exspouse to remove yourself
                        List<Hero> _exSpousesList2 = _exSpouses!(hero);
                        _exSpousesList2.Remove(hero);

                        MBReadOnlyList<Hero> ExSpousesReadOnlyList2 = new(_exSpousesList2);
                        _exSpouses(exSpouse) = _exSpousesList2;
                        ExSpouses!(exSpouse) = ExSpousesReadOnlyList2;
                    }
                }
            }

            MBReadOnlyList<Hero> ExSpousesReadOnlyList = new(_exSpousesList);
            _exSpouses(hero) = _exSpousesList;
            ExSpouses!(hero) = ExSpousesReadOnlyList;
        }

        public static void CheatOnSpouse()
        {
            List<Hero> _exSpousesList = _exSpouses!(Hero.MainHero);
            List<Hero> cheatedHeroes = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();

            foreach (Hero cheatedHero in cheatedHeroes)
            {
                RemoveExSpouses(cheatedHero, RemoveExSpousesEnum.All);
                if (cheatedHero != Hero.MainHero.Spouse)
                {
                    // Almost forgot to add in an ended romantic state for cheated heroes!
                    ChangeRomanticStateAction.Apply(Hero.MainHero, cheatedHero, Romance.RomanceLevelEnum.Ended);
                    Print($"Broke off marriage with {cheatedHero.Name}");
                }
                else
                {
                    Print($"Removed duplicate spouse {cheatedHero.Name}");
                }
            }
        }
    }
}