using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
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

            if (hero.IsFemale && hero.IsAlive && hero.Age > 18f) // Is an alive woman
            {
                if (hero == Hero.MainHero)  // Is the main hero
                {
                    ResetSpouse(hero);
                    InformationManager.DisplayMessage(new InformationMessage("Main Hero: " + hero.Name));
                    if (!hero.ExSpouses.Contains(hero.Spouse) && hero.Spouse != null && hero.Spouse.IsAlive && hero.IsFemale != hero.Spouse.IsFemale) // If the woman's Spouse in question exists
                    {
                        Spouses.Add(hero.Spouse); // Add current Spouse to spouses
                        InformationManager.DisplayMessage(new InformationMessage("Adding Spouse: " + hero.Spouse.Name));
                    }
                    if (hero.ExSpouses.Any()) // If woman has any ExSpouse(s)
                    {
                        foreach (Hero exSpouse in hero.ExSpouses)
                        {
                            if (exSpouse.IsAlive && hero.IsFemale != exSpouse.IsFemale)
                            {
                                Spouses.Add(exSpouse);
                                InformationManager.DisplayMessage(new InformationMessage("Adding ExSpouse: " + exSpouse.Name));
                            }
                        }
                    }
                    if (Spouses.Any()) // If there is/are spouse(s) in spouses
                    {
                        Random random = new Random();
                        hero.Spouse = Spouses.ElementAt(random.Next(Spouses.Count));   // Set Spouse at random for now
                        hero.Spouse.Spouse = hero;
                        if (hero.Spouse != null)
                        {
                            InformationManager.DisplayMessage(new InformationMessage("Assigning Spouse: " + hero.Spouse.Name));
                        }
                    }
                }
                else if (hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    ResetSpouse(hero);
                    InformationManager.DisplayMessage(new InformationMessage("Not Main Hero: " + hero.Name));
                    if (!hero.IsAlive)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Is Dead: " + hero.Name));
                        return;
                    }
                    else if (hero.Spouse == null)
                    {
                        hero.Spouse = Hero.MainHero;
                        hero.Spouse.Spouse = hero;
                        InformationManager.DisplayMessage(new InformationMessage("Assigning Spouse: " + hero.Spouse.Name));
                    }
                    if (hero.Spouse != null && hero.IsFemale == hero.Spouse.IsFemale)   // If there is same sex character spouse
                    {
                        ResetSpouse(hero);
                        InformationManager.DisplayMessage(new InformationMessage("Same Sex"));
                    }
                    if (hero == Hero.MainHero.Spouse)
                    {
                        if (hero.Spouse != null)
                        {
                            InformationManager.DisplayMessage(new InformationMessage("Spouse: " + hero.Name));
                        }
                    }
                    if (Hero.MainHero.ExSpouses.Contains(hero))
                    {
                        InformationManager.DisplayMessage(new InformationMessage("ExSpouse: " + hero.Name));
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
                InformationManager.DisplayMessage(new InformationMessage("Reset Spouse"));
            }
        }
    }
}