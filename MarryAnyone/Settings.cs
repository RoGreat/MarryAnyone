namespace MarryAnyone
{
    internal sealed class Settings
    {
        private readonly ISettingsProvider _provider;

        public Settings()
        {
            if (MCMSettings.Instance is not null)
            {
                _provider = MCMSettings.Instance;
            }
            else if (CustomConfig.Instance is not null)
            {
                _provider = CustomConfig.Instance!;
            }
            else
            {
                new CustomConfig();
                _provider = CustomConfig.Instance!;
            }
        }

        public string SexualOrientation
        {
            get => _provider.SexualOrientation;
            set => _provider.SexualOrientation = value;
        }

        public bool Polygamy
        {
            get => _provider.Polygamy;
            set => _provider.Polygamy = value;
        }

        public bool Polyamory
        {
            get => _provider.Polyamory;
            set => _provider.Polyamory = value;
        }

        public bool Incest
        {
            get => _provider.Incest;
            set => _provider.Incest = value;
        }

        public bool Cheating
        {
            get => _provider.Cheating;
            set => _provider.Cheating = value;
        }

        public bool SkipCourtship
        {
            get => _provider.SkipCourtship;
            set => _provider.SkipCourtship = value;
        }

        public bool RetryCourtship
        {
            get => _provider.RetryCourtship;
            set => _provider.RetryCourtship = value;
        }

        public bool Debug
        {
            get => _provider.Debug;
            set => _provider.Debug = value;
        }
    }
}