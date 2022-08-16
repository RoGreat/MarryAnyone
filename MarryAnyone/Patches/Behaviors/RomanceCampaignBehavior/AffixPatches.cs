using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    internal partial class RomanceCampaignBehaviorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
        private static void Postfix1(ref bool __result, object __instance)
        {
            __result = Hero.OneToOneConversationHero is not null 
                && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) is null 
                && MarriageCourtshipPossibilityPatch(__instance, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        [HarmonyPostfix]
        [HarmonyPatch("conversation_player_eligible_for_marriage_with_hero_rltv_on_condition")]
        private static void Postfix2(ref bool __result)
        {
            __result = Hero.OneToOneConversationHero is not null;
        }

        // Would really like to make this better...
        [HarmonyPostfix]
        [HarmonyPatch("RomanceCourtshipAttemptCooldown", MethodType.Getter)]
        private static void Postfix3(ref CampaignTime __result)
        {
            MASettings settings = new();
            if (settings.RetryCourtship)
            {
                __result = CampaignTime.DaysFromNow(1f);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        private static bool Prefix1(ref bool __result)
        {
            if (CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && !Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}