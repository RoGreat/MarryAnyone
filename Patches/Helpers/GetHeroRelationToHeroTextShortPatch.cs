using HarmonyLib;
using MarryAnyone.Settings;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Helpers
{
    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
    internal class GetHeroRelationToHeroTextShortPatch
    {
        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }

        private static bool ResultContains(string id)
        {
            return _stringResult!.Contains(FindText(id).ToString());
        }

        private static TextObject ReplaceResult(string idF, string idM, string nidF, string nidM)
        {
            _stringResult = _stringResult!.Replace(FindText(_isFemale ? idF : idM).ToString(), FindText(_isFemale ? nidF : nidM).ToString());
            return new TextObject(_stringResult!);
        }

        private static string? _stringResult;

        private static bool _isFemale;

        private static void Postfix(ref TextObject __result, Hero queriedHero, Hero baseHero)
        {
            _stringResult = __result.ToString();
            _isFemale = queriedHero.IsFemale;
            // In case of polygamy setting
            if (queriedHero.IsAlive && baseHero.IsAlive)
            {
                if (ResultContains("str_ex_husband_motherinlaw") || ResultContains("str_ex_husband_fatherinlaw"))
                {
                    __result = ReplaceResult("str_ex_husband_motherinlaw", "str_ex_husband_fatherinlaw", "str_ma_husband_motherinlaw", "str_ma_husband_fatherinlaw");
                }
                if (ResultContains("str_ex_wife_motherinlaw") || ResultContains("str_ex_wife_fatherinlaw"))
                {
                    __result = ReplaceResult("str_ex_wife_motherinlaw", "str_ex_wife_fatherinlaw", "str_ma_wife_motherinlaw", "str_ma_wife_fatherinlaw");
                }
                if (ResultContains("str_ex_husband_sisterinlaw") || ResultContains("str_ex_husband_brotherinlaw"))
                {
                    __result = ReplaceResult("str_ex_husband_sisterinlaw", "str_ex_husband_brotherinlaw", "str_ma_husband_sisterinlaw", "str_ma_husband_brotherinlaw");
                }
                if (ResultContains("str_ex_wife_sisterinlaw") || ResultContains("str_ex_wife_brotherinlaw"))
                {
                    __result = ReplaceResult("str_ex_wife_sisterinlaw", "str_ex_wife_brotherinlaw", "str_ma_wife_sisterinlaw", "str_ma_wife_brotherinlaw");
                }
            }
            TextObject tempResult = __result;
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
            ISettingsProvider settings = new MASettings();
            if (settings.AdoptionTitles && settings.Adoption)
            {
                if ((baseHero.Mother is null) != (baseHero.Father is null))
                {
                    if (baseHero.Mother == queriedHero)
                    {
                        return GameTexts.FindText("str_adoptivemother", null);
                    }
                    if (baseHero.Father == queriedHero)
                    {
                        return GameTexts.FindText("str_adoptivefather", null);
                    }
                }
                if ((queriedHero.Mother is null) != (queriedHero.Father is null))
                {
                    if (baseHero.Children.Contains(queriedHero))
                    {
                        if (!queriedHero.IsFemale)
                        {
                            return GameTexts.FindText("str_adoptedson", null);
                        }
                        return GameTexts.FindText("str_adopteddaughter", null);
                    }
                }
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
                else if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_husband", null);
                }
                return GameTexts.FindText("str_wife", null);
            }

            // Spouse is Spouse
            Hero spouse = baseHero.Spouse;
            // Spouse is ExSpouse
            Hero spouse2 = baseHero.ExSpouses.FirstOrDefault(spouse => spouse.IsAlive);
            // Find ExSpouse in Spouse
            Hero? otherSpouse = spouse?.ExSpouses.FirstOrDefault(exSpouse => exSpouse == queriedHero);
            // Find ExSpouse in ExSpouse
            Hero? otherSpouse2 = spouse2?.ExSpouses.FirstOrDefault(exSpouse => exSpouse == queriedHero);
            // Find Spouse in ExSpouse
            Hero? otherSpouse3 = spouse2?.Spouse;
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