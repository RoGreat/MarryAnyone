using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    class DailyTickHeroPatch
    {
        private static void Prefix(Hero hero)
        {
            _spouses = new List<Hero>();
            if (hero.IsFemale && hero.IsAlive && hero.Age > 18f) // Standard check
            {
                if (hero.ExSpouses != null && hero.ExSpouses.Any()) // If there is an ExSpouse
                {
                    foreach (Hero spouse in hero.ExSpouses)
                    {
                        if (!_spouses.Contains(spouse)) // If spouse is not already one of multiple spouses
                        {
                            _spouses.Add(spouse);
                            InformationManager.DisplayMessage(new InformationMessage("Adding Spouse"));
                        }
                        if (spouse.IsDead)  // If spouse is dead
                        {
                            _spouses.Remove(spouse);
                            InformationManager.DisplayMessage(new InformationMessage("Removing Spouse"));
                        }
                    }
                }
                if (_spouses != null && _spouses.Any()) // If there is multiple spouses
                {
                    if (i > _spouses.Count - 1)
                    {
                        i = 0;
                    }
                    hero.Spouse = _spouses.ElementAt(i);
                    InformationManager.DisplayMessage(new InformationMessage("Assigning Spouse"));
                    InformationManager.DisplayMessage(new InformationMessage("Index " + i));
                    i++;
                }
            }
        }

        private static int i = 0;

        private static List<Hero> _spouses;
    }
}