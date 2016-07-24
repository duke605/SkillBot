using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using SkillBot.Commands;
using SkillBot.Utilities;

namespace SkillBot {

    class Start {
        public static readonly DiscordClient Client = new DiscordClient();
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public static void Main(string[] args) {
            Setup();
            Connect();
        }

        /// <summary>
        /// Setting up listeners for events
        /// </summary>
        private static void Setup()
        {
            Console.Write("Setting up... ");

            // Looking for commands
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(T => typeof(ICommand).IsAssignableFrom(T) && typeof(ICommand) != T);

            // Registering commands
            foreach (Type t in types)
            {
                ICommand command = (ICommand) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                _commands[command.GetCommandName()] = command;
            }

            // Setting up command listener
            Client.MessageReceived += async (s, e) =>
            {
                // Ignoring self or messages that don't start with !
                if (e.Message.IsAuthor || !e.Message.Text.StartsWith("!") || e.Message.Text.Length <= 1)
                    return;

                string[] args = e.Message.Text.Substring(1).Split(' ');
                string command = args[0].ToLower();

                // Checking if command has arguments
                if (args.Length > 1)
                    args = e.Message.Text.Substring(command.Length + 2).Split(' ');

                // Ignoring if command not found
                if (!_commands.ContainsKey(command))
                    return;

                // Telling using that stuff is being processed
                await e.Channel.SendIsTyping();
               
                // Parsing user input
                object definition = _commands[command].ParseCommand(args);

                if (definition is string)
                {
                    await e.Channel.SendMessage($"```{definition.ToString()}```");
                    return;
                }

                #pragma warning disable CS4014
                Task.Run(() => _commands[command].Handle(definition, e));
            };

            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Connecting to discord
        /// </summary>
        private static void Connect()
        {
            Client.ExecuteAndWait(async () =>
            {
                Console.Write("Connecting to Discord... ");
                await Client.Connect(Secret.DiscordToken);
                Console.WriteLine("Done.");
            });
        }
    }
}
