using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Fclp;
using SkillBot.Extensions;
using SkillBot.Utilities;

namespace SkillBot.Commands {
    class AddMethod : ICommand {

        public string GetCommandName()
        {
            return "add_method";
        }

        public object ParseCommand(string[] args)
        {
            FluentCommandLineParser parser = new FluentCommandLineParser();
            Arguments a = new Arguments();
            string help = "**ERROR**";

            parser.Setup<List<string>>('i', "inputs")
                .Callback(inputs => a.Inputs = inputs.Select(i =>
                {
                    string[] pair = i.Split(':');

                    return new Arguments.Item()
                    {
                        Id = pair[0].ToInt(),
                        Qty = pair[1].ToInt()
                    };
                }))
                .SetDefault(new List<string>())
                .WithDescription("List of items ids and quantities needed for this method. (Eg. id:quantity)");

            parser.Setup<List<string>>('o', "outputs")
                .Callback(outputs => a.Outputs = outputs.Select(i =>
                {
                    string[] pair = i.Split(':');

                    return new Arguments.Item() {
                        Id = pair[0].ToInt(),
                        Qty = pair[1].ToInt()
                    };
                }))
                .SetDefault(new List<string>())
                .WithDescription("List of item ids and quantities produced by this method (Eg. id:quantity)");

            parser.Setup<List<string>>('n', "name")
                .Required()
                .Callback(name => a.Name = String.Join(" ", name))
                .WithDescription("The name of the method. (Usually named the item it produces)");

            parser.Setup<double>('e', "exp")
                .Required()
                .Callback(exp => a.Exp = exp)
                .WithDescription("The amount of exp that this method will render.");

            parser.Setup<int>('u', "units")
                .Required()
                .Callback(units => a.Units = units)
                .WithDescription("The amount of times this method can be done in an hour.");

            parser.Setup<int>('l', "level")
                .Required()
                .Callback(level => a.Level = level)
                .WithDescription("The required level to do this method. (Without boosting)");

            parser.Setup<Skill>('s', "skill")
                .Required()
                .Callback(skill => a.Skill = skill)
                .WithDescription("The skill this method of training is for.");

            parser.SetupHelp("?", "help")
                .Callback(h => help = h);

            var result = parser.Parse(args);
            parser.HelpOption.ShowHelp(parser.Options);

            // Showing mistakes and proper command usage
            if (result.HasErrors) {
                return $"{result.ErrorText}\r\n" +
                       $"Command Usage:\r\n" +
                       $"To resolve item names into item ids use http://runescape.wikia.com/wiki/Module:Exchange/item_name\r\n" +
                       $"{help}";
            }

            // Showing help
            if (result.HelpCalled) {
                return $"Command Usage:\r\n" +
                       $"To resolve item names into item ids use http://runescape.wikia.com/wiki/Module:Exchange/item_name\r\n" +
                       $"{help}";
            }

            return a;
        }

        public async void Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;
            Message m = await e.Channel.SendMessage("`Adding method... [Adding new items to DB]`");

            using (DevelopmentEntities db = new DevelopmentEntities()) 
            {
                // Adding new items to the DB
                if (!await saveNewItems(a.Inputs, a.Outputs, db))
                {
                    await m.Edit("`Could not add method because of an error when adding items. (Are your item ids correct?)`");
                    return;
                }

                Method method = new Method()
                {
                    Exp = (decimal) a.Exp,
                    Units = a.Units,
                    Level = a.Level,
                    Skill = a.Skill.ToString(),
                    Name = a.Name,
                    User = e.User.Name
                };

                await m.Edit("`Adding method... [Adding method to DB]`");

            
                // Adding the method to the DB
                db.Methods.Add(method);
                db.SaveChanges();

                await m.Edit($"`Adding method... [Adding inputs to DB 0/{a.Inputs.Count()}]`");

                // Adding inputs
                for (int i = 0; i < a.Inputs.Count(); i++)
                {
                    await m.Edit($"`Adding method... [Adding inputs to DB {i}/{a.Inputs.Count()}]`");

                    Input input = new Input()
                    {
                        MethodId = method.MethodId,
                        ItemId = a.Inputs.ElementAt(i).Id,
                        Qty = a.Inputs.ElementAt(i).Qty
                    };
                    db.Inputs.Add(input);
                }

                await m.Edit($"`Adding method... [Saving inputs to DB]`");
                db.SaveChanges();
                await m.Edit($"`Adding method... [Adding outputs to DB 0/{a.Outputs.Count()}]`");

                // Adding outputs
                for (int i = 0; i < a.Outputs.Count(); i++)
                {
                    await m.Edit($"`Adding method... [Adding outputs to DB {i}/{a.Inputs.Count()}]`");

                    Output output = new Output()
                    {
                        MethodId = method.MethodId,
                        ItemId = a.Outputs.ElementAt(i).Id,
                        Qty = a.Outputs.ElementAt(i).Qty
                    };

                    db.Outputs.Add(output);
                }

                await m.Edit($"`Adding method... [Saving outputs to DB]`");
                db.SaveChanges();
                await m.Edit($"`Adding method... Done.`");
            }
        }

        private async Task<bool> saveNewItems(IEnumerable<Arguments.Item> inputs, IEnumerable<Arguments.Item> outputs, DevelopmentEntities db)
        {
            IEnumerable<int> untrackedItems = inputs.Select(i => i.Id).Union(outputs.Select(i => i.Id));
            
            // Getting items that are not yet in the DB
            untrackedItems = untrackedItems.Except(db.Items.Select(i => i.ItemId));

            // Getting untrack item info
            foreach (var itemId in untrackedItems)
            {
                Item i = await Runescape.GetItemInfo(itemId);

                // Checking if item was successfully retrieved
                if (i == null)
                    return false;

                db.Items.Add(i);
            }

            // Checking if any changes need to be saved
            if (untrackedItems.Any())
                db.SaveChanges();

            return true;
        }

        private struct Arguments
        {
            public IEnumerable<Item> Inputs { get; set; }
            public IEnumerable<Item> Outputs { get; set; }
            public Skill Skill { get; set; }
            public string Name { get; set; }
            public double Exp { get; set; }
            public int Units { get; set; }
            public int Level { get; set; }

            public struct Item
            {
                public int Id { get; set; }
                public int Qty { get; set; }
            }
        }
    }
}
