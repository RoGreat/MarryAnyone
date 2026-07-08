using System.Collections.Generic;
using System.Linq;
using System;

using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;

namespace MarryAnyone.CampaignBehaviors
{
    internal class CommonerRomanceCampaignBehavior : CampaignBehaviorBase
    {
        private readonly List<Agent> _courtedAgents = new();

        private readonly Dictionary<Agent, Hero> _createdHeroes = new();

        private Location? _locationOfConversation = null;

        private string? _specialTargetTag = null;

        public CommonerRomanceCampaignBehavior() { }

        public override void SyncData(IDataStore dataStore) { }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnSettlementLeft));
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            foreach (KeyValuePair<Agent, Hero> hero in _createdHeroes)
            {
                LocationCharacter locationCharacterOfHero = settlement.LocationComplex.GetLocationCharacterOfHero(hero.Value);
                locationCharacterOfHero.SpecialTargetTag = _specialTargetTag;
                _locationOfConversation?.AddCharacter(locationCharacterOfHero);
                Log.Debug(hero.Value.GetName().ToString());
            }
            _createdHeroes.Clear();
            _locationOfConversation = null;
            _specialTargetTag = null;
        }

        private static bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        {
            return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2) && !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction);
        }

        private IEnumerable<RomanceReservationDescription> GetRomanceReservations(Hero wooed, Hero wooer)
        {
            Hero? conversationHero = GetConversationHero();
            List<RomanceReservationDescription> list = new();
            bool flag = wooed.GetTraitLevel(DefaultTraits.Honor) + wooed.GetTraitLevel(DefaultTraits.Mercy) > 0;
            bool flag2 = wooed.GetTraitLevel(DefaultTraits.Honor) < 1 && wooed.GetTraitLevel(DefaultTraits.Valor) < 1 && wooed.GetTraitLevel(DefaultTraits.Calculating) < 1;
            bool flag3 = wooed.GetTraitLevel(DefaultTraits.Calculating) - wooed.GetTraitLevel(DefaultTraits.Mercy) >= 0;
            bool flag4 = wooed.GetTraitLevel(DefaultTraits.Valor) - wooed.GetTraitLevel(DefaultTraits.Calculating) > 0 && wooed.GetTraitLevel(DefaultTraits.Mercy) <= 0;

            if (conversationHero is null)
            {
                return list;
            }

            if (flag)
            {
                list.Add(RomanceReservationDescription.CompatibilityINeedSomeoneUpright);
            }
            else if (flag4 && wooed.IsFemale)
            {
                list.Add(RomanceReservationDescription.CompatibiliyINeedSomeoneDangerous);
            }
            else
            {
                list.Add(RomanceReservationDescription.CompatibilityNeedSomethingInCommon);
            }
            int attractionValuePercentage = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(conversationHero, Hero.MainHero);
            if (attractionValuePercentage > 70)
            {
                list.Add(RomanceReservationDescription.AttractionIAmDrawnToYou);
            }
            else if (attractionValuePercentage > 40)
            {
                list.Add(RomanceReservationDescription.AttractionYoureGoodEnough);
            }
            else
            {
                list.Add(RomanceReservationDescription.AttractionYoureNotMyType);
            }
            List<Settlement> list2 = Enumerable.ToList(Enumerable.Where(Settlement.All, x => x.OwnerClan == wooer.Clan));
            if (flag3 && wooer.IsFemale && list2.Count < 1)
            {
                list.Add(RomanceReservationDescription.PropertyHowCanIMarryAnAdventuress);
            }
            else if (flag3 && list2.Count < 3)
            {
                list.Add(RomanceReservationDescription.PropertyIWantRealWealth);
            }
            else if (list2.Count < 1)
            {
                list.Add(RomanceReservationDescription.PropertyWeNeedToBeComfortable);
            }
            else
            {
                list.Add(RomanceReservationDescription.PropertyYouSeemRichEnough);
            }
            float unmodifiedClanLeaderRelationshipWithPlayer = conversationHero.GetUnmodifiedClanLeaderRelationshipWithPlayer();
            if (unmodifiedClanLeaderRelationshipWithPlayer < -10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalHowCanYouBeEnemiesWithOurFamily);
            }
            else if (!flag2 && unmodifiedClanLeaderRelationshipWithPlayer < 10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalItWouldBeBestToBefriendOurFamily);
            }
            else if (flag2 && unmodifiedClanLeaderRelationshipWithPlayer < 10f)
            {
                list.Add(RomanceReservationDescription.FamilyApprovalYouNeedToBeFriendsWithOurFamily);
            }
            else
            {
                list.Add(RomanceReservationDescription.FamilyApprovalIAmGladYouAreFriendsWithOurFamily);
            }
            return list;
        }

        private Hero? GetConversationHero()
        {
            Agent? agent = GetConversationAgent();
            if (agent is null)
            {
                return null;
            }
            _createdHeroes.TryGetValue(agent, out Hero? conversationHero);
            return conversationHero;
        }

        private static bool IsCommoner()
        {
            Agent? agent = GetConversationAgent();
            if (agent is null)
            {
                return false;
            }
            if (((CharacterObject)agent.Character).Occupation != Occupation.Lord)
            {
                return true;
            }
            return false;
        }

        private static Agent? GetConversationAgent()
        {
            Agent conversationAgent;
            try
            {
                conversationAgent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;
            }
            catch
            {
                Log.Warning("Failed to get Agent");
                return null;
            }
            return conversationAgent;
        }

        private bool conversation_player_can_open_courtship_on_condition()
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignCheats.CreateRandomClan
            Agent? conversationAgent = GetConversationAgent();
            if (conversationAgent is null || !IsCommoner() || _courtedAgents.Contains(conversationAgent))
            {
                Log.Debug("conversation_player_can_open_courtship_on_condition -> False");
                return false;
            }

            Hero? hero = null;
            if (hero is null && !_createdHeroes.ContainsKey(conversationAgent))
            {
                Log.Debug("Create Hero");
                Settlement settlement = Hero.MainHero.CurrentSettlement;
                // TextObject textObject = NameGenerator.Current.GenerateClanName(settlement.Culture, settlement);
                // Clan clan = Clan.CreateClan("test_clan_" + Clan.All.Count);
                // clan.ChangeClanName(textObject, textObject);
                // clan.Culture = settlement.Culture;
                // clan.Banner = Banner.CreateRandomClanBanner(-1);
                // clan.SetInitialHomeSettlement(settlement);
                CharacterObject characterObject = (CharacterObject)conversationAgent.Character;
                hero = HeroCreator.CreateSpecialHero(characterObject, settlement, null, null, (int)conversationAgent.Age);
                hero.StaticBodyProperties = conversationAgent.BodyPropertiesValue.StaticProperties;
                hero.Weight = conversationAgent.BodyPropertiesValue.DynamicProperties.Weight;
                hero.Build = conversationAgent.BodyPropertiesValue.DynamicProperties.Build;
                // conversationHero.SetNewOccupation(Occupation.Lord);
                hero.HeroDeveloper.InitializeHeroDeveloper();
                hero.ChangeState(Hero.CharacterStates.Active);
                // clan.SetLeader(conversationHero);
                EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                GiveGoldAction.ApplyBetweenCharacters(null, hero, MBRandom.RandomInt(0, 1000), false);
                // CampaignEventDispatcher.Instance.OnClanCreated(clan, false);
                hero.SetHasMet();
                _locationOfConversation = CampaignMission.Current.Location;
                _specialTargetTag = settlement.LocationComplex.GetFirstLocationCharacterOfCharacter((CharacterObject)conversationAgent.Character).SpecialTargetTag;
                // LocationCharacter locationCharacterOfHero = settlement.LocationComplex.GetLocationCharacterOfHero(hero);
                // LocationCharacter locationCharacterOfConversationAgent = settlement.LocationComplex.GetFirstLocationCharacterOfCharacter((CharacterObject)conversationAgent.Character);
                // Location currentLocation = CampaignMission.Current.Location;
                // locationCharacterOfHero.SpecialTargetTag = locationCharacterOfConversationAgent.SpecialTargetTag;
                // currentLocation.AddCharacter(locationCharacterOfHero);
                _createdHeroes.Add(conversationAgent, hero);
            }
            else
            {
                hero = GetConversationHero();
            }

            if (hero is null)
            {
                Log.Debug("conversation_player_can_open_courtship_on_condition -> False");
                return false;
            }
            if (MarriageCourtshipPossibility(Hero.MainHero, hero) && Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.Untested)
            {
                if (Hero.MainHero.IsFemale)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=bjJs0eeB}My lord, I note that you have not yet taken a wife.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                }
                Log.Debug("conversation_player_can_open_courtship_on_condition -> True");
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                if (Hero.MainHero.IsFemale)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=2WnhUBMM}My lord, may you give me another chance to prove myself?", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);
                }
                Log.Debug("conversation_player_can_open_courtship_on_condition -> True");
                return true;
            }
            Log.Debug("conversation_player_can_open_courtship_on_condition -> False");
            return false;
        }

        private void conversation_player_opens_courtship_on_consequence()
        {
            Log.Debug("conversation_player_opens_courtship_on_consequence");
            Hero? conversationHero = GetConversationHero();
            if (Romance.GetRomanticLevel(Hero.MainHero, conversationHero) != Romance.RomanceLevelEnum.FailedInCompatibility && Romance.GetRomanticLevel(Hero.MainHero, conversationHero) != Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                ChangeRomanticStateAction.Apply(Hero.MainHero, conversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
            }
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            Hero? conversationHero = GetConversationHero();
            if (conversationHero is null || !IsCommoner())
            {
                Log.Debug("conversation_courtship_initial_reaction_on_condition -> False");
                return false;
            }
            IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(conversationHero, Hero.MainHero);
            if (Romance.GetRomanticLevel(Hero.MainHero, conversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities || Romance.GetRomanticLevel(Hero.MainHero, conversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                Log.Debug("conversation_courtship_initial_reaction_on_condition -> False");
                return false;
            }
            MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou) ? "{=WEkjz9tg}Ah! Yes... We are considering offers... Did you have someone in mind?" : "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
            Log.Debug("conversation_courtship_initial_reaction_on_condition -> True");
            return true;
        }

        private bool conversation_courtship_decline_reaction_to_player_on_condition()
        {
            Hero? conversationHero = GetConversationHero();
            if (conversationHero is null || !IsCommoner())
            {
                Log.Debug("conversation_courtship_decline_reaction_to_player_on_condition -> False");
                return false;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, conversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=emLBsWj6}I am terribly sorry. It is practically not possible for us to be married.", false);
                Log.Debug("conversation_courtship_decline_reaction_to_player_on_condition -> True");
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, conversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=s7idfhBO}I am terribly sorry. We are not really compatible with each other.", false);
                Log.Debug("conversation_courtship_decline_reaction_to_player_on_condition -> True");
                return true;
            }
            Log.Debug("conversation_courtship_decline_reaction_to_player_on_condition -> False");
            return false;
        }

        private bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            Hero? conversationHero = GetConversationHero();
            if (conversationHero is null || !IsCommoner())
            {
                Log.Debug("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition -> False");
                return false;
            }
            bool result = Hero.MainHero.Spouse is null && conversationHero is not null && MarriageCourtshipPossibility(Hero.MainHero, conversationHero);
            Log.Debug("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition -> " + result);
            return result;
        }

        private bool conversation_courtship_reaction_to_player_on_condition()
        {
            Hero? conversationHero = GetConversationHero();
            if (conversationHero is null || !IsCommoner())
            {
                Log.Debug("conversation_courtship_reaction_to_player_on_condition -> False");
                return false;
            }
            IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(conversationHero, Hero.MainHero);
            bool flag = conversationHero.GetTraitLevel(DefaultTraits.Generosity) + conversationHero.GetTraitLevel(DefaultTraits.Mercy) > 0;
            TraitObject persona = conversationHero.CharacterObject.GetPersona();
            bool flag2 = ConversationTagHelper.UsesHighRegister(conversationHero.CharacterObject);
            if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou))
            {
                if (persona == DefaultTraits.PersonaIronic && flag2)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=5ao0RdRT}Well, I do not deny that there is something about you to which I am drawn.", false);
                }
                if (persona == DefaultTraits.PersonaIronic && !flag2)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=r77ZrSUJ}You're straightforward. I like that.", false);
                }
                else if (persona == DefaultTraits.PersonaCurt)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", Hero.MainHero.IsFemale ? "{=YXCGUSYd}Mm. Well, you'd make a very unusual match. But, well, I won't rule it out." : "{=iKYSgoZx}You're a handsome devil, I'll give you that.", false);
                }
                else if (persona == DefaultTraits.PersonaEarnest)
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=UCjFAPnk}I am flattered, {?PLAYER.GENDER}my lady{?}sir{\\?}.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=8PwNj5tR}Yes... Yes. We should, em, discuss this.", false);
                }
            }
            else if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyHowCanIMarryAnAdventuress))
            {
                MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=YRN4RBeI}Very well, madame, but I would have you know.... I intend to marry someone of my own rank.", false);
            }
            else
            {
                if (!flag)
                {
                    if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyIWantRealWealth || x == RomanceReservationDescription.PropertyWeNeedToBeComfortable))
                    {
                        MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=P407baEa}I think you would need to rise considerably in the world before I could consider such a thing...", false);
                        Log.Debug("conversation_courtship_reaction_to_player_on_condition -> True");
                        return true;
                    }
                }
                if (flag)
                {
                    if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyIWantRealWealth))
                    {
                        MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=gS1noLvf}I do not know whether to find that charming or impertinent...", false);
                        Log.Debug("conversation_courtship_reaction_to_player_on_condition -> True");
                        return true;
                    }
                }
                if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionYoureNotMyType))
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=ltXu3DbR}Em... Yes, well, I suppose I can consider your offer.", false);
                }
                else if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.FamilyApprovalIAmGladYouAreFriendsWithOurFamily))
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=UQtXV3kf}Certainly, you have always been close to our family.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=VYmQmqIv}We are considering many offers. You may certainly add your name to the list.", false);
                }
            }
            Log.Debug("conversation_courtship_reaction_to_player_on_condition -> True");
            return true;
        }

        private void courtship_conversation_leave_on_consequence()
        {
            Agent? conversationAgent = GetConversationAgent();
            if (conversationAgent is not null)
            {
                _courtedAgents.Add(conversationAgent);
            }
            if (PlayerEncounter.Current is not null)
            {
                Log.Debug("Leave Encounter");
                PlayerEncounter.LeaveEncounter = true;
            }
            Log.Debug("courtship_conversation_leave_on_consequence");
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors
            // CaravansCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "caravan_companion_talk_start_reply", "lord_pretalk");
            // CompanionRolesCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "hero_main_options", "companion_okay");
            // CraftingCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "blacksmith_player", "player_blacksmith_after_craft");
            // WorkshopsCharactersCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "shopworker_npc_player", "start_2");

            // Modules\SandBox\bin\...\SandBox.dll -> CampaignBehaviors
            // AlleyCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "alley_talk_start", "alley_options");
            // ArenaMasterCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "arena_master_talk", "arena_master_pre_talk");
            // BarberCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "barber_question1", "no_haircut_conversation_token");
            // BoardGameCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "taverngamehost_talk");
            // CommonVillagersCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "town_or_village_player", "town_or_village_pretalk");
            // GuardsCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "prison_guard_talk");
            // TavernEmployeesCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "ransom_broker_talk", "ransom_broker_pretalk");
            CampaignBehaviorRomanceDialog(starter, "talk_bard_player");
            CampaignBehaviorRomanceDialog(starter, "tavernkeeper_talk", "tavernkeeper_pretalk");
            CampaignBehaviorRomanceDialog(starter, "tavernmaid_talk");
            // TradersCampaignBehavior
            CampaignBehaviorRomanceDialog(starter, "weaponsmith_talk_player", "merchant_response_3");
        }

        private void CampaignBehaviorRomanceDialog(CampaignGameStarter starter, string input, string output = "close_window")
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.RomanceCampaignBehavior
            // MainHero
            starter.AddPlayerLine(input + "lord_special_request_flirt", input, input + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response", input + "lord_start_courtship_response", input + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_decline", input + "lord_start_courtship_response", output, "{=!}{COURTSHIP_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_decline_reaction_to_player_on_condition), null, 100, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_2", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_nevermind", input + "lord_start_courtship_response_player_offer", output, "{=D33fIGQe}Never mind.", null, null, 120, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_3", "{=!}{INITIAL_COURTSHIP_REACTION_TO_PLAYER}", new ConversationSentence.OnConditionDelegate(conversation_courtship_reaction_to_player_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_3", input + "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(courtship_conversation_leave_on_consequence), 100, null);
            // Clan
        }

        private enum RomanceReservationDescription
        {
            CompatibilityINeedSomeoneUpright,
            CompatibilityNeedSomethingInCommon,
            CompatibiliyINeedSomeoneDangerous,
            CompatibilityStrongPoliticalBeliefs,
            AttractionYoureNotMyType,
            AttractionYoureGoodEnough,
            AttractionIAmDrawnToYou,
            PropertyYouSeemRichEnough,
            PropertyWeNeedToBeComfortable,
            PropertyIWantRealWealth,
            PropertyHowCanIMarryAnAdventuress,
            FamilyApprovalIAmGladYouAreFriendsWithOurFamily,
            FamilyApprovalYouNeedToBeFriendsWithOurFamily,
            FamilyApprovalHowCanYouBeEnemiesWithOurFamily,
            FamilyApprovalItWouldBeBestToBefriendOurFamily
        }
    }
}
