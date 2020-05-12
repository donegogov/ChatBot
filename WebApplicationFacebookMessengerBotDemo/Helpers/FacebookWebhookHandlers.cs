using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebApplicationFacebookMessengerBotDemo.Helpers
{
    public class FacebookWebhookHandlers
    {
        private static IOptions<PageAccessTokenVerifyToken> _pageAccessTokenVerifyTokenConfig;

        public FacebookWebhookHandlers(IOptions<PageAccessTokenVerifyToken> pageAccessTokenVerifyTokenConfig)
        {
            _pageAccessTokenVerifyTokenConfig = pageAccessTokenVerifyTokenConfig;
        }

        // Handles messages events
        public static async Task<bool> HandleMessage(string senderPsId, string receivedMessage)
        {

            var responseText = "";

            // call Regex.Match.
            var match = Regex.Match(receivedMessage, @"\w+",
                RegexOptions.IgnoreCase);

            //var returnResponse = "";

            //  check the Match for Success.
            if (match.Success)
                // Create the payload for a basic text message
                responseText = "Thank you for reaching out to us, an agent will be with you shortly";

            // Sends the response message
            return await CallSendApi(senderPsId, responseText);
        }

        // Handles messaging_postbacks events
        public static async Task<bool> HandlePostback(string senderPsId, string receivedPostback)
        {
            if (receivedPostback == null) throw new ArgumentNullException(nameof(receivedPostback));
            if (receivedPostback.Equals("Get Started"))
                //await callSendAPI(sender_psid, $"Welcome to Homemade Food Fan Page where food is made with passion.");
                await CallSendApiSendJson(GetStartedMessage(senderPsId));
            else if (receivedPostback.Equals("CARE_HELP") || receivedPostback.Equals("CARE_HELP_CLICKED_QUICK_REPLIES_BUTTON"))
                await CallSendApi(senderPsId, "Thank you for reaching out to us, an agent will be with you shortly");
            /*else if (receivedPostback.Equals("TOMORROW_MENU") ||
                     receivedPostback.Equals("TOMORROW_MENU_CLICKED_QUICK_REPLIES_BUTTON"))
                await CallSendApi(senderPsId, "The menu for tomorrow is...");*/
            else if (receivedPostback.Equals("TODAYS_MENU") ||
                     receivedPostback.Equals("TODAYS_MENU_CLICKED_QUICK_REPLIES_BUTTON"))
                await CallSendApi(senderPsId, "The menu for today is...");

            return true;
        }

        // Sends response messages via the Send API
        public static async Task<bool> CallSendApi(string senderPsId, string response)
        {
            // Query string parameters
            var queryString = new Dictionary<string, string>()
            {
                { "access_token", _pageAccessTokenVerifyTokenConfig.Value.PageAccessToken }
            };

            // Create json for body
            var jsonStringResponse = "{\"recipient\":{\"id\":\" " + senderPsId + "\"},\"message\":{\"text\":\" " + response + "\" } }";

            var content = JsonConvert.DeserializeObject(jsonStringResponse);
            var requestContent = JsonConvert.SerializeObject(content);

            // Create HttpClient
            var client = new HttpClient
            {
                BaseAddress = new Uri(_pageAccessTokenVerifyTokenConfig.Value.FacebookGraphUrl)
            };

            // Query string for the request
            var requestQueryString = QueryHelpers.AddQueryString("", queryString);

            var request = new HttpRequestMessage(HttpMethod.Post, requestQueryString);
            // Setup header(s)
            request.Headers.Add("Accept", "application/json");
            // Add body content
            request.Content = new StringContent(
                requestContent,
                Encoding.UTF8,
                "application/json"
            );

            // Send the request
            await client.SendAsync(request);

            return true;
        }

        public static async Task<bool> CallSendApiSendJson(string jsonReadyResponse)
        {
            // Query string parameters
            var queryString = new Dictionary<string, string>()
            {
                { "access_token", _pageAccessTokenVerifyTokenConfig.Value.PageAccessToken }
            };

            // Create json for body
            var jsonStringResponse = jsonReadyResponse;

            var content = JsonConvert.DeserializeObject(jsonStringResponse);
            var requestContent = JsonConvert.SerializeObject(content);

            // Create HttpClient
            var client = new HttpClient
            {
                BaseAddress = new Uri(_pageAccessTokenVerifyTokenConfig.Value.FacebookGraphUrl)
            };

            // Query string for the request
            var requestQueryString = QueryHelpers.AddQueryString("", queryString);

            var request = new HttpRequestMessage(HttpMethod.Post, requestQueryString);
            // Setup header(s)
            request.Headers.Add("Accept", "application/json");
            // Add body content
            request.Content = new StringContent(
                requestContent,
                Encoding.UTF8,
                "application/json"
            );

            // Send the request
            await client.SendAsync(request);

            return true;
        }

        // Handles messaging_quick_replay events
        public async Task<bool> HandleQuickReplay(string senderPsId, string quickReplayPostback)
        {
            if (quickReplayPostback.Equals("CARE_HELP") || quickReplayPostback.Equals("CARE_HELP_CLICKED_QUICK_REPLIES_BUTTON"))
                //return true;
                await CallSendApi(senderPsId, "Thank you for reaching out, an agent will be with you");
            /*else if (quick_replay_postback.Equals("TOMORROW_MENU") || quick_replay_postback.Equals("TOMORROW_MENU_CLICKED_QUICK_REPLIES_BUTTON"))
            {
                //return true;
                await callSendAPI(sender_psid, $"The menu for tomorrow is...");
            }*/
            else if (quickReplayPostback.Equals("TODAYS_MENU") || quickReplayPostback.Equals("TODAYS_MENU_CLICKED_QUICK_REPLIES_BUTTON"))
                //return true;
                await CallSendApi(senderPsId, "Today's menu is" +
                                              "\n1) Veggie Sushi Bowls." +
                                              "\n2) Kale, Black Bean and Avocado Burrito Bowl. ..." +
                                              "\n3) Zucchini Noodles with Basil-Pumpkin Seed Pesto. ..." +
                                              "\n4) Lemony Broccoli, Chickpea &Avocado Pita Sandwiches. ..." +
                                              "\n5) Arugula, Dried Cherry and Wild Rice Salad with a Zippy Lemon Dressing. ..." +
                                              "\n6) Caprese Pasta Salad. ..." +
                                              "\n7) Vegetable Paella.");
            else if (quickReplayPostback.Equals("DELIVERY_INFORMATIONS") || quickReplayPostback.Equals("DELIVERY_INFORMATIONS_CLICKED_QUICK_REPLIES_BUTTON"))
                //return true;
                await CallSendApi(senderPsId, "Delievery cost $");

            return true;
        }

        public static string GetStartedMessage(string senderPsId)
        {
            var jsonResponse = "{ " +
                               "\"get_started\": { " +
                               "\"payload\": \"Get Started\"" +
                               "}," +
                               "\"recipient\": {" +
                               "\"id\":\" " + senderPsId + "\"" +
                               "}," +
                               "\"messaging_type\": \"RESPONSE\"," +
                               "\"message\": {" +
                               "\"text\": \"Welcome to the Homemade Food Fan Page where food is made with passion.\nIf you like to find out more about today's and tomorrow's menu please choose an item from the menu.\"," +
                               "\"quick_replies\":[" +
                               "{" +
                               "\"content_type\": \"text\"," +
                               "\"title\": \"Customer service\"," +
                               "\"payload\": \"CARE_HELP_CLICKED_QUICK_REPLIES_BUTTON\"" +
                               "}," +
                               /*
                                                "{" +
                                                "\"content_type\": \"text\"," +
                                                "\"title\": \"Tomorrow's menu\"," +
                                                "\"payload\": \"TOMORROW_MENU_CLICKED_QUICK_RePLIES_BUTTON\"" +
                                                "}," +
                                                */
                               "{" +
                               "\"content_type\": \"text\"," +
                               "\"title\": \"Today's menu\"," +
                               "\"payload\": \"TODAYS_MENU_CLICKED_QUICK_REPLIES_BUTTON\"" +
                               "}," +
                               "{" +
                               "\"content_type\": \"text\"," +
                               "\"title\": \"Delivery Informations\"," +
                               "\"payload\": \"DELIVERY_INFORMATIONS_CLICKED_QUICK_REPLIES_BUTTON\"" +
                               "}" +
                               "]" +
                               "}" +
                               "}";

            return jsonResponse;
        }
    }
}
