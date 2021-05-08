using Newtonsoft.Json;
using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone.Settings
{
    internal class MASettings : ISettingsProvider
    {
        public static bool UsingMCM = false;

        public bool Incest { get => _provider.Incest; set => _provider.Incest = value; }
        public bool Polygamy { get => _provider.Polygamy; set => _provider.Polygamy = value; }
        public bool Cheating { get => _provider.Cheating; set => _provider.Cheating = value; }
        public bool Debug { get => _provider.Debug; set => _provider.Debug = value; }
        public string Difficulty { get => _provider.Difficulty; set => _provider.Difficulty = value; }
        public string SexualOrientation { get => _provider.SexualOrientation; set => _provider.SexualOrientation = value;  }
        public bool Adoption { get => _provider.Adoption; set => _provider.Adoption = value; }
        public float AdoptionChance { get => _provider.AdoptionChance; set => _provider.AdoptionChance = value; }
        public bool AdoptionTitles { get => _provider.AdoptionTitles; set => _provider.AdoptionTitles = value; }
        public bool RetryCourtship { get => _provider.RetryCourtship; set => _provider.RetryCourtship = value; }

        public MASettings()
        {
            if (UsingMCM)
            {
                if (MCMSettings.Instance is not null)
                {
                    _provider = MCMSettings.Instance;
                    return;
                }
            }
            else if (MAConfig.Instance is not null)
            {
                if (File.Exists(_configPath))
                {
                    try
                    {
                        MAConfig config = JsonConvert.DeserializeObject<MAConfig>(File.ReadAllText(_configPath));
                        MAConfig.Instance.Polygamy = config.Polygamy;
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
                    }
                    catch (Exception exception)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Marry Anyone: " + exception.Message, Colors.Red));
                    }
                }
            }
            if (MAConfig.Instance is null)
            {
                MAConfig.Instance = new MAConfig();
            }
            _provider = MAConfig.Instance;
        }

        private readonly ISettingsProvider _provider;

        public static readonly string _configPath = BasePath.Name + "Modules/MarryAnyone/config.json";
    }
}