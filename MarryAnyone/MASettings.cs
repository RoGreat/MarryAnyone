using MarryAnyone.Settings;

namespace MarryAnyone
{
    internal sealed class MASettings
    {
        private readonly ISettingsProvider _provider;

        public MASettings()
        {
            if (MCMSettings.Instance is not null)
            {
                _provider = MCMSettings.Instance;
                return;
            }
            else if (MASettingsConfig.Instance is null)
            {
                new MASettingsConfig();
            }
            _provider = MASettingsConfig.Instance!;
            MAConfig.Initialize();
        }

        public string SexualOrientation
        {
            get => _provider.SexualOrientation;
            set => _provider.SexualOrientation = value;
        }

        public string TemplateCharacter
        {
            get => _provider.TemplateCharacter;
            set => _provider.TemplateCharacter = value;
        }

        public bool Polygamy
        {
            get => _provider.Polygamy;
            set => _provider.Polygamy = value;
        }

        public bool PregnancyPlus
        {
            get => _provider.PregnancyPlus;
            set => _provider.PregnancyPlus = value;
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

        public string FactionLeader
        {
            get => _provider.FactionLeader;
            set => _provider.FactionLeader = value;
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