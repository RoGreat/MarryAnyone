using TaleWorlds.CampaignSystem;
using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace MarryAnyone
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior), "conversation_player_can_open_courtship_on_condition")]
    internal class ConversationPlayerCanOpenCourtshipOnConditionPatch
    {
        private static bool Prefix(ref bool __result)
        {
            __result = ConversationPlayerCanOpenCourtshipOnConditionPatch.conversation_player_can_open_courtship_on_condition();
            return false;
        }

        private static bool conversation_player_can_open_courtship_on_condition()
        {
            MAConfig config = MASettings.Config;
            bool isHeterosexual = config.SexualOrientation == SexualOrientation.Heterosexual;
            bool isHomosexual = config.SexualOrientation == SexualOrientation.Homosexual;
            bool isBisexual = config.SexualOrientation == SexualOrientation.Bisexual;

            bool flag = Hero.MainHero.IsFemale && isHeterosexual || !Hero.MainHero.IsFemale && isHomosexual || !Hero.OneToOneConversationHero.IsFemale && isBisexual;

            if (Hero.OneToOneConversationHero == null)
            {
                return false;
            }
            if (Romance.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
            {
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=bjJs0eeB}My lord, I note that you have not yet taken a wife."
                            : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("FLIRTATION_LINE",
                        flag
                            ? "{=bjJs0eeB}Goodman, I note that you have not yet taken a wife."
                            : "{=v1hC6Aem}Goodwife, I wish to profess myself your most ardent admirer", false);
                }
                return true;
            }
            if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities)
            {
                if (Hero.OneToOneConversationHero.IsNoble || Hero.OneToOneConversationHero.IsMinorFactionHero)
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
                            ? "{=2WnhUBMM}Goodman, may you give me another chance to prove myself?"
                            : "{=4iTaEZKg}Goodwife, may you give me another chance to prove myself?", false);
                }
                return true;
            }
            return false;
        }
    }
}