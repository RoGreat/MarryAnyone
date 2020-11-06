//using System;
//using System.Collections.Generic;
//using TaleWorlds.CampaignSystem;

//namespace MarryAnyone
//{
//    internal class MATavernEmployeesCampaignBehavior : CampaignBehaviorBase
//    {
//        protected void AddDialogs(CampaignGameStarter starter)
//        {
//            MASubModule.Debug("Test: " + Hero.OneToOneConversationHero);
//            System.Diagnostics.Debug.WriteLine("Test 1a: " + Hero.OneToOneConversationHero);
//            System.Diagnostics.Trace.WriteLine("Test 1b: " + Hero.OneToOneConversationHero);
//            starter.AddPlayerLine("main_option_discussions_3", "hero_main_options", "lord_politics_request", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_character_on_condition), null, 100, null, null);
//            System.Diagnostics.Debug.WriteLine("Test 4a: " + Hero.OneToOneConversationHero);
//            System.Diagnostics.Trace.WriteLine("Test 4b: " + Hero.OneToOneConversationHero);
//        }

//        //private bool conversation_hero_main_options_discussions()
//        //{
//        //    return Hero.OneToOneConversationHero != null && (Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion || Hero.OneToOneConversationHero.IsNotable);
//        //}

//        private bool conversation_character_on_condition()
//        {
//            System.Diagnostics.Debug.WriteLine("Test 2a: " + Hero.OneToOneConversationHero);
//            System.Diagnostics.Trace.WriteLine("Test 2b: " + Hero.OneToOneConversationHero);
//            return CharacterObject.OneToOneConversationCharacter != null && CharacterObject.OneToOneConversationCharacter.IsHero && character_occupation() && CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner;
//        }

//        private bool character_occupation()
//        {
//            switch(CharacterObject.OneToOneConversationCharacter.Occupation)
//            {
//                case Occupation.Tavernkeeper:
//                case Occupation.Mercenary:
//                case Occupation.Lord:
//                case Occupation.Lady:
//                case Occupation.GoodsTrader:
//                case Occupation.ArenaMaster:
//                case Occupation.Companion:
//                case Occupation.Villager:
//                case Occupation.Soldier:
//                case Occupation.Townsfolk:
//                case Occupation.GuildMaster:
//                case Occupation.Marshall:
//                case Occupation.TournamentFixer:
//                case Occupation.RansomBroker:
//                case Occupation.Weaponsmith:
//                case Occupation.Armorer:
//                case Occupation.HorseTrader:
//                case Occupation.TavernWench:
//                case Occupation.ShopKeeper:
//                case Occupation.TavernGameHost:
//                case Occupation.Bandit:
//                case Occupation.Wanderer:
//                case Occupation.Artisan:
//                case Occupation.Merchant:
//                case Occupation.Preacher:
//                case Occupation.Headman:
//                case Occupation.GangLeader:
//                case Occupation.RuralNotable:
//                case Occupation.Outlaw:
//                case Occupation.MinorFactionCharacter:
//                case Occupation.PrisonGuard:
//                case Occupation.Guard:
//                case Occupation.ShopWorker:
//                case Occupation.Musician:
//                case Occupation.Gangster:
//                case Occupation.Blacksmith:
//                case Occupation.Judge:
//                case Occupation.BannerBearer:
//                case Occupation.CaravanGuard:
//                    return true;
//                default:
//                    return false;
//            }
//        }

//        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
//        {
//            AddDialogs(campaignGameStarter);
//        }

//        public override void RegisterEvents()
//        {
//            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
//            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnNewGameCreated));
//        }

//        private void OnNewGameCreated(CampaignGameStarter starter)
//        {
//            _previouslyMetWandererTemplates = new Dictionary<CharacterObject, CharacterObject>();
//        }

//        public override void SyncData(IDataStore dataStore)
//        {
//            dataStore.SyncData<Dictionary<CharacterObject, CharacterObject>>("_previouslyMetWandererTemplates", ref _previouslyMetWandererTemplates);
//        }

//        private Dictionary<CharacterObject, CharacterObject> _previouslyMetWandererTemplates;
//    }
//}