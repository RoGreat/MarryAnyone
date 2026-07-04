using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.CampaignBehaviors
{
    internal sealed class CommonerRomanceCampaignBehavior : CampaignBehaviorBase
    {
        private Hero? _companionHero;

        private readonly Dictionary<Agent, Hero> _heroes;

        public CommonerRomanceCampaignBehavior()
        {
            _heroes = new();
            // _previousRomancePersuasionAttempts = new List<PersuasionAttempt>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
        }
    }
}
