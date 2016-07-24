using System.Linq;
using Discord;
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
            return null;
        }
        
        #pragma warning disable CS4014
        public async void Handle(object args, MessageEventArgs e)
        {
            Message m = await e.Channel.SendMessage("`Updating item prices...`");
            DevelopmentEntities db = new DevelopmentEntities();
            int count = db.Items.Count();

            // Rapidly updating items
            db.Items.ForEach(async item =>
            {
                Item i = await Runescape.GetItemInfo(item.ItemId);

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
                    db.SaveChanges();
                }
            });
        }
    }
}
