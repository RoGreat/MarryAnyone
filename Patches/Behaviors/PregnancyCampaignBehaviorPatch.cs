using HarmonyLib;
using Helpers;
using MarryAnyone.Behaviors;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.Conversation.Tags.ConversationTagHelper;

namespace MarryAnyone.Patches.Behaviors
{
    // Add in a setting for enabling polyamory so it does not have to be a harem
    [HarmonyPatch(typeof(PregnancyCampaignBehavior))]
    internal static class PregnancyCampaignBehaviorPatch
    {

        private static List<Hero>? _spouses;
        private static Hero? _sideFemaleHero;
        private static Hero? _sauveSpouse;
        private static bool _wasPregnant = false;
        private static bool _playerRelation = false;
#if TRACEPREGNANCY
        private static bool _needTrace = false;
#endif
        private static bool okToDoIt(Hero hero, Hero? otherHero = null)
        {
            if (hero.IsAlive && hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (otherHero != null)
                {
                    if (!otherHero.IsAlive || otherHero.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge)
                        return false;

                    if (MAHelper.MASettings.RelationLevelMinForSex >= 0)
                    {
                        int relation = hero.GetRelation(otherHero);

                        int compatible = MAHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Calculating)
                                        + MAHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Generosity) * 2
                                        + MAHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Valor)
                                        + MAHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Honor); // TaleWorlds.CampaignSystem.Conversation.Tags.ConversationTagHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Calculating)

                        return relation + compatible > MAHelper.MASettings.RelationLevelMinForSex;
                    }
                }
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero", new Type[] {typeof(Hero) })]
        [HarmonyPrefix]
        private static void DailyTickHeroPrefix(Hero hero)
        {
            _spouses = null;
            _sideFemaleHero = null;
            _sauveSpouse = hero.Spouse;
            _wasPregnant = hero.IsPregnant;
            _playerRelation = false;
#if TRACEPREGNANCY
            _needTrace = false;
#endif

            if (hero.IsFemale && okToDoIt(hero))
            {
                bool isPartner = MARomanceCampaignBehavior.Instance != null
                                    && MARomanceCampaignBehavior.Instance.Partners != null
                                    && MARomanceCampaignBehavior.Instance.Partners.Contains(hero);

                if (hero.Spouse is null && !isPartner && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                {
#if TRACEPREGNANCY
                    MAHelper.Print(string.Format("DailyTickHero:: {0} has No Spouse", hero.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                }
                else if (hero == Hero.MainHero || isPartner || hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero)) // If you are the MainHero go through advanced process
                {
                    _playerRelation = true;
                    MASettings settings = MAHelper.MASettings;

                    if (_spouses == null)
                        _spouses = new List<Hero>();
#if TRACEPREGNANCY
                    _needTrace = true;
                    MAHelper.Print(string.Format("DailyTickHero::{0} Pregnant {2}\r\nPolyamory ?= {1}", hero.Name, settings.Polyamory, hero.IsPregnant), MAHelper.PRINT_TRACE_PREGNANCY);
#endif

                    if ((isPartner || Hero.MainHero.ExSpouses.Contains(hero)) && okToDoIt(hero, Hero.MainHero) && hero.CurrentSettlement == Hero.MainHero.CurrentSettlement)
                    {
#if TRACEPREGNANCY
                        MAHelper.Print(string.Format("DailyTickHero::{0} ISPartener or exSpouse add mainHero", hero.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                        _spouses.Add(Hero.MainHero);

                    }

                    if (hero.Spouse != null && okToDoIt(hero, hero.Spouse) && hero.CurrentSettlement == hero.Spouse.CurrentSettlement && _spouses.IndexOf(hero.Spouse) < 0)
                    {
#if TRACEPREGNANCY
                        MAHelper.Print(String.Format("DailyTickHero::{0} add hero Spouse {1}", hero.Name, hero.Spouse.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                        _spouses.Add(hero.Spouse);
                    }

                    if (settings.Polyamory)
                    {
                        if (MARomanceCampaignBehavior.Instance.Partners != null) {

                            foreach (Hero withHero in MARomanceCampaignBehavior.Instance.Partners)
                            {
                                if (withHero != hero && okToDoIt(hero, withHero) && withHero.CurrentSettlement == hero.CurrentSettlement && _spouses.IndexOf(withHero) < 0)
                                {
#if TRACEPREGNANCY
                                    MAHelper.Print(String.Format("DailyTickHero::{0} add partner {1}", hero.Name, withHero.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif

                                    _spouses.Add(withHero);

                                }
                            }

                        }

                        if (Hero.MainHero.ExSpouses != null)
                        {
                            foreach (Hero withHero in Hero.MainHero.ExSpouses)
                            {
                                if (withHero.IsAlive && withHero != hero && okToDoIt(hero, withHero) && withHero.CurrentSettlement == hero.CurrentSettlement && _spouses.IndexOf(withHero) < 0)
                                {
#if TRACEPREGNANCY
                                    MAHelper.Print(String.Format("DailyTickHero::{0} add exSpouse {1}", hero.Name, withHero.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif

                                    _spouses.Add(withHero);

                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_spouses == null)
                        _spouses = new List<Hero>();

                    if (hero.Spouse != null && okToDoIt(hero, hero.Spouse) && hero.CurrentSettlement == hero.Spouse.CurrentSettlement && _spouses.IndexOf(hero.Spouse) < 0)
                    {
#if TRACEPREGNANCY
                        MAHelper.Print(String.Format("DailyTickHero::{0} add hero Spouse {1}", hero.Name, hero.Spouse.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                        _spouses.Add(hero.Spouse);
                    }

                    if (hero.ExSpouses != null)
                    {
                        foreach (Hero withHero in hero.ExSpouses)
                        {
                            if (withHero.IsAlive && withHero != hero && okToDoIt(hero, withHero) && withHero.CurrentSettlement == hero.CurrentSettlement && _spouses.IndexOf(withHero) < 0)
                            {
#if TRACEPREGNANCY
                                MAHelper.Print(String.Format("DailyTickHero::{0} add exSpouse {1}", hero.Name, withHero.Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif

                                _spouses.Add(withHero);

                            }
                        }
                    }
                }

                if (_spouses != null && _spouses.Count() > 1)
                {
                    // The shuffle!
                    List<int> attractionGoal = new();
                    int attraction = 0;
                    int addAttraction = 0;
                    foreach (Hero spouse in _spouses)
                    {
                        addAttraction = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(hero, spouse);
                        attraction += addAttraction * (spouse.IsFemale ? 1 : 3); // To up the pregnancy chance
                        attractionGoal.Add(attraction);
#if TRACEPREGNANCY
                        MAHelper.Print(string.Format("Spouse {0} attraction {1}", spouse.Name, attraction), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                    }
                    int attractionRandom = MBRandom.RandomInt(attraction);
                    MAHelper.Print("Random: " + attractionRandom, MAHelper.PRINT_TRACE_PREGNANCY);
                    int i = 0;
                    while (i < _spouses.Count)
                    {
                        if (attractionRandom <= attractionGoal[i])
                        {
#if TRACEPREGNANCY
                            MAHelper.Print(string.Format("Résoud Index{0} => Spouse {1}", i, _spouses[i].Name), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                            break;
                        }
                        i++;
                    }
                    hero.Spouse = _spouses[i];
                    _spouses[i].Spouse = hero;
                }
                else if (_spouses != null && _spouses.Count() == 1)
                {
                    Hero spouse = _spouses[0];
                    hero.Spouse = spouse;
                    spouse.Spouse = hero;
                }
                else
                {
                    hero.Spouse = null;
                }
            }

            // Outside of female pregnancy behavior
            if (hero.Spouse is not null)
            {
                if (hero.IsFemale == hero.Spouse.IsFemale)
                {
                    // Decided to do this at the end so that you are not always going out with the opposite gender
#if TRACEPREGNANCY
                    MAHelper.Print("DailyTickHero:: Spouse Unassigned because (same sex): " + hero.Spouse, MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                    _sideFemaleHero = hero.Spouse;
                    hero.Spouse.Spouse = null;
                    hero.Spouse = null;
                }
            }
        }

        [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero", new Type[] { typeof(Hero) })]
        [HarmonyPostfix]
        private static void DailyTickHeroPostfix(Hero hero)
        {
            // Make things looks better in the encyclopedia
#if TRACEPREGNANCY
            String traceAff = "";
#endif

            if (MAHelper.MASettings.ImproveRelation && hero != null && (hero.Spouse != null || _sideFemaleHero != null))
            {
                if (_sideFemaleHero == null)
                    _sideFemaleHero = hero.Spouse;

                bool justPregnant = hero.IsPregnant && !_wasPregnant;
                int relationChange = 0;
                float zeroAUn = MBRandom.RandomFloat;

                int relactionActuelle = hero.GetRelation(_sideFemaleHero);
                int compatible = (MAHelper.TraitCompatibility(hero, _sideFemaleHero, DefaultTraits.Calculating)
                                + MAHelper.TraitCompatibility(hero, _sideFemaleHero, DefaultTraits.Generosity) * 3
                                + MAHelper.TraitCompatibility(hero, _sideFemaleHero, DefaultTraits.Valor)) / 2; // TaleWorlds.CampaignSystem.Conversation.Tags.ConversationTagHelper.TraitCompatibility(hero, otherHero, DefaultTraits.Calculating)

#if TRACEPREGNANCY
                traceAff = String.Format("relactionActuelle ?= {0}, compatible ?= {1} zeroAUn ?= {2}", relactionActuelle, compatible, zeroAUn);
#endif
                

                if (justPregnant)
                {
                    relationChange = ((int)(zeroAUn * 9)) + 1 + (compatible > 0 ? compatible * 2 : 0);
                }
                if (relactionActuelle < 0)
                    relationChange = ((int)(zeroAUn * 6)) - 2 + (compatible <= -2 ? -1 : compatible);
                else if (relactionActuelle < 25)
                    relationChange = ((int)(zeroAUn * 5)) - 1 + (compatible <= -2 ? -1 : compatible);
                else if (relactionActuelle < 50)
                    relationChange = ((int)(zeroAUn * 4)) + (compatible <= -2 ? -1 : compatible);
                else 
                    relationChange = ((int)(zeroAUn * 5)) + (compatible <= -3 ? -2 : compatible);

#if TRACEPREGNANCY
                traceAff += String.Format("\r\n\t => relationChange {0}", relationChange);
#endif

                if (relationChange != 0)
                {
                    if (_playerRelation)
                    {
                        StringHelpers.SetCharacterProperties("HEROONE", hero.CharacterObject);
                        StringHelpers.SetCharacterProperties("HEROTOW", _sideFemaleHero.CharacterObject);
                        MBTextManager.SetTextVariable("INCREMENT", relationChange);
                        if (justPregnant)
                        {
                            TextObject textObject = new TextObject("{=TheTwoOfThemHaveAGoodTime}{HEROONE.NAME} and {HEROTOW.NAME} have a good time together, their relationship up from {INCREMENT} points");
                            MAHelper.PrintWithColor(textObject.ToString(), MAHelper.yellowCollor);
                        }
                        else
                        {
                            TextObject textObject = new TextObject("{=TheTwoOfThemSpendTime}{HEROONE.NAME} and {HEROTOW.NAME} spend time together, their relationship up from {INCREMENT} points");
                            MAHelper.PrintWithColor(textObject.ToString(), Color.White);
                        }
                    }

                    if (relationChange != 0) 
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, _sideFemaleHero, relationChange, false);
                }
            }

#if TRACEPREGNANCY
            if (_needTrace)
            {
                bool justPregnant = hero.IsPregnant && !_wasPregnant;
                MAHelper.Print(String.Format("Post Pregnancy Hero {0} justPregnant ?= {1}\r\n{2}", hero.Name, justPregnant, traceAff), MAHelper.PRINT_TRACE_PREGNANCY);
            }
#endif
            if (hero == Hero.MainHero)
            {
#if TRACEPREGNANCY
                MAHelper.Print(string.Format("Post Pregnancy main hero {0} IsPregnant {1} Check unassigne spouse", hero.Name, hero.IsPregnant), MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                hero.Spouse = null;
            }

            bool isPartner = MARomanceCampaignBehavior.Instance != null
                                    && MARomanceCampaignBehavior.Instance.Partners != null
                                    && MARomanceCampaignBehavior.Instance.Partners.Contains(hero);

            if ((Hero.MainHero.ExSpouses.Contains(hero) || hero.Spouse == Hero.MainHero) && !isPartner)
            {
#if TRACEPREGNANCY
                MAHelper.Print(string.Format("Post Pregnancy {0} IsPregnant {1} ", hero.Name, hero.IsPregnant), MAHelper.PRINT_TRACE_PREGNANCY | MAHelper.PrintHow.UpdateLog);
#endif
                if (hero.Spouse is null || hero.Spouse != Hero.MainHero)
                {
                    ISettingsProvider settings = new MASettings();

#if TRACEPREGNANCY
                    MAHelper.Print("   Spouse is Main Hero", MAHelper.PRINT_TRACE_PREGNANCY);
#endif
                    if (!MAHelper.MASettings.Polyamory)
                    {
                        // Remove any extra duplicate exspouses
                        MAHelper.RemoveExSpouses(hero, true);
                    }
                    hero.Spouse = Hero.MainHero;
                }
            }
            else if (hero != Hero.MainHero)
            {
                hero.Spouse = _sauveSpouse;
            }

            MAHelper.RemoveExSpouses(hero);

            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                MAHelper.RemoveExSpouses(exSpouse);
            }
        }

    }
}