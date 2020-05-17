using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MarryAnyone
{
    class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=nnutwjOZ}We'll need to work out the details of how we divide our property.", new ConversationSentence.OnConditionDelegate(courtship_hero_not_noble_on_condition), delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 120, null);
            starter.AddPlayerLine("hero_romance_task_pt3a", "hero_main_options", "hero_courtship_final_barter", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null, null);
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition_companion), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence_companion), 140, null);
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

        private void conversation_courtship_stage_2_success_on_consequence_companion()
        {
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
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