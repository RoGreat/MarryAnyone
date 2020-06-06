using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class DailyTickHeroPatch
    {
        public static List<Hero> Spouses;

        private static void Prefix(Hero hero)
        {
            Spouses = new List<Hero>();
            if (hero.IsFemale && hero.IsAlive && hero.Age > 18f)
            {
                if (hero == Hero.MainHero)
                {
                    ResetSpouse(hero);
                    if (!hero.ExSpouses.Contains(hero.Spouse) && hero.Spouse != null && hero.Spouse.IsAlive && hero.IsFemale != hero.Spouse.IsFemale)
                    {
                        Spouses.Add(hero.Spouse);
                        MASubModule.MADebug("Added Spouse to Spouses: " + hero.Spouse.Name);
                    }
                    if (hero.ExSpouses.Any())
                    {
                        foreach (Hero exSpouse in hero.ExSpouses)
                        {
                            if (exSpouse.IsAlive && hero.IsFemale != exSpouse.IsFemale)
                            {
                                Spouses.Add(exSpouse);
                                MASubModule.MADebug("Added ExSpouse to Spouses: " + exSpouse.Name);
                            }
                        }
                    }
                    if (Spouses.Any())
                    {
                        Random random = new Random();
                        hero.Spouse = Spouses.ElementAt(random.Next(Spouses.Count));
                        hero.Spouse.Spouse = hero;
                        MASubModule.MADebug("Random spouse assigned:");
                        MASubModule.MADebug("   Hero: " + hero.Spouse.Spouse);
                        MASubModule.MADebug("   Spouse: " + hero.Spouse);
                    }
                }
                else if (hero.Spouse == Hero.MainHero || hero.ExSpouses.Contains(Hero.MainHero))
                {
                    ResetSpouse(hero);
                    if (hero.IsDead)
                    {
                        MASubModule.MADebug("Spouse is dead");
                        return;
                    }
                    else if (hero.Spouse == null)
                    {
                        hero.Spouse = Hero.MainHero;
                        hero.Spouse.Spouse = hero;
                        MASubModule.MADebug("Spouse assigned:");
                        MASubModule.MADebug("   Hero: " + hero.Spouse.Spouse);
                        MASubModule.MADebug("   Spouse: " + hero.Spouse);
                    }
                    if (hero.Spouse != null && hero.IsFemale == hero.Spouse.IsFemale)
                    {
                        MASubModule.MADebug("Spouse is of same sex");
                        ResetSpouse(hero);
                    }
                }
            }
            else if (hero.Spouse == Hero.MainHero || hero.ExSpouses.Contains(Hero.MainHero))
            {
                ResetSpouse(hero);
            }
        }

        private static void ResetSpouse(Hero hero)
        {
            if (hero.Spouse != null)
            {
                hero.Spouse.Spouse = null;
                hero.Spouse = null;
                MASubModule.MADebug("Spouse reset");
            }
        }
    }
}