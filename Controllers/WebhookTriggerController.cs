//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using WebhooksReceiver.Models;

//namespace WebhooksReceiver.Controllers;

//[ApiController]
//[Route("[controller]")]
//public class WebhookTriggerController : ControllerBase
//{
//    private readonly AuthProfile _authProfile;

//    public WebhookTriggerController(AuthProfile authProfile)
//    {
//        _authProfile = authProfile;
//    }

//    // This endpoint simulates sending a webhook to the listener. In production, this would be triggered by an event in your system.
//    [HttpPost]
//    [Route("trigger")]
//    public async Task<ActionResult<string>> Trigger([FromBody] ApiRequest request)
//    {
//        var webhookListenerUrl = "http://localhost:5000/sample/webhook";

//        // Create simplified webhook request
//        var webhookRequest = new WebhookRequest
//        {
//            Type = "test.webhook",
//            Payload = new { FieldName = "test" },
//        };

//        // Serialize the webhook payload to JSON
//        var webhookPayload = JsonSerializer.Serialize(webhookRequest);

//        // Create HTTP request to send webhook
//        var requestMessage = new HttpRequestMessage(HttpMethod.Post, webhookListenerUrl);
//        requestMessage.Content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");
//        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(webhookPayload, _authProfile.ClientPrivateKey));

//        try
//        {
//            using var client = new HttpClient();
//            await client.SendAsync(requestMessage);

//            return Ok("Webhook sent successfully");
//        }
//        catch (Exception)
//        {
//            return BadRequest("Failed to send webhook");
//        }
//    }
//}
