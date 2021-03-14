using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;

namespace MarryAnyone.Behaviors.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static void Prefix(Hero hero)
        {
            ISettingsProvider settings = new MASettings();
            _spouses = new List<Hero>();
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (Hero.MainHero == hero || Hero.MainHero == hero.Spouse || hero.ExSpouses.Contains(Hero.MainHero))
                {
                    MASubModule.Print("Female Hero: " + hero.Name);
                    if (hero.Spouse is not null)
                    {
                        if (hero.IsFemale == hero.Spouse.IsFemale)
                        {
                            MASubModule.Print("Spouse Same Gender: " + hero.Spouse.Name);
                            hero.Spouse = null;
                        }
                        else
                        {
                            _spouses.Add(hero.Spouse);
                            MASubModule.Print("Spouse to Spouses: " + hero.Spouse.Name);
                        }
                    }
                    foreach (Hero exSpouse in hero.ExSpouses.ToList())
                    {
                        MAHelper.RemoveExSpouses(hero);
                        MAHelper.RemoveExSpouses(exSpouse);
                    }
                    foreach (Hero exSpouse in hero.ExSpouses.ToList())
                    {
                        if (hero.IsFemale == exSpouse.IsFemale)
                        {
                            MASubModule.Print("ExSpouse Same Gender: " + exSpouse.Name);
                        }
                        else if (!exSpouse.IsAlive)
                        {
                            MASubModule.Print("ExSpouse Dead: " + exSpouse.Name);
                        }
                        else
                        {
                            _spouses.Add(exSpouse);
                            MASubModule.Print("ExSpouse to Spouses: " + exSpouse.Name);
                        }
                    }
                    if (_spouses.WhereQ(spouse => spouse is not null).Count() > 1)
                    {
                        List<int> attractionGoal = new();
                        int attraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(hero, spouse);
                            attractionGoal.Add(attraction);
                            MASubModule.Print("Spouse: " + spouse.Name);
                            MASubModule.Print("Attraction: " + attraction.ToString());
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MASubModule.Print("Random: " + attractionRandom.ToString());
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MASubModule.Print("Index: " + i.ToString());
                                break;
                            }
                            i++;
                        }
                        hero.Spouse = _spouses[i];
                    }
                    else if (hero.Spouse is null)
                    {
                        hero.Spouse = _spouses.WhereQ(spouse => spouse is not null).FirstOrDefault();
                    }
                    if (hero.Spouse is not null)
                    {
                        hero.Spouse.Spouse = hero;
                        MASubModule.Print("Spouse Assigned:");
                        MASubModule.Print("   Hero: " + hero.Spouse.Spouse);
                        MASubModule.Print("   Spouse: " + hero.Spouse);
                    }
                }
            }
            if (settings.SexualOrientation == "Homosexual" && (hero == Hero.MainHero || hero.Spouse == Hero.MainHero))
            {
                hero.Spouse = null;
                return;
            }
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                MAHelper.RemoveExSpouses(hero);
                MAHelper.RemoveExSpouses(exSpouse);
            }
        }

        private static List<Hero>? _spouses;
    }
}