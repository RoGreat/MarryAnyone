using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Models
{
    public class MarryAnyoneMarriageModel : MarriageModel
    {
        private readonly MarriageModel? _previousModel;

        public MarryAnyoneMarriageModel(MarriageModel? previousModel)
        {
            _previousModel = previousModel;
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            if (!Settings.Instance!.EnableLeaderRomance && !(firstHero == Hero.MainHero || secondHero == Hero.MainHero))
            {
                Clan clan = firstHero.Clan;
                if (clan?.Leader == firstHero)
                {
                    Clan clan2 = secondHero.Clan;
                    if (clan2?.Leader == secondHero)
                    {
                        return false;
                    }
                }
            }
            if (firstHero.IsFemale != secondHero.IsFemale && !AreHeroesRelated(firstHero, secondHero, 3))
            {
                if (firstHero.Clan is null || secondHero.Clan is null)
                {
                    return true;
                }
                Hero courtedHeroInOtherClan = Romance.GetCourtedHeroInOtherClan(firstHero, secondHero);
                if (courtedHeroInOtherClan is not null && courtedHeroInOtherClan != secondHero)
                {
                    return false;
                }
                Hero courtedHeroInOtherClan2 = Romance.GetCourtedHeroInOtherClan(secondHero, firstHero);
                return (courtedHeroInOtherClan2 is null || courtedHeroInOtherClan2 == firstHero) && firstHero.CanMarry() && secondHero.CanMarry();
            }
            return false;
        }

        private bool AreHeroesRelated(Hero firstHero, Hero secondHero, int ancestorDepth)
        {
            return AreHeroesRelatedAux2(firstHero, secondHero, ancestorDepth, ancestorDepth);
        }

        private bool AreHeroesRelatedAux1(Hero firstHero, Hero secondHero, int ancestorDepth)
        {
            return firstHero == secondHero
                || (ancestorDepth > 0 && ((secondHero.Mother is not null && AreHeroesRelatedAux1(firstHero, secondHero.Mother, ancestorDepth - 1))
                    || (secondHero.Father is not null && AreHeroesRelatedAux1(firstHero, secondHero.Father, ancestorDepth - 1))));
        }

        private bool AreHeroesRelatedAux2(Hero firstHero, Hero secondHero, int ancestorDepth, int secondAncestorDepth)
        {
            return AreHeroesRelatedAux1(firstHero, secondHero, secondAncestorDepth)
                || (ancestorDepth > 0 && ((firstHero.Mother is not null && AreHeroesRelatedAux2(firstHero.Mother, secondHero, ancestorDepth - 1, secondAncestorDepth))
                    || (firstHero.Father is not null && AreHeroesRelatedAux2(firstHero.Father, secondHero, ancestorDepth - 1, secondAncestorDepth))));
        }

        public override int GetEffectiveRelationIncrease(Hero firstHero, Hero secondHero) => _previousModel?.GetEffectiveRelationIncrease(firstHero, secondHero) ?? default;

        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero) => _previousModel?.GetClanAfterMarriage(firstHero, secondHero) ?? Clan.PlayerClan;

        public override bool IsSuitableForMarriage(Hero hero) => _previousModel?.IsSuitableForMarriage(hero) ?? default;

        public override bool IsClanSuitableForMarriage(Clan clan) => _previousModel?.IsClanSuitableForMarriage(clan) ?? default;

        public override float NpcCoupleMarriageChance(Hero firstHero, Hero secondHero) => _previousModel?.NpcCoupleMarriageChance(firstHero, secondHero) ?? default;

        public override bool ShouldNpcMarriageBetweenClansBeAllowed(Clan consideringClan, Clan targetClan) => _previousModel?.ShouldNpcMarriageBetweenClansBeAllowed(consideringClan, targetClan) ?? default;

        public override List<Hero> GetAdultChildrenSuitableForMarriage(Hero hero) => _previousModel?.GetAdultChildrenSuitableForMarriage(hero) ?? new List<Hero>();

        public override int MinimumMarriageAgeMale => _previousModel?.MinimumMarriageAgeMale ?? 18;

        public override int MinimumMarriageAgeFemale => _previousModel?.MinimumMarriageAgeFemale ?? 18;
    }
}
