using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MarryAnyone
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
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_husband", null);
                }
                return GameTexts.FindText("str_wife", null);
            }
            if (baseHero.Children.Contains(queriedHero))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_son", null);
                }
                return GameTexts.FindText("str_daughter", null);
            }
            if (baseHero.Spouse != null)
            {
                if (baseHero.Spouse.Children.Contains(queriedHero))
                {
                    if (!queriedHero.IsFemale)
                    {
                        return GameTexts.FindText("str_stepson", null);
                    }
                    return GameTexts.FindText("str_stepdaughter", null);
                }
                if (baseHero.Spouse.Siblings.Contains(queriedHero))
                {
                    if (!queriedHero.IsFemale)
                    {
                        return GameTexts.FindText("str_stepbrother", null);
                    }
                    return GameTexts.FindText("str_stepsister", null);
                }
            }
            return TextObject.Empty;
        }
    }
}