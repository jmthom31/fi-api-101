using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebhooksReceiver.Models;

namespace WebhooksReceiver.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    private readonly AuthProfile _authProfile;

    public SampleController()
    {
        _authProfile = new AuthProfile
        {
            ApiUrl = @"https://institution-api-sim.clearbank.co.uk",
            ApiToken = @"",
            ClientPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----

-----END RSA PRIVATE KEY-----",
            ClearBankPublicKey = @"-----BEGIN PUBLIC KEY-----

-----END PUBLIC KEY-----"
        };
    }

    [HttpPost]
    [Route("api")]
    public async Task<ActionResult<string>> Call([FromBody] ApiRequest request)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/test");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authProfile.ApiToken);

        // It is important to sign exactly same payload that is going to be send in the http request
        // Conversion from object to JSON in different places may result in different strings and request will fail digital signature validation
        var requestAsString = JsonSerializer.Serialize(request);
        requestMessage.Content = new StringContent(requestAsString, Encoding.UTF8, "application/json");
        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(requestAsString, _authProfile.ClientPrivateKey));

        // X-Request-Id - unique string that identifies the request. Do not reuse the same in 24 hour period.
        // If your request result in server error, use the same X-Request-Id for retries
        requestMessage.Headers.Add("X-Request-Id", Guid.NewGuid().ToString("N"));

        using var client = new HttpClient { BaseAddress = new Uri(_authProfile.ApiUrl) };
        var response = await client.SendAsync(requestMessage);

        // You should save X-Correlation-Id for future reference
        // If you have any questions about your request our support will ask you to provide X-Request-Id and X-Correlation-Id
        response.Headers.TryGetValues("X-Correlation-Id", out var headers);
        var correlationId = headers?.First();
        var body = await response.Content.ReadAsStringAsync();

        return $"Body: {body}, correlation id: {correlationId}";
    }

    [HttpPost]
    [Route("webhook")]
    public IActionResult Post([FromBody] WebhookRequest webhookRequest, [FromHeader(Name = "DigitalSignature")] string digitalSignature)
    {
        Console.WriteLine($"Received webhook: {webhookRequest.Type}, payload: {webhookRequest.Payload}");

        // It is important to get body from request object to calculate hash
        // Conversion back from WebhookRequest object may result in different string that will fail validation
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body);
        var body = reader.ReadToEnd();

        var verified = DigitalSignature.Verify(digitalSignature, body, _authProfile.ClearBankPublicKey);

        if (!verified)
        {
            return BadRequest("Incorrect signature");
        }

        // In production system you should put that webhook into the internal queue for processing 
        // Don't do heavy processing here and respond to webhook as quick as possible

        var result = new WebhookResponse { Nonce = webhookRequest.Nonce };

        var response = JsonSerializer.Serialize(result);
        var signature = DigitalSignature.Generate(response, _authProfile.ClientPrivateKey);

        Request.HttpContext.Response.Headers.Add("DigitalSignature", signature);

        return Content(response);
    }
}
