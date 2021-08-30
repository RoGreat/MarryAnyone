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

        private static FileStream? _fichier = null;
        private static StreamWriter? _sw = null;

        public enum PrintHow // Bitwise enumération
        {
            PrintRAS = 0,
            PrintDisplay = 1,
            PrintForceDisplay = 2,
            PrintToLog = 4,
            UpdateLog = 8,
            PrintToLogAndWrite = 12
        }

#if TESTROMANCE
        public const PrintHow PRINT_TEST_ROMANCE = PrintHow.PrintToLog;
        public const PrintHow PRINT_PATCH = PrintHow.PrintToLog | PrintHow.PrintForceDisplay;
#else
        public const PrintHow PRINT_TEST_ROMANCE = PrintHow.PrintRAS;
        public const PrintHow PRINT_PATCH = PrintHow.PrintRAS;
#endif
        public static string? LogPath { get; set; }

        public static void Print(string message, PrintHow printHow = PrintHow.PrintRAS)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.Debug || (printHow & PrintHow.PrintForceDisplay) !=0)
            {
                // Custom purple!
                Color color = new(0.6f, 0.2f, 1f);
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
            if ((printHow & PrintHow.PrintToLog) != 0 && LogPath != null)
            {
                Log(message);
            }
            if ((printHow & PrintHow.UpdateLog) != 0 && LogPath != null)
                LogClose();
        }
        public static void PrintWithColor(string message, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, color));
        }

        public static void Log(string text, string? prefix = null)
        {
            if (_sw == null && !string.IsNullOrEmpty(LogPath))
            {
                try
                {
                    _fichier = new FileStream(LogPath + "\\MarryAnyOne.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                    if (_fichier != null)
                    {
                        _fichier.Seek(0, SeekOrigin.End);
                        _sw = new StreamWriter(stream: (FileStream)_fichier, encoding: System.Text.Encoding.UTF8);
                    }

                }
                catch (Exception ex)
                {
                    Print("Exception during StreamWrite" + ex.ToString(), PrintHow.PrintForceDisplay);
                    //Something has gone horribly wrong.
                }
            }

            if (_sw != null)
            {
                //string version = ModuleInfo.GetModules().Where(x => x.Name == "Tournaments XPanded").FirstOrDefault().Version.ToString();
                string version = "MarryAnyOne v160";
                _sw.WriteLine(string.Concat("(", version, ") ", prefix != null ? prefix + "" : "", "[", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), "]::", text));
            }
        }

        public static void LogClose()
        {
            try
            {
                try
                {
                    if (_sw != null)
                    {
                        _sw.Flush();
                        _sw.Close();
                        _sw = null;
                    }
                }
                finally
                {
                    _sw = null;
                    if (_fichier != null)
                        _fichier.Dispose();
                    _fichier = null;
                }
            }
            finally
            {
                _fichier = null;

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