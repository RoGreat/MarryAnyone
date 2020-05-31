using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Linq;
using System.Collections.Generic;
using System;
using TaleWorlds.Core;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class DailyTickHeroPatch
    {
        public static List<Hero> Spouses;

        private static void Prefix(Hero hero)
        {
            MAConfig config = MASubModule.Config;
            Spouses = new List<Hero>();
            if (!config.IsPolygamous)
            {
                return;
            }    
            if (hero.IsFemale && hero.IsAlive && hero.Age > 18f)
            {
                if (hero == Hero.MainHero)
                {
                    ResetSpouse(hero);
                    if (!hero.ExSpouses.Contains(hero.Spouse) && hero.Spouse != null && hero.Spouse.IsAlive && hero.IsFemale != hero.Spouse.IsFemale)
                    {
                        Spouses.Add(hero.Spouse);
                    }
                    if (hero.ExSpouses.Any())
                    {
                        foreach (Hero exSpouse in hero.ExSpouses)
                        {
                            if (exSpouse.IsAlive && hero.IsFemale != exSpouse.IsFemale)
                            {
                                Spouses.Add(exSpouse);
                            }
                        }
                    }
                    if (Spouses.Any())
                    {
                        Random random = new Random();
                        hero.Spouse = Spouses.ElementAt(random.Next(Spouses.Count));
                        hero.Spouse.Spouse = hero;
                    }
                }
                else if (hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    ResetSpouse(hero);
                    if (!hero.IsAlive)
                    {
                        return;
                    }
                    else if (hero.Spouse == null)
                    {
                        hero.Spouse = Hero.MainHero;
                        hero.Spouse.Spouse = hero;
                    }
                    if (hero.Spouse != null && hero.IsFemale == hero.Spouse.IsFemale)
                    {
                        ResetSpouse(hero);
                    }
                }
            }
        }

        private static void ResetSpouse(Hero hero)
        {
            if (hero.Spouse != null)
            {
                hero.Spouse.Spouse = null;
                hero.Spouse = null;
            }
        }
    }
}