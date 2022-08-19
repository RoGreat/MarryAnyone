using HarmonyLib;
using MarryAnyone.Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.MountAndBlade;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using MarryAnyone.Actions;
using HarmonyLib.BUTR.Extensions;

namespace MarryAnyone.Behaviors
{
    internal sealed class MarryAnyoneCampaignBehavior : RomanceCampaignBehavior
    {
        // public static MarryAnyoneCampaignBehavior? Instance { get; private set; }

        private Hero? _companionHero;

        private Dictionary<Agent, Hero> _heroes;

        public MarryAnyoneCampaignBehavior()
        {
            // Instance = this;
            _heroes = new();
        }

        /* Patching private methods and fields in RomanceCampaignBehavior */
        // https://butr.github.io/documentation/advanced/switching-from-membertinfo-to-accesstools2/

        // private List<PersuasionTask> _allReservations
        private static readonly AccessTools.FieldRef<RomanceCampaignBehavior, List<PersuasionTask>>? _allReservations = AccessTools2.FieldRefAccess<RomanceCampaignBehavior, List<PersuasionTask>>("_allReservations");

        // private bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        private delegate bool MarriageCourtshipPossibilityDelegate(RomanceCampaignBehavior instance, Hero person1, Hero person2);
        private static readonly MarriageCourtshipPossibilityDelegate MarriageCourtshipPossibility = AccessTools2.GetDelegate<MarriageCourtshipPossibilityDelegate>(typeof(RomanceCampaignBehavior), "MarriageCourtshipPossibility", new Type[] { typeof(Hero), typeof(Hero) });

        /* Conditions */
        // private bool conversation_finalize_courtship_for_hero_on_condition()
        private delegate bool conversation_finalize_courtship_for_hero_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_finalize_courtship_for_hero_on_condition_delegate conversation_finalize_courtship_for_hero_on_condition = AccessTools2.GetDelegate<conversation_finalize_courtship_for_hero_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_finalize_courtship_for_hero_on_condition");

        // private bool conversation_courtship_decline_reaction_to_player_on_condition()
        private delegate bool conversation_courtship_decline_reaction_to_player_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_courtship_decline_reaction_to_player_on_condition_delegate conversation_courtship_decline_reaction_to_player_on_condition = AccessTools2.GetDelegate<conversation_courtship_decline_reaction_to_player_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_courtship_decline_reaction_to_player_on_condition");

        // private void conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        private delegate bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate conversation_player_eligible_for_marriage_with_conversation_hero_on_condition = AccessTools2.GetDelegate<conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_player_eligible_for_marriage_with_conversation_hero_on_condition");

        // private void conversation_courtship_reaction_to_player_on_condition()
        private delegate bool conversation_courtship_reaction_to_player_on_condition_delegate (RomanceCampaignBehavior instance);
        private static readonly conversation_courtship_reaction_to_player_on_condition_delegate conversation_courtship_reaction_to_player_on_condition = AccessTools2.GetDelegate<conversation_courtship_reaction_to_player_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_courtship_reaction_to_player_on_condition");

        /* Consequences */
        // private void courtship_conversation_leave_on_consequence()
        private delegate void courtship_conversation_leave_on_consequence_delegate(RomanceCampaignBehavior instance);
        private static readonly courtship_conversation_leave_on_consequence_delegate courtship_conversation_leave_on_consequence = AccessTools2.GetDelegate<courtship_conversation_leave_on_consequence_delegate>(typeof(RomanceCampaignBehavior), "courtship_conversation_leave_on_consequence");

        // private void conversation_player_opens_courtship_on_consequence
        private delegate void conversation_player_opens_courtship_on_consequence_delegate (RomanceCampaignBehavior instance);
        private static readonly conversation_player_opens_courtship_on_consequence_delegate conversation_player_opens_courtship_on_consequence = AccessTools2.GetDelegate<conversation_player_opens_courtship_on_consequence_delegate>(typeof(RomanceCampaignBehavior), "conversation_player_opens_courtship_on_consequence");

        // private void conversation_courtship_stage_2_success_on_consequence()
        private delegate void conversation_courtship_stage_2_success_on_consequence_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_courtship_stage_2_success_on_consequence_delegate conversation_courtship_stage_2_success_on_consequence = AccessTools2.GetDelegate<conversation_courtship_stage_2_success_on_consequence_delegate>(typeof(RomanceCampaignBehavior), "conversation_courtship_stage_2_success_on_consequence");

        protected new void AddDialogs(CampaignGameStarter starter)
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

            /* Lord skip courtship dialogs */
            // Essentially for each check here turn the marriage flag on
            starter.AddDialogLine("MA" + "lord_start_courtship_response_2", "lord_start_courtship_response_2", "lord_conclude_courtship_stage_2", "{=!}{INITIAL_COURTSHIP_REACTION_TO_PLAYER}", new ConversationSentence.OnConditionDelegate(skip_courtship_conversation_courtship_reaction_to_player_on_condition), new ConversationSentence.OnConsequenceDelegate(skip_courtship_on_consequence), 200, null);
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_start", "hero_courtship_task_1_begin_reservations", "lord_conclude_courtship_stage_2", "{=bW3ygxro}Yes, it's good to have a chance to get to know each other.", new ConversationSentence.OnConditionDelegate(skip_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(skip_courtship_on_consequence), 200, null);
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_2_start", "hero_courtship_task_2_begin_reservations", "lord_conclude_courtship_stage_2", "{=VNFKqpyV}Yes, well, I've been thinking about that.", new ConversationSentence.OnConditionDelegate(skip_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(skip_courtship_on_consequence), 200, null);
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
            starter.AddPlayerLine("MA" + "lord_special_request_flirt", start + "lord_talk_speak_diplomacy_2", start + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), null, 100, null, null);
            // Initial courtship reaction
            starter.AddDialogLine("MA" + "lord_start_courtship_response", start + "lord_start_courtship_response", start + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            // Decline courtship reaction
            starter.AddDialogLine("MA" + "lord_start_courtship_response_decline", start + "lord_start_courtship_response", end, "{=!}{COURTSHIP_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(MA_conversation_courtship_decline_reaction_to_player_on_condition), null, 100, null);

            // Skip courtship option
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "hero_courtship_task_2_next_reservation", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "hero_courtship_task_2_next_reservation", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            // After initial courtship ask for hand in marriage
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            // Leave if not ready
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_nevermind", start + "lord_start_courtship_response_player_offer", end, "{=D33fIGQe}Never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 120, null, null);

            // This will occur first before the original dialog if courtship is not skipped
            starter.AddDialogLine("MA" + "lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", () => Hero.OneToOneConversationHero.Occupation != Occupation.Lord, new ConversationSentence.OnConsequenceDelegate(MA_courtship_conversation_leave_on_consequence), 200, null);
        }

        private bool marriage_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
            {
                return false;
            }
            return true;
        }

        private void marriage_on_consequence()
        {
            MASettings settings = new();
            Hero spouseHero = Hero.OneToOneConversationHero;

            // Skip courtship means there is no prior romance so crash. Crash crash crash...
            if (settings.SkipCourtship)
            {
                _allReservations!(this) = null!;
                ConversationManager.EndPersuasion();
            }
            else
            {
                conversation_courtship_stage_2_success_on_consequence(this);
            }

            if (_companionHero == Hero.OneToOneConversationHero)
            {
                RemoveHeroObjectFromCharacter();
                spouseHero = _companionHero;
            }
            // Change to Lord
            ActivateNewHero(Occupation.Lord);
            // Apply marriage action
            MarryAnyoneMarriageAction.Apply(Hero.MainHero, spouseHero, true);
        }

        private bool skip_courtship_on_condition()
        {
            // Only for lords...
            if (Hero.OneToOneConversationHero is null || Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                return false;
            }
            // Marriage possible from before and check if skip courtship is on
            MASettings settings = new();
            return conversation_player_eligible_for_marriage_with_conversation_hero_on_condition(this) && settings.SkipCourtship;
        }

        private bool skip_courtship_conversation_courtship_reaction_to_player_on_condition()
        {
            // Only for lords...
            if (Hero.OneToOneConversationHero is null || Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                return false;
            }
            // Do the cool dialog stuff Bannerlord is known for
            conversation_courtship_reaction_to_player_on_condition(this);
            // And skip
            return skip_courtship_on_condition();
        }

        private void skip_courtship_on_consequence()
        {
            _allReservations!(this) = null!;
            ConversationManager.EndPersuasion();
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleAgreedOnMarriage);
        }

        private void MA_courtship_conversation_leave_on_consequence()
        {
            Agent conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            // Activate!
            ActivateNewHero(Occupation.Wanderer);

            // Start courtship consequence after hero is setup
            courtship_conversation_leave_on_consequence(this);

            // Remove hero object from character if required
            if (conversationAgent is not null)
            {
                if (_heroes.ContainsKey(conversationAgent))
                {
                    RemoveHeroObjectFromCharacter();
                }
            }
        }

        private void ActivateNewHero(Occupation occupation)
        {
            if (_companionHero is null)
            {
                return;
            }
            // Setup the hero to be free
            Settlement settlement = Hero.MainHero.CurrentSettlement;
            if (_companionHero.Occupation != occupation)
            {
                _companionHero.SetNewOccupation(occupation);
            }
            if (_companionHero.StayingInSettlement != settlement)
            {
                _companionHero.StayingInSettlement = settlement;
            }
            if (_companionHero.HeroState != Hero.CharacterStates.Active)
            {
                _companionHero.ChangeState(Hero.CharacterStates.Active);
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

        private bool MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            MASettings settings = new();
            return settings.SkipCourtship && Hero.OneToOneConversationHero is not null &&
                MarriageCourtshipPossibility(this, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private bool MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            MASettings settings = new();
            return !settings.SkipCourtship && Hero.OneToOneConversationHero is not null && 
                MarriageCourtshipPossibility(this, Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private void MA_conversation_player_opens_courtship_on_consequence()
        {
            conversation_player_opens_courtship_on_consequence(this);
        }

        private bool MA_conversation_courtship_decline_reaction_to_player_on_condition()
        {
            return conversation_courtship_decline_reaction_to_player_on_condition(this);
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities 
                || romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                return false;
            }
            MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
            return true;
        }

        public void RemoveHeroObjectFromCharacter()
        {
            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            // Remove hero association from character
            if (conversationCharacter.HeroObject is not null)
            {
                // character.HeroObject = null;
                AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(conversationCharacter, null);
            }
        }

        private void conversation_exit_consequence()
        {
            Agent conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (conversationAgent is null || _companionHero is null)
            {
                return;
            }

            if (!_heroes.ContainsKey(conversationAgent))
            {
                return;
            }

            RemoveHeroObjectFromCharacter();

            // Learned that this is used in some Issues to disable quest heroes!
            DisableHeroAction.Apply(_companionHero);
        }

        private void create_new_hero_consequence()
        {
            Agent conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (Hero.OneToOneConversationHero is not null || conversationAgent is null)
            {
                return;
            }

            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            // Remembering the last agents you talked to
            if (_heroes.ContainsKey(conversationAgent))
            {
                // Use existing hero
                _heroes.TryGetValue(conversationAgent, out _companionHero);
                // character.HeroObject = _companionHero;
                AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(conversationCharacter, _companionHero);
                return;
            }

            // Remove agents not in scene
            RemoveAgents();

            MASettings settings = new();
            CharacterObject template = conversationCharacter;
            // CompanionCampaignBehavior -> IntializeCompanionTemplateList()
            if (settings.TemplateCharacter == "Wanderer")
            {
                // Give hero random wanderer's focus, skills, and combat equipment with same culture and sex
                template = conversationCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale == conversationCharacter.IsFemale);
            }

            // Create a new hero!
            _companionHero = HeroCreator.CreateSpecialHero(template, Hero.MainHero.CurrentSettlement, null, null, (int)conversationAgent.Age);

            // Name permanence from the adoption module of old
            AccessTools.Field(typeof(Agent), "_name").SetValue(conversationAgent, _companionHero.Name);

            // Meet character for first time
            _companionHero.HasMet = true;

            // Add hero to heroes list
            _heroes.Add(conversationAgent, _companionHero);

            // Give hero the agent's appearance
            // hero.StaticBodyProperties = agent.BodyPropertiesValue.StaticProperties;
            AccessTools.Property(typeof(Hero), "StaticBodyProperties").SetValue(_companionHero, conversationAgent.BodyPropertiesValue.StaticProperties);

            // Give hero agent's equipment
            Equipment civilianEquipment = conversationAgent.SpawnEquipment.Clone();
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
            AccessTools.Property(typeof(CharacterObject), "HeroObject").SetValue(conversationCharacter, _companionHero);
        }

        // Need to clean out agents that cannot be found anymore...
        // Referenced ClanMemberRolesCampaignBehavior -> clan_members_dont_follow_me_on_consequence
        private void RemoveAgents()
        {
            // If the list is empty then return
            if (_heroes.IsEmpty())
            {
                return;
            }
            // Clear all items in _heroes if none of the agents are in the current mission
            foreach (Agent agent in Mission.Current.Agents)
            {
                // If there is an agent present then return
                if (_heroes.ContainsKey(agent))
                {
                    return;
                }
            }
            _heroes.Clear();
        }

        private bool conversation_hero_main_options_discussions()
        {
            // Leave patch accounts for agents that are temporary heroes
            MASettings settings = new();
            MADebug.Print("Orientation: " + settings.SexualOrientation);
            MADebug.Print("Cheating: " + settings.Cheating);
            MADebug.Print("Polygamy: " + settings.Polygamy);
            MADebug.Print("Incest: " + settings.Incest);
            // Lords will go the old fashion way!
            if (Hero.OneToOneConversationHero is not null && Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
            {
                return false;
            }
            // For player agent interaction
            if (PlayerIsAttractedToAgent())
            {
                return true;
            }
            return false;
        }

        private bool PlayerIsAttractedToAgent()
        {
            MASettings settings = new();

            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
            // Avoid potential crashes for quick talk by using IAgent
            IAgent conversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;

            if (conversationAgent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge && Hero.MainHero.CanMarry())
            {
                bool isAttracted = true;
                if (settings.SexualOrientation == "Heterosexual")
                {
                    isAttracted = conversationCharacter.IsFemale != Hero.MainHero.IsFemale;
                }
                if (settings.SexualOrientation == "Homosexual")
                {
                    isAttracted = conversationCharacter.IsFemale == Hero.MainHero.IsFemale;
                }
                return isAttracted;
            }
            return false;
        }

        private bool conversation_player_can_open_courtship_on_condition()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }

            MASettings settings = new();

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            bool courtshipPossible = MarriageCourtshipPossibility(this, Hero.MainHero, Hero.OneToOneConversationHero);

            MADebug.Print("Romantic Level: " + romanticLevel);
            MADebug.Print("Skip Courtship: " + settings.SkipCourtship);
            MADebug.Print("Retry Courtship: " + settings.RetryCourtship);
            MADebug.Print("Courtship Possible: " + courtshipPossible);

            if (courtshipPossible && romanticLevel == Romance.RomanceLevelEnum.Untested)
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer."
                        : "{=goodman_flirt}Goodman, I note that you have not yet taken a wife.", false);
                return true;
            }

            if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility 
                || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                || (romanticLevel == Romance.RomanceLevelEnum.Ended && settings.RetryCourtship))
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=goodwife_chance}Goodwife, may you give me another chance to prove myself?"
                        : "{=goodman_chance}Goodman, may you give me another chance to prove myself?", false);

                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanticLevel == Romance.RomanceLevelEnum.Ended)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
                    }
                    else if (romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                    }
                }
                return true;
            }
            return false;
        }

        public new void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
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