namespace MarryAnyone.Settings
{
    internal interface ISettingsProvider
    {
        bool Polygamy { get; set; }
        bool Polyamory { get; set; }
        bool Incest { get; set; }
        bool Cheating { get; set; }
        bool Notable { get; set; }
        bool ImproveRelation { get; set; }
        string Difficulty { get; set; }
        string SexualOrientation { get; set; }
        bool Adoption { get; set; }
        float AdoptionChance { get; set; }
        bool AdoptionTitles { get; set; }
        bool RetryCourtship { get; set; }
        bool SpouseJoinArena { get; set; }
        int RelationLevelMinForRomance { get; set; }
        int RelationLevelMinForCheating { get; set; }
        int RelationLevelMinForSex { get; set; }
        bool Debug { get; set; }
        bool NotifyRelationImprovementWithinFamily { get; set; }
    }
}