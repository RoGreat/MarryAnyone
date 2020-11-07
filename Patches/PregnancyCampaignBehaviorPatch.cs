using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        public static List<Hero> Spouses;

        private static void Prefix(Hero hero)
        {
            Hero femaleHero = hero;
            Spouses = new List<Hero>();
            if (femaleHero.IsFemale && femaleHero.IsAlive && femaleHero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (Hero.MainHero == femaleHero || Hero.MainHero == femaleHero.Spouse  || femaleHero.ExSpouses.Contains(Hero.MainHero))
                {
                    MASubModule.Debug("Female Hero: " + femaleHero.Name);
                    if (femaleHero.Spouse != null)
                    {
                        Spouses.Add(femaleHero.Spouse);
                        MASubModule.Debug("Spouse to Spouses: " + femaleHero.Spouse.Name);
                    }
                    List<Hero> ExSpouses = new List<Hero>();
                    foreach (Hero exSpouse in femaleHero.ExSpouses)
                    {
                        ExSpouses.Add(exSpouse);
                    }
                    for (int i = 0; i < ExSpouses.Count; i++)
                    {
                        RemoveExSpouses(femaleHero);
                        RemoveExSpouses(ExSpouses[i]);
                    }
                    foreach (Hero exSpouse in femaleHero.ExSpouses)
                    {
                        if (exSpouse.IsAlive)
                        {
                            Spouses.Add(exSpouse);
                            MASubModule.Debug("ExSpouse to Spouses: " + exSpouse.Name);
                        }
                    }
                    if (Spouses.Count > 1)
                    {
                        ResetSpouse(femaleHero);
                        List<int> attractionGoal = new List<int>();
                        int attraction = 0;
                        foreach (Hero spouse in Spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(femaleHero, spouse);
                            attractionGoal.Add(attraction);
                            MASubModule.Debug("Spouse: " + spouse.Name);
                            MASubModule.Debug("Attraction: " + attraction.ToString());
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MASubModule.Debug("Random: " + attractionRandom.ToString());
                        int i = 0;
                        while (i < Spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MASubModule.Debug("Index: " + i.ToString());
                                break;
                            }
                            i++;
                        }
                        femaleHero.Spouse = Spouses[i];
                    }
                    else
                    {
                        MASubModule.Debug("Find Single Spouse: " + Spouses.Where(spouse => spouse != null).FirstOrDefault().Name.ToString());
                        femaleHero.Spouse = Spouses.Where(spouse => spouse != null).FirstOrDefault();
                    }
                    if (femaleHero.Spouse != null)
                    {
                        femaleHero.Spouse.Spouse = femaleHero;
                        MASubModule.Debug("Spouse Assigned:");
                        MASubModule.Debug("   Hero: " + femaleHero.Spouse.Spouse);
                        MASubModule.Debug("   Spouse: " + femaleHero.Spouse);
                        if (femaleHero.IsFemale == femaleHero.Spouse.IsFemale)
                        {
                            ResetSpouse(femaleHero);
                        }
                    }
                }
                foreach (Hero exSpouse in femaleHero.ExSpouses)
                {
                    RemoveExSpouses(femaleHero);
                    RemoveExSpouses(exSpouse);
                }
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
                MASubModule.Debug("Removed: " + hero.Spouse.Name);
                exSpouseList.Remove(hero.Spouse);
            }
            exSpouseReadOnlyList = new MBReadOnlyList<Hero>(exSpouseList);
            _exSpouses.SetValue(hero, exSpouseList);
            ExSpouses.SetValue(hero, exSpouseReadOnlyList);
        }
    }
}