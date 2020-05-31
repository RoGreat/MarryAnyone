using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;

namespace MarryAnyone
{
    internal class MADefaultMarriageModel : DefaultMarriageModel
    {
        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            MAConfig config = MASubModule.Config;
            bool isPolygamous = config.IsPolygamous && (maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero);

            if (Hero.MainHero.ExSpouses.Contains(maidenOrSuitor) || maidenOrSuitor.IsTemplate || !maidenOrSuitor.IsAlive || maidenOrSuitor.Spouse == Hero.OneToOneConversationHero)
            {
                return false;
            }
            if (maidenOrSuitor.Spouse == null || isPolygamous)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeMale;
            }
            return false;
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            MAConfig config = MASubModule.Config;
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = config.SexualOrientation == SexualOrientation.Homosexual && isMainHero;
            bool isBisexual = config.SexualOrientation == SexualOrientation.Bisexual && isMainHero;
            bool isIncestual = config.IsIncestual && DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && isMainHero;
            bool discoverAncestors = true;

            if (!isIncestual)
            { 
                discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>();
            }
            if (isHomosexual)
            {
                return firstHero.IsFemale == secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            if (isBisexual)
            {
                return discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            return firstHero.IsFemale != secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
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