using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
    public static class CreateNewHeroPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();
            for (var i = 0; i < instructionsList.Count; i++)
            {
                var instruction = instructionsList[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Brfalse_S
                    && instructionsList[i-1].operand is MethodInfo && (instructionsList[i-1].operand as MethodInfo) == AccessTools.PropertyGetter(typeof(Hero), nameof(Hero.IsWanderer)))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Hero), "Spouse"));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instruction.operand);
                }
            }
        }
    }
}