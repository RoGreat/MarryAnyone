namespace MarryAnyone.Settings
{
    internal class MASettings : ICustomSettingsProvider
    {
        private ICustomSettingsProvider _provider;
        public bool IsIncestuous { get => _provider.IsIncestuous; set => _provider.IsIncestuous = value; }
        public bool IsPolygamous { get => _provider.IsPolygamous; set => _provider.IsPolygamous = value; }
        public bool Debug { get => _provider.Debug; set => _provider.Debug = value; }
        public string Difficulty { get => _provider.Difficulty; set => _provider.Difficulty = value; }
        public string SexualOrientation { get => _provider.SexualOrientation; set => _provider.SexualOrientation = value;  }

        public MASettings()
        {
            if (MACustomSettings.Instance is { })
            {
                _provider = MACustomSettings.Instance;
            }
            else
            {
                _provider = new MAHardcodedSettings();
            }
        }
    }

    internal interface ICustomSettingsProvider
    {
        bool IsPolygamous { get; set; }
        bool IsIncestuous { get; set; }
        bool Debug { get; set; }
        string Difficulty { get; set; }
        string SexualOrientation { get; set; }
    }

    internal class MAHardcodedSettings : ICustomSettingsProvider
    {
        public bool IsPolygamous { get; set; } = false;
        public bool IsIncestuous { get; set; } = false;
        public bool Debug { get; set; } = false;
        public string Difficulty { get; set; } = "Easy";
        public string SexualOrientation { get; set; } = "Heterosexual";
    }
}