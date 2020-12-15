namespace MarryAnyone.Settings
{
    internal class MAConfig : ISettingsProvider
    {
        public static MAConfig Instance = null;
        public bool Polygamy { get; set; } = false;
        public bool Incest { get; set; } = false;
        public bool BecomeRuler { get; set; } = false;
        public bool Cheating { get; set; } = false;
        public bool Debug { get; set; } = false;
        public string Difficulty { get; set; } = "Easy";
        public string SexualOrientation { get; set; } = "Heterosexual";
    }
}