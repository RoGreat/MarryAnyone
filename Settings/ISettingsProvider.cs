namespace MarryAnyone.Settings
{
    internal interface ISettingsProvider
    {
        bool Polygamy { get; set; }
        bool Incest { get; set; }
        bool Cheating { get; set; }
        bool Debug { get; set; }
        string Difficulty { get; set; }
        string SexualOrientation { get; set; }
        bool Adoption { get; set; }
        float AdoptionChance { get; set; }
        bool RetryCourtship { get; set; }
    }
}