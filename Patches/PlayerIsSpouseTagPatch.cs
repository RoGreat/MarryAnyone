using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Tags;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PlayerIsSpouseTag), "IsApplicableTo")]
    internal class PlayerIsSpouseTagPatch
    {
        [HarmonyPatch(typeof(PlayerIsSpouseTag), "IsApplicableTo", new Type[] { typeof(CharacterObject) } )]
        [HarmonyPostfix]
        private static void PlayerIsSpouseTagIsApplicableTo(ref bool __result, CharacterObject character)
        {
            if (__result)
            {
                return;
            }
            __result = character.IsHero && Hero.MainHero.ExSpouses.Contains(character.HeroObject);
        }
    }
}