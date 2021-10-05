using Bannerlord.BUTR.Shared.Helpers;
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
    internal class MCMSettings : AttributePerSaveSettings<MCMSettings>, ISettingsProvider
    {
        public override string Id { get; } = "MarryAnyone_v2";

        public override string DisplayName => TextObjectHelper.Create("{=marryanyone}Marry Anyone {VERSION}", new Dictionary<string, TextObject?>
        {
            { "VERSION", TextObjectHelper.Create(typeof(MCMSettings).Assembly.GetName().Version?.ToString(3) ?? "ERROR") }
        })?.ToString() ?? "ERROR";

        [SettingPropertyDropdown("{=difficulty}Difficulty", Order = 0, RequireRestart = false, HintText = "{=difficulty_desc}Very Easy - skip the romance mini-game. Easy - skip the romance mini-game for lowborn. Realistic - always play the romance mini-game.")]
        [SettingPropertyGroup("{=general}General")]
        public DropdownDefault<string> DifficultyDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Very Easy",
            "Easy",
            "Realistic"
        }, 1);

        [SettingPropertyDropdown("{=orientation}Sexual Orientation", Order = 1, RequireRestart = false, HintText = "{=orientation_desc}Player character can choose what gender the player can marry.")]
        [SettingPropertyGroup("{=general}General")]
        public DropdownDefault<string> SexualOrientationDropdown { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);

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

        [SettingPropertyFloatingInteger("{=min_marriage_age} Min Marriage Age", 0f, 100f, "0.00", RequireRestart = false, Order = 1, HintText = "{=min_marriage_age_desc} Minimum age for marriage to be allowed.")]
        [SettingPropertyGroup("{=relationship}Relationship Options", GroupOrder = 2)]
        public float MinMarriageAge { get; set; } = 16.00f;

        [SettingPropertyBool("{=debug}Debug", RequireRestart = false, HintText = "{=debug_desc}Displays mod developer debug information in the game's message log.")]
        public bool Debug { get; set; } = false;

        public string Difficulty { get => DifficultyDropdown.SelectedValue; set => DifficultyDropdown.SelectedValue = value; }
        public string SexualOrientation { get => SexualOrientationDropdown.SelectedValue; set => SexualOrientationDropdown.SelectedValue = value; }

        [SettingPropertyBool("{=adoption}Adoption", RequireRestart = false, HintText = "{=adoption_desc}Player can adopt children in towns and villages by talking to them.", IsToggle = true)]
        [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 5)]
        public bool Adoption { get; set; } = false;

        [SettingPropertyFloatingInteger("{=adoption_chance}Adoption Chance", 0f, 1f, "#0%", RequireRestart = false, Order = 1, HintText = "{=adoption_chance_desc}Chance that a child is up for adoption.")]
        [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 5)]
        public float AdoptionChance { get; set; } = 0.05f;

        [SettingPropertyBool("{=adoption_titles}Adoption Titles", RequireRestart = false, Order = 2, HintText = "{=adoption_titles_desc}Encyclopedia displays children without a parent as adopted.")]
        [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 5)]
        public bool AdoptionTitles { get; set; } = false;

        [SettingPropertyBool("{=retry_courtship}Retry Courtship", RequireRestart = false, HintText = "{=retry_courtship_desc}Player can retry courtship after failure.")]
        [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 3)]
        public bool RetryCourtship { get; set; } = false;


        // Credit to Lazeras
        [SettingPropertyDropdown("{=pregnancies}Pregnancies", RequireRestart = false, Order = 0, 
            HintText = "{=pregnancies_desc}Controls or disables which partner can become pregnant.")]
        [SettingPropertyGroup("{=pregnancy}Pregnancy", GroupOrder = 4)]
        public DropdownDefault<string> PregnancyModeSetting { get; set; } = new DropdownDefault<string>(new string[]
        {
            "Default",
            "Disabled",
            "Player",
            "Partner",
            "Random"
        }, 0);

        public string PregnancyMode { get => PregnancyModeSetting.SelectedValue; set => PregnancyModeSetting.SelectedValue = value; }

        [SettingPropertyFloatingInteger("{=fertility_bonus}Fertility Bonus", 1f, 10f, "0%", Order = 1, RequireRestart = false,
            HintText = "{=fertility_bonus_desc}Adds modifier to chance of pregnancy. 100% = No Bonus, 200% = 2x chance. Note: May not do much after ~6-8 kids due to the base pregnancy calculations."),
            SettingPropertyGroup("{=pregnancy}Pregnancy", GroupOrder = 4)]
        public float FertilityBonus { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=min_pregnancy_age} Min Pregnancy Age", 0f, 100f, "0.00", RequireRestart = false, Order = 1, HintText = "{=min_pregnancy_age_desc} Minimum age for pregnancy to be allowed.")]
        [SettingPropertyGroup("{=pregnancy}Pregnancy", GroupOrder = 4)]
        public float MinPregnancyAge { get; set; } = 16.00f;

    }
}