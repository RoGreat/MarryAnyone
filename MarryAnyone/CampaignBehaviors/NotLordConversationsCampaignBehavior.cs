using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Localization;

namespace MarryAnyone.CampaignBehaviors
{
    internal class NotLordConversationsCampaignBehavior : CampaignBehaviorBase
    {

        public NotLordConversationsCampaignBehavior() { }

        public override void SyncData(IDataStore dataStore) { }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        private bool conversation_hero_main_options_discussions()
        {
            if (Hero.OneToOneConversationHero is not null && Hero.OneToOneConversationHero.IsLord)
            {
                return false;
            }
            return true;
        }

        private bool conversation_lord_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter), false);
            return true;
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors
            // CaravansCampaignBehavior: Hero.IsPlayerCompanion, Clan; MobileParty.IsCaravan
            CampaignBehaviorConversationsDialog(starter, "caravan_companion_talk_start_reply", "lord_pretalk");
            CampaignBehaviorConversationsDialog(starter, "caravan_talk", "caravan_pretalk");
            // CompanionRolesCampaignBehavior: Clan
            CampaignBehaviorConversationsDialog(starter, "hero_main_options", "companion_okay");
            // CraftingCampaignBehavior: Occupation.Blacksmith
            CampaignBehaviorConversationsDialog(starter, "blacksmith_player", "player_blacksmith_after_craft");
            // WorkshopsCharactersCampaignBehavior: Occupation.ShopWorker
            CampaignBehaviorConversationsDialog(starter, "shopworker_npc_player", "start_2");

            // Modules\SandBox\bin\...\SandBox.dll -> CampaignBehaviors
            // AlleyCampaignBehavior: Occupation.Gangster
            CampaignBehaviorConversationsDialog(starter, "alley_talk_start", "alley_options");
            // ArenaMasterCampaignBehavior: Occupation.ArenaMaster
            CampaignBehaviorConversationsDialog(starter, "arena_master_talk", "arena_master_pre_talk");
            // BarberCampaignBehavior: CultureObject.Barber
            CampaignBehaviorConversationsDialog(starter, "barber_question1", "no_haircut_conversation_token");
            // BoardGameCampaignBehavior: Occupation.TavernGameHost
            CampaignBehaviorConversationsDialog(starter, "taverngamehost_talk");
            // CommonVillagersCampaignBehavior: Occupation.Villager, Occupation.Townsfolk
            CampaignBehaviorConversationsDialog(starter, "town_or_village_player", "town_or_village_pretalk");
            // GuardsCampaignBehavior: Occupation.PrisonGuard
            CampaignBehaviorConversationsDialog(starter, "prison_guard_talk");
            // TavernEmployeesCampaignBehavior: Occupation.RansomBroker, Occupation.Musician, Occupation.Tavernkeeper, Occupation.TavernWench
            CampaignBehaviorConversationsDialog(starter, "ransom_broker_talk", "ransom_broker_pretalk");
            CampaignBehaviorConversationsDialog(starter, "talk_bard_player");
            CampaignBehaviorConversationsDialog(starter, "tavernkeeper_talk", "tavernkeeper_pretalk");
            CampaignBehaviorConversationsDialog(starter, "tavernmaid_talk");
            // TradersCampaignBehavior: Occupation.Weaponsmith, Occupation.Armorer, Occupation.HorseTrader, Occupation.Blacksmith
            CampaignBehaviorConversationsDialog(starter, "weaponsmith_talk_player", "merchant_response_3");
        }

        private void CampaignBehaviorConversationsDialog(CampaignGameStarter starter, string input, string output = "close_window")
        {
            // bin\...\\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.LordConversationsCampaignBehavior
            starter.AddPlayerLine(input + "main_option_discussions_3", input, input + "lord_politics_request", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), null, 100, null, null);
            starter.AddDialogLine(input + "lord_politics_request", input + "lord_politics_request", input + "lord_talk_speak_diplomacy_2", "{=!}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(conversation_lord_agrees_to_discussion_on_condition), null, 100, null);
            starter.AddPlayerLine(input + "hero_special_request", input + "lord_talk_speak_diplomacy_2", output, "{=PznWhAdU}Actually, never mind.", null, null, 1, null, null);
        }
    }
}
