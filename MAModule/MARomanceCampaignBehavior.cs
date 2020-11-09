using HarmonyLib;
using MarryAnyone.Patches;
using MarryAnyone.Settings;
using MCM.Abstractions.Settings.Base.PerSave;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

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
            MASubModule.Debug("Difficulty: " + MASettings.Instance.Difficulty.SelectedValue);
            MASubModule.Debug("Orientation: " + MASettings.Instance.SexualOrientation.SelectedValue);
            MASubModule.Debug("Polygamy: " + MASettings.Instance.IsPolygamous);
            MASubModule.Debug("Incest: " + MASettings.Instance.IsIncestuous);
            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion;
        }

        private bool conversation_character_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter), false);
            return true;
        }

        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (MASettings.Instance.Difficulty.SelectedValue == "Realistic")
            {
                if (DefaultMarriageModelPatch.DiscoverAncestors(Hero.MainHero, 3).Intersect(DefaultMarriageModelPatch.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any() && MASettings.Instance.IsIncestuous)
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
                if (DefaultMarriageModelPatch.DiscoverAncestors(Hero.MainHero, 3).Intersect(DefaultMarriageModelPatch.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any() && MASettings.Instance.IsIncestuous)
                {
                    if (MASettings.Instance.Difficulty.SelectedValue == "Easy")
                    {
                        MASubModule.Debug("Easy: Incest");
                        return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    }
                    MASubModule.Debug("Very Easy: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                }
                if (MASettings.Instance.Difficulty.SelectedValue == "Easy" && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
                {
                    MASubModule.Debug("Easy: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
            }
        }

        private void conversation_courtship_success_on_consequence()
        {
            if (Hero.OneToOneConversationHero.IsFactionLeader && !Hero.OneToOneConversationHero.IsMinorFactionHero)
            {
                if (Hero.MainHero.Clan.Kingdom != Hero.OneToOneConversationHero.Clan.Kingdom)
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(Hero.MainHero.Clan, Hero.OneToOneConversationHero.Clan.Kingdom, true);
                    MASubModule.Debug("Joined Spouse's Kingdom");
                }
            }
            if (Hero.OneToOneConversationHero.Clan == null)
            {
                AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(Hero.OneToOneConversationHero.CharacterObject, Occupation.Lord);
                Hero.OneToOneConversationHero.Clan = Clan.PlayerClan;
                MASubModule.Debug("Joined Player's Clan");
            }
            // Activate character if not already activated
            if (!Hero.OneToOneConversationHero.IsActive)
            {
                Hero.OneToOneConversationHero.HasMet = true;
                Hero.OneToOneConversationHero.ChangeState(Hero.CharacterStates.Active);
                MASubModule.Debug("Activated Spouse");
            }
            // Dodge the party crash for characters
            if (!Hero.OneToOneConversationHero.IsNoble)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(Hero.OneToOneConversationHero, null, null);
            }
            // Apply marriage
            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
            // Finalize marriage for new nobility
            if (!Hero.OneToOneConversationHero.IsNoble)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(Hero.OneToOneConversationHero, MobileParty.MainParty, null);
                Hero.OneToOneConversationHero.IsNoble = true;
                MASubModule.Debug("Spouse To Lord");
            }
            if (Hero.OneToOneConversationHero.IsPlayerCompanion)
            {
                Hero.OneToOneConversationHero.CompanionOf = null;
                MASubModule.Debug("No Longer Companion");
            }
            MASubModule.Debug("Marriage Action Applied");
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