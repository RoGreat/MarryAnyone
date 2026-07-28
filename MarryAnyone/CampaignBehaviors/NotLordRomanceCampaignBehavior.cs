using System.Collections.Generic;
using System.Linq;
using System;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using MarryAnyone.Helpers;

namespace MarryAnyone.CampaignBehaviors
{
    // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignBehaviors.RomanceCampaignBehavior
    internal class NotLordRomanceCampaignBehavior : CampaignBehaviorBase
    {
        private readonly List<Agent> _courtedAgents = new();

        public NotLordRomanceCampaignBehavior() { }

        public override void SyncData(IDataStore dataStore) { }

        public NotLordConversationsCampaignBehavior? NotLordConversationsCampaignBehavior;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            NotLordConversationsCampaignBehavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<NotLordConversationsCampaignBehavior>();
            NotLordConversationsCampaignBehavior.AddDialogs(campaignGameStarter, CampaignBehaviorRomanceDialog);
        }

        private static bool MarriageCourtshipPossibility(Hero person1, Hero person2)
        {
            return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2) && !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction);
        }

        private IEnumerable<RomanceReservationDescription> GetRomanceReservations(Hero wooed, Hero wooer)
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            List<RomanceReservationDescription> list = new();
            bool flag = wooed.GetTraitLevel(DefaultTraits.Honor) + wooed.GetTraitLevel(DefaultTraits.Mercy) > 0;
            bool flag4 = wooed.GetTraitLevel(DefaultTraits.Valor) - wooed.GetTraitLevel(DefaultTraits.Calculating) > 0 && wooed.GetTraitLevel(DefaultTraits.Mercy) <= 0;

            if (createdHero is null)
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
            int attractionValuePercentage = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(createdHero, Hero.MainHero);
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
            return list;
        }

        private bool conversation_player_can_open_courtship_on_condition()
        {
            // bin\...\TaleWorlds.CampaignSystem.dll -> CampaignCheats.CreateRandomClan
            Agent? conversationAgent = MarryAnyoneHelper.GetConversationAgent();
            if (conversationAgent is null || !MarryAnyoneHelper.IsNotLord() || _courtedAgents.Contains(conversationAgent))
            {
                return false;
            }

            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (createdHero is null)
            {
                return false;
            }

            StringHelpers.SetCharacterProperties("HERO", createdHero.CharacterObject, null, false);
            if (MarriageCourtshipPossibility(Hero.MainHero, createdHero) && Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.Untested)
            {
                if (Hero.MainHero.IsFemale)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=hdfyaseB}{HERO.NAME}, I note that you have not yet taken a wife.", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=udfhsbvm}{HERO.NAME}, I wish to profess myself your most ardent admirer.", false);
                }
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                if (Hero.MainHero.IsFemale)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=8fdsvnMM}{HERO.NAME}, may you give me another chance to prove myself?", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=8fdsvnMM}{HERO.NAME}, may you give me another chance to prove myself?", false);
                }
                return true;
            }
            return false;
        }

        private void conversation_player_opens_courtship_on_consequence()
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (Romance.GetRomanticLevel(Hero.MainHero, createdHero) != Romance.RomanceLevelEnum.FailedInCompatibility && Romance.GetRomanticLevel(Hero.MainHero, createdHero) != Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                ChangeRomanticStateAction.Apply(Hero.MainHero, createdHero, Romance.RomanceLevelEnum.CourtshipStarted);
            }
        }

        private bool conversation_courtship_initial_reaction_on_condition()
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (createdHero is null || !MarryAnyoneHelper.IsNotLord())
            {
                return false;
            }
            IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(createdHero, Hero.MainHero);
            if (Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInPracticalities || Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                return false;
            }
            MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.AttractionIAmDrawnToYou) ? "{=WEkjz9tg}Ah! Yes... We are considering offers... Did you have someone in mind?" : "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
            return true;
        }

        private bool conversation_courtship_decline_reaction_to_player_on_condition()
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (createdHero is null || !MarryAnyoneHelper.IsNotLord())
            {
                return false;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=emLBsWj6}I am terribly sorry. It is practically not possible for us to be married.", false);
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, createdHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
            {
                MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=s7idfhBO}I am terribly sorry. We are not really compatible with each other.", false);
                return true;
            }
            return false;
        }

        private bool conversation_player_eligible_for_marriage_with_conversation_hero_on_condition()
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (createdHero is null || !MarryAnyoneHelper.IsNotLord())
            {
                return false;
            }
            bool result = Hero.MainHero.Spouse is null && createdHero is not null && MarriageCourtshipPossibility(Hero.MainHero, createdHero);
            return result;
        }

        private bool conversation_courtship_reaction_to_player_on_condition()
        {
            Hero? createdHero = MarryAnyoneHelper.GetCreatedHero(NotLordConversationsCampaignBehavior!.createdHeroes);
            if (createdHero is null || !MarryAnyoneHelper.IsNotLord())
            {
                return false;
            }
            IEnumerable<RomanceReservationDescription> romanceReservations = GetRomanceReservations(createdHero, Hero.MainHero);
            bool flag = createdHero.GetTraitLevel(DefaultTraits.Generosity) + createdHero.GetTraitLevel(DefaultTraits.Mercy) > 0;
            TraitObject persona = createdHero.CharacterObject.GetPersona();
            bool flag2 = ConversationTagHelper.UsesHighRegister(createdHero.CharacterObject);
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
                        return true;
                    }
                }
                if (flag)
                {
                    if (Enumerable.Any(romanceReservations, x => x == RomanceReservationDescription.PropertyIWantRealWealth))
                    {
                        MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION_TO_PLAYER", "{=gS1noLvf}I do not know whether to find that charming or impertinent...", false);
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
            return true;
        }

        private void courtship_conversation_leave_on_consequence()
        {
            Agent? conversationAgent = MarryAnyoneHelper.GetConversationAgent();
            if (conversationAgent is not null)
            {
                _courtedAgents.Add(conversationAgent);
            }
            if (PlayerEncounter.Current is not null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        private void CampaignBehaviorRomanceDialog(CampaignGameStarter starter, string input, string output = "close_window")
        {
            starter.AddPlayerLine(input + "lord_special_request_flirt", input + "lord_talk_speak_diplomacy_2", input + "lord_start_courtship_response", "{=!}{FLIRTATION_LINE}", new ConversationSentence.OnConditionDelegate(conversation_player_can_open_courtship_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_player_opens_courtship_on_consequence), 100, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response", input + "lord_start_courtship_response", input + "lord_start_courtship_response_player_offer", "{=!}{INITIAL_COURTSHIP_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_initial_reaction_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_decline", input + "lord_start_courtship_response", output, "{=!}{COURTSHIP_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(conversation_courtship_decline_reaction_to_player_on_condition), null, 100, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_2", input + "lord_start_courtship_response_player_offer", input + "lord_start_courtship_response_2", "{=gnXoIChw}Perhaps you and I...", new ConversationSentence.OnConditionDelegate(conversation_player_eligible_for_marriage_with_conversation_hero_on_condition), null, 120, null, null);
            starter.AddPlayerLine(input + "lord_start_courtship_response_player_offer_nevermind", input + "lord_start_courtship_response_player_offer", output, "{=D33fIGQe}Never mind.", null, null, 120, null, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_2", input + "lord_start_courtship_response_3", "{=!}{INITIAL_COURTSHIP_REACTION_TO_PLAYER}", new ConversationSentence.OnConditionDelegate(conversation_courtship_reaction_to_player_on_condition), null, 100, null);
            starter.AddDialogLine(input + "lord_start_courtship_response_3", input + "lord_start_courtship_response_3", "close_window", "{=YHZsHohq}We meet from time to time, as is the custom, to see if we are right for each other. I hope to see you again soon.", null, new ConversationSentence.OnConsequenceDelegate(courtship_conversation_leave_on_consequence), 100, null);
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
