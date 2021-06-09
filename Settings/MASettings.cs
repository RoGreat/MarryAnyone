using Newtonsoft.Json;
using System;
using System.IO;
using TaleWorlds.Library;

namespace MarryAnyone.Settings
{
    internal class MASettings : ISettingsProvider
    {
        public static bool UsingMCM;

        public static bool NoMCMWarning;

        public static bool NoConfigWarning;

        public static readonly string ConfigPath = BasePath.Name + "Modules/MarryAnyone/config.json";

        public bool Incest { get => _provider.Incest; set => _provider.Incest = value; }
        public bool Polygamy { get => _provider.Polygamy; set => _provider.Polygamy = value; }
        public bool Polyamory { get => _provider.Polyamory; set => _provider.Polyamory = value; }
        public bool Cheating { get => _provider.Cheating; set => _provider.Cheating = value; }
        public bool Debug { get => _provider.Debug; set => _provider.Debug = value; }
        public string Difficulty { get => _provider.Difficulty; set => _provider.Difficulty = value; }
        public string SexualOrientation { get => _provider.SexualOrientation; set => _provider.SexualOrientation = value;  }
        public bool Adoption { get => _provider.Adoption; set => _provider.Adoption = value; }
        public float AdoptionChance { get => _provider.AdoptionChance; set => _provider.AdoptionChance = value; }
        public bool AdoptionTitles { get => _provider.AdoptionTitles; set => _provider.AdoptionTitles = value; }
        public bool RetryCourtship { get => _provider.RetryCourtship; set => _provider.RetryCourtship = value; }
        public string pregnancyMode { get => _provider.pregnancyMode; set => _provider.pregnancyMode = value; }

        public MASettings()
        {
            if (MCMSettings.Instance is { } settings)
            {
                _provider = settings;
                NoMCMWarning = NoConfigWarning = false;
                UsingMCM = true;
                return;
            }
            UsingMCM = false;
            MAConfig.Instance = new MAConfig();
            if (File.Exists(ConfigPath))
            {
                try
                {
                    MAConfig config = JsonConvert.DeserializeObject<MAConfig>(File.ReadAllText(ConfigPath));
                    MAConfig.Instance.Polygamy = config.Polygamy;
                    MAConfig.Instance.Polyamory = config.Polyamory;
                    MAConfig.Instance.Incest = config.Incest;
                    MAConfig.Instance.Cheating = config.Cheating;
                    MAConfig.Instance.Debug = config.Debug;
                    MAConfig.Instance.Warning = config.Warning;
                    MAConfig.Instance.Difficulty = config.Difficulty;
                    MAConfig.Instance.SexualOrientation = config.SexualOrientation;
                    MAConfig.Instance.Adoption = config.Adoption;
                    MAConfig.Instance.AdoptionChance = config.AdoptionChance;
                    MAConfig.Instance.AdoptionTitles = config.AdoptionTitles;
                    MAConfig.Instance.RetryCourtship = config.RetryCourtship;
                    NoMCMWarning = true;
                    NoConfigWarning = false;
                }
                catch (Exception exception)
                {
                    MAHelper.Error(exception);
                }
            }
            else
            {
                NoConfigWarning = true;
                NoMCMWarning = false;
            }
            _provider = MAConfig.Instance;
        }

        private readonly ISettingsProvider _provider;
    }
}