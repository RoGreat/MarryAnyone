using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using static MarryAnyone.Helpers;
using static MarryAnyone.Debug;

namespace MarryAnyone.Patches
{
    /* Old pregnancy behavior without all the extra stuff... */
    // Hope that it still works as intended heehee
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal static class PregnancyCampaignBehaviorPatches
    {
        private static List<Hero>? _spouses;

        private static void Prefix(Hero hero)
        {
            MASettings settings = new();
            if (settings.PregnancyPlus)
            {
                if (hero != Hero.MainHero && hero.Spouse != Hero.MainHero && Hero.MainHero.Spouse != hero
                    && !hero.ExSpouses.Contains(Hero.MainHero) && !Hero.MainHero.ExSpouses.Contains(hero))
                {
                    return;
                }
                if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
                {
                    // If you are the MainHero go through advanced process
                    if (hero == Hero.MainHero || hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                    {
                        if (hero.Spouse is null && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                        {
                            Print("    No Spouse");
                            return;
                        }
                        _spouses = new List<Hero>();
                        Print("Hero: " + hero);
                        if (hero.Spouse is not null && hero == Hero.MainHero)
                        {
                            _spouses.Add(hero.Spouse);
                            Print("Spouse to Collection: " + hero.Spouse);
                        }
                        if (settings.Polyamory && hero != Hero.MainHero)
                        {
                            Print("Polyamory");
                            if (hero.Spouse != Hero.MainHero)
                            {
                                _spouses.Add(Hero.MainHero);
                                Print("Main Hero to Collection: " + Hero.MainHero);
                            }
                            if (Hero.MainHero.Spouse is not null && Hero.MainHero.Spouse != hero)
                            {
                                _spouses.Add(Hero.MainHero.Spouse);
                                Print("Main Hero Spouse to Collection: " + Hero.MainHero.Spouse);
                            }
                            foreach (Hero exSpouse in Hero.MainHero.ExSpouses.Distinct().ToList())
                            {
                                if (exSpouse != hero && exSpouse.IsAlive)
                                {
                                    _spouses.Add(exSpouse);
                                    Print("Main Hero ExSpouse to Collection: " + exSpouse);
                                }
                            }
                        }
                        else
                        {
                            // Taken out of polyamory mode
                            if (hero.Spouse != Hero.MainHero && hero != Hero.MainHero)
                            {
                                _spouses.Add(Hero.MainHero);
                                Print("Spouse is Main Hero: " + Hero.MainHero);
                            }
                            if (hero == Hero.MainHero)
                            {
                                foreach (Hero exSpouse in hero.ExSpouses.Distinct().ToList())
                                {
                                    if (exSpouse.IsAlive)
                                    {
                                        _spouses.Add(exSpouse);
                                        Print("ExSpouse to Collection: " + exSpouse);
                                    }
                                }
                            }
                        }
                        if (_spouses.Count() > 1)
                        {
                            // The shuffle!
                            List<int> attractionGoal = new();
                            int attraction = 0;
                            foreach (Hero spouse in _spouses)
                            {
                                attraction += Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(hero, spouse);
                                attractionGoal.Add(attraction);
                                Print("Spouse: " + spouse);
                                Print("Attraction: " + attraction);
                            }
                            int attractionRandom = MBRandom.RandomInt(attraction);
                            Print("Random: " + attractionRandom);
                            int i = 0;
                            while (i < _spouses.Count)
                            {
                                if (attractionRandom < attractionGoal[i])
                                {
                                    Print("Index: " + i);
                                    break;
                                }
                                i++;
                            }
                            hero.Spouse = _spouses[i];
                            _spouses[i].Spouse = hero;
                        }
                        else
                        {
                            var spouse = _spouses.FirstOrDefault();
                            if (spouse is not null)
                            {
                                hero.Spouse = spouse;
                                spouse.Spouse = hero;
                            }
                        }
                        if (hero.Spouse is null)
                        {
                            Print("   No Spouse");
                        }
                        else
                        {
                            Print("   Spouse Assigned: " + hero.Spouse);
                        }
                    }
                }
                if (hero.Spouse is not null)
                {
                    if (hero.IsFemale == hero.Spouse.IsFemale)
                    {
                        // Decided to do this at the end so that you are not always going out with the opposite gender
                        Print("   Spouse Unassigned: " + hero.Spouse);
                        hero.Spouse.Spouse = null;
                        hero.Spouse = null;
                    }
                }
            }
        }

        private static void Postfix(Hero hero)
        {
            MASettings settings = new();
            if (settings.PregnancyPlus)
            {
                if (hero != Hero.MainHero && hero.Spouse != Hero.MainHero && Hero.MainHero.Spouse != hero
                    && !hero.ExSpouses.Contains(Hero.MainHero) && !Hero.MainHero.ExSpouses.Contains(hero))
                {
                    return;
                }
                // Make things looks better in the encyclopedia
                if (hero == Hero.MainHero)
                {
                    Print("Post Pregnancy Check: " + hero);
                    Print("   Main Hero Spouse Unassigned");
                    hero.Spouse = null;
                }
                if (Hero.MainHero.ExSpouses.Contains(hero) || hero.Spouse == Hero.MainHero)
                {
                    if (hero.Spouse is null || hero.Spouse != Hero.MainHero)
                    {
                        Print("Post Pregnancy Check: " + hero);
                        Print("   Spouse is Main Hero");
                        // Possibly dangerous now...
                        // Added mode to define remove exspouses from self
                        if (!settings.Polyamory)
                        {
                            // Remove all exspouses from self
                            RemoveExSpouses(hero, RemoveExSpousesEnum.Self);
                        }
                        hero.Spouse = Hero.MainHero;
                    }
                }
                foreach (Hero exSpouse in hero.ExSpouses.ToList())
                {
                    RemoveExSpouses(hero);
                    RemoveExSpouses(exSpouse);
                }
            }
        }
    }
}