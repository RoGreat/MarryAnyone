using MarryAnyone.Patches.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using System.Linq;

namespace MarryAnyone.Models
{
    internal class MarryAnyoneMarriageModel : DefaultMarriageModel
    {
        private static DefaultMarriageModel? _instance;

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            if (_instance is null)
            {
                _instance = new();
            }

            Settings settings = new();
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = settings.SexualOrientation == "Homosexual" && isMainHero;
            bool isBisexual = settings.SexualOrientation == "Bisexual" && isMainHero;
            bool isIncestuous = settings.Incest && isMainHero;
            bool discoverAncestors = DefaultMarriageModelPatches.DiscoverAncestors(_instance, firstHero, 3).Intersect(DefaultMarriageModelPatches.DiscoverAncestors(_instance, secondHero, 3)).Any();

            Clan clan = firstHero.Clan;
            if (clan?.Leader == firstHero && !isMainHero)
            {
                Clan clan2 = secondHero.Clan;
                if (clan2?.Leader == secondHero)
                {
                    return false;
                }
            }
            if (!isIncestuous)
            {
                if (discoverAncestors)
                {
                    return false;
                }
            }
            if (isHomosexual)
            {
                return firstHero.IsFemale == secondHero.IsFemale && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            if (isBisexual)
            {
                return IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            return firstHero.IsFemale != secondHero.IsFemale && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            Settings settings = new();
            bool inConversation, isCheating, isPolygamous;
            inConversation = isCheating = isPolygamous = false;
            if (Hero.OneToOneConversationHero is not null)
            {
                inConversation = maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero;
                isCheating = settings.Cheating && inConversation;
                isPolygamous = settings.Polygamy && inConversation;
            }
            if (!maidenOrSuitor.IsAlive || maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate)
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