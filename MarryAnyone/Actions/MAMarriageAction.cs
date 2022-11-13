using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using Helpers;
using TaleWorlds.CampaignSystem.Party;
using System;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
        /* Properties */
        private delegate void PlayerDefaultFactionDelegate(Campaign instance, Clan @value);
        private static readonly PlayerDefaultFactionDelegate? PlayerDefaultFaction = AccessTools2.GetPropertySetterDelegate<PlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        // Appears to ultimately avoid disbanding parties and the like...
        // Never disband party for hero, do for everyone else...
        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            MASettings settings = new();
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);

            Clan clanAfterMarriage = Hero.MainHero.Clan;

            // Need to lay down the law on marrying into clans:
            // - Player needs to stay in the default clan
            //   - Player should always be the default clan leader
            //     (current approach leads to instability)
            // - Can join a kingdom after marriage
            // - Can leave a kingdom after marriage

            if (settings.ClanAfterMarriage == "Spouse")
            {
                // Only time it is OK for the player to stay in clan
                if (firstHero == Hero.MainHero && secondHero.Clan is null)
                {
                    clanAfterMarriage = firstHero.Clan;
                }
                else
                {
                    clanAfterMarriage = secondHero.Clan;
                }
            }

            // Cautious marriage action
            Clan clan1 = firstHero.Clan;
            Clan clan2 = secondHero.Clan;
            if (firstHero.Clan != clanAfterMarriage)
            {
                if (firstHero != Hero.MainHero)
                {
                    if (firstHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(firstHero);
                    }
                    if (firstHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = firstHero.PartyBelongedTo;
                        if (clan1 is not null)
                        {
                            if (clan1.Kingdom != clanAfterMarriage.Kingdom)
                            {
                                if (firstHero.PartyBelongedTo.Army is not null)
                                {
                                    if (firstHero.PartyBelongedTo.Army.LeaderParty == firstHero.PartyBelongedTo)
                                    {
                                        firstHero.PartyBelongedTo.Army.DisperseArmy(Army.ArmyDispersionReason.Unknown);
                                    }
                                    else
                                    {
                                        firstHero.PartyBelongedTo.Army = null;
                                    }
                                }
                                IFaction kingdom = clanAfterMarriage.Kingdom;
                                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(firstHero, kingdom ?? clanAfterMarriage);
                            }
                        }
                        if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == firstHero)
                        {
                            DisbandPartyAction.StartDisband(partyBelongedTo);
                            partyBelongedTo.Party.SetCustomOwner(null);
                        }
                        firstHero.ChangeState(Hero.CharacterStates.Fugitive);
                        MobileParty partyBelongedTo2 = firstHero.PartyBelongedTo;
                        if (partyBelongedTo2 is not null)
                        {
                            partyBelongedTo2.MemberRoster.RemoveTroop(firstHero.CharacterObject, 1, default, 0);
                        }
                    }
                }
                firstHero.Clan = clanAfterMarriage;
                if (clan1 is not null)
                {
                    foreach (Hero hero in clan1.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            else if (secondHero.Clan != clanAfterMarriage)
            {
                if (secondHero != Hero.MainHero)
                {
                    if (secondHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(secondHero);
                    }
                    if (secondHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = secondHero.PartyBelongedTo;
                        if (clan2 is not null)
                        {
                            if (clan2.Kingdom != clanAfterMarriage.Kingdom)
                            {
                                if (secondHero.PartyBelongedTo.Army is not null)
                                {
                                    if (secondHero.PartyBelongedTo.Army.LeaderParty == secondHero.PartyBelongedTo)
                                    {
                                        secondHero.PartyBelongedTo.Army.DisperseArmy(Army.ArmyDispersionReason.Unknown);
                                    }
                                    else
                                    {
                                        secondHero.PartyBelongedTo.Army = null;
                                    }
                                }
                                IFaction kingdom = clanAfterMarriage.Kingdom;
                                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(secondHero, kingdom ?? clanAfterMarriage);
                            }
                        }
                        if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == secondHero)
                        {
                            DisbandPartyAction.StartDisband(partyBelongedTo);
                            partyBelongedTo.Party.SetCustomOwner(null);
                        }
                        secondHero.ChangeState(Hero.CharacterStates.Fugitive);
                        MobileParty partyBelongedTo2 = secondHero.PartyBelongedTo;
                        if (partyBelongedTo2 is not null)
                        {
                            partyBelongedTo2.MemberRoster.RemoveTroop(secondHero.CharacterObject, 1, default, 0);
                        }
                    }
                }
                secondHero.Clan = clanAfterMarriage;
                if (clan2 is not null)
                {
                    foreach (Hero hero in clan2.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            // CampaignCheats -> lead_your_faction
            //if (Hero.MainHero.MapFaction.Leader != Hero.MainHero)
            //{
            //    if (Hero.MainHero.MapFaction.IsKingdomFaction)
            //    {
            //        // ChangeRulingClanAction.Apply(Hero.MainHero.MapFaction as Kingdom, Clan.PlayerClan);
            //        // Breaking it down:
            //        // 	    kingdom.RulingClan = clan;
            //        //      CampaignEventDispatcher.Instance.OnRulingClanChanged(kingdom, clan);
            //        var clan = Clan.PlayerClan;
            //        (Hero.MainHero.MapFaction as Kingdom).RulingClan = clan;
            //        CampaignEventDispatcher.Instance.OnRulingClanChanged(Hero.MainHero.MapFaction as Kingdom, clan);
            //    }
            //    else
            //    {
            //        (Hero.MainHero.MapFaction as Clan).SetLeader(Hero.MainHero);
            //    }
            //}
            // Romance.EndAllCourtships(firstHero);
            EndAllCourtshipsPatch.EndAllCourtships(firstHero);
            // Romance.EndAllCourtships(secondHero);
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