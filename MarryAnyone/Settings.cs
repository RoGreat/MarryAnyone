using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

using TaleWorlds.Localization;

namespace MarryAnyone
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "MarryAnyoneSettings_v0.1";
        public override string DisplayName => new TextObject("{=aaaaaa}Marry Anyone").ToString();
        public override string FolderName => "MarryAnyone";
        public override string FormatType => "json2";

        [SettingPropertyBool(displayName: "{=bbbbbbb}not lord", Order = 0, RequireRestart = false, HintText = "{=cccccc}")]
        public bool EnableNotLordRomance { get; set; } = true;

        [SettingPropertyBool(displayName: "{=ddddddd}leader", Order = 10, RequireRestart = false, HintText = "{=eeeeee}")]
        public bool EnableLeaderRomance { get; set; } = true;
    }
}
