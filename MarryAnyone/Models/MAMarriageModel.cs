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
        private static readonly DiscoverAncestorsDelegate DiscoverAncestors = AccessTools2.GetDelegate<DiscoverAncestorsDelegate>(typeof(DefaultMarriageModel), "DiscoverAncestors", new Type[] { typeof(Hero), typeof(int) });

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
            // Does not directly call IsSuitableForMarriage, instead calls CanMarry -> IsSuitableForMarriage
            bool canMarry = firstHero.CanMarry() && secondHero.CanMarry();
            if (!canMarry)
            {
                return false;
            }

            MASettings settings = new();
            bool isHeterosexual = settings.SexualOrientation == "Heterosexual";
            bool isHomosexual = settings.SexualOrientation == "Homosexual";
            bool isIncestuous = settings.Incest;
            bool discoverAncestors = DiscoverAncestors(this, firstHero, 3).Intersect(DiscoverAncestors(this, secondHero, 3)).Any();
            // If incest setting is off then look for ancestor relations
            if (!isIncestuous)
            {
                if (discoverAncestors)
                {
                    return false;
                }
            }

            bool isAttracted = true;
            if (isHeterosexual)
            {
                isAttracted = firstHero.IsFemale != secondHero.IsFemale;
            }
            else if (isHomosexual)
            {
                isAttracted = firstHero.IsFemale == secondHero.IsFemale;
            }
            return isAttracted;
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            /* Reminder that AI also uses this method */
            MASettings settings = new();
            bool isCheating = false;
            bool isPolygamous = false;

            if (Hero.OneToOneConversationHero is not null)
            {
                bool inConversation = maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero;
                if (inConversation)
                {
                    isCheating = settings.Cheating;
                    isPolygamous = settings.Polygamy;
                }
            }
            if (!maidenOrSuitor.IsAlive || maidenOrSuitor.IsTemplate)
            {
                return false;
            }
            if ((maidenOrSuitor.Spouse is null && !maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive)) || isPolygamous || isCheating)
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