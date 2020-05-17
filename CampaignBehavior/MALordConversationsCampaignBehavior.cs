using System;
using TaleWorlds.CampaignSystem;

namespace MarryAnyone
{
    class MALordConversationsCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("main_option_discussions_3", "hero_main_options", "lord_politics_request", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), null, 100, null, null);
        }

        private bool conversation_hero_main_options_discussions()
        {
            return Hero.OneToOneConversationHero != null && ((Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion) || Hero.OneToOneConversationHero.IsNotable);
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