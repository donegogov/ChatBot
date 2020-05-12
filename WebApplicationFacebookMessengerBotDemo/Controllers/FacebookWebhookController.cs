using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WebApplicationFacebookMessengerBotDemo.Helpers;

namespace WebApplicationFacebookMessengerBotDemo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FacebookWebhookController : ControllerBase
    {
        private readonly IOptions<PageAccessTokenVerifyToken> _pageAccessTokenVerifyTokenConfig;
        private readonly ILogger _logger;

        public FacebookWebhookController(IOptions<PageAccessTokenVerifyToken> pageAccessTokenVerifyTokenConfig, ILogger<FacebookWebhookController> logger)
        {
            // Facebook Page Access Token is private to you
            // in this project I store it in appsettings.json
            _pageAccessTokenVerifyTokenConfig = pageAccessTokenVerifyTokenConfig;
            _logger = logger;
            _logger.LogInformation("Webhook kontroler konstruktor");
        }

        // GET: Webhook
        [HttpGet]
        public IActionResult Get(Hub hub)
        {
            if (hub == null) return Forbid();
            _logger.LogInformation($"Get method mode={hub.Mode} token={hub.VerifyToken} challenge={hub.Challenge} ");

            // Checks if a token and mode is in the query string of the request
            if (hub.Mode != null && hub.VerifyToken != null)
            {
                // Checks the mode and token sent is correct
                if (hub.Mode.Equals("subscribe") &&
                    hub.VerifyToken.Equals(_pageAccessTokenVerifyTokenConfig.Value.VerifyToken))
                {
                    // Responds with the challenge token from the request
                    Console.WriteLine("WEBHOOK_VERIFIED");
                    return Ok(hub.Challenge);
                }
                else
                {
                    // Responds with '403 Forbidden' if verify tokens do not match
                    return Forbid();
                }
            }
            return Forbid();
        }

        // POST: Webhook
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                var data = await FacebookWebhookHandlersHelpers.GetRequestJson(Request);
                if (data == null) throw new ArgumentNullException(nameof(data));

                var objectPropertyValue = data["object"].ToString();

                // Checks this is an event from a page subscription
                if (objectPropertyValue != null && objectPropertyValue.Equals("page"))
                {
                    // Iterates over each entry - there may be multiple if batched
                    foreach (var entry in data["entry"])
                    foreach (var messaging in entry["messaging"])
                    {
                        var postbackProperty = messaging.SelectToken(
                            "$.postback");

                        var quickReplayProperty =
                            messaging.SelectToken(
                                "$.quick_reply"); //Value<JToken?>("postback").Value<string?>("payload");

                        var messageProperty =
                            messaging.SelectToken("$.message"); //.Value<JToken>("message").Value<string>("text");

                        // Check if the event is a message or postback or quick replay and
                        // pass the event to the appropriate handler function
                        if (postbackProperty != null)
                            await FacebookWebhookHandlers.HandlePostback(messaging.Value<JToken>("sender").Value<string>("id"),
                                messaging.Value<JToken>("postback").Value<string>("payload"));
                        else if (messageProperty != null && quickReplayProperty == null)
                            await FacebookWebhookHandlers.HandleMessage(messaging.Value<JToken>("sender").Value<string>("id"),
                                messaging.Value<JToken>("message").Value<string>("text"));
                        else if (quickReplayProperty != null && messageProperty != null)
                            await FacebookWebhookHandlers.HandleMessage(messaging.Value<JToken>("sender").Value<string>("id"),
                                messaging.Value<JToken>("quick_reply").Value<string>("payload"));
                    }

                    // Returns a '200 OK' response to all requests
                    return Ok("EVENT_RECEIVED");
                }
                else
                {
                    // Returns a '404 Not Found' if event is not from a page subscription
                    return NotFound();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message + " " + e);
            }
            
            return NotFound();
        }
    }
}
