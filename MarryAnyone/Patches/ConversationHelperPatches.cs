using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(ConversationHelper))]
    internal sealed class ConversationHelperPatches
    {
        [HarmonyPatch("GetHeroRelationToHeroTextShort")]
        private static void Postfix1()
        {
            /* Should be reimplemented */
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }

        private static TextObject SpousesSpouse(Hero spouse, Hero queriedHero)
        {
            MASettings settings = new();

            // Find out spouse's gender
            // Male spouse
            if (!spouse.IsFemale)
            {
                // Find out spouse's spouse's gender
                if (!queriedHero.IsFemale)
                {
                    // Male other spouse
                    return settings.Polyamory ? FindText("str_husband") : FindText("str_husbands_husband");
                }
                // Female other spouse
                return settings.Polyamory ? FindText("str_wife") : FindText("str_husbands_wife");
            }
            if (!queriedHero.IsFemale)
            {
                return settings.Polyamory ? FindText("str_husband") : FindText("str_wifes_husband");
            }
            return settings.Polyamory ? FindText("str_wife") : FindText("str_wifes_wife");
        }

        [HarmonyPatch("HeroRefersToHero")]
        private static void Postfix2(ref TextObject __result, Hero talkTroop)
        {
            TextObject tempResult = __result;
            __result = HeroAddressesPlayer(talkTroop);
            if (__result == TextObject.Empty)
            {
                __result = tempResult;
            }
        }

        // Account for different relationships
        private static TextObject HeroAddressesPlayer(Hero talkTroop)
        {
            // Same-sex
            if (talkTroop.Spouse == Hero.MainHero && !talkTroop.IsFemale && !Hero.MainHero.IsFemale)
            {
                return new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.Spouse == Hero.MainHero && talkTroop.IsFemale && Hero.MainHero.IsFemale)
            {
                return new TextObject("{=t6sRVI5C}My wife", null);
            }
            // Polygamy and same-sex
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && !talkTroop.IsFemale && !Hero.MainHero.IsFemale)
            {
                return new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && talkTroop.IsFemale && Hero.MainHero.IsFemale)
            {
                return new TextObject("{=t6sRVI5C}My wife", null);
            }
            // Polygamy
            if (talkTroop.ExSpouses.Contains(Hero.MainHero) && talkTroop.IsFemale)
            {
                return new TextObject("{=rPrBa7gK}My husband", null);
            }
            if (talkTroop.ExSpouses.Contains(Hero.MainHero))
            {
                return new TextObject("{=t6sRVI5C}My wife", null);
            }
            return TextObject.Empty;
        }
    }
}