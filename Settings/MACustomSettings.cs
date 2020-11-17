using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base.PerSave;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace MarryAnyone.Settings
{
    // Instance is null for some reason...
    // Seems to be that setting fields are null on new game creation
    // Have to reload save in order for it to work.
    internal class MACustomSettings : AttributePerSaveSettings<MACustomSettings>, ICustomSettingsProvider
    {
        public override string Id { get; } = "MarryAnyone_v2";
        public override string DisplayName { get; } = new TextObject("{=marryanyone}Marry Anyone {VERSION}", new Dictionary<string, TextObject>
        {
            { "VERSION", new TextObject(typeof(MACustomSettings).Assembly.GetName().Version.ToString(3)) }
        }).ToString();

        [SettingPropertyDropdown("{=difficulty}Difficulty", Order = 0, RequireRestart = false, HintText = "{=difficulty_desc}Very Easy - no mini-game | Easy - mini-game nobles only | Realistic - mini-game all.")]
        [SettingPropertyGroup("{=general}General")]
        public DropdownDefault<string> DifficultyDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Very Easy",
            "Easy",
            "Realistic"
        }, 1);

        [SettingPropertyDropdown("{=orientation}Sexual Orientation", Order = 1, RequireRestart = false, HintText = "{=orientation_desc}Player character can choose what gender the player can marry")]
        [SettingPropertyGroup("{=general}General")]
        public DropdownDefault<string> SexualOrientationDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);

        [SettingPropertyBool("{=polygamy}Polygamy", Order = 2, RequireRestart = false, HintText = "{=polygamy_desc}Player character can have polygamous relationships")]
        [SettingPropertyGroup("{=other}Other")]
        public bool IsPolygamous { get; set; } = false;

        [SettingPropertyBool("{=incest}Incest", Order = 3, RequireRestart = false, HintText = "{=incest_desc}Player character can have incestuous relationships")]
        [SettingPropertyGroup("{=other}Other")]
        public bool IsIncestuous { get; set; } = false;

        [SettingPropertyBool("{=debug}Debug", Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("{=other}Other")]
        public bool Debug { get; set; } = false;

        public string Difficulty { get => DifficultyDropdown.SelectedValue; set => DifficultyDropdown.SelectedValue = value; }
        public string SexualOrientation { get => SexualOrientationDropdown.SelectedValue; set => SexualOrientationDropdown.SelectedValue = value; }
    }
}