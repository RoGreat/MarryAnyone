using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(Location))]
    class LocationPatch
    {
        [HarmonyPatch(typeof(Location), "DeserializeDelegate", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        public static bool DeserializeDelegatePatchPrefix(string text, ref CanUseDoor? __result)
        {
            __result = null;

#if TRACELOCATION
            if (String.IsNullOrEmpty(text))
                MAHelper.Print(String.Format("LocationPatch PATCH [{0}]", text), MAHelper.PrintHow.PrintToLogAndWrite);
            else
                MAHelper.Print(String.Format("LocationPatch will be deserialize [{0}]", text), MAHelper.PrintHow.PrintToLogAndWrite);
#endif
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }
            return true;
        }

#if TRACELOCATION
        [HarmonyPatch(typeof(Location), "DeserializeDelegate", new Type[] { typeof(string) })]
        [HarmonyPostfix]
        public static void DeserializeDelegatePatchPostfix(string text)
        {
            MAHelper.Print(String.Format("LocationPatch has deserialized [{0}] FAIT", text), MAHelper.PrintHow.PrintToLogAndWrite);
        }
#endif

        [HarmonyPatch(typeof(Location), "CanAIExit", new Type[] { typeof(LocationCharacter) })]
        [HarmonyPrefix]
        public static bool CanAIExitPatchPrefix(Location __instance, LocationCharacter character, ref bool __result)
        {
            __result = false;
            if (character == null)
            {
                MAHelper.Print(String.Format("CanAIExit on {0} pour {1} PATH return FALSE", __instance.Name, character == null ? "character NULL" : character.ToString()), MAHelper.PrintHow.PrintToLogAndWrite);
                __result = false; // rien peux sortir de n'importe quoi !!!
                return false;
            }
            MAHelper.Print(String.Format("CanAIExit on {0} pour {1} VA FAIRE", __instance.Name, character == null ? "character NULL" : character.ToString()), MAHelper.PrintHow.PrintToLogAndWrite);
            return true;
        }
    }
}
