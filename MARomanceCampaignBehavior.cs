using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone
{
    class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine("lord_start_courtship_response_3", "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 120, null);
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_1", "close_window", "{=SP7I61x2}Perhaps we can talk more when we meet again.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 120, null);
            starter.AddDialogLine("hero_courtship_end_conversation", "hero_courtship_end_conversation", "close_window", "{=Mk9k8Sec}As always, it is a delight to speak to you.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 120, null);
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=nnutwjOZ}We'll need to work out the details of how we divide our property.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 120, null);
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition_2), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence), 140, null);
            starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null, null);
        }

        private bool courtship_hero_not_noble_on_condition()
        {
            return !(Hero.OneToOneConversationHero != null && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero));
        }

        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && !Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }

        private bool conversation_finalize_courtship_for_hero_on_condition_2()
        {
            return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Hero.OneToOneConversationHero.IsPlayerCompanion && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }

        private void conversation_courtship_stage_2_success_on_consequence()
        {
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
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