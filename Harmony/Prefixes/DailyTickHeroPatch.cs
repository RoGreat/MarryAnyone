using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using System;
using System.Diagnostics;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class DailyTickHeroPatch
    {

        //public static int Index;

        public static List<Hero> Spouses;

        static void Prefix(Hero hero)
        {
            Spouses = new List<Hero>();
            if (hero.IsFemale && hero.IsAlive && hero.Age > 18f) // Is a woman
            {
                if (hero == Hero.MainHero)  // Is the main hero
                {
                    InformationManager.DisplayMessage(new InformationMessage("Main Hero: " + hero.Name));
                    if (hero.Spouse != null && hero.Spouse.IsAlive) // If the woman's Spouse in question exists
                    {
                        Spouses.Add(hero.Spouse); // Add current Spouse to spouses
                        InformationManager.DisplayMessage(new InformationMessage("Adding Spouse: " + hero.Spouse.Name));
                    }
                    if (hero.ExSpouses.Any()) // If woman has any ExSpouse(s)
                    {
                        foreach (Hero exSpouse in hero.ExSpouses)
                        {
                            if (!Spouses.Contains(exSpouse) && exSpouse.IsAlive) // If exSpouse(s) in question exists
                            {
                                Spouses.Add(exSpouse);
                                InformationManager.DisplayMessage(new InformationMessage("Adding ExSpouse: " + exSpouse.Name));
                            }
                        }
                    }
                    if (Spouses.Any()) // If there is/are spouse(s) in spouses
                    {
                        Random random = new Random();
                        hero.Spouse = Spouses.ElementAt(random.Next(Spouses.Count));   // Set Spouse at current element of spouses
                        if (hero.Spouse != null)
                        {
                            InformationManager.DisplayMessage(new InformationMessage("Assigning Spouse: " + hero.Spouse.Name));
                        }
                    }
                }
                else
                {
                    if (hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Not Main Hero: " + hero.Name));
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
                        hero.Spouse = Hero.MainHero;             
                        if (hero.Spouse != null)
                        {
                            InformationManager.DisplayMessage(new InformationMessage("Assigning Spouse: " + hero.Spouse.Name));
                        }
                    }
                }
            }
        }
    }
}