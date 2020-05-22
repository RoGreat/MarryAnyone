using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.Core;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    class EndAllCourtshipsPatch
    {
        static bool Prefix(Hero forHero)
        {
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
            {
                if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                {
                    // InformationManager.DisplayMessage(new InformationMessage("UNTESTED"));
                    romanticState.Level = Romance.RomanceLevelEnum.Untested;
                }
                else if (romanticState.Person1 == forHero || romanticState.Person2 == forHero)
                {
                    // InformationManager.DisplayMessage(new InformationMessage("ENDED"));
                    romanticState.Level = Romance.RomanceLevelEnum.Ended;
                }
            }
            return false;
        }
    }
}