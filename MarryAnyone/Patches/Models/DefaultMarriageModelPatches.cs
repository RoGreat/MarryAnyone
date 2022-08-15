using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(DefaultMarriageModel))]
    internal class DefaultMarriageModelPatches
    {
        // Private Method
        [HarmonyReversePatch]
        [HarmonyPatch("DiscoverAncestors")]
        public static IEnumerable<Hero> DiscoverAncestors(object instance, Hero hero, int n)
        {
            throw new NotImplementedException();
        }
    }
}