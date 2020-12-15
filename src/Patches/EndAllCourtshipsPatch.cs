using HarmonyLib;
using MarryAnyone.Settings;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static bool Prefix(Hero forHero)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.Polygamy)
            {
                foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
                {
                    if (forHero == Hero.MainHero && romanticState.Level == Romance.RomanceLevelEnum.Marriage)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Ended;
                    }
                }
                return false;
            }
            return true;
        }
    }
}