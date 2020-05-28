using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.Realistic)
            {
                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=nnutwjOZ}We'll need to work out the details of how we divide our property.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
                {
                    PlayerEncounter.LeaveEncounter = true;
                }, 120, null);

                starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null, null);

                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition_companion), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence), 140, null);
            }
            if (config.Difficulty == Difficulty.Easy || config.Difficulty == Difficulty.VeryEasy)
            {
                starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(courtship_hero_skip_clan_leader_on_condition), null, 120, null);

                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(courtship_hero_skip_clan_leader_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence), 140, null);

                starter.AddPlayerLine("lord_start_courtship_response_player_offer", "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_on_condition), null, 140, null, null);

                starter.AddPlayerLine("lord_start_courtship_response_player_offer_2", "lord_start_courtship_response_player_offer", "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_on_condition), null, 140, null, null);

                starter.AddPlayerLine("hero_romance_task_pt1_pt2", "hero_main_options", "lord_start_courtship_response_3", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_and_courtship_started_on_condition), null, 140, null, null);
            }
        }

        private bool courtship_hero_not_noble_on_condition()
        {
            return Hero.OneToOneConversationHero != null && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero;
        }

        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }

        private bool conversation_finalize_courtship_for_hero_on_condition_companion()
        {
            return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Hero.OneToOneConversationHero.IsPlayerCompanion && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }

        private bool courtship_hero_skip_clan_leader_on_condition()
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.Easy && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                return false;
            }
            return Hero.OneToOneConversationHero != null || Clan.PlayerClan.Heroes.Contains(Hero.OneToOneConversationHero);
        }

        private bool courtship_hero_not_noble_and_courtship_started_on_condition()
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.Easy && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                return false;
            }
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            return Hero.OneToOneConversationHero != null && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
        }

        private bool conversation_player_eligible_for_marriage_on_condition()
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.Easy && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                return false;
            }
            return (!config.IsPolygamous && Hero.MainHero.Spouse == null || config.IsPolygamous && Hero.MainHero.Spouse != null) && Hero.OneToOneConversationHero != null && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) == null && Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero);
        }

        private void conversation_courtship_stage_2_success_on_consequence()
        {
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
            if (Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                Hero.OneToOneConversationHero.CompanionOf = null;
            }
            PlayerEncounter.LeaveEncounter = true;
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