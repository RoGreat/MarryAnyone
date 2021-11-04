using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SandBox.Source.CampaignComponents;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone.Patches.Behaviors
{
#if V1640MORE
    [HarmonyPatch(typeof(CompanionRolesCampaignBehavior))]
    internal static class SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch
    {

        private static bool IsChild(Hero child, Hero parent)
        {
            if (child.Father == parent || child.Mother == parent)
                return true;

            if (parent.Spouse != null && (child.Father == parent.Spouse || child.Mother == parent.Spouse))
                return true;

            foreach (Hero spouse in parent.ExSpouses)
            {
                if (child.Father == spouse || child.Mother == spouse)
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(CompanionRolesCampaignBehavior), "turn_companion_to_lord_on_condition")]
        [HarmonyPrefix]
        public static bool turn_companion_to_lord_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero != null && Hero.MainHero.MapFaction.IsKingdomFaction && Hero.MainHero.IsFactionLeader && Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan)
            {
                __result = false;
                if (IsChild(Hero.OneToOneConversationHero, Hero.MainHero))
                    __result = true;
                else if (Hero.MainHero.Father != null && IsChild(Hero.OneToOneConversationHero, Hero.MainHero.Father))
                    __result = true;
                else if (Hero.MainHero.Mother != null && IsChild(Hero.OneToOneConversationHero, Hero.MainHero.Mother))
                    __result = true;
#if TRACECREATECLAN
                MAHelper.Print(String.Format("turn_companion_to_lord_on_conditionPatch __result ?= {0}", __result.ToString()), MAHelper.PRINT_TRACE_CREATE_CLAN);
#endif

                return false;
            }
            return true;
        }
    }
#endif
            }
