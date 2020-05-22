using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(CampaignUIHelper), "GetHeroRelationToHeroTextShort")]
    public static class GetHeroRelationToHeroTextShortPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "str_exspouse")
                {
                    codes[i].operand = "str_spouse";
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}