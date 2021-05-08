using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static void Prefix(Hero hero)
        {
            bool mainHero = hero == Hero.MainHero || hero == Hero.MainHero.Spouse;
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                // Associated with MainHero, going through female pregnancy behavior
                // Those that are the main hero or spouses/exspouses of main hero
                if (mainHero || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    if (hero.Spouse is null && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                    {
                        return;
                    }
                    _spouses = new List<Hero>();
                    MAHelper.Print("Female Hero: " + hero);
                    if (hero.Spouse is not null)
                    {
                        _spouses.Add(hero.Spouse);
                        MAHelper.Print("Spouse to Collection: " + hero.Spouse);
                    }
                    foreach (Hero exSpouse in hero.ExSpouses.Distinct().ToList())   
                    {
                        if (exSpouse.IsAlive)
                        {
                            _spouses.Add(exSpouse);
                            hero.Spouse = exSpouse;
                            MAHelper.Print("ExSpouse to Collection: " + exSpouse);
                        }
                    }
                    if (_spouses.Count() > 1)
                    {
                        // The shuffle!
                        List<int> attractionGoal = new();
                        int attraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(hero, spouse);
                            attractionGoal.Add(attraction);
                            MAHelper.Print("Spouse: " + spouse);
                            MAHelper.Print("Attraction: " + attraction.ToString());
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MAHelper.Print("Random: " + attractionRandom.ToString());
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MAHelper.Print("Index: " + i.ToString());
                                break;
                            }
                            i++;
                        }
                        hero.Spouse = _spouses[i];
                    }
                    if (hero.Spouse is null)
                    {
                        MAHelper.Print("   No Spouse");
                    }
                    else
                    {
                        MAHelper.Print("   Spouse Assigned: " + hero.Spouse);
                    }
                }
            }
            // Outside of female pregnancy behavior
            if (hero.Spouse is not null && mainHero)
            {
                if (hero.IsFemale == hero.Spouse.IsFemale)
                {
                    // Decided to do this at the end so that you are not always going out with the opposite gender
                    MAHelper.Print("   Spouse Unassigned: " + hero.Spouse);
                    hero.Spouse = null;
                }
            }
            // Remove any extra duplicate exspouses
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                MAHelper.RemoveExSpouses(hero);
                MAHelper.RemoveExSpouses(exSpouse);
            }
        }

        private static List<Hero>? _spouses;
    }
}