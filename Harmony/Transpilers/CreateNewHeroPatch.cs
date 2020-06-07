using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
    internal static class CreateNewHeroPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Brfalse_S
                    && codes[i - 1].operand is MethodInfo && (codes[i - 1].operand as MethodInfo) == AccessTools.PropertyGetter(typeof(Hero), "IsWanderer"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Hero), "Spouse"));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instruction.operand);
                }
            }
        }
    }
}