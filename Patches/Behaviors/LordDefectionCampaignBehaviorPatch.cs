using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(LordDefectionCampaignBehavior))]
    class LordDefectionCampaignBehaviorPatch
    {
        [HarmonyPatch(typeof(LordDefectionCampaignBehavior), "conversation_player_is_asking_to_recruit_enemy_on_condition", new Type[] { })]
        [HarmonyPrefix]
        public static bool conversation_player_is_asking_to_recruit_enemy_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null
                || Hero.OneToOneConversationHero.Clan == null)
            {

                __result = false;
                return false; // Stop
            }
            return true;
        }

        [HarmonyPatch(typeof(LordDefectionCampaignBehavior), "conversation_player_is_asking_to_recruit_neutral_on_condition", new Type[] { })]
        [HarmonyPrefix]
        public static bool conversation_player_is_asking_to_recruit_neutral_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null
                || Hero.OneToOneConversationHero.Clan == null)
            {

                __result = false;
                return false; // Stop
            }
            return true;
        }

        [HarmonyPatch(typeof(LordDefectionCampaignBehavior), "conversation_suggest_treason_on_condition", new Type[] { })]
        [HarmonyPrefix]
        public static bool conversation_suggest_treason_on_conditionPatch(ref bool __result)
        {
            if (Hero.OneToOneConversationHero == null
                || Hero.OneToOneConversationHero.Clan == null)
            {

                __result = false;
                return false; // Stop
            }
            return true;
        }
    }
}
