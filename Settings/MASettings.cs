namespace MarryAnyone.Settings
{
    internal class MASettings : ISettingsProvider
    {
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
            if (MAMCMSettings.Instance is { })
            {
                _provider = MAMCMSettings.Instance;
            }
            else
            {
                _provider = new MADefaultSettings();
            }
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

    internal class MADefaultSettings : ISettingsProvider
    {
        public bool Polygamy { get; set; } = false;
        public bool Incest { get; set; } = false;
        public bool BecomeRuler { get; set; } = false;
        public bool Cheating { get; set; } = false;
        public bool Debug { get; set; } = false;
        public string Difficulty { get; set; } = "Easy";
        public string SexualOrientation { get; set; } = "Heterosexual";
    }
}