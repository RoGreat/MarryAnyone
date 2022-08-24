using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Party;
using static MarryAnyone.Debug;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
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

            // Many ways to get what clan to marry now
            Clan clanAfterMarriage;
            if (settings.PlayerClan == "Always")
            {
                if (firstHero == Hero.MainHero)
                {
                    clanAfterMarriage = firstHero.Clan;
                }
                else
                {
                    clanAfterMarriage = secondHero.Clan;
                }
            }
            else if (settings.PlayerClan == "Never")
            {
                if (firstHero == Hero.MainHero)
                {
                    clanAfterMarriage = secondHero.Clan;
                }
                else
                {
                    clanAfterMarriage = firstHero.Clan;
                }
            }
            else
            {
                clanAfterMarriage = GetClanAfterMarriage(firstHero, secondHero);
            }

            // Cautious marriage action
            if (firstHero.Clan != clanAfterMarriage)
            {
                Clan clan = firstHero.Clan;
                if (firstHero != Hero.MainHero)
                {
                    if (firstHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(firstHero);
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
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
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
                Clan clan = secondHero.Clan;
                if (secondHero != Hero.MainHero)
                {
                    if (secondHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(secondHero);
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
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            // For settings
            if (firstHero == Hero.MainHero)
            {
                PlayerDefaultFaction!(Campaign.Current, firstHero.Clan);
                // It was not a bug, it was an not implemented feature! Until now...
                Print("Player hero's default clan reassigned");
                if (((settings.ClanLeader == "Default" && !firstHero.IsFemale) || settings.ClanLeader == "Always")
                    && clanAfterMarriage.Leader == secondHero)
                {
                    Print("Player hero is the new clan leader");
                    ChangeClanLeaderAction.ApplyWithSelectedNewLeader(clanAfterMarriage, firstHero);
                }
            }
            if (secondHero == Hero.MainHero)
            {
                PlayerDefaultFaction!(Campaign.Current, secondHero.Clan);
                Print("Player hero's default clan reassigned");
                if (((settings.ClanLeader == "Default" && !secondHero.IsFemale) || settings.ClanLeader == "Always")
                    && clanAfterMarriage.Leader == firstHero)
                {
                    Print("Player hero is the new clan leader");
                    ChangeClanLeaderAction.ApplyWithSelectedNewLeader(clanAfterMarriage, secondHero);
                }
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
            // Should be male inheritance by default
            // Still want to prioritize the main hero if at all possible
            // So !IsFemale > MainHero when the clan exists
            int rank1 = ClanOrder(firstHero);
            int rank2 = ClanOrder(secondHero);
            if (rank1 >= rank2)
            {
                if (firstHero.Clan is not null)
                {
                    heroes.Add(firstHero);
                }
                if (secondHero.Clan is not null)
                {
                    heroes.Add(secondHero);
                }
            }
            else
            {
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

        private static int ClanOrder(Hero hero)
        {
            // Male hero
            if (!hero.IsFemale)
            {
                // Main hero 1st priority
                if (hero == Hero.MainHero)
                {
                    return 3;
                }
                // NPC 2nd priority
                return 2;
            }
            // Female hero
            else
            {
                // Main hero 3rd priority
                if (hero == Hero.MainHero)
                {
                    return 1;
                }
                // NPC 4th priority
                return 0;
            }
        }
    }
}