using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System;
using System.Reflection;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.MountAndBlade;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using MarryAnyone.Actions;
using static MarryAnyone.Helpers;
using static MarryAnyone.Debug;

namespace MarryAnyone.Behaviors
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        private Hero? _companionHero;

        private readonly Dictionary<Agent, Hero> _heroes;

        public MARomanceCampaignBehavior()
        {
            _heroes = new();
        }

        /* Patching private methods and fields in RomanceCampaignBehavior using Delegates */
        // https://butr.github.io/documentation/advanced/switching-from-membertinfo-to-accesstools2/

        // private List<PersuasionTask> _allReservations
        private static readonly AccessTools.FieldRef<RomanceCampaignBehavior, List<PersuasionTask>>? _allReservations = AccessTools2.FieldRefAccess<RomanceCampaignBehavior, List<PersuasionTask>>("_allReservations");

        private static readonly AccessTools.FieldRef<RomanceCampaignBehavior, Hero>? _playerProposalHero = AccessTools2.FieldRefAccess<RomanceCampaignBehavior, Hero>("_playerProposalHero");

        private static readonly AccessTools.FieldRef<RomanceCampaignBehavior, Hero>? _proposedSpouseForPlayerRelative = AccessTools2.FieldRefAccess<RomanceCampaignBehavior, Hero>("_proposedSpouseForPlayerRelative");

        // private bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        private delegate bool MarriageCourtshipPossibilityDelegate(RomanceCampaignBehavior instance, Hero person1, Hero person2);
        private static readonly MarriageCourtshipPossibilityDelegate? MarriageCourtshipPossibility = AccessTools2.GetDelegate<MarriageCourtshipPossibilityDelegate>(typeof(RomanceCampaignBehavior), "MarriageCourtshipPossibility", new Type[] { typeof(Hero), typeof(Hero) });

        /* Conditions */
        // private bool conversation_courtship_decline_reaction_to_player_on_condition()
        private delegate bool conversation_courtship_decline_reaction_to_player_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_courtship_decline_reaction_to_player_on_condition_delegate? conversation_courtship_decline_reaction_to_player_on_condition = AccessTools2.GetDelegate<conversation_courtship_decline_reaction_to_player_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_courtship_decline_reaction_to_player_on_condition");

        // private void conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        private delegate bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate? conversation_player_eligible_for_marriage_with_conversation_hero_on_condition = AccessTools2.GetDelegate<conversation_player_eligible_for_marriage_with_conversation_hero_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_player_eligible_for_marriage_with_conversation_hero_on_condition");

        // private void conversation_courtship_reaction_to_player_on_condition()
        private delegate bool conversation_courtship_reaction_to_player_on_condition_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_courtship_reaction_to_player_on_condition_delegate? conversation_courtship_reaction_to_player_on_condition = AccessTools2.GetDelegate<conversation_courtship_reaction_to_player_on_condition_delegate>(typeof(RomanceCampaignBehavior), "conversation_courtship_reaction_to_player_on_condition");

        /* Consequences */
        // private void courtship_conversation_leave_on_consequence()
        private delegate void courtship_conversation_leave_on_consequence_delegate(RomanceCampaignBehavior instance);
        private static readonly courtship_conversation_leave_on_consequence_delegate? courtship_conversation_leave_on_consequence = AccessTools2.GetDelegate<courtship_conversation_leave_on_consequence_delegate>(typeof(RomanceCampaignBehavior), "courtship_conversation_leave_on_consequence");

        // private void conversation_player_opens_courtship_on_consequence()
        private delegate void conversation_player_opens_courtship_on_consequence_delegate(RomanceCampaignBehavior instance);
        private static readonly conversation_player_opens_courtship_on_consequence_delegate? conversation_player_opens_courtship_on_consequence = AccessTools2.GetDelegate<conversation_player_opens_courtship_on_consequence_delegate>(typeof(RomanceCampaignBehavior), "conversation_player_opens_courtship_on_consequence");


        /* MemberInfo caching */
        private static readonly FieldInfo? AgentName = AccessTools2.Field(typeof(Agent), "_name");

        private static readonly PropertyInfo? CharacterHeroObject = AccessTools2.Property(typeof(CharacterObject), "HeroObject");

        private static readonly PropertyInfo? HeroStaticBodyProperties = AccessTools2.Property(typeof(Hero), "StaticBodyProperties");

        private static readonly MethodInfo? CompanionAdjustEquipment = AccessTools2.Method(typeof(CompanionsCampaignBehavior), "AdjustEquipment");


        protected void AddDialogs(CampaignGameStarter starter)
        {
            /* RecruitEveryone-like dialogs that can handle hero creation process if needed */
            // Heroes like Wanderers and Lords, need to distinguish the two
            RomanceCharacter(starter, "hero_main_options", "lord_pretalk");
            // Tavernkeeper
            RomanceCharacter(starter, "tavernkeeper_talk", "tavernkeeper_pretalk");
            // Tavernmaid
            RomanceCharacter(starter, "tavernmaid_talk");
            // Bard
            RomanceCharacter(starter, "talk_bard_player");
            // Tavern Game Host
            RomanceCharacter(starter, "taverngamehost_talk", "start");
            // Townsfolk and Villager
            RomanceCharacter(starter, "town_or_village_player", "town_or_village_pretalk");
            // Weaponsmith
            RomanceCharacter(starter, "weaponsmith_talk_player", "merchant_response_3");
            // Barber
            RomanceCharacter(starter, "barber_question1", "no_haircut_conversation_token");
            // Shopworker
            RomanceCharacter(starter, "shopworker_npc_player", "start_2");
            // Blacksmith
            RomanceCharacter(starter, "blacksmith_player", "player_blacksmith_after_craft");
            // Thug
            RomanceCharacter(starter, "alley_talk_start");
            // Arena Master
            RomanceCharacter(starter, "arena_master_talk");
            // Ransom Broker
            RomanceCharacter(starter, "ransom_broker_talk");
            // Probably more that I don't recall...

            /* After the initial romance option */
            // Skip barter and marry immediately
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_2_success", "lord_conclude_courtship_stage_2", "close_window", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(MA_marriage_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_marriage_on_consequence), 200, null);
            // In case there is a fail
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_2_success", "lord_conclude_courtship_stage_2", "close_window", "{=PoDVgQaz}Well, it would take a bit long to discuss this.", new ConversationSentence.OnConditionDelegate(MA_marriage_rejection_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_courtship_conversation_leave_on_consequence), 200, null);

            // Skip courtship later down the line immediately
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_start", "hero_courtship_task_1_begin_reservations", "lord_conclude_courtship_stage_2", "{=bW3ygxro}Yes, it's good to have a chance to get to know each other.", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 200, null);
            starter.AddDialogLine("MA" + "hero_courtship_persuasion_2_start", "hero_courtship_task_2_begin_reservations", "lord_conclude_courtship_stage_2", "{=VNFKqpyV}Yes, well, I've been thinking about that.", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 200, null);

            /* Lord courtship dialogs */
            // Lord skip courtship
            starter.AddDialogLine("MA" + "Lord" + "hero_courtship_persuasion_2_success", "lord_conclude_courtship_stage_2", "close_window", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(lord_skip_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(ApplyMarriageAction), 200, null);
            // Essentially for each check here turn the marriage flag on
            starter.AddDialogLine("MA" + "Lord" + "lord_start_courtship_response_2", "lord_start_courtship_response_2", "lord_conclude_courtship_stage_2", "{=!}{INITIAL_COURTSHIP_REACTION_TO_PLAYER}", new ConversationSentence.OnConditionDelegate(lord_skip_courtship_conversation_courtship_reaction_to_player_on_condition), null, 200, null);
            starter.AddDialogLine("MA" + "Lord" + "hero_courtship_persuasion_start", "hero_courtship_task_1_begin_reservations", "lord_conclude_courtship_stage_2", "{=bW3ygxro}Yes, it's good to have a chance to get to know each other.", new ConversationSentence.OnConditionDelegate(lord_skip_courtship_on_condition), null, 200, null);
            starter.AddDialogLine("MA" + "Lord" + "hero_courtship_persuasion_2_start", "hero_courtship_task_2_begin_reservations", "lord_conclude_courtship_stage_2", "{=VNFKqpyV}Yes, well, I've been thinking about that.", new ConversationSentence.OnConditionDelegate(lord_skip_courtship_on_condition), null, 200, null);

            // Lord bartering other members avoidance
            starter.AddDialogLine("MA" + "Lord" + "lord_propose_marriage_to_clan_leader_confirm", "lord_propose_marriage_to_clan_leader_confirm", "close_window", "{=VJEM0IcV}Let's discuss the details then.", () => Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan, new ConversationSentence.OnConsequenceDelegate(ApplyMarriageActionSameClan), 200, null);

            // Clan members need to have another option to retry if failed...
            starter.AddPlayerLine("MA" + "Lord" + "hero_romance_task_pt3a", "hero_main_options", "lord_conclude_courtship_stage_2", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(lord_clan_member_courtship_recover_on_condition), null, 200, null, null);
        }

        private void ApplyMarriageActionSameClan()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            // Apply marriage action
            MarriageAction.Apply(_proposedSpouseForPlayerRelative!(instance), _playerProposalHero!(instance), true);
            // Leave encounter
            if (PlayerEncounter.Current is not null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
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
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "lord_conclude_courtship_stage_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "lord_conclude_courtship_stage_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(MA_skip_courtship_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);

            // After initial courtship ask for hand in marriage
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_2", start + "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(MA_conversation_player_opens_courtship_on_consequence), 120, null, null);
            // Leave if not ready
            starter.AddPlayerLine("MA" + "lord_start_courtship_response_player_offer_nevermind", start + "lord_start_courtship_response_player_offer", end, "{=D33fIGQe}Never mind.", null, new ConversationSentence.OnConsequenceDelegate(conversation_exit_consequence), 120, null, null);

            // This will occur first before the original dialog if courtship is not skipped
            starter.AddDialogLine("MA" + "lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", () => Hero.OneToOneConversationHero.Occupation != Occupation.Lord, new ConversationSentence.OnConsequenceDelegate(MA_courtship_conversation_leave_on_consequence), 200, null);
        }

        // Will marry if not a lord as well as not a clan member
        private bool MA_marriage_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord && Hero.OneToOneConversationHero.Clan.Leader != Hero.MainHero)
            {
                return false;
            }
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            return conversation_player_eligible_for_marriage_with_conversation_hero_on_condition!(instance);
        }

        private bool MA_marriage_rejection_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord && Hero.OneToOneConversationHero.Clan.Leader != Hero.MainHero)
            {
                return false;
            }
            // Inverse marriage condition
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            return !conversation_player_eligible_for_marriage_with_conversation_hero_on_condition!(instance);
        }

        private bool lord_clan_member_courtship_recover_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord && Hero.OneToOneConversationHero.Clan.Leader != Hero.MainHero)
            {
                return false;
            }
            return Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }

        // Will marry a lord using this quick marriage action
        private void ApplyMarriageAction()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            // End persuasion stuff
            _allReservations!(instance) = null!;
            ConversationManager.EndPersuasion();
            // Change to agreed marriage
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleAgreedOnMarriage);
            // Apply marriage action
            MarriageAction.Apply(Hero.OneToOneConversationHero, Hero.MainHero, true);
            // Leave encounter
            if (PlayerEncounter.Current is not null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        // Will marry commoners or clan members if you are the clan leader
        private void MA_marriage_on_consequence()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            MASettings settings = new();
            Hero spouseHero = Hero.OneToOneConversationHero;

            _allReservations!(instance) = null!;
            ConversationManager.EndPersuasion();

            // Need to clean out _companionHero after agent stage to prevent removal of hero...
            if (_companionHero == Hero.OneToOneConversationHero)
            {
                RemoveHeroObjectFromCharacter();
                spouseHero = _companionHero;
            }
            // Can check for conversating hero here for rest of marriage interaction
            if (_companionHero is null)
            {
                _companionHero = Hero.OneToOneConversationHero;
            }
            // Deactivate all issues
            if (_companionHero.Issue is not null)
            {
                _companionHero.OnIssueDeactivatedForHero();
            }
            // Change to Lord
            ActivateHero(Occupation.Lord);
            // Apply marriage action
            MAMarriageAction.Apply(Hero.MainHero, spouseHero, true);
            // Do NOT break off marriages if polygamy is on...
            if (settings.Cheating && !settings.Polygamy)
            {
                CheatOnSpouse();
            }
            // Remove duplicates
            RemoveExSpouses(Hero.MainHero);
            RemoveExSpouses(spouseHero);
            // Leave encounter
            if (PlayerEncounter.Current is not null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        private bool lord_skip_courtship_conversation_courtship_reaction_to_player_on_condition()
        {
            // Make sure wanderers and notables do not go down this path
            if (Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                return false;
            }
            // Do the cool dialog stuff Bannerlord is known for
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            conversation_courtship_reaction_to_player_on_condition!(instance);
            // And skip run the skip condition
            return lord_skip_courtship_on_condition();
        }

        private bool lord_skip_courtship_on_condition()
        {
            // Make sure wanderers and notables do not go down this path
            if (Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                return false;
            }
            // Marriage possible from before and check if skip courtship is on
            MASettings settings = new();
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            return conversation_player_eligible_for_marriage_with_conversation_hero_on_condition!(instance) && settings.SkipCourtship;
        }

        private void MA_courtship_conversation_leave_on_consequence()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();

            // Activate!
            ActivateHero(Occupation.Wanderer);

            // Start courtship consequence after hero is setup
            courtship_conversation_leave_on_consequence!(instance);

            Agent conversationAgent;
            try
            {
                conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            }
            catch
            {
                return;
            }

            // Remove hero object from character if required
            if (_heroes.ContainsKey(conversationAgent))
            {
                RemoveHeroObjectFromCharacter();
            }
            else if (PlayerEncounter.Current is not null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        private void ActivateHero(Occupation occupation)
        {
            if (_companionHero is null)
            {
                // Set in case there is no _companionHero at this stage
                _companionHero = Hero.OneToOneConversationHero;
            }
            // Setup the hero to be free
            Settlement settlement = Hero.MainHero.CurrentSettlement;
            if (_companionHero.Occupation != occupation)
            {
                // To preserve notable occupation if not married yet
                if (_companionHero.IsNotable)
                {
                    if (occupation == Occupation.Lord)
                    {
                        _companionHero.SetNewOccupation(occupation);
                    }
                }
                else
                {
                    _companionHero.SetNewOccupation(occupation);
                }
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
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
            {
                return false;
            }
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            MASettings settings = new();
            return settings.SkipCourtship && conversation_player_eligible_for_marriage_with_conversation_hero_on_condition!(instance);
        }

        private bool MA_conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
            {
                return false;
            }
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            MASettings settings = new();
            return !settings.SkipCourtship && conversation_player_eligible_for_marriage_with_conversation_hero_on_condition!(instance);
        }

        private void MA_conversation_player_opens_courtship_on_consequence()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            conversation_player_opens_courtship_on_consequence!(instance);
        }

        private bool MA_conversation_courtship_decline_reaction_to_player_on_condition()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            return conversation_courtship_decline_reaction_to_player_on_condition!(instance);
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities || romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility)
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
                CharacterHeroObject!.SetValue(conversationCharacter, null);
            }
        }

        private void conversation_exit_consequence()
        {
            if (_companionHero is null)
            {
                return;
            }

            Agent conversationAgent;
            try
            {
                conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            }
            catch
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
            if (Hero.OneToOneConversationHero is not null)
            {
                return;
            }

            Agent conversationAgent;
            try
            {
                conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            }
            catch
            {
                return;
            }

            CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

            // Remembering the last agents you talked to
            if (_heroes.ContainsKey(conversationAgent))
            {
                // Use existing hero
                _heroes.TryGetValue(conversationAgent, out _companionHero);
                CharacterHeroObject!.SetValue(conversationCharacter, _companionHero);
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
                template = conversationCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate(
                    (CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsFemale == conversationCharacter.IsFemale);
            }

            // Create a new hero!
            _companionHero = HeroCreator.CreateSpecialHero(template, Hero.MainHero.CurrentSettlement, null, null, (int)conversationAgent.Age);

            // Name permanence from the adoption module of old
            AgentName!.SetValue(conversationAgent, _companionHero.Name);

            // Meet character for first time
            _companionHero.HasMet = true;

            // Add hero to heroes list
            _heroes.Add(conversationAgent, _companionHero);

            // Give hero the agent's appearance
            HeroStaticBodyProperties!.SetValue(_companionHero, conversationAgent.BodyPropertiesValue.StaticProperties);

            // Give hero agent's equipment
            Equipment civilianEquipment = conversationAgent.SpawnEquipment.Clone();
            // CharacterObject -> RandomBattleEquipment
            Equipment battleEquipment = template.AllEquipments.GetRandomElementWithPredicate((Equipment e) => !e.IsCivilian).Clone();
            EquipmentHelper.AssignHeroEquipmentFromEquipment(_companionHero, civilianEquipment);
            EquipmentHelper.AssignHeroEquipmentFromEquipment(_companionHero, battleEquipment);

            // Adjust Equipment like the wanderer do
            CompanionsCampaignBehavior companionsCampaignBehaviorInstance = Campaign.Current.CampaignBehaviorManager.GetBehavior<CompanionsCampaignBehavior>();
            CompanionAdjustEquipment!.Invoke(companionsCampaignBehaviorInstance, new object[] { _companionHero });

            HeroHelper.DetermineInitialLevel(_companionHero);

            CharacterDevelopmentCampaignBehavior characterDevelopmentCampaignBehaviorInstance = Campaign.Current.CampaignBehaviorManager.GetBehavior<CharacterDevelopmentCampaignBehavior>();
            characterDevelopmentCampaignBehaviorInstance.DevelopCharacterStats(_companionHero);

            //character.HeroObject = _companionHero;
            CharacterHeroObject!.SetValue(conversationCharacter, _companionHero);
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
            // Clear previous companion heroes before starting
            _companionHero = null;
            // Leave patch accounts for agents that are temporary heroes
            MASettings settings = new();
            Print("Orientation: " + settings.SexualOrientation);
            Print("Cheating: " + settings.Cheating);
            Print("Polygamy: " + settings.Polygamy);
            Print("Incest: " + settings.Incest);
            if (Hero.OneToOneConversationHero is not null)
            {
                // Heroes will avoid romance if they are a lord or they cannot marry the player
                if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
                {
                    return false;
                }
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
            // Attracted under these conditions
            if (Hero.MainHero.Spouse is null || settings.Polygamy || settings.Cheating)
            {
                CharacterObject conversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
                // Avoid potential crashes for quick talk by using IAgent
                IAgent conversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;

                if (conversationAgent.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
                {
                    bool isAttracted = true;
                    if (settings.SexualOrientation == "Heterosexual")
                    {
                        isAttracted = conversationCharacter.IsFemale != Hero.MainHero.IsFemale;
                    }
                    else if (settings.SexualOrientation == "Homosexual")
                    {
                        isAttracted = conversationCharacter.IsFemale == Hero.MainHero.IsFemale;
                    }
                    return isAttracted;
                }
            }
            return false;
        }

        private bool conversation_player_can_open_courtship_on_condition()
        {
            RomanceCampaignBehavior instance = Campaign.Current.CampaignBehaviorManager.GetBehavior<RomanceCampaignBehavior>();
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }

            MASettings settings = new();

            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            bool courtshipPossible = MarriageCourtshipPossibility!(instance, Hero.MainHero, Hero.OneToOneConversationHero);

            Print("Romantic Level: " + romanticLevel);
            Print("Skip Courtship: " + settings.SkipCourtship);
            Print("Retry Courtship: " + settings.RetryCourtship);
            Print("Courtship Possible: " + courtshipPossible);

            if (courtshipPossible && romanticLevel == Romance.RomanceLevelEnum.Untested
                || (romanticLevel == Romance.RomanceLevelEnum.Ended && (settings.Cheating || settings.Polygamy)))
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer."
                        : "{=goodman_flirt}Goodman, I note that you have not yet taken a wife.", false);
                return true;
            }

            if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility 
                || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MBTextManager.SetTextVariable("FLIRTATION_LINE", Hero.OneToOneConversationHero.IsFemale
                        ? "{=goodwife_chance}Goodwife, may you give me another chance to prove myself?"
                        : "{=goodman_chance}Goodman, may you give me another chance to prove myself?", false);

                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility)
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