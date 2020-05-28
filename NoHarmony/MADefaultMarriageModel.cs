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
            bool IsPolyamorous = !MASubModule.IsPolyamorous && (maidenOrSuitor != Hero.MainHero || maidenOrSuitor != Hero.OneToOneConversationHero);

            if ((maidenOrSuitor.Spouse != null && IsPolyamorous) || maidenOrSuitor.IsTemplate)
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
            bool IsMainHeroRelated = firstHero == Hero.MainHero || firstHero == Hero.OneToOneConversationHero || secondHero == Hero.MainHero || secondHero == Hero.OneToOneConversationHero;
            bool IsHomosexual = MASubModule.IsHomosexual && IsMainHeroRelated;
            bool IsBisexual = MASubModule.IsBisexual && IsMainHeroRelated;

            if (IsHomosexual)
            {
                InformationManager.DisplayMessage(new InformationMessage("Homosexual"));
                return firstHero.IsFemale == secondHero.IsFemale && !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
            }
            if (IsBisexual)
            {
                InformationManager.DisplayMessage(new InformationMessage("Bisexual"));
                return !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && this.IsSuitableForMarriage(firstHero) && this.IsSuitableForMarriage(secondHero);
            }
            InformationManager.DisplayMessage(new InformationMessage("Heterosexual"));
            return firstHero.IsFemale != secondHero.IsFemale && !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any<Hero>() && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            bool IsIncestual = MASubModule.IsIncestual && (hero == Hero.MainHero || hero == Hero.OneToOneConversationHero);

            if (hero != null)
            {
                yield return hero;
                if (IsIncestual)
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