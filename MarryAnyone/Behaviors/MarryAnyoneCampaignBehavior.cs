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
using MarryAnyone.Actions;
using System.Runtime;

namespace MarryAnyone.Behaviors
{
    internal class MarryAnyoneCampaignBehavior : CampaignBehaviorBase
    {
        public static MarryAnyoneCampaignBehavior? Instance { get; private set; }

        private CharacterObject ConversationCharacter
        {
            get => Campaign.Current.ConversationManager.OneToOneConversationCharacter;
        }

        private IAgent ConversationAgent
        {
            get => Campaign.Current.ConversationManager.OneToOneConversationAgent;
        }

        private int AgentKey
        {
            get => MathF.Abs(ConversationAgent.GetHashCode());
        }

        private Hero? _companionHero;

        private Dictionary<int, Hero> _heroes;

        public MarryAnyoneCampaignBehavior()
        {
            Instance = this;
            _heroes = new();
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
            // Skip barter and marry immediately
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_2_success", "hero_courtship_task_2_next_reservation", "close_window", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(marriage_on_condition), new ConversationSentence.OnConsequenceDelegate(marriage_on_consequence), 200, null);
        }

        private void RomanceCharacter(CampaignGameStarter starter, string start, string end = "close_window")
        {
            /* LordConversationCampaignBehavior */
            // There is something to discuss
            starter.AddPlayerLine("MA" + "main_option_discussions_3", start, start + "lord_talk_speak_diplomacy", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), new ConversationSentence.OnConsequenceDelegate(create_new_hero_consequence), 100, null, null);
            // Reply to inquiry
            starter.AddDialogLine("MA" + "conversation_lord_agrees_to_discussion_on_condition", start + "lord_talk_speak_diplomacy", start + "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(conversation_lord_agrees_to_discussion_on_condition), null, 100, null);
            // AddFinalLines
            starter.AddPlayerLine("MA" + "hero_special_request", start + "lord_talk_speak_diplomacy_2", end, "{=PznWhAdU}Actually, never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 1, null, null);

            /* RomanceCamapignBehavior */
            // Player starts new courtship
            starter.AddPlayerLine("MA" + "lord_special_request_flirt", start + "lord_talk_speak_diplomacy_2", start + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            // Initial courtship reaction
            starter.AddDialogLine("MA" + "lord_start_courtship_response", start + "lord_start_courtship_response", start + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            // Decline courtship reaction
            starter.AddDialogLine("MA" + "lord_start_courtship_response_decline", start + "lord_start_courtship_response", end, "{=!}{COURTSHIP_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_decline_reaction_to_player_on_condition), null, 100, null);

            // Skip courtship option
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "hero_courtship_task_2_next_reservation", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "hero_courtship_task_2_next_reservation", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            // After initial courtship ask for hand in marriage
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            // Leave if not ready
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_nevermind", start + "lord_start_courtship_response_player_offer", end, "{=D33fIGQe}Never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 120, null, null);

            // This will occur first before the original dialog
            starter.AddDialogLine("MA" + "lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(first_time_courtship_conversation_leave_on_consequence), 200, null);
        }

        private bool marriage_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                return true;
            }
            return false;
        }

        private void first_time_courtship_conversation_leave_on_consequence()
        {
            if (_companionHero is null)
            {
                return;
            }

            // Setup the hero to be free
            Settlement settlement = Hero.MainHero.CurrentSettlement;
            if (_companionHero.Occupation != Occupation.Wanderer)
            {
                _companionHero.SetNewOccupation(Occupation.Wanderer);
            }
            if (_companionHero.StayingInSettlement != settlement)
            {
                _companionHero.StayingInSettlement = settlement;
            }
            if (_companionHero.HeroState != Hero.CharacterStates.Active)
            {
                _companionHero.ChangeState(Hero.CharacterStates.Active);
            }

            // Start courship consequence after hero is setup
            RomanceCampaignBehaviorPatches.courtship_conversation_leave_on_consequence(SubModule.RomanceCampaignBehaviorInstance!);

            // Remove hero object from character if required
            if (_heroes.ContainsKey(AgentKey))
            {
                RemoveHeroObjectFromCharacter();
            }
        }

        private bool conversation_lord_agrees_to_discussion_on_condition()
        {
            // Get a random lord response that is valid
            CharacterObject randomLord = CharacterObject.All.GetRandomElementWithPredicate((CharacterObject x) => 
                Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", x) is not null);
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", randomLord));
            return true;
        }

        private bool skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            MASettings settings = new();
            return settings.SkipCourtship && Hero.OneToOneConversationHero is not null &&
                RomanceCampaignBehaviorPatches.MarriageCourtshipPossibility(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            MASettings settings = new();
            return !settings.SkipCourtship && Hero.OneToOneConversationHero is not null && 
                RomanceCampaignBehaviorPatches.MarriageCourtshipPossibility(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private void conversation_player_opens_courtship_on_consequence()
        {
            RomanceCampaignBehaviorPatches.conversation_player_opens_courtship_on_consequence(SubModule.RomanceCampaignBehaviorInstance!);
        }

        private void marriage_on_consequence()
        {
            // Couple agreed on marriage
            RomanceCampaignBehaviorPatches.conversation_courtship_stage_2_success_on_consequence(SubModule.RomanceCampaignBehaviorInstance!);
            // Apply marriage action
            MarryAnyoneMarriageAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, true);
        }

        private bool conversation_courtship_decline_reaction_to_player_on_condition()
        {
            return RomanceCampaignBehaviorPatches.conversation_courtship_decline_reaction_to_player_on_condition(SubModule.RomanceCampaignBehaviorInstance!);
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

        public bool conversation_marriage_barter_successful_on_condition()
        {
            return RomanceCampaignBehaviorPatches.conversation_marriage_barter_successful_on_condition(SubModule.RomanceCampaignBehaviorInstance!);
        }

        public void RemoveHeroObjectFromCharacter()
        {
            // Remove hero association from character
            if (ConversationCharacter.HeroObject is not null)
            {
                // character.HeroObject = null;
                AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(ConversationCharacter, null);
            }
            if (_companionHero is null)
            {
                return;
            }
            // Name permanence from the adoption module of old
            if (((Agent)ConversationAgent).Name != _companionHero.Name.ToString())
            {
                AccessTools.Field(typeof(Agent), "_name").SetValue((Agent)ConversationAgent, _companionHero.Name);
            }
        }

        private void conversation_exit_consequence()
        {
            if (!_heroes.ContainsKey(AgentKey) || _companionHero is null)
            {
                return;
            }

            RemoveHeroObjectFromCharacter();

            // Learned that this is used in some Issues to disable quest heroes!
            DisableHeroAction.Apply(_companionHero);
        }

        private void create_new_hero_consequence()
        {
            if (Hero.OneToOneConversationHero is not null)
            {
                return;
            }

            if (_heroes.ContainsKey(AgentKey))
            {
                // Use existing hero
                _heroes.TryGetValue(AgentKey, out _companionHero);
                // character.HeroObject = _companionHero;
                AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(ConversationCharacter, _companionHero);
                return;
            }

            MASettings settings = new();
            CharacterObject template = ConversationCharacter;
            // CompanionCampaignBehavior -> IntializeCompanionTemplateList()
            if (settings.TemplateCharacter == "Wanderer")
            {
                // Give hero random wanderer's focus, skills, and combat equipment with same culture and sex
                template = ConversationCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale == ConversationCharacter.IsFemale);
            }

            // Create a new hero!
            _companionHero = HeroCreator.CreateSpecialHero(template, Hero.MainHero.CurrentSettlement, null, null, (int)ConversationAgent.Age);

            // Meet character for first time
            _companionHero.HasMet = true;

            // Add hero to heroes list
            _heroes.Add(AgentKey, _companionHero);

            // Give hero the agent's appearance
            // hero.StaticBodyProperties = agent.BodyPropertiesValue.StaticProperties;
            AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(_companionHero, ((Agent)ConversationAgent).BodyPropertiesValue.StaticProperties);

            // Give hero agent's equipment
            Equipment civilianEquipment = ((Agent)ConversationAgent).SpawnEquipment.Clone();
            // CharacterObject -> RandomBattleEquipment
            Equipment battleEquipment = template.AllEquipments.GetRandomElementWithPredicate((Equipment e) => !e.IsCivilian).Clone();
            EquipmentHelper.AssignHeroEquipmentFromEquipment(_companionHero, civilianEquipment);
            EquipmentHelper.AssignHeroEquipmentFromEquipment(_companionHero, battleEquipment);
            // Do equipment adjustment with companions
            // Not exactly sure what it does...
            // this.AdjustEquipment(_companionHero);
            AccessTools.Method(typeof(CompanionsCampaignBehavior), "AdjustEquipment").Invoke(SubModule.CompanionsCampaignBehaviorInstance, new object[] { _companionHero });

            HeroHelper.DetermineInitialLevel(_companionHero);
            SubModule.CharacterDevelopmentCampaignBehaviorInstance!.DevelopCharacterStats(_companionHero);

            // character.HeroObject = _companionHero;
            AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(ConversationCharacter, _companionHero);
        }

        private bool conversation_hero_main_options_discussions()
        {
            // Leave patch accounts for agents that are temporary heroes
            MASettings settings = new();
            MADebug.Print("Orientation: " + settings.SexualOrientation);
            MADebug.Print("Cheating: " + settings.Cheating);
            MADebug.Print("Polygamy: " + settings.Polygamy);
            MADebug.Print("Incest: " + settings.Incest);
            // Hero patch for courtship condition
            if (Hero.OneToOneConversationHero is not null)
            {
                // Lords will go the old fashion way!
                if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
                {
                    return false;
                }
                return conversation_player_can_open_courtship_on_condition();
            }
            // For agents
            if (IsAttractedToCharacter())
            {
                return true;
            }
            return false;
        }

        private bool IsAttractedToCharacter()
        {
            MASettings settings = new();

            CharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            if (ConversationAgent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
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

        public bool conversation_player_can_open_courtship_on_condition()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }
            MASettings settings = new();
            bool flag = Hero.MainHero.IsFemale && settings.SexualOrientation == "Heterosexual"
                || !Hero.MainHero.IsFemale && settings.SexualOrientation == "Homosexual"
                || !Hero.OneToOneConversationHero.IsFemale && settings.SexualOrientation == "Bisexual";

            Romance.RomanceLevelEnum romanceLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
                        MADebug.Print("Romantic Level: " + romanceLevel);
            MADebug.Print("Courtship Possible: " + RomanceCampaignBehaviorPatches.MarriageCourtshipPossibility(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero));
            MADebug.Print("Retry Courtship: " + settings.RetryCourtship);

            if (RomanceCampaignBehaviorPatches.MarriageCourtshipPossibility(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero)
                && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE",
                    flag
                        ? "{=goodman_flirt}Goodman, I note that you have not yet taken a spouse."
                        : "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer.", false);
                return true;
            }
            if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility
                || romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                || (romanceLevel == Romance.RomanceLevelEnum.Ended && settings.RetryCourtship))
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE",
                    flag
                        ? "{=goodman_chance}Goodman, may you give me another chance to prove myself?"
                        : "{=goodwife_chance}Goodwife, may you give me another chance to prove myself?", false);
                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanceLevel == Romance.RomanceLevelEnum.Ended)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
                    }
                    else if (romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                    }
                }
                return true;
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