using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MarryAnyone.CampaignSystem
{
    /* Reference CampaignCheats */
    public static class MACommands
    {
        /* Actions */
        [CommandLineFunctionality.CommandLineArgumentFunction("reset_courtships", "marry_anyone")]
        public static string ResetCourtships(List<string> strings)
        {
            if (CampaignCheats.CheckParameters(strings, 0) && !CampaignCheats.CheckHelp(strings))
            {
                Helpers.ResetEndedCourtships();
                return "Success";
            }
            return "Format is \"marry_anyone.reset_courtships\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_main_hero_primary_spouse", "marry_anyone")]
        public static string SetPrimarySpouse(List<string> strings)
        {
            if (CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"marry_anyone.set_main_hero_primary_spouse [HeroName]\".";
            }
            string text = CampaignCheats.ConcatenateString(strings);
            Hero hero = CampaignCheats.GetHero(text);
            if (hero is not null) 
            {
                if (Hero.MainHero.ExSpouses.Contains(hero)) 
                {
                    string result = "Success";
                    Hero.MainHero.Spouse = hero;
                    hero.Spouse = Hero.MainHero;
                    Helpers.RemoveExSpouses(hero);
                    Helpers.RemoveExSpouses(Hero.MainHero);
                    MASettings settings = new();
                    if (settings.PregnancyPlus)
                    {
                        settings.PregnancyPlus = false;
                        result += "\nPregnancy+ disabled to prevent primary spouse from changing.";
                    }
                    return result;
                }
                else if (Hero.MainHero.Spouse == hero)
                {
                    return "Hero is already the Primary Spouse";
                }
                else if (!hero.IsAlive)
                {
                    return "Hero " + text + " is dead.";
                }
                else
                {
                    return "Hero is not married to the Main Hero";
                }
            }
            return "Hero is not Found.\nFormat is \"marry_anyone.set_main_hero_primary_spouse [HeroName]\".";
        }

        /* Settings */
        [CommandLineFunctionality.CommandLineArgumentFunction("set_polygamy_is_enabled", "marry_anyone")]
        public static string SetPolygamyIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.Polygamy = flag;
            return "Setting polygamy is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_polyamory_is_enabled", "marry_anyone")]
        public static string SetPolyamoryIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.Polyamory = flag;
            return "Setting polyamory is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_pregnancy+_is_enabled", "marry_anyone")]
        public static string SetPregnancyPlusIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.PregnancyPlus = flag;
            return "Setting pregnancy+ is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_cheating_is_enabled", "marry_anyone")]
        public static string SetCheatingIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.PregnancyPlus = flag;
            return "Setting cheating is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_skip_courtship_is_enabled", "marry_anyone")]
        public static string SetSkipCourtshipIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.SkipCourtship = flag;
            return "Setting skip courtship is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_retry_courtship_is_enabled", "marry_anyone")]
        public static string SetRetryCourtshipIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect [0/1].";
            }
            bool flag = strings[0] == "1";
            settings.RetryCourtship = flag;
            return "Setting retry courtship is " + (flag ? "enabled." : "disabled.");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_sexual_orientation", "marry_anyone")]
        public static string SetSexualOrientation(List<string> strings)
        {
            MASettings settings = new();
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"marry_anyone.set_sexual_orientation [\"heterosexual\"/\"homosexual\"/\"bisexual\"].";
            }
            string template = CampaignCheats.ConcatenateString(strings);
            if (template == null)
            {
                return "Please enter \"heterosexual\", \"homosexual\", or \"bisexual\"";
            }
            else if (string.Equals(template, "heterosexual", StringComparison.OrdinalIgnoreCase))
            {
                settings.SexualOrientation = "Heterosexual";
                return "Success";
            }
            else if (string.Equals(template, "homosexual", StringComparison.OrdinalIgnoreCase))
            {
                settings.SexualOrientation = "Homosexual";
                return "Success";
            }
            else if (string.Equals(template, "bisexual", StringComparison.OrdinalIgnoreCase))
            {
                settings.SexualOrientation = "Bisexual";
                return "Success";
            }
            return "Please enter \"heterosexual\", \"homosexual\", or \"bisexual\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_clan_after_marriage", "marry_anyone")]
        public static string SetClanAfterMarriage(List<string> strings)
        {
            MASettings settings = new();
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"marry_anyone.set_player_clan [\"default\"/\"always\"/\"never\"].";
            }
            string template = CampaignCheats.ConcatenateString(strings);
            if (template == null)
            {
                return "Please enter \"default\", \"always\", or \"never\"";
            }
            else if (string.Equals(template, "default", StringComparison.OrdinalIgnoreCase))
            {
                settings.ClanAfterMarriage = "Default";
                return "Success";
            }
            else if (string.Equals(template, "always", StringComparison.OrdinalIgnoreCase))
            {
                settings.ClanAfterMarriage = "Always";
                return "Success";
            }
            return "Please enter \"player\" or \"spouse\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_character_template", "marry_anyone")]
        public static string SetCharacterTemplate(List<string> strings)
        {
            MASettings settings = new();
            if (!CampaignCheats.CheckHelp(strings) || CampaignCheats.CheckParameters(strings, 0))
            {
                return "Format is \"marry_anyone.set_character_template [\"default\"/\"wanderer\"]\".";
            }
            string template = CampaignCheats.ConcatenateString(strings);
            if (template == null)
            {
                return "Please enter \"default\" or \"wanderer\"";
            }
            else if (string.Equals(template, "default", StringComparison.OrdinalIgnoreCase))
            {
                settings.TemplateCharacter = "Default";
                return "Success";
            }
            else if (string.Equals(template, "wanderer", StringComparison.OrdinalIgnoreCase))
            {
                settings.TemplateCharacter = "Wanderer";
                return "Success";
            }
            return "Please enter \"default\" or \"wanderer\"";
        }
    }
}