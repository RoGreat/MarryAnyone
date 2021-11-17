using HarmonyLib;
using MarryAnyone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace PatchViaHarmony.Patches
{
    [HarmonyPatch(typeof(TournamentGame))]
    public static class TaleworldsCampaignSystemTournamentGame161
    {

		private static void GetUpgradeTargetsPatch(CharacterObject troop, ref List<CharacterObject> list)
		{
			if (!list.Contains(troop))
			{
				list.Add(troop);
			}
			if (troop.UpgradeTargets != null)
			{
				CharacterObject[] upgradeTargets = troop.UpgradeTargets;
				for (int i = 0; i < upgradeTargets.Length; i++)
				{
					GetUpgradeTargetsPatch(upgradeTargets[i], ref list);
				}
			}
		}

		//private static void SortTournamentParticipantsPatch(List<CharacterObject> participantCharacters)
		//{
		//	for (int i = 0; i < participantCharacters.Count - 1; i++)
		//	{
		//		for (int j = participantCharacters.Count - 1; j > i; j--)
		//		{
		//			if (GetTroopPriorityPointForTournamentPatch(participantCharacters[j]) > GetTroopPriorityPointForTournamentPatch(participantCharacters[i]))
		//			{
		//				CharacterObject value = participantCharacters[j];
		//				CharacterObject value2 = participantCharacters[i];
		//				participantCharacters[j] = value2;
		//				participantCharacters[i] = value;
		//			}
		//		}
		//	}
		//}

		// Token: 0x06001620 RID: 5664 RVA: 0x0005FB8C File Offset: 0x0005DD8C
		private static int GetTroopPriorityPointForTournamentPatch(CharacterObject troop)
		{
			int num = 40000;
			if (troop == CharacterObject.PlayerCharacter)
			{
				num += 80000;
			}
			if (troop.IsHero)
			{
				num += 20000;
			}
			if (troop.IsHero && troop.HeroObject.IsPlayerCompanion)
			{
				num += 10000;
			}
			else
			{
				Hero heroObject = troop.HeroObject;
				if (((heroObject != null) ? heroObject.Clan : null) != null)
				{
					int num2 = num;
					Clan clan = troop.HeroObject.Clan;
					num = num2 + (int)((clan != null) ? new float?(clan.Renown) : null).Value;
				}
				else
				{
					num += troop.Level;
				}
			}
			return num;
		}

#if V1650MORE

#endif

#if V1560LESS || V1640 || V1620 || V1610

		[HarmonyPatch(typeof(TournamentGame), "GetParticipantCharacters", new Type[] {typeof(Settlement), typeof(int), typeof(bool), typeof(bool)})]
        [HarmonyPrefix]
        internal static bool GetParticipantCharactersPatch(Settlement settlement, int maxParticipantCount, bool includePlayer, bool includeHeroes, ref List<CharacterObject> __result)
#endif
#if V1650MORE
		[HarmonyPatch(typeof(TournamentGame), "GetParticipantCharacters", new Type[] { typeof(Settlement), typeof(int), typeof(TournamentGame), typeof(bool), typeof(bool) })]
		[HarmonyPrefix]
		internal static bool GetParticipantCharactersPatch(Settlement settlement, int maxParticipantCount, TournamentGame tournament, bool includePlayer , bool includeHeroes , ref List<CharacterObject> __result)
#endif
        {

			if (!MAHelper.MASettings.SpouseJoinArena)
				return true;

#if TRACE_ARENA_PARTICIPANT_START

			String aff = String.Format("GetParticipantCharacters:: Settlement {0}, maxParticipant {1}, Player {2} Heroes {3}"
									, settlement.Name
									, maxParticipantCount
									, includePlayer
									, includeHeroes);

			Helper.Print(aff, Helper.PrintHow.PrintForceDisplay);
#endif

			MethodInfo methodInfoCanNpcJoinTournament = typeof(TournamentGame).GetMethod("CanNpcJoinTournament", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo methodInfoSortTournamentParticipants = typeof(TournamentGame).GetMethod("SortTournamentParticipants", BindingFlags.Static | BindingFlags.NonPublic);

			List<CharacterObject> list = new List<CharacterObject>();
			if (includePlayer)
			{
				list.Add(CharacterObject.PlayerCharacter);
			}
			int num = 0;
			int maxParticipantCount12 = (int)(maxParticipantCount / 2.0);
			if (includeHeroes)
			{
				while (num < settlement.Parties.Count && list.Count < maxParticipantCount12)
				{
#if V1560LESS || V1640 || V1620 || V1610
					CharacterObject leader = settlement.Parties[num].Leader;
					if (leader != null && includeHeroes && leader.HeroObject != null && !leader.HeroObject.IsWounded && !leader.HeroObject.Noncombatant && !list.Contains(leader) && leader != CharacterObject.PlayerCharacter)
					{
						list.Add(leader);
					}
#else
					Hero leaderHero = settlement.Parties[num].LeaderHero;
					if ((bool)methodInfoCanNpcJoinTournament.Invoke(null, new Object[] { leaderHero, list, true, tournament }) && leaderHero.IsNoble)
					{
						list.Add(leaderHero.CharacterObject);
					}

#endif
					num++;
				}
			}
			if (Settlement.CurrentSettlement == settlement && includeHeroes)
			{
				int maxParticipantCount23 = (int)(maxParticipantCount * 2.0 / 3);
				num = 0;
				foreach (TroopRosterElement troopRosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
				{
#if TRACE_ARENA_PARTICIPANT_START
					if (troopRosterElement.Character.IsHero) {
						aff = "Test hero " + troopRosterElement.Character.HeroObject.Name.ToString();

						if (troopRosterElement.Character.HeroObject.IsPlayerCompanion)
							aff += " Player companion";

						if (troopRosterElement.Character.HeroObject.Noncombatant)
							aff += " Non Combatant";

						if (troopRosterElement.Character.HeroObject.Spouse == Hero.MainHero)
							aff += " Spouse";

						if (Hero.MainHero.ExSpouses.Contains(troopRosterElement.Character.HeroObject))
							aff += " exSpouse";

						if (troopRosterElement.Character.HeroObject.IsWounded)
							aff += " Wounded";

						Helper.Print(aff, Helper.PrintHow.PrintToLog);
					}
#endif

					if (list.Count < maxParticipantCount 
						&& num < maxParticipantCount23
						&& troopRosterElement.Character.IsHero 
						&& !troopRosterElement.Character.HeroObject.IsWounded 
						&& (	(troopRosterElement.Character.HeroObject.IsPlayerCompanion 
									&& !troopRosterElement.Character.HeroObject.Noncombatant)
								|| troopRosterElement.Character.HeroObject.Spouse == Hero.MainHero
								|| Hero.MainHero.ExSpouses.Contains(troopRosterElement.Character.HeroObject))
						&& !list.Contains(troopRosterElement.Character))
					{
						MAHelper.Print(String.Format("Ajoute le caractère {0}", troopRosterElement.Character.ToString()), MAHelper.PrintHow.PrintToLog);
						list.Add(troopRosterElement.Character);
						num++;
					}
				}
			}
			if (list.Count < maxParticipantCount && includeHeroes)
			{
				foreach (Hero hero in settlement.HeroesWithoutParty)
				{
					if (!hero.Noncombatant && hero.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && (hero.IsWanderer || (hero.IsNoble && hero.PartyBelongedTo == null)))
					{
						list.Add(hero.CharacterObject);
						if (list.Count >= maxParticipantCount)
						{
							break;
						}
					}
				}
			}
			if (list.Count < maxParticipantCount)
			{

#if TRACE_ARENA_PARTICIPANT
				String aff = "GetParticipantCharacters:: Settlement Name ?= " + settlement.Name;
				if (settlement.Parties == null)
					aff += "Sans partie";

                Helper.Print(aff, Helper.PrintHow.PrintToLogAndWrite);
#endif
				List<CharacterObject> list2 = new List<CharacterObject>();
				if (settlement.Parties == null)
					goto IL_292;

				using (List<MobileParty>.Enumerator enumerator3 = settlement.Parties.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						MobileParty? mobileParty = enumerator3.Current;

#if TRACE_ARENA_PARTICIPANT
						aff = String.Format("Va traiter la Party {0}", (mobileParty != null ? mobileParty.Name : "NULL"));
						if (mobileParty.MemberRoster == null)
							aff += " !!! MemberRoster == null !!!";

						Helper.Print(aff, Helper.PrintHow.PrintToLogAndWrite);
#endif
						foreach (TroopRosterElement troopRosterElement2 in mobileParty.MemberRoster.GetTroopRoster())
						{
#if TRACE_ARENA_PARTICIPANT
							aff = String.Format("Va traiter le TroopRooster Number : {0}", troopRosterElement2.Number.ToString());
							if (troopRosterElement2.Character == null)
								aff += "Sans Character";
                            else
                            {
								aff += " - Character " + troopRosterElement2.Character.Name;
								if (troopRosterElement2.Character.Culture == null)
									aff += " SANS CULTURE !!!";
							}
							Helper.Print(aff, Helper.PrintHow.PrintToLogAndWrite);
							if (String.Equals(troopRosterElement2.Character.Name.ToString(), "Looter"))
							{
								Helper.Print("Mon Cas", Helper.PrintHow.PrintDisplay);
							}
#endif

							if (!troopRosterElement2.Character.IsHero && !troopRosterElement2.Character.Culture.IsBandit && !list2.Contains(troopRosterElement2.Character))
							{
								list2.Add(troopRosterElement2.Character);
							}
						}
#if TRACE_ARENA_PARTICIPANT
						Helper.Print("Va Effectuer une boucle", Helper.PrintHow.PrintToLogAndWrite);
#endif
					}
					goto IL_38A;
				}
			IL_292:
				if (list2.Count <= 0)
				{
					List<CharacterObject> list3 = new List<CharacterObject>();
					CultureObject troopCulture = (settlement != null) ? settlement.Culture : Game.Current.ObjectManager.GetObject<CultureObject>("empire");
					GetUpgradeTargetsPatch(CharacterObject.FindFirst((CharacterObject x) => x.IsBasicTroop && x.Culture == troopCulture), ref list3);
					list3.Shuffle<CharacterObject>();
					for (int i = 0; i < list3.Count; i++)
					{
						if (list.Count >= maxParticipantCount)
						{
						IL_381:
							while (list.Count < maxParticipantCount)
							{
								list3.Shuffle<CharacterObject>();
								int num2 = 0;
								while (num2 < list3.Count && list.Count < maxParticipantCount)
								{
									list.Add(list3[num2]);
									num2++;
								}
							}
							goto IL_38A;
						}
						if (!list.Contains(list3[i]))
						{
							list.Add(list3[i]);
						}
					}

					while (list.Count < maxParticipantCount)
					{
						list3.Shuffle<CharacterObject>();
						int num2 = 0;
						while (num2 < list3.Count && list.Count < maxParticipantCount)
						{
							list.Add(list3[num2]);
							num2++;
						}
					}
					goto IL_38A;
				}

				CharacterObject randomElement = list2.GetRandomElement<CharacterObject>();
				list.Add(randomElement);
				list2.Remove(randomElement);

			IL_38A:
				if (list.Count < maxParticipantCount)
				{
					goto IL_292;
				}
			}

			//SortTournamentParticipantsPatch(list);
			methodInfoSortTournamentParticipants.Invoke(null, new Object[] { list });

			__result = list;

#if TRACE_ARENA_PARTICIPANT_START
			Helper.Print("GetParticipantCharacters::FAIT", Helper.PrintHow.PrintToLogAndWrite);
#endif

			return false; // Skip default execution
        }
	}
}
