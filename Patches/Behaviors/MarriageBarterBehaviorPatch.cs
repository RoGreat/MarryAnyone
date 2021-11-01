using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace MarryAnyone.Patches.Behaviors
{
//#if V1620
#if NOTDEEDED
	[HarmonyPatch(typeof(MarriageBarterBehavior))]
    internal static class MarriageBarterBehaviorPatch
    {

		private static bool IsPlayerMarriageBarterApplicable(Hero offerer, Hero heroBeingProposedTo, Hero heroToMarry, Hero offeredHero)
		{
			return (offerer != Hero.MainHero && heroBeingProposedTo != Hero.MainHero && heroToMarry != Hero.MainHero) 
				|| (offerer == Hero.MainHero 
					&& Romance.GetRomanticLevel(Hero.MainHero, heroToMarry) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage 
					&& (heroToMarry.Clan == null || offeredHero == heroToMarry.Clan.Leader));
		}


		[HarmonyPatch(typeof(MarriageBarterBehavior), "CheckForBarters", new Type[] { typeof(BarterData) })]
        [HarmonyPrefix]
        internal static bool CheckForBartersPatch(MarriageBarterBehavior __instance, BarterData args)
        {
			Hero offererHero = args.OffererHero;
			Hero otherHero = args.OtherHero;
			if (offererHero != null && otherHero != null)
			{
				MarriageModel marriageModel = Campaign.Current.Models.MarriageModel;

				List<Hero> one = MAHelper.ListClanLord(offererHero);
				List<Hero> two = MAHelper.ListClanLord(otherHero);
				foreach (Hero hero in one)
				{
					foreach (Hero hero2 in two)
					{
						if (marriageModel.IsCoupleSuitableForMarriage(hero, hero2) && IsPlayerMarriageBarterApplicable(offererHero, hero, hero2, otherHero))
						{
							Barterable barterable = new MarriageBarterable(args.OffererHero, args.OffererParty, hero2, hero);
							args.AddBarterable<OtherBarterGroup>(barterable, true);
						}
					}
				}
			}
			return false;
		}

	}
#endif
}
