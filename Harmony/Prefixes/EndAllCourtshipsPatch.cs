using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static bool Prefix(Hero forHero)
        {
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
            {
                if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                {
                    romanticState.Level = Romance.RomanceLevelEnum.Untested;
                }
                else if (romanticState.Person1 == forHero || romanticState.Person2 == forHero)
                {
                    romanticState.Level = Romance.RomanceLevelEnum.Ended;
                }
            }
            return false;
        }
    }
}