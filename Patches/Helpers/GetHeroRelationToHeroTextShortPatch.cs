using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
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
            if (baseHero.Father == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                return GameTexts.FindText("str_fatherhusband");
            }
            if (baseHero.Mother == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                return GameTexts.FindText("str_motherwife");
            }
            if (baseHero.Siblings.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_brotherhusband");
                }
                return GameTexts.FindText("str_sisterwife");
            }
            if (baseHero.Children.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_sonhusband");
                }
                return GameTexts.FindText("str_daughterwife");
            }
            ISettingsProvider settings = new MASettings();
            if (settings.AdoptionTitles && settings.Adoption)
            {
                if ((baseHero.Mother is null) != (baseHero.Father is null))
                {
                    if (baseHero.Mother == queriedHero)
                    {
                        return GameTexts.FindText("str_adoptivemother");
                    }
                    if (baseHero.Father == queriedHero)
                    {
                        return GameTexts.FindText("str_adoptivefather");
                    }
                }
                if ((queriedHero.Mother is null) != (queriedHero.Father is null))
                {
                    if (baseHero.Children.Contains(queriedHero))
                    {
                        if (!queriedHero.IsFemale)
                        {
                            return GameTexts.FindText("str_adoptedson");
                        }
                        return GameTexts.FindText("str_adopteddaughter");
                    }
                }
            }
            if (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero))
            {
                if (!queriedHero.IsAlive || !baseHero.IsAlive)
                {
                    if (!queriedHero.IsFemale)
                    {
                        return GameTexts.FindText("str_exhusband");
                    }
                    return GameTexts.FindText("str_exwife");
                }
                else if (!queriedHero.IsFemale)
                {
                    return GameTexts.FindText("str_husband");
                }
                return GameTexts.FindText("str_wife");
            }

            // Revamped spouse's spouse
            if (baseHero.Spouse is not null)
            {
                // Spouse to ExSpouse
                foreach (Hero spouse in baseHero.Spouse.ExSpouses)
                {
                    List<Hero> otherSpouses = spouse.ExSpouses.Where(x => x.IsAlive).ToList();
                    foreach (Hero otherSpouse in otherSpouses)
                    {
                        if (otherSpouse == queriedHero)
                        {
                            return SpousesSpouse(spouse, queriedHero);
                        }
                    }
                }
            }
            List<Hero> spouses = baseHero.ExSpouses.Where(x => x.IsAlive).ToList();
            foreach (Hero spouse in spouses)
            {
                // ExSpouse to Spouse
                if (spouse.Spouse == queriedHero)
                {
                    return SpousesSpouse(spouse, queriedHero);
                }
                List<Hero> otherSpouses = spouse.ExSpouses.Where(x => x.IsAlive).ToList();
                // ExSpouse to ExSpouse
                foreach (Hero otherSpouse in otherSpouses)
                {
                    if (otherSpouse == queriedHero)
                    {
                        return SpousesSpouse(spouse, queriedHero);
                    }
                }
            }
            return TextObject.Empty;
        }

        private static TextObject SpousesSpouse(Hero spouse, Hero queriedHero)
        {
            ISettingsProvider settings = new MASettings();
            // Find out spouse's gender
            // Male spouse
            if (!spouse.IsFemale)
            {
                // Find out spouse's spouse's gender
                if (!queriedHero.IsFemale)
                {
                    // Male other spouse
                    return settings.Polyamory ? FindText("str_husband") : FindText("str_husbands_husband");
                }
                // Female other spouse
                return settings.Polyamory ? FindText("str_wife") : FindText("str_husbands_wife");
            }
            if (!queriedHero.IsFemale)
            {
                return settings.Polyamory ? FindText("str_husband") : FindText("str_wifes_husband");
            }
            return settings.Polyamory ? FindText("str_wife") : FindText("str_wifes_wife");
        }
    }
}