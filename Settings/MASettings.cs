using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Data;
using MCM.Abstractions.Settings.Base.PerCharacter;
using System.ComponentModel;

namespace MarryAnyone
{
    // Instance is null for some reason...
    internal sealed class MASettings :   AttributePerCharacterSettings<MASettings> // AttributeGlobalSettings<MASettings>
    {
        public override string Id { get; } = "MarryAnyone_v11";
        public override string DisplayName { get; } = $"Marry Anyone {typeof(MASettings).Assembly.GetName().Version.ToString(2)}";
        public override string FolderName { get; } = "MarryAnyone";
        public override string Format => "json";

        [SettingPropertyBool("Polygamy", RequireRestart = false, HintText = "Player character can have polygamous relationships", Order = 2)]
        [SettingPropertyGroup("Misc")]
        public bool IsPolygamous { get; set; } = false;

        [SettingPropertyBool("Incest", RequireRestart = false, HintText = "Player character can have incestual relationships", Order = 3)]
        [SettingPropertyGroup("Misc")]
        public bool IsIncestual { get; set; } = false;

        [SettingPropertyBool("Debug", RequireRestart = false, Order = 4)]
        [SettingPropertyGroup("Misc")]
        public bool Debug { get; set; } = false;

        [SettingPropertyDropdown("Difficulty", RequireRestart = false, HintText = "Very Easy - no mini-game | Easy - mini-game nobles only | Realistic - mini-game all.", Order = 0)]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> Difficulty { get; set; } = new DefaultDropdown<string>(new string[]
        {
            "Very Easy",
            "Easy",
            "Realistic"
        }, 1);

        [SettingPropertyDropdown("Sexual Orientation", RequireRestart = false, HintText = "Player character can choose what gender the player can marry", Order = 1)]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> SexualOrientation { get; set; } = new DefaultDropdown<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);
    }
}