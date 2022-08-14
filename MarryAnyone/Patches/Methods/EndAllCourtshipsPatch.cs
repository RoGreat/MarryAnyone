using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches.Methods
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static bool Prefix(Hero forHero)
        {
            Settings settings = new();

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