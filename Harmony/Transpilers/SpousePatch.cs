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

            int i;

            int j = 0;

            for (i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                var instruction2 = codes[j];

                yield return instruction;
                if (instruction.opcode == OpCodes.Brfalse_S
                    && codes[i + 2].operand is FieldInfo && (codes[i + 2].operand as FieldInfo) == AccessTools.Field(typeof(Hero), "_exSpouses"))
                {
                    j = i;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Hero), "_exSpouses"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Hero>), "Contains"));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instruction.operand);
                }
            }
        }
    }
}