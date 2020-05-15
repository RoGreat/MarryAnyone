using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(PlayerEncounter), nameof(PlayerEncounter.LeaveEncounter))]
    public static class LeaveEncounterTranspilerPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0)
                {
                    /*
                    instruction.opcode = OpCodes.Nop;
                    yield return new CodeInstruction(OpCodes.Call, typeof(Hero).GetMethod("OneToOneConversationHero"));
                    yield return new CodeInstruction(OpCodes.Brfalse_S);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(bool));
                    yield return new CodeInstruction(OpCodes.Call, typeof(Hero).GetMethod("IsPlayerCompanion"));
                    yield return new CodeInstruction(OpCodes.Brfalse_S);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    */
                }
            }
            return codes.AsEnumerable();
        }
    }
}