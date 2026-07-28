using MarryAnyone.Helpers;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.CampaignBehaviors
{
    // bin\...\\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.LordConversationsCampaignBehavior
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
            AddDialogs(campaignGameStarter, CampaignBehaviorConversationsDialog);
        }

        private bool conversation_hero_main_options_discussions()
        {
            return MarryAnyoneHelper.IsNotLord();
        }

        public readonly Dictionary<Agent, Hero> createdHeroes = new();

        private bool conversation_lord_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter), false);
            Agent? conversationAgent = MarryAnyoneHelper.GetConversationAgent();
            if (conversationAgent is null || !MarryAnyoneHelper.IsNotLord())
            {
                return true;
            }
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(createdHeroes);
            if (!conversationAgent.IsHero && createdHero is null && !createdHeroes.ContainsKey(conversationAgent))
            {
                // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignCheats.CreateRandomClan
                Settlement settlement = Hero.MainHero.CurrentSettlement;
                CharacterObject characterObject = (CharacterObject)conversationAgent.Character;
                createdHero = HeroCreator.CreateSpecialHero(characterObject, settlement, null, null, (int)conversationAgent.Age);
                createdHero.StaticBodyProperties = conversationAgent.BodyPropertiesValue.StaticProperties;
                createdHero.Weight = conversationAgent.BodyPropertiesValue.DynamicProperties.Weight;
                createdHero.Build = conversationAgent.BodyPropertiesValue.DynamicProperties.Build;
                createdHero.HeroDeveloper.InitializeHeroDeveloper();
                createdHero.SetNewOccupation(Occupation.Wanderer);
                createdHero.ChangeState(Hero.CharacterStates.Active);
                EnterSettlementAction.ApplyForCharacterOnly(createdHero, settlement);
                GiveGoldAction.ApplyBetweenCharacters(null, createdHero, MBRandom.RandomInt(0, 1000), false);
                createdHero.SetHasMet();
                createdHeroes.Add(conversationAgent, createdHero);
            }
            return true;
        }

        public delegate void CampaignBehaviorDialog(CampaignGameStarter starter, string input, string output = "close_window");

        public void AddDialogs(CampaignGameStarter starter, CampaignBehaviorDialog dialog)
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors
            // CaravansCampaignBehavior: Hero.IsPlayerCompanion, Clan; MobileParty.IsCaravan
            dialog(starter, "caravan_companion_talk_start_reply", "lord_pretalk");
            dialog(starter, "caravan_talk", "caravan_pretalk");
            // CompanionRolesCampaignBehavior: Clan
            dialog(starter, "hero_main_options", "companion_okay");
            // CraftingCampaignBehavior: Occupation.Blacksmith
            dialog(starter, "blacksmith_player", "player_blacksmith_after_craft");
            // WorkshopsCharactersCampaignBehavior: Occupation.ShopWorker
            dialog(starter, "shopworker_npc_player", "start_2");

            // Modules\SandBox\bin\...\SandBox.dll -> CampaignBehaviors
            // AlleyCampaignBehavior: Occupation.Gangster
            dialog(starter, "alley_talk_start", "alley_options");
            // ArenaMasterCampaignBehavior: Occupation.ArenaMaster
            dialog(starter, "arena_master_talk", "arena_master_pre_talk");
            // BarberCampaignBehavior: CultureObject.Barber
            dialog(starter, "barber_question1", "no_haircut_conversation_token");
            // BoardGameCampaignBehavior: Occupation.TavernGameHost
            dialog(starter, "taverngamehost_talk");
            // CommonVillagersCampaignBehavior: Occupation.Villager, Occupation.Townsfolk
            dialog(starter, "town_or_village_player", "town_or_village_pretalk");
            // GuardsCampaignBehavior: Occupation.PrisonGuard
            dialog(starter, "prison_guard_talk");
            // TavernEmployeesCampaignBehavior: Occupation.RansomBroker, Occupation.Musician, Occupation.Tavernkeeper, Occupation.TavernWench
            dialog(starter, "ransom_broker_talk", "ransom_broker_pretalk");
            dialog(starter, "talk_bard_player");
            dialog(starter, "tavernkeeper_talk", "tavernkeeper_pretalk");
            dialog(starter, "tavernmaid_talk");
            // TradersCampaignBehavior: Occupation.Weaponsmith, Occupation.Armorer, Occupation.HorseTrader, Occupation.Blacksmith
            dialog(starter, "weaponsmith_talk_player", "merchant_response_3");
        }

        private void CampaignBehaviorConversationsDialog(CampaignGameStarter starter, string input, string output = "close_window")
        {
            starter.AddPlayerLine(input + "main_option_discussions_3", input, input + "lord_politics_request", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_hero_main_options_discussions), null, 100, null, null);
            starter.AddDialogLine(input + "lord_politics_request", input + "lord_politics_request", input + "lord_talk_speak_diplomacy_2", "{=!}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(conversation_lord_agrees_to_discussion_on_condition), null, 100, null);
            starter.AddPlayerLine(input + "hero_special_request", input + "lord_talk_speak_diplomacy_2", output, "{=PznWhAdU}Actually, never mind.", null, null, 1, null, null);
        }
    }
}
