using System;

using TaleWorlds.CampaignSystem;

namespace MarryAnyone.CampaignBehaviors
{
    // bin\...\\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.LordConversationsCampaignBehavior
    internal class LeaderRomanceCampaignBehavior : CampaignBehaviorBase
    {
        public LeaderRomanceCampaignBehavior()
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) { }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
        }
    }
}
