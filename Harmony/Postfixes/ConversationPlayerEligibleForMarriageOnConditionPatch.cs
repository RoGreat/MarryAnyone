using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_player_eligible_for_marriage_on_condition")]
    internal class ConversationPlayerEligibleForMarriageOnConditionPatch
    {
        private static void Postfix(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero != null && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) == null && Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero);
        }
    }
}