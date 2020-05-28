using System.Collections.Generic;
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
            MAConfig config = MASettings.Config;
            bool isPolygamous = !config.IsPolygamous && (maidenOrSuitor != Hero.MainHero || maidenOrSuitor != Hero.OneToOneConversationHero);

            if ((maidenOrSuitor.Spouse != null && isPolygamous) || maidenOrSuitor.IsTemplate)
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
            MAConfig config = MASettings.Config;
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = config.SexualOrientation == SexualOrientation.Homosexual && isMainHero;
            bool isBisexual = config.SexualOrientation == SexualOrientation.Bisexual && isMainHero;

            if (isHomosexual)
            {
                InformationManager.DisplayMessage(new InformationMessage("Homosexual"));
                return firstHero.IsFemale == secondHero.IsFemale && !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
            }
            if (isBisexual)
            {
                InformationManager.DisplayMessage(new InformationMessage("Bisexual"));
                return !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
            }
            return firstHero.IsFemale != secondHero.IsFemale && !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            MAConfig config = MASettings.Config;
            bool isIncestual = config.IsIncestual && (hero == Hero.MainHero || hero == Hero.OneToOneConversationHero);

            if (hero != null)
            {
                yield return hero;
                if (isIncestual)
                {
                    yield break;
                }
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