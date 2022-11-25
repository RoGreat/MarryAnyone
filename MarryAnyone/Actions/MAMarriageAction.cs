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
            Clan playerClan = Clan.PlayerClan;

            // CampaignCheats -> lead_your_faction
            // CampaignCheats -> join_kingdom
            // ChangeOwnerOfSettlementAction -> ApplyByDefault
            Kingdom? playerKingdom = Hero.MainHero.MapFaction as Kingdom;
            Kingdom? spouseKingdom = spouse.MapFaction as Kingdom;

            // If spouse is in a kingdom
            if (spouseKingdom is not null)
            {
                // Not already in the same kingdom
                if (playerKingdom != spouseKingdom)
                {
                    if (spouseKingdom.RulingClan is not null)
                    {
                        // If spouse is the kingdom ruler 
                        if (spouseKingdom.RulingClan.Leader == spouse)
                        {
                            // Become or stay as kingdom ruler if these settings are true
                            if ((settings.FactionLeader == "Default" && !Hero.MainHero.IsFemale) || settings.FactionLeader == "Player")
                            {
                                // Spouse should always abdicate the throne in this situation
                                Campaign.Current.KingdomManager.AbdicateTheThrone(spouseKingdom);

                                if (playerKingdom is null)
                                {
                                    // When there is no player kingdom
                                    // Player joins spouse's kingdom and takes the throne
                                    ChangeKingdomAction.ApplyByJoinToKingdom(playerClan, spouseKingdom);
                                    spouseKingdom!.RulingClan = playerClan;
                                    CampaignEventDispatcher.Instance.OnRulingClanChanged(spouseKingdom, playerClan);
                                }
                                else if (spouseClan is not null)
                                {
                                    // When there is a player kingdom
                                    // Spouse joins player kingdom
                                    ChangeKingdomAction.ApplyByJoinToKingdom(spouseClan, playerKingdom);
                                }
                            }
                            else
                            {
                                // Player does not stay kingdom ruler
                                if (playerKingdom is not null)
                                {
                                    if (playerKingdom.RulingClan is not null)
                                    {
                                        // Abdicate throne and join spouse's kingdom if player clan is the ruling clan leader
                                        if (playerKingdom.RulingClan.Leader == Hero.MainHero)
                                        {
                                            Campaign.Current.KingdomManager.AbdicateTheThrone(playerKingdom);
                                        }
                                    }
                                }
                                ChangeKingdomAction.ApplyByJoinToKingdom(playerClan, spouseKingdom);
                            }
                        }
                        // For regular clan leaders. Solving the kingdom issue.
                        else if (spouseClan is not null)
                        {
                            // If talking to a clan leader
                            if (spouseClan.Leader == spouse)
                            {
                                // If there is a player kingdom
                                if (playerKingdom is not null)
                                {
                                    // Determines whether to join kingdom or stay in your kingdom and take spouse with you
                                    if ((settings.FactionLeader == "Default" && !Hero.MainHero.IsFemale) || settings.FactionLeader == "Player")
                                    {
                                        ChangeKingdomAction.ApplyByJoinToKingdom(spouseClan, playerKingdom);
                                    }
                                    else
                                    {
                                        if (playerKingdom.RulingClan is not null)
                                        {
                                            // Abdicate throne and join spouse's kingdom if player clan is the ruling clan leader
                                            if (playerKingdom.RulingClan.Leader == Hero.MainHero)
                                            {
                                                Campaign.Current.KingdomManager.AbdicateTheThrone(playerKingdom);
                                            }
                                        }
                                        ChangeKingdomAction.ApplyByJoinToKingdom(playerClan, spouseKingdom);
                                    }
                                }
                                else
                                {
                                    if ((settings.FactionLeader == "Default" && !Hero.MainHero.IsFemale) || settings.FactionLeader == "Player")
                                    {
                                        // "Elope"
                                    }
                                    else
                                    {
                                        // "JOIN US!"
                                        ChangeKingdomAction.ApplyByJoinToKingdom(playerClan, spouseKingdom);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Necessary to prevent you from getting kicked out kingdoms and breaking the game
            if (spouseClan is not null)
            {
                if (spouseClan.Leader == spouse)
                {
                    // Spouse clan must choose a new clan leader or there is a 40% chance on daily tick that you get removed from the kingdom...
                    // Referring to DiplomaticBartersBehavior -> ConsiderClanLeaveKingdom
                    ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouseClan);
                }
            }

            // Cautious marriage action
            if (spouse.Clan != playerClan)
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
                        if (spouseClan.Kingdom != playerClan.Kingdom)
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
                            IFaction kingdom = playerClan.Kingdom;
                            FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(spouse, kingdom ?? playerClan);
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
                spouse.Clan = playerClan;
                if (spouseClan is not null)
                {
                    foreach (Hero hero in spouseClan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in playerClan.Heroes)
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