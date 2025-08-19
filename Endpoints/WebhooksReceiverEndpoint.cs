using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebhooksReceiver.Models;

namespace WebhooksReceiver.Endpoints;

public static class WebhooksReceiverEndpoint
{
    public static void MapWebhooksReceiverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sample/webhook", ReceiveWebhook);
    }

    private static async Task<IResult> ReceiveWebhook(
        HttpContext context,
        [FromBody] WebhookRequest webhookRequest,
        [FromHeader(Name = "DigitalSignature")] string digitalSignature,
        AuthProfile authProfile)
    {
        System.Console.WriteLine($"Received webhook: {webhookRequest.Type}, payload: {webhookRequest.Payload}");

        // It is important to get the body from the request object to calculate the hash.
        // Conversion back from the WebhookRequest object may result in a different string which will fail validation.
        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        var verified = DigitalSignature.Verify(digitalSignature, body, authProfile.PublicKey);

        if (!verified)
        {
            return Results.BadRequest("Incorrect signature");
        }

        // In the production system, the webhook should be placed into the internal queue for processing.
        // The webhook should be responded to as quickly as possible and any heavy processing should be avoided.

        var result = new WebhookResponse(Nonce: webhookRequest.Nonce);

        var response = JsonSerializer.Serialize(result);
        var signature = DigitalSignature.Generate(response, authProfile.PrivateKey);

        context.Response.Headers.Append("DigitalSignature", signature);

        return Results.Content(response);
    }
}
