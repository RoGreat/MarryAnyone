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
        private static bool _needToSupprimeFichier = false;

        public static MASettings MASettings
        {
            get
            {
                if (_MASettings == null)
                    _MASettings = new MASettings();
                return _MASettings;
            }
        }

        private static MASettings? _MASettings = null;

        public static void MASettingsClean()
        {
            _MASettings = null;
        }


        public enum PrintHow // Bitwise enumération
        {
            PrintRAS = 0,
            PrintDisplay = 1,
            PrintForceDisplay = 2,
            PrintToLog = 4,
            UpdateLog = 8,
            PrintToLogAndWrite = 12,
            PrintToLogAndWriteAndDisplay = 13,
            PrintToLogAndWriteAndForceDisplay = 14
        }

        public enum Etape
        {
            EtapeInitialize = 1,
            EtapeLoad = 2,
            EtapeLoadPas2 = 4,
        }

        public static Etape MAEtape;

#if TRACEWEDDING
        public const PrintHow PRINT_TRACE_WEDDING = PrintHow.PrintToLog | PrintHow.PrintDisplay;
#else
        public const PrintHow PRINT_TRACE_WEDDING = PrintHow.PrintDisplay;
#endif
#if TRACELOAD
        public const PrintHow PRINT_TRACE_LOAD = PrintHow.PrintToLog;
#else
        public const PrintHow PRINT_TRACE_LOAD = PrintHow.PrintDisplay;
#endif
#if TESTPREGNANCY
        public const PrintHow PRINT_TEST_PREGNANCY = PrintHow.PrintToLog | PrintHow.PrintDisplay;
#else
        public const PrintHow PRINT_TEST_PREGNANCY = PrintHow.PrintDisplay;
#endif
#if TESTROMANCE
        public const PrintHow PRINT_TEST_ROMANCE = PrintHow.PrintToLog | PrintHow.UpdateLog;
#else
        public const PrintHow PRINT_TEST_ROMANCE = PrintHow.PrintRAS;
#endif
#if TRACELOAD
        public const PrintHow PRINT_PATCH = PrintHow.PrintToLog | PrintHow.UpdateLog;
        public const PrintHow PRINT_PATCH = PrintHow.PrintToLog | PrintHow.PrintForceDisplay;
#else
        public const PrintHow PRINT_PATCH = PrintHow.PrintToLogAndWrite;
#endif

        public static string? LogPath 
        {
            get => _logPath;
            set
            {
                _logPath = value;
                _needToSupprimeFichier = true;

            }
        }
        private static string? _logPath;

        public static string VersionGet
        {
            get { 
                if (_version == null) {
                    //_version = Assembly.GetEntryAssembly().GetCustomAttributes<Version>().ToString();
                    _version = typeof(MASubModule).Assembly.GetName().Version.ToString();
                    //_version = Version.
                    if (_version == null)
                        _version = "Retrieve Version FAIL";
                }
                return _version;
            }

        }
        private static string? _version = null;

        public static string ModuleNameGet
        {
            get
            {
                if (_moduleName == null)
                {
                    _moduleName = typeof(MASubModule).Assembly.GetName().Name.ToString();
                    if (_moduleName == null)
                        _moduleName = "Retrieve module name FAIL";
                }
                return _moduleName;
            }
        }
        private static string? _moduleName = null;

        public static void Print(string message, PrintHow printHow = PrintHow.PrintRAS)
        {
            if ((MASettings.Debug  && (printHow & PrintHow.PrintDisplay) != 0)  || (printHow & PrintHow.PrintForceDisplay) !=0)
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
                    if (_needToSupprimeFichier)
                    {
                        _fichier = new FileStream(LogPath + "\\MarryAnyOne.log", FileMode.Create, FileAccess.Write, FileShare.Read);
                        _needToSupprimeFichier = false;
                    }
                    else
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
                _sw.WriteLine(string.Concat(ModuleNameGet, "(", VersionGet, ") ", prefix != null ? prefix + "" : "", "[", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), "]::", text));
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
            String message = ModuleNameGet + ": " + exception.Message;
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

                Print(String.Format("Swap Occupation To Lord for {0}", character.Name.ToString()), PrintHow.PrintToLogAndWriteAndDisplay);

                AccessTools.Field(typeof(CharacterObject), "_originCharacter").SetValue(character, CharacterObject.PlayerCharacter);
                AccessTools.Field(typeof(CharacterObject), "_originCharacterStringId").SetValue(character, CharacterObject.PlayerCharacter.StringId);
            }
        }

        public static bool PatchHeroPlayerClan(Hero hero)
        {
            if (hero.Clan != Clan.PlayerClan
                || (Clan.PlayerClan != null && Clan.PlayerClan.Lords.IndexOf(hero) < 0)) // Else lost the town govenor post on hero.Clan = null !!
            {
                hero.Clan = null;
                hero.Clan = Clan.PlayerClan;
                Print("Patch Hero with PlayerClan " + hero.Name.ToString(), PrintHow.PrintToLogAndWriteAndDisplay);
                return true;
            }
            return false;
        }

        public static List<Hero> ListClanLord(Hero hero)
        {
            List<Hero> ret = new List<Hero>();
            ret.Add(hero);
            if (hero.Clan != null)
            {
                foreach(Hero h in hero.Clan.Lords) 
                {
                    if (h != hero)
                        ret.Add(h);
                }
            }
            return ret;
        }

#if TRACELOAD || TESTROMANCE
        public static String TraceHero(Hero hero, String prefix = null)
        {
            String aff = (String.IsNullOrWhiteSpace(prefix) ? "" : (prefix + "::")) + hero.Name;

            if (!hero.IsAlive)
                aff += ", DEAD";

            if (hero.IsDead)
                aff += ", REALY DEAD";

            if (!hero.IsActive)
                aff += ", INACTIF";

            aff += ", State " + hero.HeroState;

            if (hero.IsWanderer)
                aff += ", Wanderer";

            if (hero.IsPlayerCompanion)
                aff += ", PLAYER Companion";

            if (hero.IsPrisoner)
                aff += ", PRISONER";

            if (hero.Clan != null)
                aff += ", Clan " + hero.Clan.Name;

            if (hero.MapFaction != null)
                aff += ", MAP Faction " + hero.MapFaction.Name;

            if (hero.Spouse != null)
                aff += ", Spouse" + hero.Spouse.Name;

            if (hero.CurrentSettlement != null)
                aff += ", Settlement " + hero.CurrentSettlement.Name;

            if (MAEtape >= Etape.EtapeLoadPas2)
            {
                if (hero.PartyBelongedTo != null)
                    aff += ", In Party " + hero.PartyBelongedTo.Name;

                if (hero.IsSpecial)
                    aff += ", IS Special";

                if (hero.IsTemplate)
                    aff += ", IS Tempalte";

                if (hero.IsPreacher)
                    aff += ", IS Preacher";
            }

            return aff;

        }
#endif

    }
}