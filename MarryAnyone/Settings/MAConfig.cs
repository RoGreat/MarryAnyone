using System;
using System.IO;
using Newtonsoft.Json;
using static MarryAnyone.Debug;

namespace MarryAnyone.Settings
{
    internal sealed class Config
    {
        public string? SexualOrientation { get; set; }
        public string? TemplateCharacter { get; set; }
        public bool Polygamy { get; set; }
        public bool Polyamory { get; set; }
        public bool PregnancyPlus { get; set; }
        public bool Cheating { get; set; }
        public bool Incest { get; set; }
        public bool SkipCourtship { get; set; }
        public bool RetryCourtship { get; set; }
        public string? PlayerClan { get; set; }
        public string? ClanLeader { get; set; }
        public bool Debug { get; set; }
    }

    internal sealed class MAConfig : ISettingsProvider
    {
        public static MAConfig? Instance { get; private set; }

        private readonly string _filePath = "..\\..\\Modules\\MarryAnyone\\Config.json";

        public MAConfig()
        {
            Instance = this;
            ReadConfig();
            WriteConfig();
        }

        private bool _polyamory = false;

        private bool _polygamy = false;

        private bool _pregnancyPlus = false;

        private bool _incest = false;

        private bool _cheating = false;

        private bool _skipCourtship = false;

        private bool _retryCourtship = false;

        private bool _debug = false;

        private string _playerClan = "Default";

        private string _clanLeader = "Default";

        private string _sexualOrientation = "Heterosexual";

        private string _templateCharacter = "Default";

        public void WriteConfig()
        {
            try
            {
                var config = new Config
                {
                    SexualOrientation = _sexualOrientation,
                    Polygamy = _polygamy,
                    Polyamory = _polyamory,
                    PregnancyPlus = _pregnancyPlus,
                    Incest = _incest,
                    Cheating = _cheating,
                    SkipCourtship = _skipCourtship,
                    RetryCourtship = _retryCourtship,
                    TemplateCharacter = _templateCharacter,
                    PlayerClan = _playerClan,
                    ClanLeader = _clanLeader,
                    Debug = _debug
                };
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        public void ReadConfig()
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                var config = JsonConvert.DeserializeObject<Config>(jsonString);
                // Added error handling!
                if (config.SexualOrientation != "Heterosexual" && config.SexualOrientation != "Homosexual" && config.SexualOrientation != "Bisexual")
                {
                    throw new Exception($"{config.SexualOrientation} is not a valid SexualOrientation. Valid options: \"Heterosexual\", \"Homosexual\", or \"Bisexual\"");
                }
                if (config.TemplateCharacter != "Default" && config.TemplateCharacter != "Wanderer")
                {
                    throw new Exception($"{config.TemplateCharacter} is not a valid TemplateCharacter. Valid options: \"Default\" or \"Wanderer\"");
                }
                if (config.PlayerClan != "Default" && config.PlayerClan != "Always" && config.PlayerClan != "Never")
                {
                    throw new Exception($"{config.PlayerClan} is not a valid PlayerClan. Valid options: \"Default\", \"Always\", or \"Never\"");
                }
                if (config.ClanLeader != "Default" && config.ClanLeader != "Always" && config.ClanLeader != "Never")
                {
                    throw new Exception($"{config.ClanLeader} is not a valid ClanLeader. Valid options: \"Default\", \"Always\", or \"Never\"");
                }
                _sexualOrientation = config.SexualOrientation!;
                _polygamy = config.Polygamy;
                _polyamory = config.Polyamory;
                _pregnancyPlus = config.PregnancyPlus;
                _incest = config.Incest;
                _cheating = config.Cheating;
                _retryCourtship = config.RetryCourtship;
                _skipCourtship = config.SkipCourtship;
                _templateCharacter = config.TemplateCharacter!;
                _playerClan = config.PlayerClan!;
                _clanLeader = config.ClanLeader!;
                _debug = config.Debug;
            }
            catch (Exception e)
            {
                Error(e);
                WriteConfig();
            }
        }

        public string SexualOrientation
        {
            get
            {
                ReadConfig();
                return _sexualOrientation;
            }
            set
            {
                if (_sexualOrientation != value)
                {
                    _sexualOrientation = value;
                    WriteConfig();
                }
            }
        }

        public bool RetryCourtship
        {
            get
            {
                ReadConfig();
                return _retryCourtship;
            }
            set
            {
                if (_retryCourtship != value)
                {
                    _retryCourtship = value;
                    WriteConfig();
                }
            }
        }

        public bool Polygamy
        {
            get
            {
                ReadConfig();
                return _polygamy;
            }
            set
            {
                if (_polygamy != value)
                {
                    _polygamy = value;
                    WriteConfig();
                }
            }
        }

        public bool Polyamory
        {
            get
            {
                ReadConfig();
                return _polyamory;
            }
            set
            {
                if (_polyamory != value)
                {
                    _polyamory = value;
                    WriteConfig();
                }
            }
        }

        public bool PregnancyPlus
        {
            get
            {
                ReadConfig();
                return _pregnancyPlus;
            }
            set
            {
                if (_pregnancyPlus != value)
                {
                    _pregnancyPlus = value;
                    WriteConfig();
                }
            }
        }

        public bool Incest
        {
            get
            {
                ReadConfig();
                return _incest;
            }
            set
            {
                if (_incest != value)
                {
                    _incest = value;
                    WriteConfig();
                }
            }
        }

        public bool Cheating
        {
            get
            {
                ReadConfig();
                return _cheating;
            }
            set
            {
                if (_cheating != value)
                {
                    _cheating = value;
                    WriteConfig();
                }
            }
        }

        public bool SkipCourtship
        {
            get
            {
                ReadConfig();
                return _skipCourtship;
            }
            set
            {
                if (_skipCourtship != value)
                {
                    _skipCourtship = value;
                    WriteConfig();
                }
            }
        }

        public bool Debug
        {
            get
            {
                ReadConfig();
                return _debug;
            }
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    WriteConfig();
                }
            }
        }

        public string TemplateCharacter
        {
            get
            {
                ReadConfig();
                return _templateCharacter;
            }
            set
            {
                if (_templateCharacter != value)
                {
                    _templateCharacter = value;
                    WriteConfig();
                }
            }
        }

        public string PlayerClan
        {
            get
            {
                ReadConfig();
                return _playerClan;
            }
            set
            {
                if (_playerClan != value)
                {
                    _playerClan = value;
                    WriteConfig();
                }
            }
        }

        public string ClanLeader
        {
            get
            {
                ReadConfig();
                return _clanLeader;
            }
            set
            {
                if (_clanLeader != value)
                {
                    _clanLeader = value;
                    WriteConfig();
                }
            }
        }
    }
}