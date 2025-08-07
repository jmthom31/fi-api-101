//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
//using System;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using WebhooksReceiver.Models;

//namespace WebhooksReceiver.Endpoints;

//public static class WebhookTriggerEndpoint
//{
//    public static void MapWebhookTriggerEndpoints(this IEndpointRouteBuilder endpoints)
//    {
//        endpoints.MapPost("/webhooktrigger/trigger", TriggerWebhook);           
//    }

//    private static async Task<IResult> TriggerWebhook(
//        [FromBody] ApiRequest request, 
//        AuthProfile authProfile)
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
//        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(webhookPayload, authProfile.ClientPrivateKey));

//        try
//        {
//            using var client = new HttpClient();
//            await client.SendAsync(requestMessage);

//            return Results.Ok("Webhook sent successfully");
//        }
//        catch (Exception)
//        {
//            return Results.BadRequest("Failed to send webhook");
//        }
//    }
//}
