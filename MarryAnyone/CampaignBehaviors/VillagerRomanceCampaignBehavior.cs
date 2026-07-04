using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.SaveSystem;

namespace MarryAnyone.CampaignBehaviors
{
    // 1. Create a hero from villager character object.
    //  - Refer to previous recipe and improve.
    // 2. Gate dialog to work for occupations that are not lords.
    //  - This might take some thought. Try not to over-patch.
    //  -
    internal sealed class VillagerRomanceCampaignBehavior : RomanceCampaignBehavior
    {
        [SaveableField(1)]
        private List<PersuasionAttempt> _previousRomancePersuasionAttempts;

        public VillagerRomanceCampaignBehavior()
        {
            _previousRomancePersuasionAttempts = new List<PersuasionAttempt>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public new void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            if (Hero.OneToOneConversationHero.Occupation != Occupation.Lord)
            {
                base.AddDialogs(campaignGameStarter);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<PersuasionAttempt>>("previousRomancePersuasionAttempts", ref _previousRomancePersuasionAttempts);
        }
    }
}
