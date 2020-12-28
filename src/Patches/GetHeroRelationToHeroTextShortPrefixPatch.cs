using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
    internal class GetHeroRelationToHeroTextShortPatch
    {
        private static void Postfix(ref TextObject __result, Hero queriedHero, Hero baseHero)
        {
            var tempResult = __result;
            __result = GetHeroRelationToHeroTextShort(queriedHero, baseHero);
            if (__result == TextObject.Empty)
            {
                __result = tempResult;
            }
        }

        private static TextObject GetHeroRelationToHeroTextShort(Hero queriedHero, Hero baseHero)
        {
            if (baseHero.Father == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
            {
                return GameTexts.FindText("str_fatherhusband", null);
            }
            if (baseHero.Mother == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
            {
                return GameTexts.FindText("str_motherwife", null);
            }
            if (baseHero.Siblings.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_brotherhusband", null);
                }
                return GameTexts.FindText("str_sisterwife", null);
            }
            if (baseHero.Children.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_sonhusband", null);
                }
                return GameTexts.FindText("str_daughterwife", null);
            }
            // Don't need to add in all of this...
            //if (baseHero.Mother == null != (baseHero.Father == null))
            //{
            //    if (baseHero.Mother == queriedHero)
            //    {
            //        return GameTexts.FindText("str_adoptivemother", null);
            //    }
            //    if (baseHero.Father == queriedHero)
            //    {
            //        return GameTexts.FindText("str_adoptivefather", null);
            //    }
            //}
            //if (queriedHero.Mother == null != (queriedHero.Father == null))
            //{
            //    if (baseHero.Children.Contains(queriedHero))
            //    {
            //        if (!queriedHero.IsFemale)
            //        {
            //            return GameTexts.FindText("str_adoptedson", null);
            //        }
            //        return GameTexts.FindText("str_adopteddaughter", null);
            //    }
            //}
            if (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero))
            {
                if (!queriedHero.IsAlive || !baseHero.IsAlive)
                {
                    if (!queriedHero.IsFemale)
                    {
                        return GameTexts.FindText("str_exhusband", null);
                    }
                    return GameTexts.FindText("str_exwife", null);
                }
                else if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_husband", null);
                }
                return GameTexts.FindText("str_wife", null);
            }

            // Spouse is Spouse
            Hero spouse = baseHero.Spouse;
            // Spouse is ExSpouse
            Hero spouse2 = baseHero.ExSpouses.Where(spouse => spouse.IsAlive).FirstOrDefault();
            // Find ExSpouse in Spouse
            Hero otherSpouse = spouse?.ExSpouses.Where(exSpouse => exSpouse == queriedHero).FirstOrDefault();
            // Find ExSpouse in ExSpouse
            Hero otherSpouse2 = spouse2?.ExSpouses.Where(exSpouse => exSpouse == queriedHero).FirstOrDefault();
            // Find Spouse in ExSpouse
            Hero otherSpouse3 = spouse2?.Spouse;
            if (otherSpouse == queriedHero || otherSpouse2 == queriedHero || otherSpouse3 == queriedHero)
            {
                // Find out spouse's gender
                if ((!spouse?.IsFemale ?? false) || (!spouse2?.IsFemale ?? false))  // Male spouse
                {
                    // Find out spouse of spouse's gender
                    if (!queriedHero.IsFemale)  // Male other spouse
                    {
                        return GameTexts.FindText("str_husbands_husband", null);
                    }
                    return GameTexts.FindText("str_husbands_wife", null);
                }
                if ((spouse?.IsFemale ?? false) || (spouse2?.IsFemale ?? false))    // Female spouse
                {
                    if (!queriedHero.IsFemale)  // Female other spouse
                    {
                        return GameTexts.FindText("str_wifes_husband", null);
                    }
                    return GameTexts.FindText("str_wifes_wife", null);
                }
            }
            return TextObject.Empty;
        }
    }
}