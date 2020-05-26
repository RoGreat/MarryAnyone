using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(DefaultEncyclopediaHeroPage), "GetListItems")]
    internal static class GetListItemsPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int startIndex = -1, endIndex = -1;
            bool nextState = false;

            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {

                if (!nextState && codes[i].opcode == OpCodes.Callvirt
                    && codes[i].operand is MethodInfo && (codes[i].operand as MethodInfo) == AccessTools.PropertyGetter(typeof(Hero), "IsNotable"))
                {
                    startIndex = i;
                    nextState = true;
                }
                if (nextState && codes[i].opcode == OpCodes.Blt_Un_S)
                {
                    endIndex = i;
                    break;
                }
            }
            if (startIndex > -1 && endIndex > -1)
            {
                codes[startIndex - 1].opcode = OpCodes.Nop;
                codes.RemoveRange(startIndex, endIndex - startIndex + 1);
            }
            return codes.AsEnumerable();
        }
    }
}