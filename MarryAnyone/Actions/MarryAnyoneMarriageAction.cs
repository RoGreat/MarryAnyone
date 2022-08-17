using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace MarryAnyone.Actions
{
    internal static class MarryAnyoneMarriageAction
    {
        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);
            // Whoever's clan exists lol
            Clan clanAfterMarriage = firstHero.Clan ?? secondHero.Clan;
            if (firstHero.Clan != clanAfterMarriage)
            {
                firstHero.Clan = clanAfterMarriage;
                firstHero.UpdateHomeSettlement();
                firstHero.SetNewOccupation(Occupation.Lord);
            }
            else
            {
                secondHero.Clan = clanAfterMarriage;
                secondHero.UpdateHomeSettlement();
                secondHero.SetNewOccupation(Occupation.Lord);
            }
            // Romance.EndAllCourtships(firstHero);
            AccessTools.Method(typeof(Romance), "EndAllCourtships").Invoke(null, new object[] { firstHero });
            // Romance.EndAllCourtships(secondHero);
            AccessTools.Method(typeof(Romance), "EndAllCourtships").Invoke(null, new object[] { secondHero });
            ChangeRomanticStateAction.Apply(firstHero, secondHero, Romance.RomanceLevelEnum.Marriage);
            CampaignEventDispatcher.Instance.OnHeroesMarried(firstHero, secondHero, showNotification);
        }

        public static void Apply(Hero firstHero, Hero secondHero, bool showNotification = true)
        {
            ApplyInternal(firstHero, secondHero, showNotification);
        }
    }
}