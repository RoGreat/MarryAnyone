namespace MarryAnyone.Settings
{
    public enum PregnancyMode
    {
        Default,
        Disabled,
        Player,
        Partner,
        Random
    }

    internal interface ISettingsProvider
    {
        bool Polygamy { get; set; }
        bool Polyamory { get; set; }
        bool Incest { get; set; }
        bool Cheating { get; set; }
        bool Debug { get; set; }
        string Difficulty { get; set; }
        string SexualOrientation { get; set; }
        bool Adoption { get; set; }
        float AdoptionChance { get; set; }
        bool AdoptionTitles { get; set; }
        bool RetryCourtship { get; set; }
        string PregnancyMode { get; set; }
        float FertilityBonus { get; set; }
    }
}