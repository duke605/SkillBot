using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Fclp.Internals.Extensions;
using SkillBot.Utilities;

namespace SkillBot.Commands {
    class DeleteMethod :ICommand {

        public string GetCommandName()
        {
            return "delete_method";
        }

        public object ParseCommand(string[] args)
        {
            FluentCommandLineParser parser = new FluentCommandLineParser();
            Arguments a = new Arguments();
            string help;

            parser.Setup<long>('i', "id")
                .Required()
                .Callback(id => a.Id = id)
                .WithDescription("The id of the method you wish to delete.");

            parser.Setup<bool>('a', "admin")
                .SetDefault(false)
                .Callback(admin => a.Admin = admin)
                .WithDescription("Uses the specified id as a user id and deletes all methods made by that user.");

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

            using (DevelopmentEntities db = new DevelopmentEntities())
            {
                // Checking if user has permission to use admin
                if (a.Admin && (long) e.User.Id != Secret.MyId)
                {
                    await e.Channel.SendMessage("`You do not have permission to use this command as admin.`");
                    return;
                }

                IQueryable<Method> methods = a.Admin
                    ? db.Methods.Where(m => m.UserId == a.Id)
                    : ((long)e.User.Id != Secret.MyId 
                    ? db.Methods.Where(m => m.MethodId == a.Id && m.UserId == (long) e.User.Id)
                    : db.Methods.Where(m => m.MethodId == a.Id));

                // Checking if any methods were returned
                if (!methods.Any())
                {
                    if (a.Admin)
                        await e.Channel.SendMessage($"`No methods could be found made by the user with the id of \"{a.Id}\"`");
                    else
                        await e.Channel.SendMessage($"`No method could be found with the id of \"{a.Id}\"`");

                    return;
                }

                // Deleting methods
                foreach (var m in methods)
                {
                    // Removing inputs/outputs
                    db.Inputs.RemoveRange(m.Inputs);
                    db.Outputs.RemoveRange(m.Outputs);
                }

                // Removing self
                db.Methods.RemoveRange(methods);
                db.SaveChanges();
                await e.Channel.SendMessage($"`Successfully removed method(s)`");
            }
        }

        private struct Arguments
        {
            public long Id { get; set; }
            public bool Admin { get; set; }
        }
    }
}
