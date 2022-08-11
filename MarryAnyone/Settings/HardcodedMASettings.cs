namespace MarryAnyone.Settings
{
    internal class HardcodedMASettings : IMASettingsProvider
    {
        public string SexualOrientation { get; set; } = "Heterosexual";
        public bool Polygamy { get; set; } = false;
        public bool Polyamory { get; set; } = false;
        public bool Incest { get; set; } = false;
        public bool Cheating { get; set; } = false;
        public bool Warning { get; set; } = true;
        public bool RetryCourtship { get; set; } = false;
        public bool Debug { get; set; } = false;
    }
}