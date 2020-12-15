using HarmonyLib;
using MarryAnyone.Models.Patches;
using MarryAnyone.Settings;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using MarryAnyone.Behaviors.Helpers;

namespace MarryAnyone.Behaviors
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        protected void AddDialogs(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.All)
            {
                if ((hero.Spouse == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(hero)) && hero.CharacterObject.Occupation != Occupation.Lord)
                {
                    AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(hero.CharacterObject, Occupation.Lord);
                    hero.Clan = null;
                    hero.Clan = Clan.PlayerClan;
                } 
            }

            // To begin the dialog for companions
            starter.AddPlayerLine("main_option_discussions_MA", "hero_main_options", "lord_talk_speak_diplomacy_MA", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(conversation_begin_courtship_for_hero_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_agrees_to_discussion_MA", "lord_talk_speak_diplomacy_MA", "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(conversation_character_agrees_to_discussion_on_condition), null, 100, null);

            // From previous iteration
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);
            starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 120, null);

            starter.AddPlayerLine("hero_romance_task", "hero_main_options", "lord_start_courtship_response_3", "{=cKtJBdPD}I wish to offer my hand in marriage.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), null, 140, null, null);

            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(conversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_courtship_success_on_consequence), 140, null);
        }

        private bool conversation_begin_courtship_for_hero_on_condition()
        {
            ISettingsProvider settings = new MASettings();
            MASubModule.Debug("MCM: " + MASettings.UsingMCM);
            MASubModule.Debug("Difficulty: " + settings.Difficulty);
            MASubModule.Debug("Orientation: " + settings.SexualOrientation);
            MASubModule.Debug("Become Ruler: " + settings.BecomeRuler);
            MASubModule.Debug("Cheating: " + settings.Cheating);
            MASubModule.Debug("Polygamy: " + settings.Polygamy);
            MASubModule.Debug("Incest: " + settings.Incest);
            return Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion;
        }

        private bool conversation_character_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter), false);
            return true;
        }

        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            ISettingsProvider settings = new MASettings();
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (settings.Difficulty == "Realistic")
            {
                if (DefaultMarriageModelPatch.DiscoverAncestors(Hero.MainHero, 3).Intersect(DefaultMarriageModelPatch.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any() && settings.Incest)
                {
                    MASubModule.Debug("Realistic: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                }
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MASubModule.Debug("Realistic: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            }
            else
            {
                if (DefaultMarriageModelPatch.DiscoverAncestors(Hero.MainHero, 3).Intersect(DefaultMarriageModelPatch.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any() && settings.Incest)
                {
                    if (settings.Difficulty == "Easy")
                    {
                        MASubModule.Debug("Easy: Incest");
                        return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    }
                    MASubModule.Debug("Very Easy: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                }
                if (settings.Difficulty == "Easy" && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
                {
                    MASubModule.Debug("Easy: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
            }
        }

        private void conversation_courtship_success_on_consequence()
        {
            ISettingsProvider settings = new MASettings();
            Hero hero = Hero.MainHero;
            Hero spouse = Hero.OneToOneConversationHero;
            Hero oldSpouse = hero.Spouse;
            Hero cheatedSpouse = spouse.Spouse;
            if (spouse.IsFactionLeader && !spouse.IsMinorFactionHero)
            {
                if (hero.Clan.Kingdom != spouse.Clan.Kingdom)
                {
                    // Extra option in case people want to become a ruler
                    // Might need to flesh this out in the future
                    if (settings.BecomeRuler)
                    {
                        if (hero.Clan.Kingdom == null)
                        {
                            ChangeKingdomAction.ApplyByCreateKingdom(hero.Clan, spouse.Clan.Kingdom, false);
                            MASubModule.Debug("Player is Kingdom Ruler");
                        }
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        ChangeKingdomAction.ApplyByJoinToKingdom(spouse.Clan, hero.Clan.Kingdom, true);
                        spouse.Clan = hero.Clan;
                        MASubModule.Debug("Spouse Joined Kingdom");
                    }
                    else
                    {
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(hero.Clan);
                        ChangeKingdomAction.ApplyByJoinToKingdom(hero.Clan, spouse.Clan.Kingdom, true);
                        hero.Clan = spouse.Clan;
                        MASubModule.Debug("Joined Spouse's Kingdom");
                    }
                }
            }
            if (CharacterObject.OneToOneConversationCharacter.Occupation != Occupation.Lord)
            {
                AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(spouse.CharacterObject, Occupation.Lord);
                MASubModule.Debug("Spouse to Lord");
            }
            if (spouse.Clan == null)
            {
                spouse.Clan = Clan.PlayerClan;
                MASubModule.Debug("Joined Player's Clan");
            }
            // Activate character if not already activated
            if (!spouse.IsActive)
            {
                spouse.ChangeState(Hero.CharacterStates.Active);
                MASubModule.Debug("Activated Spouse");
            }
            // Dodge the party crash for characters part 1
            bool dodge = false;
            if (spouse.PartyBelongedTo == MobileParty.MainParty)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, null, null);
                MASubModule.Debug("Spouse Already in Player's Party");
                dodge = true;
            }
            // Apply marriage
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            MASubModule.Debug("Marriage Action Applied");
            if (oldSpouse != null)
            {
                MASpouseHelper.RemoveExSpouses(oldSpouse);
            }
            // Dodge the party crash for characters part 2
            if (dodge)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, MobileParty.MainParty, null);
            }
            // Finalize marriage for new nobility
            if (!spouse.IsNoble)
            {
                spouse.IsNoble = true;
                MASubModule.Debug("Spouse to Noble");
            }
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
                MASubModule.Debug("Spouse No Longer Companion");
            }
            if (settings.Cheating && cheatedSpouse != null)
            {
                MASpouseHelper.RemoveExSpouses(cheatedSpouse, false);
                MASpouseHelper.RemoveExSpouses(spouse, false);
                MASubModule.Debug("Spouse Broke Off Past Marriage");
            }
            MASpouseHelper.RemoveExSpouses(hero);
            MASpouseHelper.RemoveExSpouses(spouse);
            PlayerEncounter.LeaveEncounter = true;
            if (spouse.PartyBelongedTo != MobileParty.MainParty)
            {
                // New fix to stop some kingdom rulers from disappearing
                AddHeroToPartyAction.Apply(spouse, MobileParty.MainParty, true);
            }
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