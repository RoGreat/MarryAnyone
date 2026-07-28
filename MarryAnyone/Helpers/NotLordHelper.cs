using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.Helpers
{
    public static class NotLordHelper
    {
        public static bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        {
            return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2) && !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction);
        }

        public static Agent? GetConversationAgent()
        {
            return Campaign.Current.ConversationManager.OneToOneConversationAgent as Agent;
        }

        public static bool IsNotLord()
        {
            if (Hero.OneToOneConversationHero is not null && Hero.OneToOneConversationHero.IsLord)
            {
                return false;
            }
            return true;
        }

        public static Hero? GetCreatedHero(Dictionary<Agent, Hero> createdHeroes)
        {
            Agent? agent = GetConversationAgent();
            if (agent is null)
            {
                return null;
            }
            createdHeroes.TryGetValue(agent, out Hero? createdHero);
            return createdHero;
        }
    }
}
