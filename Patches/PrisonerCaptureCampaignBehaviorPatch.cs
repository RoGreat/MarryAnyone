using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(PrisonerCaptureCampaignBehavior))]

    internal class PrisonerCaptureCampaignBehaviorPatch
    {
        [HarmonyPatch(typeof(PrisonerCaptureCampaignBehavior), "HandleSettlementHeroes", new Type[] { typeof(Settlement) })]
        [HarmonyPrefix]
        public static bool PrisonerCaptureCampaignBehaviorPatch_HandleSettlementHeroes(Settlement settlement)
        {
            if (settlement.HeroesWithoutParty.Count > 0)
            {
                //Helper.Print("Boucle 0 Commence", Helper.PrintHow.PrintToLogAndWrite);
                //foreach (Hero hero in settlement.HeroesWithoutParty)
                //{
                //    Helper.Print(String.Format("TRACE Hero {0} CurrentSettlement ?= {1}", hero.Name, hero.CurrentSettlement == null ? "NULL" : hero.CurrentSettlement.Name), Helper.PrintHow.PrintToLogAndWrite);
                //}
                //Helper.Print("Boucle 1 Commence", Helper.PrintHow.PrintToLogAndWrite);
                foreach (Hero hero in settlement.HeroesWithoutParty.Where(new Func<Hero, bool>(SettlementHeroCaptureCommonConditionInterne)).ToList<Hero>())
                {
                    TakePrisonerAction.Apply(hero.CurrentSettlement.Party, hero);
                    //Helper.Print(String.Format("Boucle 1 OK For Hero {0}", hero.Name), Helper.PrintHow.PrintToLogAndWrite);
                }
            }
            //Helper.Print("Boucle 2 Commence", Helper.PrintHow.PrintToLogAndWrite);
            foreach (MobileParty mobileParty in (from x in settlement.Parties
                                                 where x.IsLordParty && (x.Army == null || (x.Army != null && x.Army.LeaderParty == x)) && x.MapEvent == null && SettlementHeroCaptureCommonConditionInterne(x.LeaderHero)
                                                 select x).ToList<MobileParty>())
            {
                LeaveSettlementAction.ApplyForParty(mobileParty);
                SetPartyAiAction.GetActionForPatrollingAroundSettlement(mobileParty, settlement);
                //Helper.Print(String.Format("Boucle 2 OK For MobileParty {0}", mobileParty.Name), Helper.PrintHow.PrintToLogAndWrite);
            }
            //Helper.Print("Boucle 2 OK", Helper.PrintHow.PrintToLogAndWrite);
            return false;
            //return true;
        }
        private static bool SettlementHeroCaptureCommonConditionInterne(Hero hero)
        {
            return hero != null && hero != Hero.MainHero && !hero.IsWanderer && hero.HeroState != Hero.CharacterStates.Prisoner && hero.HeroState != Hero.CharacterStates.Dead
                && hero.MapFaction != null
                // && hero.MapFaction.IsAtWarWith(hero.CurrentSettlement.MapFaction);
                && hero.CurrentSettlement != null
                && hero.MapFaction.IsAtWarWith(hero.CurrentSettlement.MapFaction);
        }
    }
}
