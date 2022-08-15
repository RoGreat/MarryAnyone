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
        public static void RemoveExSpouses(Hero hero, bool completelyRemove = false)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> ExSpousesReadOnlyList;

            if (completelyRemove)
            {
                // Remove exspouse completely from list
                _exSpousesList = _exSpousesList.ToList();
                List<Hero> exSpouses = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();
                foreach (Hero exSpouse in exSpouses)
                {
                    _exSpousesList.Remove(exSpouse);
                }
            }
            else
            {
                // Standard remove duplicates spouse
                // Get exspouse list without duplicates
                _exSpousesList = _exSpousesList.Distinct().ToList();
                // If exspouse is already a spouse, then remove it
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                }
            }
            ExSpousesReadOnlyList = new MBReadOnlyList<Hero>(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }

        public static void OccupationToLord(Hero hero)
        {
            if (hero.Occupation != Occupation.Lord)
            {
                hero.SetNewOccupation(Occupation.Lord);
                MADebug.Print("Set New Occupation to Lord");
            }
        }
    }
}