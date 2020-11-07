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
                return GameTexts.FindText("str_str_daughterwife", null);
            }
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
                if (queriedHero.ExSpouses.Contains(baseHero))
                {
                    if (!queriedHero.IsFemale)
                    {
                        return GameTexts.FindText("str_husband", null);
                    }
                    return GameTexts.FindText("str_wife", null);
                }
            }
            Hero spouse = baseHero.ExSpouses.Where((Hero spouse) => spouse.ExSpouses.Contains(queriedHero)).FirstOrDefault();
            Hero spouse2 = baseHero.Spouse?.ExSpouses.Where((Hero exSpouse) => exSpouse == queriedHero).FirstOrDefault();
            // Find out spouse's gender
            if ((!spouse?.IsFemale ?? false) || (!spouse2?.IsFemale ?? false)) // Spouse is male
            {
                // Find out spouse of spouse's gender
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_husbands_husband", null);
                }
                return GameTexts.FindText("str_husbands_wife", null);
            }
            if ((spouse?.IsFemale ?? false) || (spouse2?.IsFemale ?? false))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_wifes_husband", null);
                }
                return GameTexts.FindText("str_wifes_wife", null);
            }
            return TextObject.Empty;
        }
    }
}