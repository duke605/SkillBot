using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBot.Commands {
    interface ICommand
    {

        /// <returns>Name of the command</returns>
        string GetCommandName();

        /// <summary>
        /// Parses the passed args
        /// </summary>
        /// <param name="args">The Input</param>
        /// <returns>A string with help/errors or the arguments for the next step</returns>
        object ParseCommand(string[] args);

        /// <summary>
        /// Handles the actual logic of the command
        /// </summary>
        /// <param name="args">The parsed args from the user</param>
        void Handle(object args, MessageEventArgs e);
    }

    public enum Skill 
    {
        Crafting,
        Smithing,
        Fletching,
        Mining,
        Herblore,
        Fishing,
        Cooking,
        Prayer,
        Firemaking,
        Woodcutting,
        Runecrafting,
        Farming,
        Construction,
        Hunter,
        Summoning,
        Divination
    }
}
