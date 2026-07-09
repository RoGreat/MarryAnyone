using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

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
            switch (hero.Occupation)
            {
                case Occupation.ArenaMaster:
                    break;
                case Occupation.Armorer:
                    break;
                case Occupation.Artisan:
                    break;
                case Occupation.Blacksmith:
                    break;
                case Occupation.GangLeader:
                    break;
                case Occupation.Gangster:
                    break;
                case Occupation.GoodsTrader:
                    break;
                case Occupation.Guard:
                    break;
                case Occupation.Headman:
                    break;
                case Occupation.HorseTrader:
                    break;
                case Occupation.Mercenary:
                    break;
                case Occupation.Merchant:
                    break;
                case Occupation.Musician:
                    break;
                case Occupation.Preacher:
                    break;
                case Occupation.PrisonGuard:
                    break;
                case Occupation.RansomBroker:
                    return settlement.LocationComplex.GetLocationWithId("tavern");
                case Occupation.ShipWright:
                    break;
                case Occupation.ShopWorker:
                    break;
                case Occupation.Soldier:
                    break;
                case Occupation.Special:
                    break;
                case Occupation.TavernGameHost:
                    return settlement.LocationComplex.GetLocationWithId("tavern");
                case Occupation.Tavernkeeper:
                    return settlement.LocationComplex.GetLocationWithId("tavern");
                case Occupation.TavernWench:
                    return settlement.LocationComplex.GetLocationWithId("tavern");
                case Occupation.Townsfolk:
                    List<string> locationIdArray = new() { "center", "tavern" };
                    string randomLocationId = locationIdArray[MBRandom.RandomInt(0, 1)];
                    return settlement.LocationComplex.GetLocationWithId(randomLocationId);
                case Occupation.Wanderer:
                    return settlement.LocationComplex.GetLocationWithId("tavern");
                case Occupation.Weaponsmith:
                    return settlement.LocationComplex.GetLocationWithId("center");
            }
            return _previousModel?.GetLocationForHero(hero, settlement, out heroLocationDetail) ?? null!;
        }

        public override bool WillBeListedInOverlay(LocationCharacter locationCharacter) => _previousModel?.WillBeListedInOverlay(locationCharacter) ?? default;
    }
}
