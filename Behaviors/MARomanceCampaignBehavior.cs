using HarmonyLib;
using MarryAnyone.Models;
using MarryAnyone.Settings;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace MarryAnyone.Behaviors
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.AllAliveHeroes.ToList())
            {
                // The old fix for occupations not sticking
                if (hero.Spouse == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
                    hero.Clan = null;
                    hero.Clan = Clan.PlayerClan;
                }
            }

            // To begin the dialog for companions
            starter.AddPlayerLine("main_option_discussions_MA", "hero_main_options", "lord_talk_speak_diplomacy_MA", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_begin_courtship_for_hero_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_agrees_to_discussion_MA", "lord_talk_speak_diplomacy_MA", "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(conversation_character_agrees_to_discussion_on_condition), null, 100, null);

            // From previous iteration
            //starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);
            starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null);

            //starter.AddPlayerLine("hero_romance_task", "hero_main_options", "lord_start_courtship_response_3", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 140, null, null);
            starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(this.conversation_finalize_courtship_for_hero_on_condition), null, 100, null, null);
            starter.AddPlayerLine("hero_romance_task_pt3b", "hero_main_options", "hero_courtship_final_barter", "{=jd4qUGEA}I wish to discuss the final terms of my marriage with {COURTSHIP_PARTNER}.", new ConversationSentence.OnConditionDelegate(this.conversation_finalize_courtship_for_other_on_condition), null, 100, null, null);

            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);

            starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(this.conversation_marriage_barter_successful_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_courtship_success_on_consequence), 100, null);
            starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=iunPaMFv}I guess we should put this aside, for now. But perhaps we can speak again at a later date.", () => !this.conversation_marriage_barter_successful_on_condition(), null, 100, null);
        }

        private bool conversation_begin_courtship_for_hero_on_condition()
        {
            ISettingsProvider settings = new MASettings();
            if (Hero.OneToOneConversationHero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                MAHelper.Print("MCM: " + MASettings.UsingMCM);
                MAHelper.Print("Difficulty: " + settings.Difficulty);
                MAHelper.Print("Orientation: " + settings.SexualOrientation);
                MAHelper.Print("Cheating: " + settings.Cheating);
                MAHelper.Print("Polygamy: " + settings.Polygamy);
                MAHelper.Print("Incest: " + settings.Incest);
            }
            return Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion;
        }

        private bool conversation_character_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter));
            return true;
        }

        // This will either skip or continue romance
        // CoupleAgreedOnMarriage = triggers marriage before bartering
        // CourtshipStarted = skip everything
        // return false = carry out entire romance
        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            bool ret = false;

            if (Campaign.Current.Models.MarriageModel is MADefaultMarriageModel)
                ((MADefaultMarriageModel)Campaign.Current.Models.MarriageModel).accepteSansClan = true;

            ret = Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, Hero.OneToOneConversationHero) 
                && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero)
                && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;

            if (Campaign.Current.Models.MarriageModel is MADefaultMarriageModel)
                ((MADefaultMarriageModel)Campaign.Current.Models.MarriageModel).accepteSansClan = false;

            return ret;
        }

        private bool conversation_finalize_courtship_for_other_on_condition()
        {

            if (Hero.OneToOneConversationHero != null)
            {
                Clan clan = Hero.OneToOneConversationHero.Clan;
                if (clan != null && clan.Leader == Hero.OneToOneConversationHero)
                {
                    foreach (Hero hero in clan.Lords)
                    {
                        // Trace
                        //if (Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
                        //{
                        //    MAHelper.Print(String.Format("TEST:: va tester le hero {0} possible {1} RLvl {2}", hero.Name
                        //                                , Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, hero)
                        //                                , Romance.GetRomanticLevel(Hero.MainHero, hero)));

                        //    Hero other = Romance.GetCourtedHeroInOtherClan(Hero.MainHero, hero);
                        //    if (other != null)
                        //        MAHelper.Print(String.Format("TEST:: other hero {0}", other.Name));
                        //    other = Romance.GetCourtedHeroInOtherClan(hero, Hero.MainHero);
                        //    if (other != null)
                        //        MAHelper.Print(String.Format("TEST::2d other hero {0}", other.Name));

                        //    if (!Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(hero))
                        //        MAHelper.Print(String.Format("TEST::hero {0} not suitable for mariage", hero.Name)); 

                        //    if (!Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(Hero.MainHero))
                        //        MAHelper.Print(String.Format("TEST::hero {0} not suitable for mariage", Hero.MainHero.Name));

                        //    if (!Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, hero))
                        //        MAHelper.Print("NOT Suitable"); 

                        //}
                        if (hero != Hero.OneToOneConversationHero 
                            && Campaign.Current.Models.RomanceModel.CourtshipPossibleBetweenNPCs(Hero.MainHero, hero) 
                            && Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
                        {
                            MBTextManager.SetTextVariable("COURTSHIP_PARTNER", hero.Name, false);
                            MAHelper.Print("TEST SUCCESS");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool conversation_marriage_barter_successful_on_condition()
        {
            return Campaign.Current.BarterManager.LastBarterIsAccepted;
        }

        private void conversation_courtship_success_on_consequence()
        {
            ISettingsProvider settings = new MASettings();
            Hero hero = Hero.MainHero;
            Hero spouse = Hero.OneToOneConversationHero;
            Hero oldSpouse = hero.Spouse;
            Hero cheatedSpouse = spouse.Spouse;

            // If you are marrying a kingdom ruler as a kingdom ruler yourself,
            // the kingdom ruler will have to give up being clan head.
            // Apparently causes issues if this is not done.
            if (spouse.IsFactionLeader && !spouse.IsMinorFactionHero)
            {
                if (hero.Clan.Kingdom != spouse.Clan.Kingdom)
                {
                    if (hero.Clan.Kingdom?.Leader != hero)
                    {
                        // Join kingdom due to lowborn status
                        if (hero.Clan.Leader == Hero.MainHero)
                        {
                            ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(hero.Clan);
                            if (hero.Clan.Leader == Hero.MainHero)
                            {
                                MAHelper.Print("No Heirs");
                                DestroyClanAction.Apply(hero.Clan);
                                MAHelper.Print("Eliminated Player Clan");
                            }
                        }
                        foreach (Hero companion in hero.Clan.Companions.ToList())
                        {
                            bool inParty = false;
                            if (companion.PartyBelongedTo == MobileParty.MainParty)
                            {
                                inParty = true;
                            }
                            RemoveCompanionAction.ApplyByFire(hero.Clan, companion);
                            AddCompanionAction.Apply(spouse.Clan, companion);
                            if (inParty)
                            {
                                AddHeroToPartyAction.Apply(companion, MobileParty.MainParty, true);
                            }
                        }
                        hero.Clan = spouse.Clan;
                        var current = Traverse.Create<Campaign>().Property("Current").GetValue<Campaign>();
                        Traverse.Create(current).Property("PlayerDefaultFaction").SetValue(spouse.Clan);
                        MAHelper.Print("Lowborn Player Married to Kingdom Ruler");
                    }
                    else
                    {
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        MAHelper.Print("Kingdom Ruler Stepped Down and Married to Player");
                    }
                }
            }
            // New nobility
            MAHelper.OccupationToLord(spouse.CharacterObject);
            if (!spouse.IsNoble)
            {
                spouse.IsNoble = true;
                MAHelper.Print("Spouse to Noble");
            }
            // Dodge the party crash for characters part 1
            bool dodge = false;
            if (spouse.PartyBelongedTo == MobileParty.MainParty)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, null, null);
                MAHelper.Print("Spouse Already in Player's Party");
                dodge = true;
            }
            // Apply marriage
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            MAHelper.Print("Marriage Action Applied");
            if (oldSpouse is not null)
            {
                MAHelper.RemoveExSpouses(oldSpouse);
            }
            // Dodge the party crash for characters part 2
            if (dodge)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, MobileParty.MainParty, null);
            }
            // Activate character if not already activated
            if (!spouse.HasMet)
            {
                spouse.HasMet = true;
            }
            if (!spouse.IsActive)
            {
                spouse.ChangeState(Hero.CharacterStates.Active);
                MAHelper.Print("Activated Spouse");
            }
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
                MAHelper.Print("Spouse No Longer Companion");
            }
            if (settings.Cheating && cheatedSpouse is not null)
            {
                MAHelper.RemoveExSpouses(cheatedSpouse, true);
                MAHelper.RemoveExSpouses(spouse, true);
                MAHelper.Print("Spouse Broke Off Past Marriage");
            }
            MAHelper.RemoveExSpouses(hero);
            MAHelper.RemoveExSpouses(spouse);
            PlayerEncounter.LeaveEncounter = true;
            // New fix to stop some kingdom rulers from disappearing
            if (spouse.PartyBelongedTo != MobileParty.MainParty)
            {
                AddHeroToPartyAction.Apply(spouse, MobileParty.MainParty, true);
            }
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}