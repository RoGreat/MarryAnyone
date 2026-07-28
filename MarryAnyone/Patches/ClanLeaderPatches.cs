using HarmonyLib;

using MarryAnyone.Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(RomanceCampaignBehavior))]
    class ClanLeaderPatches
    {
        [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
        static bool Prefix(ref bool __result)
        {
            if (Hero.OneToOneConversationHero.Clan is not null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero)
            {
                return true;
            }
            __result = NotLordHelper.MarriageCourtshipPossibility(Hero.MainHero, Hero.OneToOneConversationHero) && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
            return false;
        }
    }
}
