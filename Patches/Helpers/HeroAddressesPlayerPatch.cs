using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Helpers
{
    [HarmonyPatch(typeof(ConversationHelper), "HeroAddressesPlayer")]
    internal class HeroAddressesPlayerPatch
    {
        // Need to account for different relationships
        private static void Postfix(ref TextObject __result, Hero talkTroop)
        {
            // Same-sex
            if (talkTroop.Spouse == Hero.MainHero && !talkTroop.IsFemale && !Hero.MainHero.IsFemale)
            {
                __result = new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.Spouse == Hero.MainHero && talkTroop.IsFemale && Hero.MainHero.IsFemale)
            {
                __result = new TextObject("{=t6sRVI5C}My wife", null);
            }
            // Polygamy and same-sex
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && !talkTroop.IsFemale && !Hero.MainHero.IsFemale)
            {
                __result = new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && talkTroop.IsFemale && Hero.MainHero.IsFemale)
            {
                __result = new TextObject("{=t6sRVI5C}My wife", null);
            }
            // Polygamy
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && talkTroop.IsFemale)
            {
                __result = new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.ExSpouses.Contains(Hero.MainHero))
            {
                __result = new TextObject("{=t6sRVI5C}My wife", null);
            }
        }
    }
}
