using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    // If polygamous the player does not end courtships with other heroes
    [HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    internal sealed class EndAllCourtshipsPatch
    {
        private static bool Prefix(Hero forHero)
        {
            MASettings settings = new();

            if (settings.Polygamy)
            {
                foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
                {
                    if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                    {
                    }
                    else if (romanticState.Person1 == forHero || romanticState.Person2 == forHero)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Ended;
                    }
                }
                return false;
            }
            return true;
        }

        public static void EndAllCourtships(Hero forHero)
        {
            Prefix(forHero);
        }
    }
}