using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebhooksReceiver.Models;

namespace WebhooksReceiver.Endpoints;

public static class ApiTestEndpoint
{
    public static void MapApiTestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sample/api", CallApi);
    }

    private static async Task<IResult> CallApi(
        [FromBody] ApiRequest request,
        AuthProfile authProfile)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/test");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authProfile.ApiToken);

        // It is important to sign exactly the same payload that is going to be sent in the http request.
        // Conversion from an object to JSON in different places may result in different strings and the request will fail digital signature validation.
        var requestAsString = JsonSerializer.Serialize(request);
        requestMessage.Content = new StringContent(requestAsString, Encoding.UTF8, "application/json");
        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(requestAsString, authProfile.ClientPrivateKey));

        // X-Request-Id - a unique string that identifies the request. Do not reuse in a 24 hour period.
        // If your request results in a server error, use the same X-Request-Id for retries.
        requestMessage.Headers.Add("X-Request-Id", Guid.NewGuid().ToString("N"));

        using var client = new HttpClient { BaseAddress = new Uri(authProfile.ApiUrl) };
        var response = await client.SendAsync(requestMessage);

        // X-Correlation-Id should be saved for future reference.
        // If you have any questions about your request, our support team will ask you to provide X-Request-Id and X-Correlation-Id.
        response.Headers.TryGetValues("X-Correlation-Id", out var headers);
        var correlationId = headers?.First();
        var body = await response.Content.ReadAsStringAsync();

        return Results.Ok($"Body: {body}, correlation id: {correlationId}");
    }
}
