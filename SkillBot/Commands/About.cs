using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBot.Commands {
    class About : ICommand {

        public string GetCommandName()
        {
            return "about";
        }

        public object ParseCommand(string[] args)
        {
            return null;
        }

        public void Handle(object args, MessageEventArgs e)
        {
            e.Channel.SendMessage("__Author:__ Duke605\r\n" +
                                  "__Library:__ Discord.Net\r\n" +
                                  "__Version:__ 1.1.0\r\n" +
                                  "__GitHub Page:__ <https://github.com/duke605/SkillBot>\r\n" +
                                  "");
        }
    }
}
