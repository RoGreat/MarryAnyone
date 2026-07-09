using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace MarryAnyone.Models
{
    public class MarryAnyoneHeroAgentLocationModel : HeroAgentLocationModel
    {
        private readonly HeroAgentLocationModel? _previousModel;

        public MarryAnyoneHeroAgentLocationModel(HeroAgentLocationModel? previousModel)
        {
            _previousModel = previousModel;
        }

        public override Location GetLocationForHero(Hero hero, Settlement settlement, out HeroLocationDetail heroLocationDetail)
        {
            heroLocationDetail = HeroLocationDetail.None;
            if (hero.Occupation == Occupation.Special)
            {
                heroLocationDetail = HeroLocationDetail.Wanderer;
                return settlement.LocationComplex.GetLocationWithId("tavern");
            }
            return _previousModel?.GetLocationForHero(hero, settlement, out heroLocationDetail) ?? null!;
        }

        public override bool WillBeListedInOverlay(LocationCharacter locationCharacter) => _previousModel?.WillBeListedInOverlay(locationCharacter) ?? default;
    }
}
