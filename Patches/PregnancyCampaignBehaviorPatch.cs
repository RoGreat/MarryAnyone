using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static List<Hero> _spouses;

        private static void Prefix(Hero hero)
        {
            _spouses = new List<Hero>();
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (Hero.MainHero == hero || Hero.MainHero == hero.Spouse || hero.ExSpouses.Contains(Hero.MainHero))
                {
                    MASubModule.Debug("Female Hero: " + hero.Name);
                    if (hero.Spouse != null)
                    {
                        if (hero.IsFemale == hero.Spouse.IsFemale)
                        {
                            MASubModule.Debug("Spouse Same Gender: " + hero.Spouse.Name);
                            ResetSpouse(hero);
                        }
                        else
                        {
                            _spouses.Add(hero.Spouse);
                            MASubModule.Debug("Spouse to Spouses: " + hero.Spouse.Name);
                        }
                    }
                    foreach (Hero exSpouse in hero.ExSpouses.ToList())
                    {
                        RemoveExSpouses(hero);
                        RemoveExSpouses(exSpouse);
                    }
                    foreach (Hero exSpouse in hero.ExSpouses.ToList())
                    {
                        if (hero.IsFemale == exSpouse.IsFemale)
                        {
                            MASubModule.Debug("ExSpouse Same Gender: " + exSpouse.Name);
                        }
                        else if (!exSpouse.IsAlive)
                        {
                            MASubModule.Debug("ExSpouse Dead: " + exSpouse.Name);
                        }
                        else
                        {
                            _spouses.Add(exSpouse);
                            MASubModule.Debug("ExSpouse to Spouses: " + exSpouse.Name);
                        }
                    }
                    if (_spouses.Where(spouse => spouse != null).Count() > 1)
                    {
                        ResetSpouse(hero);
                        List<int> attractionGoal = new List<int>();
                        int attraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(hero, spouse);
                            attractionGoal.Add(attraction);
                            MASubModule.Debug("Spouse: " + spouse.Name);
                            MASubModule.Debug("Attraction: " + attraction.ToString());
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MASubModule.Debug("Random: " + attractionRandom.ToString());
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MASubModule.Debug("Index: " + i.ToString());
                                break;
                            }
                            i++;
                        }
                        hero.Spouse = _spouses[i];
                    }
                    else if (hero.Spouse == null)
                    {
                        hero.Spouse = _spouses.Where(spouse => spouse != null).FirstOrDefault();
                    }
                    if (hero.Spouse != null)
                    {
                        hero.Spouse.Spouse = hero;
                        MASubModule.Debug("Spouse Assigned:");
                        MASubModule.Debug("   Hero: " + hero.Spouse.Spouse);
                        MASubModule.Debug("   Spouse: " + hero.Spouse);
                    }
                }
            }
            if (MASettings.Instance != null)
            {
                if (MASettings.Instance.SexualOrientation.SelectedValue == "Homosexual" && (hero == Hero.MainHero || hero.Spouse == Hero.MainHero))
                {
                    MASubModule.Debug("Homosexual");
                    ResetSpouse(hero);
                    return;
                }
            }
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                RemoveExSpouses(hero);
                RemoveExSpouses(exSpouse);
            }
        }

        private static void ResetSpouse(Hero hero)
        {
            if (hero.Spouse != null)
            {
                hero.Spouse.Spouse = null;
                hero.Spouse = null;
                MASubModule.Debug("Spouse Reset");
            }
        }

        public static void RemoveExSpouses(Hero hero)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> exSpouseList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> exSpouseReadOnlyList;

            exSpouseList = exSpouseList.Distinct().ToList();
            if (exSpouseList.Contains(hero.Spouse))
            {
                exSpouseList.Remove(hero.Spouse);
            }
            exSpouseReadOnlyList = new MBReadOnlyList<Hero>(exSpouseList);
            _exSpouses.SetValue(hero, exSpouseList);
            ExSpouses.SetValue(hero, exSpouseReadOnlyList);
        }
    }
}