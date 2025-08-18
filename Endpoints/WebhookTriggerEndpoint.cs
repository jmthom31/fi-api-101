using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebhooksReceiver.Models;

namespace WebhooksReceiver.Endpoints;

// This endpoint simulates sending a webhook to the listener. In production, this would be triggered by an event in your system.
public static class WebhookTriggerEndpoint
{
    public static void MapWebhookTriggerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/webhooktrigger/trigger", TriggerWebhook);           
    }

    private static async void TriggerWebhook(
        [FromBody] ApiRequest request, 
        AuthProfile authProfile)
    {
        var webhookListenerUrl = "http://localhost:5000/sample/webhook";

        // Create simplified webhook request
        var webhookRequest = new WebhookRequest
        {
            Type = "test.webhook",
            Payload = new { request.FieldName}
        };

        // Serialize the webhook payload to JSON
        var webhookPayload = JsonSerializer.Serialize(webhookRequest);

        // Create HTTP request to send webhook
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, webhookListenerUrl);
        requestMessage.Content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");
        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(webhookPayload, authProfile.PrivateKey));       

        using var client = new HttpClient();
        await client.SendAsync(requestMessage);
    }
}
