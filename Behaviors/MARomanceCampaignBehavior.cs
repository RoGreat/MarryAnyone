using HarmonyLib;
using MarryAnyone.Patches;
using MarryAnyone.Settings;
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
            foreach (Hero hero in Hero.All.ToList())
            {
                if (hero.Spouse == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
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
            if (Hero.OneToOneConversationHero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                MASubModule.Print("MCM: " + MASettings.UsingMCM);
                MASubModule.Print("Difficulty: " + settings.Difficulty);
                MASubModule.Print("Orientation: " + settings.SexualOrientation);
                MASubModule.Print("Cheating: " + settings.Cheating);
                MASubModule.Print("Polygamy: " + settings.Polygamy);
                MASubModule.Print("Incest: " + settings.Incest);
            }
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
            bool discoverAncestors = DefaultMarriageModelPatch.DiscoverAncestors(Hero.MainHero, 3).Intersect(DefaultMarriageModelPatch.DiscoverAncestors(Hero.OneToOneConversationHero, 3)).Any();
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            if (settings.Difficulty == "Realistic")
            {
                if ((Clan.PlayerClan.Lords.Contains(Hero.OneToOneConversationHero) || discoverAncestors) && settings.Incest)
                {
                    MASubModule.Print("Realistic: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                }
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MASubModule.Print("Realistic: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            }
            else
            {
                if ((Clan.PlayerClan.Lords.Contains(Hero.OneToOneConversationHero) || discoverAncestors) && settings.Incest)
                {
                    if (settings.Difficulty == "Easy")
                    {
                        MASubModule.Print("Easy: Incest");
                        return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    }
                    MASubModule.Print("Very Easy: Incest");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                }
                if (settings.Difficulty == "Easy" && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
                {
                    MASubModule.Print("Easy: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
            }
        }

        private void conversation_courtship_success_on_consequence()
        {
            // New marriages not shown right away. Need to refresh?
            ISettingsProvider settings = new MASettings();
            Hero hero = Hero.MainHero;
            Hero spouse = Hero.OneToOneConversationHero;
            Hero oldSpouse = hero.Spouse;
            Hero cheatedSpouse = spouse.Spouse;

            // If you are marrying a kingdom ruler as a kingdom ruler yourself,
            // the kingdom ruler will have to give up being clan head.
            // Apparently causes issues if this is not done.
            if (spouse.IsFactionLeader && !spouse.IsMinorFactionHero)
            {
                if (hero.Clan.Kingdom != spouse.Clan.Kingdom)
                {
                    if (hero.Clan.Kingdom?.Leader != hero)
                    {
                        // Join kingdom due to lowborn status
                        // Need to separate out new player's clan and previous player's clan
                        // ChangeKingdomAction.ApplyByJoinToKingdom(hero.Clan, spouse.Clan.Kingdom);
                        if (hero.Clan.Leader == Hero.MainHero)
                        {
                            ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(hero.Clan);
                            if (hero.Clan.Leader == Hero.MainHero)
                            {
                                MASubModule.Print("No Heirs");
                                DestroyClanAction.Apply(hero.Clan);
                                MASubModule.Print("Eliminated Player Clan");
                            }
                        }
                        foreach (Hero companion in hero.Clan.Companions.ToList())
                        {
                            bool inParty = false;
                            if (companion.PartyBelongedTo == MobileParty.MainParty)
                            {
                                inParty = true;
                            }
                            RemoveCompanionAction.ApplyByFire(hero.Clan, companion);
                            AddCompanionAction.Apply(spouse.Clan, companion);
                            if (inParty)
                            {
                                AddHeroToPartyAction.Apply(companion, MobileParty.MainParty, true);
                            }
                        }
                        hero.Clan = spouse.Clan;
                        var current = Traverse.Create<Campaign>().Property("Current").GetValue<Campaign>();
                        Traverse.Create(current).Property("PlayerDefaultFaction").SetValue(spouse.Clan);
                        MASubModule.Print("Lowborn Player Married to Kingdom Ruler");
                    }
                    else
                    {
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        MASubModule.Print("Kingdom Ruler Stepped Down and Married to Player");
                    }
                }
            }
            // New nobility
            MAHelper.OccupationToLord(hero.CharacterObject);
            if (!spouse.IsNoble)
            {
                spouse.IsNoble = true;
                MASubModule.Print("Spouse to Noble");
            }
            // Dodge the party crash for characters part 1
            bool dodge = false;
            if (spouse.PartyBelongedTo == MobileParty.MainParty)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, null, null);
                MASubModule.Print("Spouse Already in Player's Party");
                dodge = true;
            }
            // Apply marriage
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            MASubModule.Print("Marriage Action Applied");
            if (oldSpouse is not null)
            {
                MAHelper.RemoveExSpouses(oldSpouse);
            }
            // Dodge the party crash for characters part 2
            if (dodge)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, MobileParty.MainParty, null);
            }
            // Activate character if not already activated
            if (!spouse.HasMet)
            {
                spouse.HasMet = true;
            }
            if (!spouse.IsActive)
            {
                spouse.ChangeState(Hero.CharacterStates.Active);
                MASubModule.Print("Activated Spouse");
            }
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
                MASubModule.Print("Spouse No Longer Companion");
            }
            if (settings.Cheating && cheatedSpouse is not null)
            {
                MAHelper.RemoveExSpouses(cheatedSpouse, true);
                MAHelper.RemoveExSpouses(spouse, true);
                MASubModule.Print("Spouse Broke Off Past Marriage");
            }
            MAHelper.RemoveExSpouses(hero);
            MAHelper.RemoveExSpouses(spouse);
            PlayerEncounter.LeaveEncounter = true;
            // New fix to stop some kingdom rulers from disappearing
            if (spouse.PartyBelongedTo != MobileParty.MainParty)
            {
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