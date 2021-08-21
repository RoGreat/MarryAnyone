using HarmonyLib;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(Romance), "GetCourtedHeroInOtherClan", new Type[] { typeof(Hero), typeof(Hero) })]
    class Romance_GetCourtedHeroInOtherClan_Patch
	{
        [HarmonyPrefix]
        public static bool Prefix(Hero person1, Hero person2, Hero? __result)
        {
			__result = null;

            if (person2.Clan == null)
                goto avantRetour;

            foreach (Hero person3 in from x in person2.Clan.Lords
									 where x != person2
									 select x)
			{
				if (Romance.GetRomanticLevel(person1, person3) >= Romance.RomanceLevelEnum.MatchMadeByFamily)
				{
					__result = person3;
					return false;
				}
			}

            avantRetour:
			return false;
		}
	}

    //[HarmonyPatch(typeof(Romance), "EndAllCourtships")]
    //internal class Romance_EndAllCourtships_Patch
    //{
    //    private static bool Prefix(Hero forHero)
    //    {
    //        ISettingsProvider settings = new MASettings();
    //        if (settings.Polygamy)
    //        {
    //            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
    //            {
    //                if (forHero == Hero.MainHero && romanticState.Level == Romance.RomanceLevelEnum.Marriage)
    //                {
    //                    romanticState.Level = Romance.RomanceLevelEnum.Ended;
    //                }
    //            }
    //            return false;
    //        }
    //        return true;
    //    }
    //}


}
