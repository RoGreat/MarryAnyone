using System;
using System.IO;
using MarryAnyone.Helpers;
using Newtonsoft.Json;

namespace MarryAnyone.Settings
{
    internal sealed class Config
    {
        public string? SexualOrientation { get; set; }
        public bool Polygamy { get; set; }
        public bool Polyamory { get; set; }
        public bool Incest { get; set; }
        public bool Cheating { get; set; }
        public bool SkipCourtship { get; set; }
        public bool RetryCourtship { get; set; }
        public bool Debug { get; set; }
        public string? TemplateCharacter { get; set; }
    }

    internal sealed class CustomConfig : ISettingsProvider
    {
        public static CustomConfig? Instance;

        private readonly string _filePath = "..\\..\\Modules\\MarryAnyone\\Config.json";

        public CustomConfig()
        {
            Instance = this;
            ReadConfig();
            WriteConfig();
        }

        private bool _polyamory = false;

        private bool _polygamy = false;

        private bool _incest = false;

        private bool _cheating = false;

        private bool _skipCourtship = false;

        private bool _retryCourtship = false;

        private bool _debug = false;

        private string _sexualOrientation = "Heterosexual";

        private string _templateCharacter = "Default";

        private void WriteConfig()
        {
            try
            {
                var config = new Config
                {
                    SexualOrientation = _sexualOrientation,
                    Polygamy = _polygamy,
                    Polyamory = _polyamory,
                    Incest = _incest,
                    Cheating = _cheating,
                    SkipCourtship = _skipCourtship,
                    RetryCourtship = _retryCourtship,
                    Debug = _debug,
                    TemplateCharacter = _templateCharacter
                };
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception e)
            {
                MADebug.Error(e);
            }
        }

        private void ReadConfig()
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                var config = JsonConvert.DeserializeObject<Config>(jsonString);
                _sexualOrientation = config!.SexualOrientation!;
                _polygamy = config!.Polygamy;
                _polyamory = config!.Polyamory;
                _incest = config!.Incest;
                _cheating = config!.Cheating;
                _retryCourtship = config!.RetryCourtship;
                _skipCourtship = config!.SkipCourtship;
                _debug = config!.Debug;
                _templateCharacter = config!.TemplateCharacter!;
            }
            catch (Exception e)
            {
                MADebug.Error(e);
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
    }
}