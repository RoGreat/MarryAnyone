//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using TaleWorlds.CampaignSystem;
//using TaleWorlds.CampaignSystem.SandBox.GameComponents;

//namespace MarryAnyone
//{
//    internal class MADefaultMarriageModel : DefaultMarriageModel
//    {
//        //public static bool MarriageCourtshipPossibility(Hero person1, Hero person2)
//        //{
//        //    return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2) && !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction);
//        //}

//        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
//        {
//            Trace.WriteLine("Test 1");
//            bool isPolygamous = MASubModule.Polygamy && (maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero);

//            if (maidenOrSuitor.IsTemplate || !maidenOrSuitor.IsAlive || Hero.MainHero.ExSpouses.Contains(maidenOrSuitor))
//            {
//                return false;
//            }
//            if (maidenOrSuitor.Spouse == null || isPolygamous)
//            {
//                if (maidenOrSuitor.IsFemale)
//                {
//                    return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeFemale;
//                }
//                return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeMale;
//            }
//            return false;
//        }

//        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
//        {
//            Trace.WriteLine("Test 2");
//            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
//            bool isHomosexual = MASubModule.Orientation == "Homosexual" && isMainHero;
//            bool isBisexual = MASubModule.Orientation == "Bisexual" && isMainHero;
//            bool isIncestual = MASubModule.Incest && isMainHero;
//            bool discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

//            if (isIncestual)
//            {
//                discoverAncestors = true;
//            }
//            if (isHomosexual)
//            {
//                return firstHero.IsFemale == secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
//            }
//            if (isBisexual)
//            {
//                return discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
//            }
//            return firstHero.IsFemale != secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
//        }

//        public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
//        {
//            if (hero != null)
//            {
//                yield return hero;
//                if (n > 0)
//                {
//                    foreach (Hero hero2 in DiscoverAncestors(hero.Mother, n - 1))
//                    {
//                        yield return hero2;
//                    }
//                    foreach (Hero hero3 in DiscoverAncestors(hero.Father, n - 1))
//                    {
//                        yield return hero3;
//                    }
//                }
//            }
//            yield break;
//        }

//        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero)
//        {
//            MASubModule.Debug("Marriage between " + firstHero.Name + " and " + secondHero.Name);
//            if (firstHero.IsFactionLeader && firstHero.MapFaction.IsKingdomFaction)
//            {
//                MASubModule.Debug("Kingdom leader is the dominant clan in marriage");
//                return firstHero.Clan;
//            }
//            if (secondHero.IsFactionLeader && secondHero.MapFaction.IsKingdomFaction)
//            {
//                MASubModule.Debug("Kingdom leader is the dominant clan in marriage");
//                return secondHero.Clan;
//            }
//            if (firstHero.IsHumanPlayerCharacter)
//            {
//                MASubModule.Debug("Human player is the dominant clan in marriage");
//                return firstHero.Clan;
//            }
//            if (secondHero.IsHumanPlayerCharacter)
//            {
//                MASubModule.Debug("Human player is the dominant clan in marriage");
//                return secondHero.Clan;
//            }
//            if (firstHero.IsFactionLeader)
//            {
//                MASubModule.Debug("Faction leader is the dominant clan in marriage");
//                return firstHero.Clan;
//            }
//            if (secondHero.IsFactionLeader)
//            {
//                MASubModule.Debug("Faction leader is the dominant clan in marriage");
//                return secondHero.Clan;
//            }
//            if (firstHero.Clan.Leader == firstHero)
//            {
//                MASubModule.Debug("Clan leader is the dominant clan in marriage");
//                return firstHero.Clan;
//            }
//            if (secondHero.Clan.Leader == secondHero)
//            {
//                MASubModule.Debug("Clan leader is the dominant clan in marriage");
//                return secondHero.Clan;
//            }
//            if (!firstHero.IsFemale)
//            {
//                return firstHero.Clan;
//            }
//            return secondHero.Clan;
//        }
//    }
//}