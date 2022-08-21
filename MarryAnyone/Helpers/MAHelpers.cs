using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MarryAnyone.Helpers
{
    internal static class MAHelpers
    {
        private static FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");

        private static FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");

        public static void RemoveExSpouses(Hero hero, bool duplicates = true)
        {
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);

            if (duplicates)
            {
                // Standard remove duplicates spouse
                // Get exspouse list without duplicates
                _exSpousesList = _exSpousesList.Distinct().ToList();
                // If exspouse is already a spouse, then remove from exspouses
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                    MADebug.Print($"Removed duplicate spouse {hero.Spouse.Name.ToString()}");
                }
            }
            else
            {
                // Remove all exspouses
                _exSpousesList = _exSpousesList.ToList();
                List<Hero> exSpouses = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();
                foreach (Hero exSpouse in exSpouses)
                {
                    // Remove exspouse from list
                    _exSpousesList.Remove(exSpouse);

                    // Look into your exspouse's exspouse to remove yourself
                    List<Hero> _exSpousesList2 = (List<Hero>)_exSpouses.GetValue(exSpouse);
                    _exSpousesList2.Remove(hero);

                    MBReadOnlyList<Hero> ExSpousesReadOnlyList2 = new(_exSpousesList2);
                    _exSpouses.SetValue(exSpouse, _exSpousesList2);
                    ExSpouses.SetValue(exSpouse, ExSpousesReadOnlyList2);
                }
            }

            MBReadOnlyList<Hero> ExSpousesReadOnlyList = new(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }

        public static void CheatOnSpouse()
        {
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(Hero.MainHero);
            List<Hero> cheatedHeroes = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();

            foreach (Hero cheatedHero in cheatedHeroes)
            {
                RemoveExSpouses(cheatedHero, false);
                if (cheatedHero != Hero.MainHero.Spouse)
                {
                    MADebug.Print($"Broke off marriage with {cheatedHero.Name.ToString()}");
                }
                else
                {
                    MADebug.Print($"Removed duplicate spouse {cheatedHero.Name.ToString()}");
                }
            }
        }
    }
}