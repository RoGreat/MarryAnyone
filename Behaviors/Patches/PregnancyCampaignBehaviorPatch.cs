using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone.Behaviors.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static List<Hero> _spouses;

        private static void Prefix(Hero hero)
        {
            ISettingsProvider settings = new MASettings();
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
                            hero.Spouse = null;
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
            if (settings.SexualOrientation == "Homosexual" && (hero == Hero.MainHero || hero.Spouse == Hero.MainHero))
            {
                MASubModule.Debug("Homosexual");
                hero.Spouse = null;
                return;
            }
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                RemoveExSpouses(hero);
                RemoveExSpouses(exSpouse);
            }
        }

        public static void RemoveExSpouses(Hero hero, bool spouse = true)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> ExSpousesReadOnlyList;

            if (spouse)
            {
                _exSpousesList = _exSpousesList.Distinct().ToList();
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                }
            }
            else
            {
                _exSpousesList = _exSpousesList.ToList();
                Hero exSpouse = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).FirstOrDefault();
                if (exSpouse != null)
                {
                    _exSpousesList.Remove(exSpouse);
                }
            }
            ExSpousesReadOnlyList = new MBReadOnlyList<Hero>(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }
    }
}