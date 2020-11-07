using HarmonyLib;
using MarryAnyone.Settings;
using MCM.Abstractions.Settings.Base.PerSave;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal class EndAllCourtshipsPatch
    {
        private static void Postfix()
        {
            EndAllCourtships();
        }

        public static void EndAllCourtships()
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