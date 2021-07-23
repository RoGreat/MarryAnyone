using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using static TaleWorlds.CampaignSystem.Romance;

namespace MarryAnyone.Util
{
    // OnNeNousDitPasTout/GrandesMaree Patch
    internal static class Util
    {
        public static bool AreMarried(Hero hero, Hero otherHero)
        {

            if (otherHero == null)
                return false;

            //if (hero.Spouse == null) NO we can have no spouse and have exSpouses who ares my spouses
            //{
            //    MAHelper.Print(String.Format("AreMarried FAIL car {0} n'a aucune épouse", hero.Name));
            //    return false;
            //}

            if (hero.Spouse != null && hero.Spouse == otherHero)
            {
                MAHelper.Print(String.Format("AreMarried SUCCEEDED because {0} married {1}", hero.Name, otherHero.Name));
                return true;
            }

            if (hero.ExSpouses != null && hero.ExSpouses.FirstOrDefault<Hero>(x => x == otherHero) != null)
            {
                MAHelper.Print(String.Format("AreMarried SUCCEEDED because {0} has {1} in his ex-spouses", hero.Name, otherHero.Name));
                return true;
            }

            MAHelper.Print(String.Format("AreMarried FAILED for {0} and {1}", hero.Name, otherHero.Name));
            return false;
        }

        public static void CleanRomance(Hero hero, Hero otherHero)
        {
            int n = 0;
            while (true)
            {
                RomanticState romance = Romance.RomanticStateList.FirstOrDefault<RomanticState>(x => (x.Person1 == hero && x.Person2 == otherHero) || (x.Person2 == hero && x.Person1 == otherHero));
                if (romance != null)
                {
                    Romance.RomanticStateList.Remove(romance);
                    n++;
                }
                else
                    break;
            }
            MAHelper.Print(String.Format("Removing romances for {0} and {1} => {2} removed", hero.Name, otherHero.Name, n));
        }
    }
}
