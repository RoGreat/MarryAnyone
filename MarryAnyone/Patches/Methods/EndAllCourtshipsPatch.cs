using HarmonyLib;
using MarryAnyone.Settings;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches.Methods
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static bool Prefix(Hero forHero)
        {
            IMASettingsProvider settings = new MASettings();

            if (settings.Polygamy)
            {
                foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
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