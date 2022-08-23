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

        private static bool _mainHeroMarriage = false;

        private static bool _mainHero = false;

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
            bool canMarry1 = false;
            bool canMarry2 = false;
            _mainHeroMarriage = true;
            if (firstHero == Hero.MainHero)
            {
                _mainHero = true;
                canMarry1 = firstHero.CanMarry();
                _mainHero = false;

                canMarry2 = secondHero.CanMarry();
            }
            else if (secondHero == Hero.MainHero)
            {
                canMarry1 = firstHero.CanMarry();

                _mainHero = true;
                canMarry2 = secondHero.CanMarry();
                _mainHero = false;
            }
            bool canMarry = canMarry1 && canMarry2;
            _mainHeroMarriage = false;
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

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            // Problem is with marriage bartering with a lack of info...
            // Hence the use of a flag
            if (!_mainHeroMarriage)
            {
                return base.IsSuitableForMarriage(maidenOrSuitor);
            }
            if (maidenOrSuitor.IsDead || maidenOrSuitor.IsTemplate || maidenOrSuitor.IsPrisoner)
            {
                return false;
            }
            MASettings settings = new();
            bool spouses = maidenOrSuitor.Spouse is not null || maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive);
            // Cheating should bypass this, while polygamy may or may not bypass this
            // Polygamy should bypass if the main hero might have spouses but the other does not
            if (!spouses || settings.Cheating || (_mainHero && settings.Polygamy))
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }
            return false;
        }
    }
}