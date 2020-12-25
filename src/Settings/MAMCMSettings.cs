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
    internal class MAMCMSettings : AttributePerSaveSettings<MAMCMSettings>, ISettingsProvider
    {
        public override string Id { get; } = "MarryAnyone_v2";
        public override string DisplayName { get; } = new TextObject("{=marryanyone}Marry Anyone {VERSION}", new Dictionary<string, TextObject>
        {
            { "VERSION", new TextObject(typeof(MAMCMSettings).Assembly.GetName().Version.ToString(3)) }
        }).ToString();

        [SettingPropertyDropdown("{=difficulty}Difficulty", Order = 0, RequireRestart = false, HintText = "{=difficulty_desc}Very Easy - no mini-game | Easy - mini-game nobles only | Realistic - mini-game all")]
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

        [SettingPropertyBool("{=becomeruler}Become Ruler", RequireRestart = false, HintText = "{=becomeruler_desc}Player character can become the kingdom ruler after marrying a kingdom ruler")]
        [SettingPropertyGroup("{=kingdom}Kingdom Ruler Marriage")]
        public bool BecomeRuler { get; set; } = false;

        [SettingPropertyBool("{=cheating}Cheating", RequireRestart = false, HintText = "{=cheating_desc}Player character can marry characters that are already married")]
        [SettingPropertyGroup("{=relationship}Relationship Options")]
        public bool Cheating { get; set; } = false;

        [SettingPropertyBool("{=polygamy}Polygamy", RequireRestart = false, HintText = "{=polygamy_desc}Player character can have polygamous relationships")]
        [SettingPropertyGroup("{=relationship}Relationship Options")]
        public bool Polygamy { get; set; } = false;

        [SettingPropertyBool("{=incest}Incest", RequireRestart = false, HintText = "{=incest_desc}Player character can have incestuous relationships")]
        [SettingPropertyGroup("{=relationship}Relationship Options")]
        public bool Incest { get; set; } = false;

        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log")]
        public bool Debug { get; set; } = false;

        public string Difficulty { get => DifficultyDropdown.SelectedValue; set => DifficultyDropdown.SelectedValue = value; }
        public string SexualOrientation { get => SexualOrientationDropdown.SelectedValue; set => SexualOrientationDropdown.SelectedValue = value; }

        [SettingPropertyBool("{=}Adoption", RequireRestart = false, HintText = "{=}Player can adopt children", IsToggle = true)]
        [SettingPropertyGroup("{=}Adoption", GroupOrder = 1)]
        public bool Adoption { get; set; } = false;

        [SettingPropertyFloatingInteger("{=}Adoption Chance", 0f, 1f, "#0%", RequireRestart = false, HintText = "{=}Chance of child allowed for adoption")]
        [SettingPropertyGroup("{=}Adoption", GroupOrder = 1)]
        public float AdoptionChance { get; set; } = 0.05f;
    }
}