using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MarryAnyone.CampaignSystem
{
    /* Reference CampaignCheats */
    public static class MACommands
    {
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

        [CommandLineFunctionality.CommandLineArgumentFunction("set_polygamy_is_enabled", "marry_anyone")]
        public static string SetPolygamyIsEnabled(List<string> strings)
        {
            MASettings settings = new();
            if (strings.Count != 1 || (strings[0] != "0" && strings[0] != "1"))
            {
                return "Input is incorrect.";
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
                return "Input is incorrect.";
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
                return "Input is incorrect.";
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
                return "Input is incorrect.";
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
                return "Input is incorrect.";
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
                return "Input is incorrect.";
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

        [CommandLineFunctionality.CommandLineArgumentFunction("set_player_clan", "marry_anyone")]
        public static string SetPlayerClan(List<string> strings)
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
                settings.PlayerClan = "Default";
                return "Success";
            }
            else if (string.Equals(template, "always", StringComparison.OrdinalIgnoreCase))
            {
                settings.PlayerClan = "Always";
                return "Success";
            }
            else if (string.Equals(template, "never", StringComparison.OrdinalIgnoreCase))
            {
                settings.PlayerClan = "Never";
                return "Success";
            }
            return "Please enter \"default\", \"always\", or \"never\"";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_clan_leader", "marry_anyone")]
        public static string SetClanLeader(List<string> strings)
        {
            MASettings settings = new();
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is \"marry_anyone.set_clan_leader [\"default\"/\"always\"/\"never\"].";
            }
            string template = CampaignCheats.ConcatenateString(strings);
            if (template == null)
            {
                return "Please enter \"default\", \"always\", or \"never\"";
            }
            else if (string.Equals(template, "default", StringComparison.OrdinalIgnoreCase))
            {
                settings.ClanLeader = "Default";
                return "Success";
            }
            else if (string.Equals(template, "always", StringComparison.OrdinalIgnoreCase))
            {
                settings.ClanLeader = "Always";
                return "Success";
            }
            else if (string.Equals(template, "never", StringComparison.OrdinalIgnoreCase))
            {
                settings.ClanLeader = "Never";
                return "Success";
            }
            return "Please enter \"default\", \"always\", or \"never\"";
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