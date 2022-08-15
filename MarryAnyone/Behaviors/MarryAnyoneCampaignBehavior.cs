using HarmonyLib;
using MarryAnyone.Helpers;
using MarryAnyone.Patches.Behaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.MountAndBlade;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace MarryAnyone.Behaviors
{
    internal class MarryAnyoneCampaignBehavior : CampaignBehaviorBase
    {
        public static MarryAnyoneCampaignBehavior? Instance;

        private Hero? _hero = null;

        private static Dictionary<int, Hero>? _heroes;

        private static List<int>? _courted;

        private static int _key
        {
            get
            {
                return MathF.Abs(Campaign.Current.ConversationManager.OneToOneConversationAgent.GetHashCode());
            }
        }

        public MarryAnyoneCampaignBehavior()
        {
            Instance = this;
            _heroes = new();
            _courted = new();
            SubModule.RomanceCampaignBehaviorInstance = new();
            SubModule.CompanionsCampaignBehaviorInstance = new();
            SubModule.LordConversationsCampaignBehaviorInstance = new();
            SubModule.CharacterDevelopmentCampaignBehaviorInstance = new();
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
            //starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 100, null, null);

            //starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);

            //starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 100, null);
            //starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=iunPaMFv}I guess we should put this aside, for now. But perhaps we can speak again at a later date.", () => !RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition(), null, 100, null);
        }

        private void RomanceCharacter(CampaignGameStarter starter, string start, string end = "close_window")
        {
            /* LordConversationCampaignBehavior */
            // There is something to discuss
            starter.AddPlayerLine("MA_main_option_discussions_3", start, start + "lord_talk_speak_diplomacy", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), new ConversationSentence.OnConsequenceDelegate(create_new_hero_consequence), 100, null, null);
            // Reply to inquiry
            starter.AddDialogLine("MA_conversation_lord_agrees_to_discussion_on_condition", start + "lord_talk_speak_diplomacy", start + "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", null, null, 100, null);
            // AddFinalLines
            starter.AddPlayerLine("MA_hero_special_request", start + "_lord_talk_speak_diplomacy_2", end, "{=PznWhAdU}Actually, never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 1, null, null);

            /* RomanceCamapignBehavior */
            // Need to initiate a romance option since we are going off the usual path
            starter.AddPlayerLine("MA_lord_special_request_flirt", start + "lord_talk_speak_diplomacy_2", start + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            // Leave now if you aren't willing
            starter.AddPlayerLine("lord_start_courtship_response_player_offer_nevermind", "lord_start_courtship_response_player_offer", "lord_pretalk", "{=D33fIGQe}Never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 120, null, null);
            // Continue courtship part 1
            starter.AddDialogLine("MA_lord_start_courtship_response", start + "lord_start_courtship_response", start + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            starter.AddPlayerLine("MA_lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            // Need extra stuff when leaving convo for first time...
            starter.AddDialogLine("MA_lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(courtship_conversation_leave_on_consequence), 200, null);
        }

        private void courtship_conversation_leave_on_consequence()
        {
            Settlement settlement = Hero.MainHero.CurrentSettlement;

            _hero!.SetNewOccupation(Occupation.Wanderer);
            _hero.StayingInSettlement = settlement;
            _hero.ChangeState(Hero.CharacterStates.Active);

            RomanceCampaignBehaviorPatches.courtship_conversation_leave_on_consequence_patch(SubModule.RomanceCampaignBehaviorInstance!);

            _courted!.Add(_key);
            RemoveHeroObjectFromCharacter();
        }

        private bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            return Hero.OneToOneConversationHero is not null && RomanceCampaignBehaviorPatches.MarriageCourtshipPossibilityPatch(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private void conversation_player_opens_courtship_on_consequence()
        {
            RomanceCampaignBehaviorPatches.conversation_player_opens_courtship_on_consequence_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                return false;
            }
            MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
            return true;
        }

        private bool conversation_finalize_courtship_for_other_on_condition()
        {
            return RomanceCampaignBehaviorPatches.conversation_finalize_courtship_for_other_on_condition_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        public bool conversation_marriage_barter_successful_on_condition()
        {
            return RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition_patch(SubModule.RomanceCampaignBehaviorInstance!);
        }

        public void RemoveHeroObjectFromCharacter()
        {
            if (_heroes!.ContainsKey(_key))
            {
                CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

                // Remove hero association from character
                if (character.HeroObject is not null)
                {
                    // character.HeroObject = null;
                    AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(character, null);
                }
            }
        }

        private void conversation_exit_consequence()
        {
            if (_heroes!.ContainsKey(_key))
            {
                RemoveHeroObjectFromCharacter();

                // Learned that this is used in some Issues to disable quest heroes!
                DisableHeroAction.Apply(_hero);
            }
        }

        private void create_new_hero_consequence()
        {
            Settings settings = new();
            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            if (Hero.OneToOneConversationHero is null)
            {
                if (!_heroes!.ContainsKey(_key))
                {
                    Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

                    CharacterObject template = character;
                    // CompanionCampaignBehavior -> IntializeCompanionTemplateList()
                    if (settings.TemplateCharacter == "Wanderer")
                    {
                        // Give hero random wanderer's focus, skills, and combat equipment with same culture and sex
                        template = character.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale == character.IsFemale);
                    }

                    // Create a new hero!
                    _hero = HeroCreator.CreateSpecialHero(template, Hero.MainHero.CurrentSettlement, null, null, (int)agent.Age);

                    // Meet character for first time
                    _hero.HasMet = true;

                    // Add hero to heroes list
                    _heroes.Add(_key, _hero);

                    // Give hero the agent's appearance
                    // hero.StaticBodyProperties = agent.BodyPropertiesValue.StaticProperties;
                    AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(_hero, agent.BodyPropertiesValue.StaticProperties);

                    // Give hero agent's equipment
                    Equipment civilianEquipment = agent.SpawnEquipment.Clone();
                    // CharacterObject -> RandomBattleEquipment
                    Equipment battleEquipment = template.AllEquipments.GetRandomElementWithPredicate((Equipment e) => !e.IsCivilian).Clone();
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(_hero, civilianEquipment);
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(_hero, battleEquipment);
                    // Do equipment adjustment with companions
                    // Not exactly sure what it does...
                    // this.AdjustEquipment(_hero);
                    AccessTools.Method(typeof(CompanionsCampaignBehavior), "AdjustEquipment").Invoke(SubModule.CompanionsCampaignBehaviorInstance, new object[] { _hero });

                    HeroHelper.DetermineInitialLevel(_hero);
                    SubModule.CharacterDevelopmentCampaignBehaviorInstance!.DevelopCharacterStats(_hero);
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
            Settings settings = new();
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
            if (Hero.OneToOneConversationHero is not null)
            {
                // Returns false if it is a Lord
                if (SubModule.LordConversationsCampaignBehaviorInstance!.conversation_hero_main_options_discussions())
                {
                    return false;
                }
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
                {
                    return false;
                }
            }
            if (IsAttractedToCharacter())
            {
                return true;
            }
            return false;
        }

        private bool IsAttractedToCharacter()
        {
            Settings settings = new();
            
            IAgent agent = Campaign.Current.ConversationManager.OneToOneConversationAgent;

            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            if (agent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                bool isAttracted = true;
                if (settings.SexualOrientation == "Heterosexual")
                {
                    isAttracted = character.IsFemale != Hero.MainHero.IsFemale;
                }
                if (settings.SexualOrientation == "Homosexual")
                {
                    isAttracted = character.IsFemale == Hero.MainHero.IsFemale;
                }
                return isAttracted;
            }
            return false;
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