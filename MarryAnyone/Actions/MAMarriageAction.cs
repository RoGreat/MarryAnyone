using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using System.Reflection;
using System.Collections.Generic;
using Helpers;
using static MarryAnyone.Debug;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
        private static readonly PropertyInfo? CampaignPlayerDefaultFaction = AccessTools2.Property(typeof(Campaign), "PlayerDefaultFaction");

        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);
            Clan clanAfterMarriage = GetClanAfterMarriage(firstHero, secondHero);

            if (firstHero.Clan == secondHero.Clan)
            {
                // Ignore clan merge if they are both from the same clan
                Print("Same clan");
            }
            else if (firstHero.Clan != clanAfterMarriage)
            {
                Clan clan = firstHero.Clan;
                firstHero.Clan = clanAfterMarriage;
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                        Print($"Updated settlement of {hero.Name}");
                    }
                }
                if (firstHero == Hero.MainHero)
                {
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, firstHero.Clan);
                    Print("Player hero assigned new default clan");
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
                        Print($"Updated settlement of {hero.Name}");
                    }
                }
                if (secondHero == Hero.MainHero)
                {
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, secondHero.Clan);
                    Print("Player hero assigned new default clan");
                }
                IFaction kingdom = clanAfterMarriage.Kingdom;
                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(secondHero, kingdom ?? clanAfterMarriage);
            }
            foreach (Hero hero in clanAfterMarriage.Heroes)
            {
                hero.UpdateHomeSettlement();
                Print($"Updated settlement of {hero.Name}");
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
            if (firstHero.Clan is not null)
            {
                heroes.Add(firstHero);
            }
            if (secondHero.Clan is not null)
            {
                heroes.Add(secondHero);
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