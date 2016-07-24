using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using Newtonsoft.Json;

namespace SkillBot.Utilities {
    class Http {

        public static async Task<HttpResponseMessage> Get(string url, string accept = "application/json")
        {
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(url);

                client.DefaultRequestHeaders.Add("Accept", accept);
 
                return await client.GetAsync(uri);
            }
        }

        public static async Task<HttpResponseMessage> Post(string url, object data)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(url);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                data.GetType().GetProperties().ForEach(p => dict[p.Name] = p.GetValue(data).ToString());
                var content = new FormUrlEncodedContent(dict);
                return await client.PostAsync(uri, content);
            }
        }
    }
}
