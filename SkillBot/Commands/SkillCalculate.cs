using System;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Linq;
using System.Net.Http;
using Discord;
using Fclp;
using Newtonsoft.Json.Linq;
using SkillBot.Utilities;

namespace SkillBot.Commands {
    class SkillCalculate : ICommand
    {
        public string GetCommandName()
        {
            return "calc";
        }

        public object ParseCommand(string[] args)
        {
            FluentCommandLineParser parser = new FluentCommandLineParser();
            Arguments a = new Arguments();
            string help = "";

            parser.Setup<List<string>>('u', "username")
                .Callback(username => a.Username = String.Join(" ", username))
                .Required()
                .WithDescription("Your runescape username.");

            parser.Setup<Skill>('s', "skill")
                .Required()
                .Callback(skill => a.Skill = skill)
                .WithDescription("Skill to calculate for (Crafting, Fletching, Smithing...)");

            parser.Setup<Arguments.SortOrder>('S', "sort")
                .Callback(sort => a.Sort = sort)
                .SetDefault(Arguments.SortOrder.Level)
                .WithDescription("The sort order of the data.");

            parser.Setup<Arguments.SortDirection>('d', "direction")
                .Callback(dir => a.Direction = dir)
                .SetDefault(Arguments.SortDirection.Asc)
                .WithDescription("The direction of the sort (Asc, Desc)");

            parser.Setup<bool>('g', "github")
                .Callback(git => a.GitHub = git)
                .SetDefault(false)
                .WithDescription("Force table to be uploaded to github. (Helps if on a smaller screen");

            parser.Setup<int>('l', "level")
                .Callback(level => a.TargetLevel = level)
                .Required()
                .WithDescription("The target level you wish to achive.");

            parser.Setup<double>('b', "boost")
                .Callback(boost => a.Boost = (decimal) ((boost / 100) + 1))
                .SetDefault(0)
                .WithDescription("Boosted exp. (Eg. For portables put \"10\" for a 10% exp boost)");


            
            parser.SetupHelp("?", "help")
                .Callback(t => help = t);
           
            var result = parser.Parse(args);
            parser.HelpOption.ShowHelp(parser.Options);

            // Showing mistakes and proper command usage
            if (result.HasErrors)
            {
                return $"{result.ErrorText}\r\n" +
                       $"Command Usage:\r\n" +
                       $"{help}";
            }

            // Showing help
            if (result.HelpCalled)
            {
                return $"Command Usage:\r\n{help}";
            }

            return a;
        }

        public async void Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;
            HttpResponseMessage resp = await Http.Get($"http://services.runescape.com/m=hiscore/index_lite.ws?player={a.Username}");
            string result;

            // Checking if response is good
            if (resp.IsSuccessStatusCode)
                result = await resp.Content.ReadAsStringAsync();
            else
            {
                await e.Channel.SendMessage($"`Username \"{a.Username}\" could not be found`");
                resp.Dispose();
                return;
            }

            resp.Dispose();
            Stats stats = Runescape.StringToStats(result);
            Stats.Stat relStat = (Stats.Stat)typeof(Stats).GetProperty(a.Skill.ToString()).GetValue(stats);
            int remainingExp = Runescape.ExpBetweenLevels(stats, a.TargetLevel, a.Skill);

            using (DevelopmentEntities db = new DevelopmentEntities())
            {
                var methods = db.Methods
                    .GroupJoin(db.Inputs, m => m.MethodId, i => i.MethodId, (method, inputs) => new {method, inputs})
                    .GroupJoin(db.Outputs, m => m.method.MethodId, o => o.MethodId,
                        (m, outputs) => new {method = m.method, inputs = m.inputs, outputs})
                    .Where(m => m.method.Level <= relStat.Level)
                    .Where(m => m.method.Skill == a.Skill.ToString())
                    .Select(m => new
                    {
                        Number = (int) Math.Ceiling(remainingExp/(a.Boost*m.method.Exp)),
                        m.method.Name,
                        m.method.Level,
                        m.method.Exp,
                        ExpPerHour = m.method.Units*(m.method.Exp*a.Boost),
                        Cost =
                            (Int64) ((!m.outputs.Any() ? 0 : m.outputs.Sum(o => o.Qty*o.Item.Price)) -
                                     (!m.inputs.Any() ? 0 : m.inputs.Sum(i => i.Qty*i.Item.Price))),
                        Time = ((decimal) Math.Ceiling(remainingExp/(m.method.Exp*a.Boost)))/m.method.Units
                    })
                    .OrderBy($"{a.Sort.ToString()} {a.Direction.ToString()}")
                    .ToList()
                    .Select(m =>
                    {
                        decimal time = m.Time;
                        int hours = (int) time;
                        int minutes = (int) ((time%1)*60);
                        string t = $"{hours}h{minutes}m";

                        return new
                        {
                            m.Number,
                            m.Name,
                            m.Level,
                            m.Exp,
                            m.ExpPerHour,
                            Cost = m.Cost*(int) Math.Ceiling(remainingExp/(m.Exp*a.Boost)),
                            GpXp = m.Cost/m.Exp,
                            Time = t
                        };
                    });

                string table = Table.MakeTable(methods,
                    new[] {"Number", "Name", "Level", "Exp", "Exp per Hour", "Profit/Loss", "Gp/Xp", "Time"});

                // Checking if it can send to the channel
                if (table.Length <= 2000 && !a.GitHub)
                {
                    await e.Channel.SendMessage($"```{table}```");
                    return;
                }

                // Uploading to github
                var data = new
                {
                    description = "Skill table",
                    @public = true,
                    files = new
                    {
                        table = new
                        {
                            content = table
                        }
                    }
                };

                
                using (resp = await Http.Post("https://api.github.com/gists", data))
                {
                    // Checking if post was successful
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        JObject o = JObject.Parse(json);
                        await e.Channel.SendMessage($"<{o["files"]["table"]["raw_url"]}>");
                    }

                    // Error
                    else
                    {
                        await
                            e.Channel.SendMessage($"`The table was to big to send via Discord and GitHub returned error code: {resp.StatusCode}`");
                    }
                }
            }

        }

        private struct Arguments 
        {
            public string Username { get; set; }
            public int TargetLevel { get; set; }
            public int TargetExp { get; set; }
            public bool GitHub { get; set; }
            public decimal Boost { get; set; }
            public Skill Skill { get; set; }
            public SortOrder Sort { get; set; }
            public SortDirection Direction { get; set; }

            public enum SortOrder
            {
                Number,
                Level,
                Exp,
                Cost,
                ExpPerHour,
                Time
            }

            public enum SortDirection
            {
                Desc,
                Asc
            }
        }
    }
}
