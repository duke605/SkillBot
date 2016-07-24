using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkillBot.Extensions;

namespace SkillBot.Utilities {
    class Http {

        public static async Task<HttpResponseMessage> Get(string url, string accept = "application/json")
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Uri uri = new Uri(url);

                    client.DefaultRequestHeaders.Add("Accept", accept);

                    return await client.GetAsync(uri);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException.Message);
                    return null;
                }
            }
        }

        public static async Task<HttpResponseMessage> Post(string url, object data)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(url);
                var content = new ByteArrayContent(JsonConvert.SerializeObject(data).ToBytes());

                // Headers
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return await client.PostAsync(uri, content);
            }
        }
    }
}
