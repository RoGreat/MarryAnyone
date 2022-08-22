using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static MarryAnyone.Helpers;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(MarriageAction), "Apply")]
    internal sealed class MarriageActionPatch
    {
        private static readonly PropertyInfo? CampaignPlayerDefaultFaction = AccessTools2.Property(typeof(Campaign), "PlayerDefaultFaction");

        private static void Prefix(Hero firstHero, Hero secondHero)
        {
            IFaction kingdomAfterMarriage = firstHero.Clan.Kingdom ?? secondHero.Clan.Kingdom;
            if (kingdomAfterMarriage is not null)
            {
                if (firstHero.Clan.Kingdom is null)
                {
                    firstHero.Clan = secondHero.Clan;
                    if (firstHero == Hero.MainHero)
                    {
                        CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, secondHero.Clan);
                    }
                }
                else if (secondHero.Clan.Kingdom is null)
                {
                    secondHero.Clan = firstHero.Clan;
                    if (secondHero == Hero.MainHero)
                    {
                        CampaignPlayerDefaultFaction!.SetValue(Campaign.Current, firstHero.Clan);
                    }
                }
            }
        }

        private static void Postfix()
        {
            MASettings settings = new();
            Hero spouseHero = Hero.OneToOneConversationHero;
            // Do NOT break off marriages if polygamy is on...
            if (settings.Cheating && !settings.Polygamy)
            {
                CheatOnSpouse();
            }
            RemoveExSpouses(Hero.MainHero);
            RemoveExSpouses(spouseHero);
        }
    }
}