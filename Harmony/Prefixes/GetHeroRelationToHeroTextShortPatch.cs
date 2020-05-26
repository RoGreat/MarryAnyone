//using HarmonyLib;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using TaleWorlds.CampaignSystem;
//using TaleWorlds.CampaignSystem.ViewModelCollection;
//using TaleWorlds.Core;
//using TaleWorlds.Localization;

//namespace MarryAnyone
//{
//    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
//    class GetHeroRelationToHeroTextShortPatch
//    {
//        static bool Prefix(Hero queriedHero, Hero baseHero, ref TextObject __result)
//        {
//            __result = GetHeroRelationToHeroTextShortPatch.GetHeroRelationToHeroTextShort(queriedHero, baseHero);
//            return false;
//        }

//        public static TextObject GetHeroRelationToHeroTextShort(Hero queriedHero, Hero baseHero)
//        {
//            if (baseHero.Father == queriedHero)
//            {
//                return GameTexts.FindText("str_father", null);
//            }
//            if (baseHero.Mother == queriedHero)
//            {
//                return GameTexts.FindText("str_mother", null);
//            }
//            if (baseHero.Siblings.Contains(queriedHero))
//            {
//                if (!queriedHero.IsFemale)
//                {
//                    return GameTexts.FindText("str_brother", null);
//                }
//                return GameTexts.FindText("str_sister", null);
//            }
//            else
//            {
//                if (baseHero.Spouse == queriedHero || queriedHero.Spouse == baseHero)
//                {
//                    return GameTexts.FindText("str_spouse", null);
//                }
//                if (baseHero.Children.Contains(queriedHero))
//                {
//                    return GameTexts.FindText("str_child", null);
//                }
//                Hero spouse = baseHero.Spouse;
//                if (((spouse != null) ? spouse.Father : null) == queriedHero)
//                {
//                    return GameTexts.FindText("str_fotherinlaw", null);
//                }
//                Hero spouse2 = baseHero.Spouse;
//                if (((spouse2 != null) ? spouse2.Mother : null) == queriedHero)
//                {
//                    return GameTexts.FindText("str_motherinlaw", null);
//                }
//                if (queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero))
//                {
//                    return GameTexts.FindText("str_spouse", null);
//                }
//                if (queriedHero.CompanionOf == baseHero.Clan)
//                {
//                    return GameTexts.FindText("str_companion", null);
//                }
//                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Mother == queriedHero))
//                {
//                    return GameTexts.FindText("str_ex_mother_in_law", null);
//                }
//                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Father == queriedHero))
//                {
//                    return GameTexts.FindText("str_ex_father_in_law", null);
//                }
//                if (baseHero.ExSpouses.Any((Hero exSpouse) => exSpouse.Siblings.Contains(queriedHero)))
//                {
//                    return GameTexts.FindText(queriedHero.IsFemale ? "str_ex_sister_in_law" : "str_ex_brother_in_law", null);
//                }
//                if (baseHero.Spouse != null && baseHero.Spouse.Siblings.Contains(queriedHero))
//                {
//                    return GameTexts.FindText(queriedHero.IsFemale ? "str_sisterinlaw" : "str_brotherinlaw", null);
//                }
//                return TextObject.Empty;
//            }
//        }
//    }

//}