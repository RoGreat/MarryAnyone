using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone
{
    class MADefaultMarriageModel : DefaultMarriageModel
    {
        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            if (maidenOrSuitor.Spouse != null || maidenOrSuitor.IsTemplate)
            {
                return false;
            }
            if (maidenOrSuitor.IsFemale)
            {
                return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeFemale;
            }
            return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeMale;
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            return firstHero.IsFemale != secondHero.IsFemale && !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            if (hero != null)
            {
                yield return hero;
                if (n > 0)
                {
                    foreach (Hero hero2 in DiscoverAncestors(hero.Mother, n - 1))
                    {
                        yield return hero2;
                    }
                    foreach (Hero hero3 in DiscoverAncestors(hero.Father, n - 1))
                    {
                        yield return hero3;
                    }
                }
            }
            yield break;
        }
    }
}