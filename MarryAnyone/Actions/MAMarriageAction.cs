using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using System.Reflection;

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

            // Ignore if they are both from the same clan
            if (firstHero.Clan == secondHero.Clan)
            {
            }
            // Commoners without a clan
            // Evaluated first before lord
            else if (firstHero.Clan is null)
            {
                firstHero.Clan = secondHero.Clan;
                firstHero.UpdateHomeSettlement();
            }
            else if (secondHero.Clan is null)
            {
                secondHero.Clan = firstHero.Clan;
                secondHero.UpdateHomeSettlement();
            }
            // Noble check with the main player
            // Main player has issues with marrying into kingdoms
            else if (firstHero == Hero.MainHero)
            {
                IFaction kingdom1 = firstHero.Clan?.Kingdom ?? null!;
                // If player character has no kingdom join other kingdom
                if (kingdom1 is null)
                {
                    firstHero.Clan = secondHero.Clan;
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, firstHero.Clan);
                    firstHero.UpdateHomeSettlement();
                }
                // If player does have a kingdom then spouse joins their kingdom
                // According to Bannerlord logic
                else
                {
                    secondHero.Clan = firstHero.Clan;
                    secondHero.UpdateHomeSettlement();
                }
            }
            else if (secondHero == Hero.MainHero)
            {
                IFaction kingdom2 = secondHero.Clan?.Kingdom ?? null!;
                if (kingdom2 is null)
                {
                    secondHero.Clan = firstHero.Clan;
                    CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, secondHero.Clan);
                    secondHero.UpdateHomeSettlement();
                }
                else
                {
                    firstHero.Clan = secondHero.Clan;
                    firstHero.UpdateHomeSettlement();
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
    }
}