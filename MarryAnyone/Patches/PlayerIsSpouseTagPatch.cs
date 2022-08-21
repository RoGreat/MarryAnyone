using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Tags;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PlayerIsSpouseTag), "IsApplicableTo")]
    internal sealed class PlayerIsSpouseTagPatch
    {
        private static void Postfix(ref bool __result, CharacterObject character)
        {
            // If addressing spouse then continue...
            if (__result)
            {
                return;
            }
            // If not true then also check if an exspouse exists as well...
            __result = character.IsHero && Hero.MainHero.ExSpouses.Contains(character.HeroObject);
        }
    }
}