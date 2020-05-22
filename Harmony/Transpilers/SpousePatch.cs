using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(Hero), "Spouse", MethodType.Setter)]
    public static class SpousePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Ldloc_0
                    && codes[i-1].operand is FieldInfo && (codes[i-1].operand as FieldInfo) == AccessTools.Field(typeof(Hero), "_spouse"))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Hero), "IsAlive"));
                }
            }
        }
    }
}