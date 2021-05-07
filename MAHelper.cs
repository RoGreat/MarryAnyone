using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone
{
    internal static class MAHelper
    {
        public static void Print(string message, bool notification = false)
        {
            ISettingsProvider settings = new MASettings();
            Color color;
            if (notification)
            {
                color = Colors.Red;
            }
            else
            {
                // My custom purple
                color = new Color(0.6f, 0.2f, 1f);
            }
            if (settings.Debug || notification)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }

        public static void RemoveExSpouses(Hero hero, bool isCheating = false)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> ExSpousesReadOnlyList;

            if (!isCheating)
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
                if (exSpouse is not null)
                {
                    _exSpousesList.Remove(exSpouse);
                }
            }
            ExSpousesReadOnlyList = new MBReadOnlyList<Hero>(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }

        public static void OccupationToLord(CharacterObject character)
        {
            if (character.Occupation != Occupation.Lord)
            {
                AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(character, Occupation.Lord);
                Print("Occupation To Lord");
                AccessTools.Field(typeof(CharacterObject), "_originCharacter").SetValue(character, CharacterObject.PlayerCharacter);
                AccessTools.Field(typeof(CharacterObject), "_originCharacterStringId").SetValue(character, CharacterObject.PlayerCharacter.StringId);
            }
        }
    }
}