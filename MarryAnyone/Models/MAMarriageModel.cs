using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using System.Linq;
using HarmonyLib.BUTR.Extensions;
using System;
using System.Collections.Generic;

namespace MarryAnyone.Models
{
    internal sealed class MAMarriageModel : DefaultMarriageModel
    {
        private delegate IEnumerable<Hero> DiscoverAncestorsDelegate(DefaultMarriageModel instance, Hero hero, int n);
        private static readonly DiscoverAncestorsDelegate? DiscoverAncestors = AccessTools2.GetDelegate<DiscoverAncestorsDelegate>(typeof(DefaultMarriageModel), "DiscoverAncestors", new Type[] { typeof(Hero), typeof(int) });

        private static bool _mainHeroCourtship = false;

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            /* Section for AI heroes from the original method */
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            if (!isMainHero)
            {
                // Other heroes use the DefaultMarriageModel
                return base.IsCoupleSuitableForMarriage(firstHero, secondHero);
            }
            /* Section for Marry Anyone method */
            // DO NOT MARRY AGAIN!
            if (Romance.GetRomanticLevel(firstHero, secondHero) == Romance.RomanceLevelEnum.Marriage)
            {
                return false;
            }
            // Does not directly call IsSuitableForMarriage, instead calls CanMarry -> IsSuitableForMarriage
            _mainHeroCourtship = true;
            bool canMarry = firstHero.CanMarry() && secondHero.CanMarry();
            _mainHeroCourtship = false;
            if (!canMarry)
            {
                return false;
            }

            MASettings settings = new();
            // If incest setting is off then look for ancestor relations
            if (!settings.Incest)
            {
                if (DiscoverAncestors!(this, firstHero, 3).Intersect(DiscoverAncestors(this, secondHero, 3)).Any())
                {
                    return false;
                }
            }

            bool isAttracted = true;
            if (settings.SexualOrientation == "Heterosexual")
            {
                isAttracted = firstHero.IsFemale != secondHero.IsFemale;
            }
            else if (settings.SexualOrientation == "Homosexual")
            {
                isAttracted = firstHero.IsFemale == secondHero.IsFemale;
            }
            return isAttracted;
        }

        // Problem is with marriage bartering with a lack of info...
        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            if (!_mainHeroCourtship)
            {
                return base.IsSuitableForMarriage(maidenOrSuitor);
            }
            if (maidenOrSuitor.IsDead || maidenOrSuitor.IsTemplate || maidenOrSuitor.IsPrisoner)
            {
                return false;
            }
            MASettings settings = new();
            if ((maidenOrSuitor.Spouse is null && !maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)) || settings.Polygamy || settings.Cheating)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }
            return false;
        }

        // Borrowed from Family Tree
        // Might adversely affect AI marriage barters
        /*
        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero)
        {
            List<Hero> heroes = new List<Hero>() { firstHero, secondHero };
            // Kingdom Ruling Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Kingdom?.Leader == hero)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Ruling Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Kingdom?.RulingClan == hero.Clan)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsKingdomFaction && hero.IsFactionLeader)
                {
                    return hero.Clan;
                }
            }
            // Kingdom Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsKingdomFaction)
                {
                    return hero.Clan;
                }
            }
            // Minor Faction Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsMinorFaction && hero.IsFactionLeader)
                {
                    return hero.Clan;
                }
            }
            // Minor Faction Clan
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.IsMinorFaction)
                {
                    return hero.Clan;
                }
            }
            // Clan Leader
            foreach (Hero hero in heroes)
            {
                if (hero.Clan.Leader == hero)
                {
                    return hero.Clan;
                }
            }
            // Other
            if (!firstHero.IsFemale)
            {
                return firstHero.Clan;
            }
            return secondHero.Clan;
        }
        */
    }
}