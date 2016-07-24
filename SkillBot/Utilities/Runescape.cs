using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SkillBot.Commands;
using SkillBot.Extensions;

namespace SkillBot.Utilities {
    class Runescape {

        /// <summary>
        /// Coverts a csv string to objects
        /// </summary>
        /// <param name="str">The csv string representing the stats</param>
        /// <returns>Stat objects</returns>
        public static Stats StringToStats(string str)
        {
            string[] s = str.Split("\n".ToCharArray());
            Stats stats = new Stats();
            PropertyInfo[] props = stats.GetType().GetProperties();

            // Populating Stats
            for (int i = 0; i < stats.GetType().GetProperties().Length; i++)
            {
                Stats.Stat stat = new Stats.Stat();
                string[] statInfo = s[i].Split(',');

                // Populating stat
                stat.Rank = statInfo[0].ToInt();
                stat.Level = statInfo[1].ToInt();
                stat.Xp = statInfo[2].ToInt();

                // Setting property
                props[i].SetValue(stats, stat);
            }

            return stats;
        }

        /// <summary>
        /// Calculates the remaining exp between current exp and desired level
        /// </summary>
        /// <param name="stats">The user's stats</param>
        /// <param name="level">The desired level</param>
        /// <param name="skill">The desired skill in which they wish to level</param>
        /// <returns>The exp between current exp and the desired level</returns>
        public static int ExpBetweenLevels(Stats stats, double level, Skill skill)
        {
            Stats.Stat stat = (Stats.Stat) typeof(Stats).GetProperty(skill.ToString()).GetValue(stats);
            int exp = 0;

            for (double i = 1; i < level; i++)
            {
                exp += (int) Math.Floor(i + 300 * Math.Pow(2, i / 7));
            }
           
            return (int) (Math.Floor(exp / 4.0) - stat.Xp);
        }

        /// <summary>
        /// Gets a user's stat for the given enum
        /// </summary>
        /// <param name="skill">The skill in which to get</param>
        /// <param name="stats">The user's stats</param>
        /// <returns>The stat for the given skill</returns>
        public static Stats.Stat GetStatForString(Skill skill, Stats stats)
        {
            return (Stats.Stat) typeof(Stats).GetProperty(skill.ToString()).GetValue(stats);
        }

        public static async Task<Item> GetItemInfo(int itemId)
        {
            Item item = new Item()
            {
                ItemId = itemId
            };

            for (int i = 0; i < 3; i++)
            {
                HttpResponseMessage resp = await Http.Get($"http://services.runescape.com/m=itemdb_rs/api/catalogue/detail.json?item={itemId}");

                // Checking if response if valid
                if (!resp.IsSuccessStatusCode)
                {

                    // Checking if the item wasn't found
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        resp.Dispose();
                        break;
                    }
                        
                    // Trying again
                    resp.Dispose();
                    continue;
                }

                // Got the proper data
                string data = await resp.Content.ReadAsStringAsync();
                JObject @object = JObject.Parse(data);
                item.Price = @object["item"]["current"]["price"].ToString().ToInt();
                item.Name = @object["item"]["name"].ToString();

                // Checking if the price was cast
                if (item.Price != -1)
                    return item;

                // Making scond call
                for (int j = 0; j < 3; j++)
                {
                    // Have to make another call because of the stupid formatting
                    resp = await Http.Get($"http://services.runescape.com/m=itemdb_rs/api/graph/{itemId}.json");

                    // Checking if response was valid
                    if (!resp.IsSuccessStatusCode)
                    {
                        
                        // Checking if item was not found (impossibru)
                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            resp.Dispose();
                            break;
                        }

                        resp.Dispose();
                        continue;
                    }

                    data = await resp.Content.ReadAsStringAsync();
                    @object = JObject.Parse(data);
                    item.Price = @object["daily"].Last.Last.ToString().ToInt();

                    // Checking it price was parsed correctly
                    if (item.Price != -1)
                        return item;

                    // In theory it should never get here
                }

                // All hope is lost
                break;
            }

            return null;
        }
    }

    class Stats {
        public struct Stat {
            public int Rank { get; set; }
            public int Level { get; set; }
            public int Xp { get; set; }
        }

        public Stat Overall { get; set; }
        public Stat Attack { get; set; }
        public Stat Defence { get; set; }
        public Stat Strength { get; set; }
        public Stat Constitution { get; set; }
        public Stat Ranged { get; set; }
        public Stat Prayer { get; set; }
        public Stat Magic { get; set; }
        public Stat Cooking { get; set; }
        public Stat Woodcutting { get; set; }
        public Stat Fletching { get; set; }
        public Stat Fishing { get; set; }
        public Stat Firemaking { get; set; }
        public Stat Crafting { get; set; }
        public Stat Smithing { get; set; }
        public Stat Mining { get; set; }
        public Stat Herblore { get; set; }
        public Stat Agility { get; set; }
        public Stat Thieving { get; set; }
        public Stat Slayer { get; set; }
        public Stat Farming { get; set; }
        public Stat Runecrafting { get; set; }
        public Stat Hunter { get; set; }
        public Stat Construction { get; set; }
        public Stat Summoning { get; set; }
        public Stat Dungeoneering { get; set; }
        public Stat Divination { get; set; }
        public Stat Invention { get; set; }

    }
}
