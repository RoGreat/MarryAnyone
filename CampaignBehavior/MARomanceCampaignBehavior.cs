using HarmonyLib;
using System;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.SandBox.Conversations;
using TaleWorlds.Core;

namespace MarryAnyone
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            MAConfig config = MASettings.Config;
            if (config.Difficulty == Difficulty.Realistic)
            {
                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence), 140, null);
            }
            else
            {
                starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(courtship_is_not_noble_on_condition), null, 120, null);

                starter.AddPlayerLine("hero_romance_task_pt1_pt2", "hero_main_options", "lord_start_courtship_response_3", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(courtship_hero_started_on_condition), null, 140, null, null);

                starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(courtship_is_not_noble_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_stage_2_success_on_consequence), 140, null);
            }
        }
        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            MAConfig config = MASettings.Config;
            return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && ((!Hero.OneToOneConversationHero.IsNoble && !Hero.OneToOneConversationHero.IsMinorFactionHero) || (Clan.PlayerClan.Heroes.Contains(Hero.OneToOneConversationHero) && config.IsIncestual)) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
        }


        private bool courtship_is_not_noble_on_condition()
        {
            MAConfig config = MASettings.Config;
            if (Clan.PlayerClan.Heroes.Contains(Hero.OneToOneConversationHero) && config.IsIncestual)
            {
                return true;
            }
            if (config.Difficulty == Difficulty.Easy && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
            {
                return false;
            }
            return true;
        }

        private bool courtship_hero_started_on_condition()
        {
            if (!courtship_is_not_noble_on_condition())
            {
                return false;
            }
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            return Hero.OneToOneConversationHero != null && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
        }

        private void conversation_courtship_stage_2_success_on_consequence()
        {
            AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(Hero.OneToOneConversationHero.CharacterObject, Occupation.Lord, null);
            Hero.OneToOneConversationHero.IsNoble = true;
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