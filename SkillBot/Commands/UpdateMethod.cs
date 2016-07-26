using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using SkillBot.Utilities;

namespace SkillBot.Commands {
    class UpdateMethod : ICommand {

        public string GetCommandName()
        {
            return "update_method";
        }

        public object ParseCommand(string[] args)
        {
            FluentCommandLineParser parser = new FluentCommandLineParser();
            Arguments a = new Arguments();
            string help;

            parser.Setup<int>('i', "id")
                .Required()
                .Callback(id => a.MethodId = id)
                .WithDescription("The id of the method to be updated.");

            parser.Setup<List<string>>('n', "name")
                .Callback(name => a.Name = name == null ? null : String.Join(" ", name))
                .SetDefault(null)
                .WithDescription("The new name of the method.");

            parser.Setup<double?>('e', "exp")
                .Callback(exp => a.Exp = (decimal?) exp)
                .SetDefault(null)
                .WithDescription("The new exp of the method.");

            parser.Setup<int?>('u', "units")
                .Callback(units => a.Units = units)
                .SetDefault(null)
                .WithDescription("The new units of the method.");

            parser.Setup<int?>('l', "level")
                .Callback(level => a.Level = level)
                .SetDefault(null)
                .WithDescription("The new level of the method");

            parser.SetupHelp("?", "help");

            var result = parser.Parse(args);
            help = HelpFormatter.GetHelpForCommand(parser);

            // Showing mistakes and proper command usage
            if (result.HasErrors) {
                return $"{result.ErrorText}\r\n" +
                       $"Command Usage:\r\n" +
                       $"{help}";
            }

            // Showing help
            if (result.HelpCalled) {
                return $"Command Usage:\r\n{help}";
            }

            return a;
        }

        public async void Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;
            long userId = (long) e.User.Id;

            using (DevelopmentEntities db = new DevelopmentEntities())
            {
                Method m = db.Methods.Find(a.MethodId);

                // Checking if a method was found
                if (m == null)
                {
                    await e.Channel.SendMessage($"`A method with the id of \"{a.MethodId}\" could not be found.`");
                    return;
                }

                // Checking if the user can edit that method
                if (m.UserId != userId && userId != Secret.MyId)
                {
                    await e.Channel.SendMessage($"`You do not have permission to update methods you did not make.`");
                    return;
                }

                // Updating
                m.Exp = a.Exp ?? m.Exp;
                m.Level = a.Level ?? m.Level;
                m.Name = a.Name ?? m.Name;
                m.Units = a.Units ?? m.Units;

                db.SaveChanges();

                await e.Channel.SendMessage($"`Successfully updated method.`");
            }
        }

        private struct Arguments
        {
            public int MethodId { get; set; }
            public string Name { get; set; }
            public decimal? Exp { get; set; }
            public int? Units { get; set; }
            public int? Level { get; set; }
        }
    }
}
