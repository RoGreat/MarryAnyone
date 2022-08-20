namespace MarryAnyone.Settings
{
    internal interface ISettingsProvider
    {
        string SexualOrientation { get; set; }
        bool Polygamy { get; set; }
        bool PregnancyPlus { get; set; }
        bool Polyamory { get; set; }
        bool Incest { get; set; }
        bool Cheating { get; set; }
        bool SkipCourtship { get; set; }
        bool RetryCourtship { get; set; }
        bool Debug { get; set; }
        string TemplateCharacter { get; set; }
    }
}