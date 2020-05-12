using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplicationFacebookMessengerBotDemo.Helpers
{
    public class FacebookWebhookHandlersHelpers
    {
        public static async Task<JObject> GetRequestJson(HttpRequest request)
        {
            dynamic json;
            using (var sr = new StreamReader(request.Body))
            {
                json = await sr.ReadToEndAsync();
            }
            JObject data = JsonConvert.DeserializeObject(json);

            return data;
        }
    }
}
