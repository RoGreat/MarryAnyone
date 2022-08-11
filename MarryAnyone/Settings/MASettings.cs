using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base.PerSave;

namespace MarryAnyone.Settings
{
    // Instance is null for some reason...
    // Seems to be that setting fields are null on new game creation
    // Have to reload save in order for it to work.
    internal class MASettings : AttributePerSaveSettings<MASettings>, IMASettingsProvider
    {
        public override string Id { get; } = "MarryAnyone_v2";

        //public override string DisplayName => TextObjectHelper.Create("{=marryanyone}Marry Anyone {VERSION}", new Dictionary<string, TextObject?>
        //{
        //    { "VERSION", TextObjectHelper.Create(typeof(MASettings).Assembly.GetName().Version?.ToString(3) ?? "ERROR") }
        //})?.ToString() ?? "ERROR";

        public override string DisplayName => "{=marryanyone}Marry Anyone";

        public override string FolderName { get; } = "MarryAnyone";


        [SettingPropertyDropdown("{=orientation}Sexual Orientation", Order = 1, RequireRestart = false, HintText = "{=orientation_desc}Player character can choose what gender the player can marry.")]
        [SettingPropertyGroup("{=general}General")]
        public DropdownDefault<string> SexualOrientationDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);
        public string SexualOrientation { get => SexualOrientationDropdown.SelectedValue; set => SexualOrientationDropdown.SelectedValue = value; }

        [SettingPropertyBool("{=cheating}Cheating", Order = 2, RequireRestart = false, HintText = "{=cheating_desc}Player character can marry characters that are already married.")]
        [SettingPropertyGroup("{=relationship}Relationship Options", GroupOrder = 2)]
        public bool Cheating { get; set; } = false;

        [SettingPropertyBool("{=polygamy}Polygamy", Order = 0, RequireRestart = false, HintText = "{=polygamy_desc}Player character can have polygamous relationships.")]
        [SettingPropertyGroup("{=relationship}Relationship Options", GroupOrder = 2)]
        public bool Polygamy { get; set; } = false;

        [SettingPropertyBool("{=polyamory}Polyamory", Order = 1, RequireRestart = false, HintText = "{=polyamory_desc}Player character's spouses can have relationships with each other.")]
        [SettingPropertyGroup("{=relationship}Relationship Options", GroupOrder = 2)]
        public bool Polyamory { get; set; } = false;

        [SettingPropertyBool("{=incest}Incest", Order = 3, RequireRestart = false, HintText = "{=incest_desc}Player character can have incestuous relationships.")]
        [SettingPropertyGroup("{=relationship}Relationship Options", GroupOrder = 2)]
        public bool Incest { get; set; } = false;

        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log.")]
        public bool Debug { get; set; } = false;

        [SettingPropertyBool("{=retry_courtship}Retry Courtship", RequireRestart = false, HintText = "{=retry_courtship_desc}Player can retry courtship after failure.")]
        [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 3)]
        public bool RetryCourtship { get; set; } = false;
    }
}