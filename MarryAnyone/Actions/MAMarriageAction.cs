using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using Helpers;
using TaleWorlds.CampaignSystem.Party;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
        /* Properties */
        //private delegate void PlayerDefaultFactionDelegate(Campaign instance, Clan @value);
        //private static readonly PlayerDefaultFactionDelegate? PlayerDefaultFaction = AccessTools2.GetPropertySetterDelegate<PlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        // Appears to ultimately avoid disbanding parties and the like...
        // Never disband party for hero, do for everyone else...
        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            MASettings settings = new();
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);

            // Need to lay down the law on marrying into clans:
            // - Player needs to stay in the default clan
            //   - Player should always be the default clan leader
            //     (current approach leads to instability)
            // - Can join a kingdom after marriage
            // - Can leave a kingdom after marriage

            // So spouse joins player clan and...
            // A: Player clan "leads" new/current faction or
            // B: Player clan stays as current faction leader

            Hero spouse = !firstHero.IsHumanPlayerCharacter ? firstHero : secondHero;
            Clan spouseClan = spouse.Clan;
            Clan clanAfterMarriage = Clan.PlayerClan;

            // CampaignCheats -> lead_your_faction
            // CampaignCheats -> join_kingdom
            // ChangeOwnerOfSettlementAction -> ApplyByDefault
            var playerKingdom = Hero.MainHero.MapFaction as Kingdom;
            var spouseKingdom = spouse.MapFaction as Kingdom;
            if (playerKingdom is null)
            {
                if (spouseKingdom is not null)
                {
                    if (Hero.MainHero.MapFaction as Kingdom != spouseKingdom)
                    {
                        ChangeKingdomAction.ApplyByJoinToKingdom(clanAfterMarriage, spouseKingdom, true);
                    }
                    if ((settings.FactionLeader == "Default" && !Hero.MainHero.IsFemale)
                            || settings.FactionLeader == "Player")
                    {
                        spouseKingdom!.RulingClan = clanAfterMarriage;
                        CampaignEventDispatcher.Instance.OnRulingClanChanged(spouseKingdom, clanAfterMarriage);
                    }
                }
            }
            else
            {
                if (spouseClan is not null)
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(spouseClan, playerKingdom, true);
                }
            }

            // Cautious marriage action
            if (spouse.Clan != clanAfterMarriage)
            {
                if (spouse.GovernorOf is not null)
                {
                    ChangeGovernorAction.RemoveGovernorOf(spouse);
                }
                if (spouse.PartyBelongedTo is not null)
                {
                    MobileParty partyBelongedTo = spouse.PartyBelongedTo;
                    if (spouseClan is not null)
                    {
                        if (spouseClan.Kingdom != clanAfterMarriage.Kingdom)
                        {
                            if (spouse.PartyBelongedTo.Army is not null)
                            {
                                if (spouse.PartyBelongedTo.Army.LeaderParty == spouse.PartyBelongedTo)
                                {
                                    spouse.PartyBelongedTo.Army.DisperseArmy(Army.ArmyDispersionReason.Unknown);
                                }
                                else
                                {
                                    spouse.PartyBelongedTo.Army = null;
                                }
                            }
                            IFaction kingdom = clanAfterMarriage.Kingdom;
                            FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(spouse, kingdom ?? clanAfterMarriage);
                        }
                    }
                    if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == spouse)
                    {
                        DisbandPartyAction.StartDisband(partyBelongedTo);
                        partyBelongedTo.Party.SetCustomOwner(null);
                    }
                    spouse.ChangeState(Hero.CharacterStates.Fugitive);
                    MobileParty partyBelongedTo2 = spouse.PartyBelongedTo;
                    if (partyBelongedTo2 is not null)
                    {
                        partyBelongedTo2.MemberRoster.RemoveTroop(spouse.CharacterObject, 1, default, 0);
                    }
                }
                spouse.Clan = clanAfterMarriage;
                if (spouseClan is not null)
                {
                    foreach (Hero hero in spouseClan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }

            EndAllCourtshipsPatch.EndAllCourtships(firstHero);
            EndAllCourtshipsPatch.EndAllCourtships(secondHero);
            ChangeRomanticStateAction.Apply(firstHero, secondHero, Romance.RomanceLevelEnum.Marriage);
            CampaignEventDispatcher.Instance.OnHeroesMarried(firstHero, secondHero, showNotification);
        }

        public static void Apply(Hero firstHero, Hero secondHero, bool showNotification = true)
        {
            ApplyInternal(firstHero, secondHero, showNotification);
        }
    }
}