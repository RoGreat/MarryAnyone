using HarmonyLib;
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
                // The old fix for occupations not sticking
                if (hero.Spouse == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    MAHelper.OccupationToLord(hero.CharacterObject);
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
            if (Hero.OneToOneConversationHero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                if (settings.Debug)
                {
                    MAHelper.Print("MCM: " + MASettings.UsingMCM);
                    MAHelper.Print("Difficulty: " + settings.Difficulty);
                    MAHelper.Print("Orientation: " + settings.SexualOrientation);
                    MAHelper.Print("Cheating: " + settings.Cheating);
                    MAHelper.Print("Polygamy: " + settings.Polygamy);
                    MAHelper.Print("Incest: " + settings.Incest);
                    MAHelper.Print("Romantic Level: " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
                }

                // In Fact we can't go through Romance.RomanceLevelEnum.Untested
                // because for the next modification, there will be another romance status
                // And we must have only have one romance status for each relation

                //bool areMarried = Util.Util.AreMarried(Hero.MainHero, Hero.OneToOneConversationHero);
                //if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Ended
                //        && settings.RetryCourtship
                //        && !areMarried)
                //{
                //    // Patch if not a spouse and Romance ended and Retry => reset the romance lvl
                //    => RomanceCampaignBehaviorPatch.conversation_player_can_open_courtship_on_condition
                //
                //    Util.Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero);
                //    ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Untested);
                //    MAHelper.Print("PATCH Ended New Romantic Level: " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
                //}
                //else
                //{

                // Patch can be removed (can stayed too ;) ), i have Crashed my save 
                // with my test and need to restore data.
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
                    {
                        Util.Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero);
                        bool areMarried = Util.Util.AreMarried(Hero.MainHero, Hero.OneToOneConversationHero);
                        if (areMarried)
                        {
                            ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Ended);
                            MAHelper.Print("PATCH Married New Romantic Level: " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString());
                        }
                    }
                //}

            }
            return Hero.OneToOneConversationHero.IsWanderer && Hero.OneToOneConversationHero.IsPlayerCompanion;
        }

        private bool conversation_character_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter));
            return true;
        }

        // This will either skip or continue romance
        // CoupleAgreedOnMarriage = triggers marriage before bartering
        // CourtshipStarted = skip everything
        // return false = carry out entire romance
        private bool conversation_finalize_courtship_for_hero_on_condition()
        {
            ISettingsProvider settings = new MASettings();
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            bool clanLeader = Hero.MainHero.Clan.Leader == Hero.MainHero && Hero.MainHero.Clan.Lords.Contains(Hero.OneToOneConversationHero);

            if (settings.Difficulty == "Realistic")
            {
                // Skip issues with bartering marriage within clans
                // If you are the leader of the clan then it is a problem
                if (clanLeader)
                {
                    MAHelper.Print("Realistic: Clan Leader");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                }
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MAHelper.Print("Realistic: Noble");
                    return false;
                }
                return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            }
            else
            {
                if (clanLeader)
                {
                    if (settings.Difficulty == "Easy")
                    {
                        MAHelper.Print("Easy: Clan Leader");
                        return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                    }
                    MAHelper.Print("Very Easy: Clan Leader");
                    return Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted || romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                }
                if (settings.Difficulty == "Easy" && (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero))
                {
                    MAHelper.Print("Easy: Noble");
                    return false;
                }
                MAHelper.Print("Very Easy");
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
                        if (hero.Clan.Leader == Hero.MainHero)
                        {
                            ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(hero.Clan);
                            if (hero.Clan.Leader == Hero.MainHero)
                            {
                                MAHelper.Print("No Heirs");
                                DestroyClanAction.Apply(hero.Clan);
                                MAHelper.Print("Eliminated Player Clan");
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
                        MAHelper.Print("Lowborn Player Married to Kingdom Ruler");
                    }
                    else
                    {
                        ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        MAHelper.Print("Kingdom Ruler Stepped Down and Married to Player");
                    }
                }
            }
            // New nobility
            MAHelper.OccupationToLord(spouse.CharacterObject);
            if (!spouse.IsNoble)
            {
                spouse.IsNoble = true;
                MAHelper.Print("Spouse to Noble");
            }
            // Dodge the party crash for characters part 1
            bool dodge = false;
            if (spouse.PartyBelongedTo == MobileParty.MainParty)
            {
                AccessTools.Property(typeof(Hero), "PartyBelongedTo").SetValue(spouse, null, null);
                MAHelper.Print("Spouse Already in Player's Party");
                dodge = true;
            }
            // Apply marriage
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            MAHelper.Print("Marriage Action Applied");
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
                MAHelper.Print("Activated Spouse");
            }
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
                MAHelper.Print("Spouse No Longer Companion");
            }
            if (settings.Cheating && cheatedSpouse is not null)
            {
                MAHelper.RemoveExSpouses(cheatedSpouse, true);
                MAHelper.RemoveExSpouses(spouse, true);
                MAHelper.Print("Spouse Broke Off Past Marriage");
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