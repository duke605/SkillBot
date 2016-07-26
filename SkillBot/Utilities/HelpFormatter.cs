using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fclp;
using Fclp.Internals.Extensions;

namespace SkillBot.Utilities {
    class HelpFormatter {

        public static string GetHelpForCommand(FluentCommandLineParser parser)
        {
            var lines = parser.Options.Select(o =>
            {
                string options = "";

                // Adding shortname
                options += o.HasShortName ? $"-{o.ShortName}, " : "";

                // Adding long name
                options += $"--{o.LongName}";

                return new
                {
                    OptionName = options,
                    Descrption = $"({(o.IsRequired ? "Required" : "Optional")}) {o.Description}"
                };
            });

            int longestOption = lines.Aggregate(0, (acc, e) => e.OptionName.Length > acc ? e.OptionName.Length : acc) + 1;
            string help = "";

            lines.ForEach(l => help += l.OptionName.PadRight(longestOption) + l.Descrption + "\r\n");
            help = help.TrimEnd(new char[] {'\r', '\n'});

            return help;
        }
    }
}
