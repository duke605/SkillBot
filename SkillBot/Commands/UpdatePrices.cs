using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using Discord;
using Fclp;
using Fclp.Internals.Extensions;
using SkillBot.Utilities;

namespace SkillBot.Commands {
    class UpdatePrices : ICommand {

        public string GetCommandName()
        {
            return "update_prices";
        }

        public object ParseCommand(string[] args)
        {
            FluentCommandLineParser parser = new FluentCommandLineParser();
            Arguments a = new Arguments();
            string help;

            parser.Setup<int?>('t', "take")
                .SetDefault(null)
                .Callback(take => a.Take = take);

            parser.Setup<int>('s', "skip")
                .SetDefault(0)
                .Callback(skip => a.Skip = skip);

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

        #pragma warning disable CS4014
        public async void Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;
            Message m = await e.Channel.SendMessage("`Updating item prices...`");
            Console.WriteLine("Updating items...");
            DevelopmentEntities db = new DevelopmentEntities();
            var items = a.Take == null ? db.Items : db.Items.Take(a.Take.Value).OrderBy(i => i.ItemId).Skip(a.Skip);
            int count = items.Count() - 1;

            // Rapidly updating items
            items.ForEach(item =>
            {
                Item i = Runescape.GetItemInfo(item.ItemId, true).Result;

                // Checking if response was successful
                if (i != null)
                    item.Price = i.Price;

                // Telling user there was an error and continuing
                else
                {
                    e.Channel.SendMessage($"`Error updating item \"{item.Name}\"`");
                }

                count--;

                // Saving if all items have been updated
                if (count <= 0)
                {
                    m.Edit("`Updating item prices... Done.`");
                }
                else
                {
                    Console.Write($"\r{count} items remaining.");
                }
            });


            db.SaveChanges();
            db.Dispose();
        }

        private struct Arguments
        {
            public int? Take { get; set; }
            public int Skip { get; set; }
        }
    }
}
