using HarmonyLib;
using MarryAnyone.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior), "RomanceCourtshipAttemptCooldown", MethodType.Getter)]
    internal class RomanceCourtshipAttemptCooldownPatch
    {
        // Cooldown has to be greater than the last time persuasion was done
        // Just set it to a day ahead to try again
        private static void Postfix(ref CampaignTime __result)
        {
            ISettingsProvider settings = new MASettings();
            if (settings.RetryCourtship)
            {
                __result = CampaignTime.DaysFromNow(1f);
            }
        }
    }
}