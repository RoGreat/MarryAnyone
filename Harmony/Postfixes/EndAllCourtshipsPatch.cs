using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static void Postfix()
        {
            if (MASettings.Instance.IsPolygamous)
            {
                foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
                {
                    if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Untested;
                    }
                }
            }
        }
    }
}