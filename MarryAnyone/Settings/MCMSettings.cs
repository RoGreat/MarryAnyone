using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base.PerSave;
using System;

namespace MarryAnyone.Settings
{
    internal sealed class MCMSettings : AttributePerSaveSettings<MCMSettings>, ISettingsProvider
    {
        public override string Id => "MASettings";

        public override string DisplayName => "Marry Anyone" + $" {typeof(MCMSettings).Assembly.GetName().Version.ToString(3)}";

        public override string FolderName => "MarryAnyone";


        [SettingPropertyDropdown("{=orientation}Sexual Orientation", Order = 0, RequireRestart = false, HintText = "{=orientation_desc}Player character can choose what sex the player can marry.")]
        [SettingPropertyGroup("{=relationship}Relationship", GroupOrder = 0)]
        public DropdownDefault<string> SexualOrientationDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);

        public string SexualOrientation 
        {
            get => SexualOrientationDropdown.SelectedValue;
            set => SexualOrientationDropdown.SelectedValue = value;
        }

        [SettingPropertyBool("{=cheating}Cheating", Order = 1, RequireRestart = false, HintText = "{=cheating_desc}Player character can marry characters that are already married.")]
        [SettingPropertyGroup("{=relationship}Relationship", GroupOrder = 0)]
        public bool Cheating { get; set; } = false;

        [SettingPropertyBool("{=incest}Incest", Order = 2, RequireRestart = false, HintText = "{=incest_desc}Player character can marry closely related family members.")]
        [SettingPropertyGroup("{=relationship}Relationship", GroupOrder = 0)]
        public bool Incest { get; set; } = false;


        [SettingPropertyBool("{=polygamy}Polygamy", Order = 0, RequireRestart = false, HintText = "{=polygamy_desc}Player character can have multiple marriages at once.")]
        [SettingPropertyGroup("{=spouses}Spouses", GroupOrder = 1)]
        public bool Polygamy { get; set; } = false;

        [SettingPropertyBool("{=pregnancyplus}Pregnancy+", Order = 1, RequireRestart = false, HintText = "{=pregnancyplus_desc}Changes pregnancy behavior to allow pregnancy with multiple spouses. Keep disabled if you are using another pregnancy mod.")]
        [SettingPropertyGroup("{=spouses}Spouses", GroupOrder = 1)]
        public bool PregnancyPlus { get; set; } = false;

        [SettingPropertyBool("{=polyamory}Polyamory", Order = 2, RequireRestart = false, HintText = "{=polyamory_desc}Player character's spouses can get each other pregnant.")]
        [SettingPropertyGroup("{=spouses}Spouses", GroupOrder = 1)]
        public bool Polyamory { get; set; } = false;


        [SettingPropertyBool("{=skip_courtship}Skip Courtship", Order = 0, RequireRestart = false, HintText = "{=skip_courtship_desc}Player can skip courtship and marry immediately.")]
        [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 2)]
        public bool SkipCourtship { get; set; } = false;

        [SettingPropertyBool("{=retry_courtship}Retry Courtship", Order = 1, RequireRestart = false, HintText = "{=retry_courtship_desc}Player can retry courtship after failure.")]
        [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 2)]
        public bool RetryCourtship { get; set; } = false;

        [SettingPropertyButton("{=reset_courtships}Reset Courtships", Content = "Press Me", Order = 2, RequireRestart = false, HintText = "{=reset_courtships_desc}Reset ended courtships with a press of a button!")]
        [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 2)]
        public Action ResetEndedCourtships { get; set; } = () => Helpers.ResetEndedCourtships();


        [SettingPropertyDropdown("{=playerclan}Player Clan", Order = 0, RequireRestart = false, HintText = "{=playerclan_desc}Player clan persists after marriage. By default the clan only persists if playing as a male.")]
        [SettingPropertyGroup("{=marriage}Marriage", GroupOrder = 3)]
        public DropdownDefault<string> PlayerClanDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Default",
            "Always",
            "Never"
        }, 0);

        public string PlayerClan
        {
            get => PlayerClanDropdown.SelectedValue;
            set => PlayerClanDropdown.SelectedValue = value;
        }

        [SettingPropertyDropdown("{=clanleader}Clan Leader", Order = 1, RequireRestart = false, HintText = "{=clanleader_desc}Player replaces clan leader after marriage. By default the a male player can replace a female leader.")]
        [SettingPropertyGroup("{=marriage}Marriage", GroupOrder = 3)]
        public DropdownDefault<string> ClanLeaderDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Default",
            "Always",
            "Never"
        }, 0);

        public string ClanLeader
        {
            get => ClanLeaderDropdown.SelectedValue;
            set => ClanLeaderDropdown.SelectedValue = value;
        }

        [SettingPropertyDropdown("{=templatechar}Template Character", RequireRestart = false, HintText = "{=templatechar_desc}Set the template character that is used to set the hero name, skills, and equipment for commoners.")]
        [SettingPropertyGroup("{=commoners}Commoners", GroupOrder = 4)]
        public DropdownDefault<string> TemplateCharacterDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Default",
            "Wanderer"
        }, 0);

        public string TemplateCharacter
        {
            get => TemplateCharacterDropdown.SelectedValue;
            set => TemplateCharacterDropdown.SelectedValue = value;
        }


        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log.")]
        public bool Debug { get; set; } = false;
    }
}