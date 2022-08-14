﻿using HarmonyLib;
using Helpers;
using MarryAnyone.Helpers;
using MarryAnyone.Patches.Behaviors;
using SandBox.CampaignBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.Behaviors
{
    internal class MarryAnyoneCampaignBehavior : CampaignBehaviorBase
    {
        public static RomanceCampaignBehavior? RomanceCampaignBehaviorInstance;

        public static LordConversationsCampaignBehavior? LordConversationsCampaignBehaviorInstance;

        private Hero? _hero = null;

        private static Dictionary<int, Hero>? _heroes;

        private static List<int>? _courted;

        private int _key;

        public MarryAnyoneCampaignBehavior()
        {
            _heroes = new();
            _courted = new();
            RomanceCampaignBehaviorInstance = new();
            LordConversationsCampaignBehaviorInstance = new();
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            /* RecruitEveryone-like dialogs that can handle hero creation process if needed */
            RomanceCharacter(starter, "hero_main_options", "lord_pretalk");
            RomanceCharacter(starter, "tavernkeeper_talk", "tavernkeeper_pretalk");
            RomanceCharacter(starter, "tavernmaid_talk");
            RomanceCharacter(starter, "talk_bard_player");
            RomanceCharacter(starter, "taverngamehost_talk", "start");
            RomanceCharacter(starter, "town_or_village_player", "town_or_village_pretalk");
            RomanceCharacter(starter, "weaponsmith_talk_player", "merchant_response_3");
            RomanceCharacter(starter, "barber_question1", "no_haircut_conversation_token");
            RomanceCharacter(starter, "shopworker_npc_player", "start_2");
            RomanceCharacter(starter, "blacksmith_player", "player_blacksmith_after_craft");
            RomanceCharacter(starter, "alley_talk_start");

            /* After the initial romance option */
            starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 100, null, null);
            starter.AddPlayerLine("hero_romance_task_pt3b", "hero_main_options", "hero_courtship_final_barter", "{=jd4qUGEA}I wish to discuss the final terms of my marriage with {COURTSHIP_PARTNER}.", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_finalize_courtship_for_other_on_condition), null, 100, null, null);

            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);

            starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 100, null);
            starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=iunPaMFv}I guess we should put this aside, for now. But perhaps we can speak again at a later date.", () => !RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition(), null, 100, null);
        }

        private void RomanceCharacter(CampaignGameStarter starter, string start, string end = "close_window")
        {
            /* LordConversationCampaignBehavior */
            // There is something to discuss
            starter.AddPlayerLine("MA_main_option_discussions_3", start, start + "_lord_talk_speak_diplomacy_MA", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), new ConversationSentence.OnConsequenceDelegate(create_new_hero_consequence), 101, null, null);
            // Reply to inquiry
            starter.AddDialogLine("MA_conversation_lord_agrees_to_discussion_on_condition", start + "_lord_talk_speak_diplomacy_MA", start + "_lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(LordConversationsCampaignBehaviorPatches.conversation_lord_agrees_to_discussion_on_condition), null, 100, null);

            /* RomanceCamapignBehavior */
            // Need extra stuff when leaving convo for first time...
            starter.AddDialogLine("MA_lord_start_courtship_response_3", start + "_lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(RomanceCampaignBehaviorPatches.courtship_conversation_leave_on_consequence), 100, null);

            /* LordConversationCampaignBehavior */
            // AddFinalLines
            starter.AddPlayerLine("MA_hero_special_request", start + "_lord_talk_speak_diplomacy_2", end, "{=PznWhAdU}Actually, never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 1, null, null);
        }

        private void RemoveHeroObjectFromCharacter()
        {
            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            // Remove hero association from character
            if (character.HeroObject is not null)
            {
                // character.HeroObject = null;
                AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(character, null);
            }
        }

        private void conversation_exit_consequence()
        {
            RemoveHeroObjectFromCharacter();

            // Learned that this is used in some Issues to disable quest heroes!
            DisableHeroAction.Apply(_hero);
        }

        private void create_new_hero_consequence()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                _key = MathF.Abs(Campaign.Current.ConversationManager.OneToOneConversationAgent.GetHashCode());

                CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
                Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

                if (!_heroes!.ContainsKey(_key))
                {
                    // Create a new hero!
                    _hero = HeroCreator.CreateSpecialHero(character, Hero.MainHero.CurrentSettlement, null, null, (int)agent.Age);

                    // Meet character for first time
                    _hero.HasMet = true;

                    // Add hero to heroes list
                    _heroes.Add(_key, _hero);

                    // Give hero the agent's appearance
                    // hero.StaticBodyProperties = agent.BodyPropertiesValue.StaticProperties;
                    AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(_hero, agent.BodyPropertiesValue.StaticProperties);

                    // Give hero the agent's clothes
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(_hero, agent.SpawnEquipment.Clone());
                }
                else
                {
                    // Use existing hero
                    _heroes.TryGetValue(_key, out _hero);
                }

                // Attach hero to character for now
                if (character.HeroObject is null)
                {
                    // character.HeroObject = _hero;
                    AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(character, _hero);
                }
            }
        }

        private bool conversation_hero_main_options_discussions()
        {
            if (Hero.OneToOneConversationHero is not null)
            {
                // Returns false if it is a Lord
                if (LordConversationsCampaignBehaviorInstance!.conversation_hero_main_options_discussions())
                {
                    return false;
                }
            }

            Settings settings = new();

            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if ((int)agent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (settings.Debug)
                {
                    MADebug.Print("Orientation: " + settings.SexualOrientation);
                    MADebug.Print("Cheating: " + settings.Cheating);
                    MADebug.Print("Polygamy: " + settings.Polygamy);
                    MADebug.Print("Incest: " + settings.Incest);
                    if (Hero.OneToOneConversationHero is not null)
                    {
                        MADebug.Print("Romantic Level: " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
                    }
                }
                return true;
            }
            return false;
        }

        // This will either skip or continue the romance
        // CoupleAgreedOnMarriage = triggers marriage before bartering
        // CourtshipStarted = skip everything
        // return false = carry out entire romance
        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);

            bool clanLeader = Hero.MainHero.Clan.Leader == Hero.MainHero && Hero.MainHero.Clan.Lords.Contains(Hero.OneToOneConversationHero);

            if (clanLeader)
            {
                return RomanceCampaignBehaviorPatches.MarriageCourtshipPossibility(RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            }
            return false;
        }

        private void conversation_courtship_success_on_consequence()
        {
            Settings settings = new();

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
                                MADebug.Print("No Heirs");
                                DestroyClanAction.Apply(hero.Clan);
                                MADebug.Print("Eliminated Player Clan");
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
                        MADebug.Print("Lowborn Player Married to Kingdom Ruler");
                    }
                    else
                    {
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        MADebug.Print("Kingdom Ruler Stepped Down and Married to Player");
                    }
                }
            }
            // New nobility
            MAHelper.OccupationToLord(spouse);
            if (spouse.Clan.IsNoble)
            {
                spouse.Clan.IsNoble = true;
                MADebug.Print("Spouse to Noble");
            }
            // Dodge the party crash for characters part 1
            //bool dodge = false;
            //if (spouse.PartyBelongedTo == MobileParty.MainParty)
            //{
            //    AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, null, null);
            //    MADebug.Print("Spouse Already in Player's Party");
            //    dodge = true;
            //}
            // Apply marriage
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            MADebug.Print("Marriage Action Applied");
            if (oldSpouse is not null)
            {
                MAHelper.RemoveExSpouses(oldSpouse);
            }
            // Dodge the party crash for characters part 2
            //if (dodge)
            //{
            //    AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, MobileParty.MainParty, null);
            //}
            // Activate character if not already activated
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
                MADebug.Print("Spouse No Longer Companion");
            }
            if (settings.Cheating && cheatedSpouse is not null)
            {
                MAHelper.RemoveExSpouses(cheatedSpouse, true);
                MAHelper.RemoveExSpouses(spouse, true);
                MADebug.Print("Spouse Broke Off Past Marriage");
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