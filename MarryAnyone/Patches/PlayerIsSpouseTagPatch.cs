using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Tags;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PlayerIsSpouseTag), "IsApplicableTo")]
    internal class PlayerIsSpouseTagPatch
    {
        private static void Postfix(ref bool __result, CharacterObject character)
        {
            if (__result)
            {
                return;
            }
            __result = character.IsHero && Hero.MainHero.ExSpouses.Contains(character.HeroObject);
        }
    }
}