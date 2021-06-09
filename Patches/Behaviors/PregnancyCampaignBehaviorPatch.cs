using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace MarryAnyone.Patches.Behaviors
{
    // Add in a setting for enabling polyamory so it does not have to be a harem
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static void Prefix(Hero hero)
        {
            ISettingsProvider settings = new MASettings();
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge )
            {
                // If you are the MainHero go through advanced process
                if (hero == Hero.MainHero || hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    if (hero.Spouse is null && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                    {
                        MAHelper.Print("    No Spouse");
                        return;
                    }
                    _spouses = new List<Hero>();
                    MAHelper.Print("Hero: " + hero);
                    if (hero.Spouse is not null && hero == Hero.MainHero)
                    {
                        _spouses.Add(hero.Spouse);
                        MAHelper.Print("Spouse to Collection: " + hero.Spouse);
                    }
                    if (settings.Polyamory && hero != Hero.MainHero)
                    {
                        MAHelper.Print("Polyamory");
                        if (hero.Spouse != Hero.MainHero)
                        {
                            _spouses.Add(Hero.MainHero);
                            MAHelper.Print("Main Hero to Collection: " + Hero.MainHero);
                        }
                        if (Hero.MainHero.Spouse is not null && Hero.MainHero.Spouse != hero)
                        {
                            _spouses.Add(Hero.MainHero.Spouse);
                            MAHelper.Print("Main Hero Spouse to Collection: " + Hero.MainHero.Spouse);
                        }
                        foreach (Hero exSpouse in Hero.MainHero.ExSpouses.Distinct().ToList())
                        {
                            if (exSpouse != hero && exSpouse.IsAlive)
                            {
                                _spouses.Add(exSpouse);
                                MAHelper.Print("Main Hero ExSpouse to Collection: " + exSpouse);
                            }
                        }
                    }
                    else
                    {
                        // Taken out of polyamory mode
                        if (hero.Spouse != Hero.MainHero && hero != Hero.MainHero)
                        {
                            _spouses.Add(Hero.MainHero);
                            MAHelper.Print("Spouse is Main Hero: " + Hero.MainHero);
                        }
                        if (hero == Hero.MainHero)
                        {
                            foreach (Hero exSpouse in hero.ExSpouses.Distinct().ToList())
                            {
                                if (exSpouse.IsAlive)
                                {
                                    _spouses.Add(exSpouse);
                                    MAHelper.Print("ExSpouse to Collection: " + exSpouse);
                                }
                            }
                        }
                    }
                    if (_spouses.Count() > 1)
                    {
                        // The shuffle!
                        List<int> attractionGoal = new();
                        int attraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(hero, spouse);
                            attractionGoal.Add(attraction);
                            MAHelper.Print("Spouse: " + spouse);
                            MAHelper.Print("Attraction: " + attraction);
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MAHelper.Print("Random: " + attractionRandom);
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MAHelper.Print("Index: " + i);
                                break;
                            }
                            i++;
                        }
                        hero.Spouse = _spouses[i];
                        _spouses[i].Spouse = hero;
                    }
                    else
                    {
                        var spouse = _spouses.FirstOrDefault();
                        if (spouse is not null)
                        {
                            hero.Spouse = spouse;
                            spouse.Spouse = hero;
                        }
                    }
                    if (hero.Spouse is null)
                    {
                        MAHelper.Print("   No Spouse");
                    }
                    else
                    {
                        MAHelper.Print("   Spouse Assigned: " + hero.Spouse);
                    }
                }
            }



            // Outside of female pregnancy behavior
            // Prevents RefreshSpouseVisit from being called on female - female marriage
            /*if (hero.Spouse is not null)
            {
                if (hero.IsFemale == hero.Spouse.IsFemale)
                {
                    // Decided to do this at the end so that you are not always going out with the opposite gender
                    MAHelper.Print("   Spouse Unassigned: " + hero.Spouse);
                    hero.Spouse.Spouse = null;
                    hero.Spouse = null;
                }
            }*/
        }

        private static void Postfix(Hero hero)
        {
            // Make things looks better in the encyclopedia
            ISettingsProvider settings = new MASettings();
            if (hero == Hero.MainHero)
            {
                MAHelper.Print("Post Pregnancy Check: " + hero);
                MAHelper.Print("   Main Hero Spouse Unassigned");
                hero.Spouse = null;
            }
            if (Hero.MainHero.ExSpouses.Contains(hero) || hero.Spouse == Hero.MainHero)
            {
                if (hero.Spouse is null || hero.Spouse != Hero.MainHero)
                {
                    MAHelper.Print("Post Pregnancy Check: " + hero);
                    MAHelper.Print("   Spouse is Main Hero");
                    if (!settings.Polyamory)
                    {
                        // Remove any extra duplicate exspouses
                        MAHelper.RemoveExSpouses(hero, true);
                    }
                    hero.Spouse = Hero.MainHero;
                }
            }
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                MAHelper.RemoveExSpouses(hero);
                MAHelper.RemoveExSpouses(exSpouse);
            }
        }
        private static List<Hero>? _spouses;
    }


    // Token: 0x02000008 RID: 8
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "RefreshSpouseVisit")]
    public static class CheckForPregnancies
    {
        // Token: 0x06000014 RID: 20 RVA: 0x00002360 File Offset: 0x00000560
        private static bool CheckAreNearbyBase(PregnancyCampaignBehavior instance, Hero hero, Hero spouse)
        {
            return (bool)CheckForPregnancies.checkAreNearbyBase.Invoke(instance, new object[]
            {
                hero,
                spouse
            });
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002380 File Offset: 0x00000580
        private static void ChildConceivedBase(PregnancyCampaignBehavior instance, Hero mother)
        {
            CheckForPregnancies.childConceivedBase.Invoke(instance, new object[]
            {
                mother
            });
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002398 File Offset: 0x00000598
        public static bool Prefix(PregnancyCampaignBehavior __instance, Hero hero)
        {
            ISettingsProvider settings = new MASettings();
            bool isNearbyBase = CheckForPregnancies.CheckAreNearbyBase(__instance, hero, hero.Spouse);
            if (isNearbyBase)
            {
                float rndChance = MBRandom.RandomFloat;
                float heroChance = Campaign.Current.Models.PregnancyModel.GetDailyChanceOfPregnancyForHero(hero);
                float heroSpouceChance = Campaign.Current.Models.PregnancyModel.GetDailyChanceOfPregnancyForHero(hero.Spouse);

                if (hero == Hero.MainHero || hero.Spouse == Hero.MainHero && settings.Debug)
                {
                    MAHelper.Print($"RefreshSpouseVisit " +
                        $"\n Hero: {hero.Name.ToString()}" +
                        $"\n Spouse: {hero.Spouse.Name.ToString()}" +
                        $"\n rndChance: {rndChance}   " +
                        $"\n DailyChance: {heroChance}" +
                        $"\n Spouce DailyChance: {heroSpouceChance}" +
                        $"\n ShouldBePregnent: {rndChance <= heroChance}" +
                        $"\n Spouce ShouldBePregnent: {rndChance <= heroSpouceChance}"
                        );
                }

                if (rndChance <= heroChance || rndChance <= heroSpouceChance)
                {
                    Hero hero2 = DetermineMother(hero, hero.Spouse);
                    if (hero2 == null)
                    {
                        MAHelper.Print("   RefreshSpouseVisit if (hero2 == null)");
                        return false;
                    }
                    MakePregnantAction.Apply(hero2);
                    CheckForPregnancies.ChildConceivedBase(__instance, hero2);
                }
            }
            return false;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x000023F0 File Offset: 0x000005F0
        private static Hero DetermineMother(Hero spouse1, Hero spouse2)
        {
            ISettingsProvider settings = new MASettings();
            if (spouse1.IsPregnant || spouse2.IsPregnant)
            {
                return null;
            }
            if ((!spouse1.IsHumanPlayerCharacter && !spouse2.IsHumanPlayerCharacter)
                || spouse1.IsFemale != spouse2.IsFemale)
            {
                bool isFemale = spouse1.IsFemale;
                bool isFemale2 = spouse2.IsFemale;
                if (isFemale)
                {
                    if (!isFemale2)
                    {
                        return spouse1;
                    }
                }
                else if (isFemale2)
                {
                    return spouse2;
                }
                return null;
            }
            Hero result;
            switch (settings.pregnancyMode)
            {
                case "Player":
                    result = (spouse1.IsHumanPlayerCharacter ? spouse1 : spouse2);
                    break;
                case "Partner":
                    result = (spouse1.IsHumanPlayerCharacter ? spouse2 : spouse1);
                    break;
                case "Random":
                    result = ((MBRandom.RandomInt(0, 1) == 0) ? spouse1 : spouse2);
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }

        // Token: 0x0400000A RID: 10
        private static readonly MethodInfo checkAreNearbyBase = AccessTools.Method(typeof(PregnancyCampaignBehavior), "CheckAreNearby", null, null);

        // Token: 0x0400000B RID: 11
        private static readonly MethodInfo childConceivedBase = AccessTools.Method(typeof(PregnancyCampaignBehavior), "ChildConceived", null, null);
    }
}