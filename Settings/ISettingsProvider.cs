namespace MarryAnyone.Settings
{
    // Token: 0x02000007 RID: 7
    public enum PregnancyMode
    {
        // Token: 0x04000006 RID: 6
        Disabled,
        // Token: 0x04000007 RID: 7
        Player,
        // Token: 0x04000008 RID: 8
        Partner,
        // Token: 0x04000009 RID: 9
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
        string pregnancyMode { get; set; }
    }
}