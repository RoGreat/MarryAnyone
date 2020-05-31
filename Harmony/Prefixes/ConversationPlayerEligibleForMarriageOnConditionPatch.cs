using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_player_eligible_for_marriage_on_condition")]
    internal class ConversationPlayerEligibleForMarriageOnConditionPatch
    {
        private static bool Prefix(ref bool __result)
        {
            MAConfig config = MASubModule.Config;
            __result = Hero.OneToOneConversationHero != null && Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) != null;
            return false;
        }
    }
}