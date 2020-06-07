using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Data;
using MCM.Abstractions.Settings.Base.Global;
using MCM.Abstractions.Settings.Base.PerCharacter;

namespace MarryAnyone
{
    public class MASettings : AttributeGlobalSettings<MASettings> // AttributePerCharacterSettings<MASettings>
    {
        public override string Id { get; } = "MarryAnyone_v11";
        public override string DisplayName { get; } = $"Marry Anyone {typeof(MASettings).Assembly.GetName().Version.ToString(2)}";
        public override string FolderName { get; } = "MarryAnyone";

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

        public bool IsVeryEasy()
        {
            if (Difficulty.SelectedValue == "Very Easy")
            {
                return true;
            }
            return false;
        }

        public bool IsEasy()
        {
            if (Difficulty.SelectedValue == "Easy")
            {
                return true;
            }
            return false;
        }

        public bool IsRealistic()
        {
            if (Difficulty.SelectedValue == "Realistic")
            {
                return true;
            }
            return false;
        }

        [SettingPropertyDropdown("Sexual Orientation", RequireRestart = false, HintText = "Player character can choose what gender the player can marry", Order = 1)]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> SexualOrientation { get; set; } = new DefaultDropdown<string>(new string[]
        {
            "Heterosexual",
            "Homosexual",
            "Bisexual"
        }, 0);

        public bool IsHeterosexual()
        {
            if (SexualOrientation.SelectedValue == "Heterosexual")
            {
                return true;
            }
            return false;
        }

        public bool IsHomosexual()
        {
            if (SexualOrientation.SelectedValue == "Homosexual")
            {
                return true;
            }
            return false;
        }

        public bool IsBisexual()
        {
            if (SexualOrientation.SelectedValue == "Bisexual")
            {
                return true;
            }
            return false;
        }
    }
}