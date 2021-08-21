using HarmonyLib;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace MarryAnyone
{
    internal static class MAHelper
    {

        public static string? LogPath { get; set; }

        public static void Print(string message, bool bForce = false)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.Debug || bForce)
            {
                // Custom purple!
                Color color = new(0.6f, 0.2f, 1f);
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }
        public static void PrintWithColor(string message, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, color));
        }

        public static void Log(string text, string? prefix = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(LogPath))
                {
                    using (var sw = new StreamWriter(LogPath, true))
                    {
                        //string version = ModuleInfo.GetModules().Where(x => x.Name == "Tournaments XPanded").FirstOrDefault().Version.ToString();
                        string version = "MarryAnyOne v160";
                        sw.WriteLine(string.Concat("(", version, ") ", prefix != null ? prefix + ":: " : "", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), "\n", text));
                    }
                }
            }
            catch
            {
                //Something has gone horribly wrong.
            }
        }

        public static void Error(Exception exception)
        {
            String message = "Marry Anyone: " + exception.Message;
            InformationManager.DisplayMessage(new InformationMessage(message, Colors.Red));
            Log(message, "ERROR");
        }

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
                foreach(Hero exSpouse in exSpouses)
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