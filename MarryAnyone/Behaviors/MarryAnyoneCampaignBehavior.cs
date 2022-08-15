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
using System.Runtime;

namespace MarryAnyone.Behaviors
{
    internal class MarryAnyoneCampaignBehavior : CampaignBehaviorBase
    {
        public static MarryAnyoneCampaignBehavior? Instance;

        private Hero? _hero = null;

        private static Dictionary<int, Hero>? _heroes;

        private static List<int>? _courted;

        private int _key;

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
            //starter.AddPlayerLine("hero_romance_task_pt3b", "hero_main_options", "hero_courtship_final_barter", "{=jd4qUGEA}I wish to discuss the final terms of my marriage with {COURTSHIP_PARTNER}.", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_finalize_courtship_for_other_on_condition), null, 100, null, null);

            //starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);

            //starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 100, null);
            //starter.AddDialogLine("hero_courtship_final_barter_setup", "hero_courtship_final_barter_conclusion", "close_window", "{=iunPaMFv}I guess we should put this aside, for now. But perhaps we can speak again at a later date.", () => !RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition(), null, 100, null);
        }

        private void RomanceCharacter(CampaignGameStarter starter, string start, string end = "close_window")
        {
            /* LordConversationCampaignBehavior */
            // There is something to discuss
            starter.AddPlayerLine("MA_main_option_discussions_3", start, start + "lord_talk_speak_diplomacy_MA", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), new ConversationSentence.OnConsequenceDelegate(create_new_hero_consequence), 100, null, null);
            // Reply to inquiry
            starter.AddDialogLine("MA_conversation_lord_agrees_to_discussion_on_condition", start + "lord_talk_speak_diplomacy_MA", start + "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", null, null, 100, null);

            // Need to initiate a romance option since we are going off the usual path
            starter.AddPlayerLine("lord_special_request_flirt", start + "lord_talk_speak_diplomacy_2", "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(RomanceCampaignBehaviorPatches.conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(RomanceCampaignBehaviorPatches.conversation_player_opens_courtship_on_consequence), 100, null, null);

            /* RomanceCamapignBehavior */
            // Need extra stuff when leaving convo for first time...
            starter.AddDialogLine("MA_lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(RomanceCampaignBehaviorPatches.courtship_conversation_leave_on_consequence), 200, null);

            /* LordConversationCampaignBehavior */
            // AddFinalLines
            starter.AddPlayerLine("MA_hero_special_request", start + "_lord_talk_speak_diplomacy_2", end, "{=PznWhAdU}Actually, never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 1, null, null);
        }

        public void RemoveHeroObjectFromCharacter()
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
            Settings settings = new();

            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (!_heroes!.ContainsKey(_key))
            {
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

        private bool conversation_hero_main_options_discussions()
        {
            if (Hero.OneToOneConversationHero is not null)
            {
                // Returns false if it is a Lord
                if (SubModule.LordConversationsCampaignBehaviorInstance!.conversation_hero_main_options_discussions())
                {
                    return false;
                }
            }

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
            if (IsAttractedToCharacter())
            {
                _key = MathF.Abs(Campaign.Current.ConversationManager.OneToOneConversationAgent.GetHashCode());

                if (Hero.OneToOneConversationHero is null)
                {
                    if (_courted!.Contains(_key))
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            }
            return false;
        }

        private bool IsAttractedToCharacter()
        {
            Settings settings = new();

            Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (agent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                bool isAttracted = true;
                if (settings.SexualOrientation == "Heterosexual")
                {
                    isAttracted = agent.IsFemale != Hero.MainHero.IsFemale;
                }
                if (settings.SexualOrientation == "Homosexual")
                {
                    isAttracted = agent.IsFemale == Hero.MainHero.IsFemale;
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