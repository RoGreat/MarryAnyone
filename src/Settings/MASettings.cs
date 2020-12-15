using Newtonsoft.Json;
using System.IO;
using TaleWorlds.Library;

namespace MarryAnyone.Settings
{
    internal class MASettings : ISettingsProvider
    {
        public static bool UsingMCM = false;

        private static string _configPath = $"{BasePath.Name}Modules/MarryAnyone/config.json";

        private ISettingsProvider _provider;

        public bool Incest { get => _provider.Incest; set => _provider.Incest = value; }
        public bool Polygamy { get => _provider.Polygamy; set => _provider.Polygamy = value; }
        public bool BecomeRuler { get => _provider.BecomeRuler; set => _provider.BecomeRuler = value; }
        public bool Cheating { get => _provider.Cheating; set => _provider.Cheating = value; }
        public bool Debug { get => _provider.Debug; set => _provider.Debug = value; }
        public string Difficulty { get => _provider.Difficulty; set => _provider.Difficulty = value; }
        public string SexualOrientation { get => _provider.SexualOrientation; set => _provider.SexualOrientation = value;  }

        public MASettings()
        {
            if (UsingMCM)
            {
                if (MAMCMSettings.Instance != null)
                {
                    _provider = MAMCMSettings.Instance;
                    return;
                }
            }
            else if (File.Exists(_configPath))
            {
                MAConfig config = JsonConvert.DeserializeObject<MAConfig>(File.ReadAllText(_configPath));
                MAConfig.Instance.Polygamy = config.Polygamy;
                MAConfig.Instance.Incest = config.Incest;
                MAConfig.Instance.BecomeRuler = config.BecomeRuler;
                MAConfig.Instance.Cheating = config.Cheating;
                MAConfig.Instance.Debug = config.Debug;
                MAConfig.Instance.Difficulty = config.Difficulty;
                MAConfig.Instance.SexualOrientation = config.SexualOrientation;
            }
            _provider = MAConfig.Instance;
        }
    }

    internal interface ISettingsProvider
    {
        bool Polygamy { get; set; }
        bool Incest { get; set; }
        bool BecomeRuler { get; set; }
        bool Cheating { get; set; }
        bool Debug { get; set; }
        string Difficulty { get; set; }
        string SexualOrientation { get; set; }
    }
}