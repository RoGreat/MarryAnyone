//using HarmonyLib;
//using MarryAnyone.Settings;
//using TaleWorlds.CampaignSystem;
//using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

//namespace MarryAnyone.Patches
//{
//    [HarmonyPatch(typeof(RomanceCampaignBehavior), "RomanceCourtshipAttemptCooldown", MethodType.Getter)]
//    internal class RomanceCourtshipAttemptCooldownPatch
//    {
//        private static bool Prefix(CampaignTime __result)
//        {
//            ISettingsProvider settings = new MASettings();
//            __result = CampaignTime.DaysFromNow(-settings.CourtshipCooldown);
//            return false;
//        }
//    }
//}