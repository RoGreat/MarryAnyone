using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using MarryAnyone.Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Behaviors
{
    internal partial class RomanceCampaignBehaviorPatches
    {
        [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_player_can_open_courtship_on_condition")]
        private static bool Prefix(ref bool __result)
        {
            __result = conversation_player_can_open_courtship_on_condition();
            return false;
        }

        public static bool conversation_player_can_open_courtship_on_condition()
        {
            if (Hero.OneToOneConversationHero is null)
            {
                return false;
            }
            MASettings settings = new();
            bool flag = Hero.MainHero.IsFemale && settings.SexualOrientation == "Heterosexual" 
                || !Hero.MainHero.IsFemale && settings.SexualOrientation == "Homosexual" 
                || !Hero.OneToOneConversationHero.IsFemale && settings.SexualOrientation == "Bisexual";

            Romance.RomanceLevelEnum romanceLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            MADebug.Print("Courtship Possible: " + MarriageCourtshipPossibilityPatch(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero));
            MADebug.Print("Romantic Level: " + romanceLevel);
            MADebug.Print("Retry Courtship: " + settings.RetryCourtship);

            if (MarriageCourtshipPossibilityPatch(SubModule.RomanceCampaignBehaviorInstance!, Hero.MainHero, Hero.OneToOneConversationHero) 
                && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
            {
                if ((Hero.OneToOneConversationHero.Clan?.IsNoble ?? false) || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    if (Hero.OneToOneConversationHero.Spouse is null)
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE",
                            flag
                                ? "{=lord_flirt}My lord, I note that you have not yet taken a spouse."
                                : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE",
                            flag
                                ? "{=lord_cheating_flirt}My lord, I note that you might wish for a new spouse."
                                : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                    }
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=goodman_flirt}Goodman, I note that you have not yet taken a spouse."
                            : "{=goodwife_flirt}Goodwife, I wish to profess myself your most ardent admirer.", false);
                }
                return true;
            }
            if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility
                || romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities
                || (romanceLevel == Romance.RomanceLevelEnum.Ended && settings.RetryCourtship))
            {
                if ((Hero.OneToOneConversationHero.Clan?.IsNoble ?? false) || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=2WnhUBMM}My lord, may you give me another chance to prove myself?"
                            : "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=goodman_chance}Goodman, may you give me another chance to prove myself?"
                            : "{=goodwife_chance}Goodwife, may you give me another chance to prove myself?", false);
                }
                // Retry Courtship feature!
                if (settings.RetryCourtship)
                {
                    if (romanceLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanceLevel == Romance.RomanceLevelEnum.Ended)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CourtshipStarted);
                    }
                    else if (romanceLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
                    {
                        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible);
                    }
                }
                return true;
            }
            return false;
        }
    }
}