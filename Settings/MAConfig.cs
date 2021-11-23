namespace MarryAnyone.Settings
{
    internal class MAConfig : ISettingsProvider
    {
        public static MAConfig? Instance;
        public bool Polygamy { get; set; } = false;
        public bool Polyamory { get; set; } = false;
        public bool Incest { get; set; } = false;
        public bool Cheating { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Warning { get; set; } = true;
        public string Difficulty { get; set; } = "Easy";
        public string SexualOrientation { get; set; } = "Heterosexual";
        public bool Adoption { get; set; } = false;
        public float AdoptionChance { get; set; } = 0.05f;
        public bool AdoptionTitles { get; set; } = false;
        public bool RetryCourtship { get; set; } = false;
        public bool SpouseJoinArena { get; set; } = true;
        public int RelationLevelMinForRomance { get; set; } = 5;
        public int RelationLevelMinForCheating { get; set; } = 10;
        public int RelationLevelMinForSex { get; set; } = 10;
        public bool ImproveRelation { get; set; } = false;
        public bool NotifyRelationImprovementWithinFamily { get; set; } = false;
        public bool Notable { get; set; } = false;
    }
}