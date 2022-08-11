namespace MarryAnyone.Settings
{
    internal interface IMASettingsProvider
    {
        string SexualOrientation { get; set; }
        bool Polygamy { get; set; }
        bool Polyamory { get; set; }
        bool Incest { get; set; }
        bool Cheating { get; set; }
        bool RetryCourtship { get; set; }
        bool Debug { get; set; }
    }
}