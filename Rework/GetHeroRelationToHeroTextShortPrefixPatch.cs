//using HarmonyLib;
//using System.Linq;
//using TaleWorlds.CampaignSystem;
//using TaleWorlds.CampaignSystem.ViewModelCollection;
//using TaleWorlds.Core;
//using TaleWorlds.Localization;

//namespace MarryAnyone
//{
//    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
//    internal class GetHeroRelationToHeroTextShortPatch
//    {
//        private static void Postfix(ref TextObject __result, Hero queriedHero, Hero baseHero)
//        {
//            var tempResult = __result;
//            __result = GetHeroRelationToHeroTextShort(queriedHero, baseHero);
//            if (__result == TextObject.Empty)
//            {
//                __result = tempResult;
//            }
//        }

//        private static TextObject GetHeroRelationToHeroTextShort(Hero queriedHero, Hero baseHero)
//        {
//            if (baseHero.Father == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
//            {
//                return GameTexts.FindText("str_father_husband", null);
//            }
//            if (baseHero.Mother == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
//            {
//                return GameTexts.FindText("str_mother_wife", null);
//            }
//            if (baseHero.Siblings.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
//            {
//                if (!queriedHero.IsFemale)
//                {
//                    return GameTexts.FindText("str_brother_husband", null);
//                }
//                return GameTexts.FindText("str_sister_wife", null);
//            }
//            if (baseHero.Children.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero)))
//            {
//                if (!queriedHero.IsFemale)
//                {
//                    return GameTexts.FindText("str_son_husband", null);
//                }
//                return GameTexts.FindText("str_daughter_wife", null);
//            }
//            if (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero))
//            {
//                if (!queriedHero.IsFemale)
//                {
//                    return GameTexts.FindText("str_husband", null);
//                }
//                return GameTexts.FindText("str_wife", null);
//            }
//            if (baseHero.Children.Contains(queriedHero))
//            {
//                if (!queriedHero.IsFemale)
//                {
//                    return GameTexts.FindText("str_son", null);
//                }
//                return GameTexts.FindText("str_daughter", null);
//            }
//            if (baseHero.Spouse != null)
//            {
//                if (baseHero.Spouse.Children.Contains(queriedHero))
//                {
//                    if (!queriedHero.IsFemale)
//                    {
//                        return GameTexts.FindText("str_step_son", null);
//                    }
//                    return GameTexts.FindText("str_step_daughter", null);
//                }
//                if (baseHero.Spouse.Siblings.Contains(queriedHero))
//                {
//                    if (!queriedHero.IsFemale)
//                    {
//                        return GameTexts.FindText("str_step_brother", null);
//                    }
//                    return GameTexts.FindText("str_step_sister", null);
//                }
//            }
//            return TextObject.Empty;
//        }
//    }
//}