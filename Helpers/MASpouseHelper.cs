using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MarryAnyone.Helpers
{
    internal static class MASpouseHelper
    {
        public static void RemoveExSpouses(Hero hero, bool spouse = true)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> ExSpousesReadOnlyList;

            if (spouse)
            {
                _exSpousesList = _exSpousesList.Distinct().ToList();
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                }
            }
            else
            {
                _exSpousesList = _exSpousesList.ToList();
                Hero exSpouse = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).FirstOrDefault();
                if (exSpouse != null)
                {
                    _exSpousesList.Remove(exSpouse);
                }
            }
            ExSpousesReadOnlyList = new MBReadOnlyList<Hero>(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }
    }
}