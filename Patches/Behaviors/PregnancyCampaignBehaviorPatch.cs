using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace MarryAnyone.Patches.Behaviors
{
    // Add in a setting for enabling polyamory so it does not have to be a harem
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static void Prefix(Hero hero)
        {
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                // If you are the MainHero go through advanced process
                if (hero == Hero.MainHero || hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    ISettingsProvider settings = new MASettings();
                    if (hero.Spouse is null && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                    {
                        MAHelper.Print(string.Format("DailyTickHero:: {0} has No Spouse", hero.Name), MAHelper.PRINT_TEST_PREGNANCY);
                        return;
                    }
                    _spouses = new List<Hero>();
                    MAHelper.Print(string.Format("DailyTickHero::{0} Pregnant {2}\r\nPolyamory ?= {1}", hero.Name, settings.Polyamory, hero.IsPregnant), MAHelper.PRINT_TEST_PREGNANCY);
                    if (hero.Spouse is not null && hero == Hero.MainHero)
                    {
                        _spouses.Add(hero.Spouse);
                        MAHelper.Print("DailyTickHero::Spouse to Collection: " + hero.Spouse, MAHelper.PRINT_TEST_PREGNANCY);
                    }
                    if (settings.Polyamory && hero != Hero.MainHero)
                    {
                        MAHelper.Print("Polyamory");
                        if (hero.Spouse != Hero.MainHero)
                        {
                            _spouses.Add(Hero.MainHero);
                        }
                        if (Hero.MainHero.Spouse is not null && Hero.MainHero.Spouse != hero)
                        {
                            _spouses.Add(Hero.MainHero.Spouse);
                        }
                        foreach (Hero exSpouse in Hero.MainHero.ExSpouses.Distinct().ToList())
                        {
                            if (exSpouse != hero && exSpouse.IsAlive)
                            {
                                _spouses.Add(exSpouse);
                            }
                        }
                    }
                    else
                    {
                        // Taken out of polyamory mode
                        if (hero.Spouse != Hero.MainHero && hero != Hero.MainHero)
                        {
                            _spouses.Add(Hero.MainHero);
                        }
                        if (hero == Hero.MainHero)
                        {
                            foreach (Hero exSpouse in hero.ExSpouses.Distinct().ToList())
                            {
                                if (exSpouse.IsAlive)
                                {
                                    _spouses.Add(exSpouse);
                                }
                            }
                        }
                    }
                    if (_spouses.Count() > 1)
                    {
                        // The shuffle!
                        List<int> attractionGoal = new();
                        int attraction = 0;
                        int addAttraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            addAttraction = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(hero, spouse);
                            attraction += addAttraction * (spouse.IsFemale ? 1 : 3); // To up the pregnancy chance
                            attractionGoal.Add(attraction);
                            MAHelper.Print(string.Format("Spouse {0} attraction {1}", spouse.Name, attraction), MAHelper.PRINT_TEST_PREGNANCY);
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MAHelper.Print("Random: " + attractionRandom, MAHelper.PRINT_TEST_PREGNANCY);
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom <= attractionGoal[i])
                            {
                                MAHelper.Print(string.Format("Résoud Index{0} => Spouse {1}", i, _spouses[i].Name), MAHelper.PRINT_TEST_PREGNANCY);
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
                        MAHelper.Print("DailyTickHero:: No Spouse");
                    }
                    else
                    {
                        MAHelper.Print("DailyTickHero:: Spouse Assigned" + hero.Spouse);
                    }
                }
            }
            // Outside of female pregnancy behavior
            if (hero.Spouse is not null)
            {
                if (hero.IsFemale == hero.Spouse.IsFemale)
                {
                    // Decided to do this at the end so that you are not always going out with the opposite gender
                    MAHelper.Print("DailyTickHero:: Spouse Unassigned because (same sex): " + hero.Spouse, MAHelper.PRINT_TEST_PREGNANCY);
                    hero.Spouse.Spouse = null;
                    hero.Spouse = null;
                }
            }
        }

        private static void Postfix(Hero hero)
        {
            // Make things looks better in the encyclopedia

            if (hero == Hero.MainHero)
            {
                MAHelper.Print(string.Format("Post Pregnancy main hero {0} IsPregnant {1} Check unassigne spouse", hero.Name, hero.IsPregnant), MAHelper.PRINT_TEST_PREGNANCY);
                hero.Spouse = null;
            }
            if (Hero.MainHero.ExSpouses.Contains(hero) || hero.Spouse == Hero.MainHero)
            {
                MAHelper.Print(string.Format("Post Pregnancy {0} IsPregnant {1} ", hero.Name, hero.IsPregnant), MAHelper.PRINT_TEST_PREGNANCY | MAHelper.PrintHow.UpdateLog);

                if (hero.Spouse is null || hero.Spouse != Hero.MainHero)
                {
                    ISettingsProvider settings = new MASettings();

                    MAHelper.Print("   Spouse is Main Hero", MAHelper.PRINT_TEST_PREGNANCY);
                    if (!settings.Polyamory)
                    {
                        // Remove any extra duplicate exspouses
                        MAHelper.RemoveExSpouses(hero, true);
                    }
                    hero.Spouse = Hero.MainHero;
                }
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