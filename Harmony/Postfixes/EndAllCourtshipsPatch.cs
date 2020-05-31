using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static void Postfix()
        {
            MAConfig config = MASubModule.Config;
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
            {
                if (config.IsPolygamous && (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero))
                {
                    romanticState.Level = Romance.RomanceLevelEnum.Untested;
                }
            }
        }
    }
}