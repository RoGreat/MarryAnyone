using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Models
{
    public class LordMarriageModel : MarriageModel
    {
        private readonly MarriageModel? _previousModel;

        public LordMarriageModel(MarriageModel? previousModel)
        {
            _previousModel = previousModel;
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero) => _previousModel?.IsCoupleSuitableForMarriage(firstHero, secondHero) ?? default;

        public override int GetEffectiveRelationIncrease(Hero firstHero, Hero secondHero) => _previousModel?.GetEffectiveRelationIncrease(firstHero, secondHero) ?? default;

        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero) => _previousModel?.GetClanAfterMarriage(firstHero, secondHero) ?? Clan.PlayerClan;

        public override bool IsSuitableForMarriage(Hero hero) => _previousModel?.IsSuitableForMarriage(hero) ?? default;

        public override bool IsClanSuitableForMarriage(Clan clan) => _previousModel?.IsClanSuitableForMarriage(clan) ?? default;

        public override float NpcCoupleMarriageChance(Hero firstHero, Hero secondHero) => _previousModel?.NpcCoupleMarriageChance(firstHero, secondHero) ?? 0f;

        public override bool ShouldNpcMarriageBetweenClansBeAllowed(Clan consideringClan, Clan targetClan) => _previousModel?.ShouldNpcMarriageBetweenClansBeAllowed(consideringClan, targetClan) ?? default;

        public override List<Hero> GetAdultChildrenSuitableForMarriage(Hero hero) => _previousModel?.GetAdultChildrenSuitableForMarriage(hero) ?? new List<Hero>();

        public override int MinimumMarriageAgeMale => _previousModel?.MinimumMarriageAgeMale ?? 18;

        public override int MinimumMarriageAgeFemale => _previousModel?.MinimumMarriageAgeFemale ?? 18;
    }
}
