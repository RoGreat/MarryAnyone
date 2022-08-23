using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using System.Reflection;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using static MarryAnyone.Debug;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
        private static readonly PropertyInfo? CampaignPlayerDefaultFaction = AccessTools2.Property(typeof(Campaign), "PlayerDefaultFaction");

        // Appears to ultimately avoid disbanding parties and the like...
        // Never disband party for hero, do for everyone else...
        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);

            if (firstHero.Clan == secondHero.Clan)
            {
                // Ignore clan merge if they are both from the same clan
                Print("Same clan");
            }

            Clan clanAfterMarriage = GetClanAfterMarriage(firstHero, secondHero);
            if (firstHero.Clan != clanAfterMarriage)
            {
                Clan clan = firstHero.Clan;
                firstHero.Clan = clanAfterMarriage;
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                        Print($"Updated home settlement of {hero.Name}");
                    }
                }
                if (firstHero == Hero.MainHero)
                {
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, firstHero.Clan);
                    Print("Player hero assigned new default clan");
                }
                else
                {
                    if (firstHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(firstHero);
                        Print($"{firstHero.Name} removed as governor");
                    }
                    if (firstHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = firstHero.PartyBelongedTo;
                        if (clan is not null)
                        {
                            if (clan.Kingdom != clanAfterMarriage.Kingdom)
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
                            partyBelongedTo2.MemberRoster.RemoveTroop(firstHero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
                        }
                    }
                }
                IFaction kingdom = clanAfterMarriage.Kingdom;
                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(firstHero, kingdom ?? clanAfterMarriage);
            }
            else if (secondHero.Clan != clanAfterMarriage)
            {
                Clan clan = secondHero.Clan;
                secondHero.Clan = clanAfterMarriage;
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                        Print($"Updated home settlement of {hero.Name}");
                    }
                }
                if (secondHero == Hero.MainHero)
                {
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, secondHero.Clan);
                    Print("Player hero assigned new default clan");
                }
                else
                {
                    if (secondHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(secondHero);
                        Print($"{secondHero.Name} removed as governor");
                    }
                    if (secondHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = secondHero.PartyBelongedTo;
                        if (clan is not null)
                        {
                            if (clan.Kingdom != clanAfterMarriage.Kingdom)
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
                            partyBelongedTo2.MemberRoster.RemoveTroop(secondHero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
                        }
                    }
                }
                IFaction kingdom = clanAfterMarriage.Kingdom;
                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(secondHero, kingdom ?? clanAfterMarriage);
            }
            foreach (Hero hero in clanAfterMarriage.Heroes)
            {
                hero.UpdateHomeSettlement();
                Print($"Updated home settlement of {hero.Name}");
            }

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

        // Borrowed from Family Tree
        private static Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero)
        {
            // Heroes list
            List<Hero> heroes = new();
            // Still want to prioritize the main hero if at all possible
            if (firstHero == Hero.MainHero)
            {
                // Add main hero first
                if (firstHero.Clan is not null)
                {
                    heroes.Add(firstHero);
                }
                if (secondHero.Clan is not null)
                {
                    heroes.Add(secondHero);
                }
            }
            else if (secondHero == Hero.MainHero)
            {
                // Add main hero first
                if (secondHero.Clan is not null)
                {
                    heroes.Add(secondHero);
                }
                if (firstHero.Clan is not null)
                {
                    heroes.Add(firstHero);
                }
            }
            // Kingdom Ruling Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Kingdom?.Leader == hero)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Ruling Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Kingdom?.RulingClan == hero.Clan)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsKingdomFaction && hero.IsFactionLeader)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsKingdomFaction)
                {
                    return hero.Clan;
                }
            }
            // Minor Faction Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsMinorFaction && hero.IsFactionLeader)
                {
                    return hero.Clan;
                }
            }
            // Minor Faction Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsMinorFaction)
                {
                    return hero.Clan;
                }
            }
            // Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Leader == hero)
                {
                    return hero.Clan;
                }
            }
            // Other
            foreach (Hero hero in heroes)
            {
                return hero.Clan;
            }
            return null!;
        }
    }
}