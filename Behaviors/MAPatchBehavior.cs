using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone.Behaviors
{
    class MAPatchBehavior : CampaignBehaviorBase
    {

        public void patchClanLeader()
        {

            bool bPatchExecute = false;

            List<Hero> spouses = new List<Hero>();
            if (Hero.MainHero.Spouse != null)
            {
                spouses.Add(Hero.MainHero.Spouse);
#if TRACELOAD
                MAHelper.Print("Main spouse " + MAHelper.TraceHero(Hero.MainHero.Spouse), MAHelper.PRINT_TRACE_LOAD);
#endif

            }

            if (Hero.MainHero.ExSpouses != null)
            {
                int nb = Hero.MainHero.ExSpouses.Count;
                MAHelper.RemoveExSpouses(Hero.MainHero);
#if TRACELOAD
                if (nb != Hero.MainHero.ExSpouses.Count)
                    MAHelper.Print(String.Format("Patch duplicate spouse for mainHero from {0} to {1}", nb, Hero.MainHero.ExSpouses.Count), MAHelper.PRINT_TRACE_LOAD);
#endif

                foreach (Hero hero in Hero.MainHero.ExSpouses) 
                {
                    if (hero.IsAlive)
                        spouses.Add(hero);
    #if TRACELOAD
                    MAHelper.Print("Other spouse " + MAHelper.TraceHero(hero), MAHelper.PRINT_TRACE_LOAD);
    #endif
                }
            }

            foreach (Clan clan in Clan.FindAll(c => c.IsClan))
            {
                if (clan.Leader != null && spouses.FirstOrDefault(h => h == clan.Leader) != null)
                {

                    Hero? ancLeader = clan.Leader;
                    Hero? newLeader = null;
                    Hero? heroRAS = null;
                    bool supprimeClan = false;

                    MAHelper.Print(String.Format("Nb Heroes in clan {0} ?= {1}", clan.Name, clan.Heroes.Count), MAHelper.PRINT_PATCH);
                    MAHelper.Print(String.Format("clan({1}).leader.clan ?= {0}", (clan.Leader != null && clan.Leader.Clan != null ? clan.Leader.Clan.Name : "NULL"), clan.Name), MAHelper.PRINT_PATCH);

                    ancLeader.Clan = clan; // to ApplyWithoutSelectedNewLeader work fine
                    Dictionary<Hero, int> heirApparents = clan.GetHeirApparents(); // ne fonctionne pas car les héros ne sont pas encores listés dans les clans
                    //heirApparents = new Dictionary<Hero, int>();
                    //int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
                    //foreach (Hero hero in Hero.AllAliveHeroes)
                    //{
                    //    if (hero.Clan == clan && hero != ancLeader && hero.IsAlive && !hero.IsNotSpawned && !hero.IsDisabled && !hero.IsWanderer && !hero.IsNotable && hero.Age >= (float)heroComesOfAge)
                    //    {
                    //        int value = Campaign.Current.Models.HeirSelectionCalculationModel.CalculateHeirSelectionPoint(hero, ancLeader, ref heroRAS);
                    //        heirApparents.Add(hero, value);
                    //    }
                    //}

                    if (heirApparents.Count > 0)
                    {
                        int max = heirApparents.AsEnumerable().Where(x => x.Key != ancLeader).Max(x => x.Value);
                        //int max = Max<int>(heirApparents.AsEnumerable().Where(x => x.Key != ancLeader).Select(x => x.Value));
                        newLeader = heirApparents.AsEnumerable().FirstOrDefault(x => x.Key != ancLeader && x.Value == max).Key;
                    }

                    if (newLeader != null)
                    {
                        ChangeClanLeaderAction.ApplyWithSelectedNewLeader(clan, newLeader);
                    }
                    else
                    {
                        DestroyClanAction.Apply(clan);
                        supprimeClan = true;
                    }
                    ancLeader.Clan = Hero.MainHero.Clan;

                    if (supprimeClan)
                        MAHelper.Print(String.Format("PATCH Leader for the clan {0} ERASE the clan", clan.Name), MAHelper.PRINT_PATCH);
                    else if (clan.Leader == ancLeader)
                        MAHelper.Print(String.Format("PATCH Leader for the clan {0} FAIL because leader unchanged", clan.Name), MAHelper.PRINT_PATCH);
                    else
                        MAHelper.Print(String.Format("PATCH Leader for the clan {0} SUCCESS swap the leader from {1} to {2}", clan.Name, ancLeader.Name, clan.Leader == null ? "NULL" : clan.Leader.Name), MAHelper.PRINT_PATCH);
                }
            }

            foreach (Hero hero in spouses)
            {
                if (hero.IsAlive)
                    MAHelper.PatchHeroPlayerClan(hero, false, true);
            }

#if TRACELOAD
            // Voir HeroAgentSpawnCampaignBehavior.AddPartyHero

            foreach (Hero hero in Clan.PlayerClan.Lords)
            {
                MAHelper.Print(String.Format("Hero {0} in Clan.PlayerClan.Lords", hero.Name.ToString()), MAHelper.PRINT_TRACE_LOAD);
            }
            using (IEnumerator<Hero> enumerator2 = Hero.MainHero.CompanionsInParty.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Hero companion = enumerator2.Current;
                    MAHelper.Print(String.Format("Hero {0} in companion via enumerator", companion.Name.ToString()), MAHelper.PRINT_TRACE_LOAD);
                }
            }

#endif
            MAHelper.Print(String.Format("patchClanLeader {0}", (bPatchExecute ? "OK SUCCESS" : "RAS")) , MAHelper.PRINT_PATCH | (bPatchExecute ? MAHelper.PrintHow.PrintForceDisplay : 0));
        }

        private void OnSessionLaunched(CampaignGameStarter cgs)
        {

            MAHelper.MASettingsClean();

            MAHelper.MAEtape = MAHelper.Etape.EtapeLoadPas2;

            // Kingdom patch
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (!kingdom.IsEliminated && kingdom.Leader != null && kingdom.Leader.Clan.Kingdom != kingdom)
                {

                    MAHelper.Print(String.Format("PATCH Kingdom will destroy the kingdom {0}", kingdom.Name), MAHelper.PRINT_PATCH | MAHelper.PrintHow.PrintForceDisplay);
                    foreach(Clan clan in kingdom.Clans)
                    {
                        MAHelper.Print(String.Format("with the clan {0}", clan.Name), MAHelper.PRINT_PATCH);
                    }
                    DestroyKingdomAction.Apply(kingdom);
                    kingdom.MainHeroCrimeRating = 0;
                    //kingdom.RulingClan = null;
                }
            }

            patchClanLeader();
        }


        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
            // RAS
        }
    }
}
