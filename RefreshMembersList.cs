using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(ClanMembersVM), "RefreshMembersList")]
    public static class RefreshMembersListPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Callvirt
                    && codes[i].operand is MethodInfo && (codes[i].operand as MethodInfo) == AccessTools.PropertyGetter(typeof(Clan), nameof(Clan.Nobles)))
                {
                    codes[i].operand = AccessTools.PropertyGetter(typeof(Clan), nameof(Clan.Heroes));
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}