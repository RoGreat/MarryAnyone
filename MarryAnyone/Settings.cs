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

        [SettingPropertyBool(displayName: "{=bbbbbbb}commoner", Order = 0, RequireRestart = true, HintText = "{=cccccc}")]
        public bool EnableCommonerRomance { get; set; } = true;

        [SettingPropertyBool(displayName: "{=ddddddd}lord", Order = 10, RequireRestart = true, HintText = "{=eeeeee}")]
        public bool EnableLeaderRomance { get; set; } = true;
    }
}
