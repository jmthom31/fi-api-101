//using System;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using WebhooksReceiver.Models;

//namespace WebhooksReceiver.Controllers;

//[ApiController]
//[Route("[controller]")]
//public class SampleController : ControllerBase
//{
//    private readonly AuthProfile _authProfile;

//    public SampleController(AuthProfile authProfile)
//    {
//        _authProfile = authProfile;
//    }

//    [HttpPost]
//    [Route("api")]
//    public async Task<ActionResult<string>> Call([FromBody] ApiRequest request)
//    {
//        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/test");
//        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authProfile.ApiToken);

//        // It is important to sign exactly the same payload that is going to be sent in the http request.
//        // Conversion from am object to JSON in different places may result in different strings and the request will fail digital signature validation.
//        var requestAsString = JsonSerializer.Serialize(request);
//        requestMessage.Content = new StringContent(requestAsString, Encoding.UTF8, "application/json");
//        requestMessage.Headers.Add("DigitalSignature", DigitalSignature.Generate(requestAsString, _authProfile.ClientPrivateKey));

//        // X-Request-Id - a unique string that identifies the request. Do not reuse in a 24 hour period.
//        // If your request results in a server error, use the same X-Request-Id for retries.
//        requestMessage.Headers.Add("X-Request-Id", Guid.NewGuid().ToString("N"));

//        using var client = new HttpClient { BaseAddress = new Uri(_authProfile.ApiUrl) };
//        var response = await client.SendAsync(requestMessage);

//        // X-Correlation-Id should be saved for future reference.
//        // If you have any questions about your request, our support team will ask you to provide X-Request-Id and X-Correlation-Id.
//        response.Headers.TryGetValues("X-Correlation-Id", out var headers);
//        var correlationId = headers?.First();
//        var body = await response.Content.ReadAsStringAsync();

//        return $"Body: {body}, correlation id: {correlationId}";
//    }

//    [HttpPost]
//    [Route("webhook")]
//    public IActionResult Post([FromBody] WebhookRequest webhookRequest, [FromHeader(Name = "DigitalSignature")] string digitalSignature)
//    {
//        Console.WriteLine($"Received webhook: {webhookRequest.Type}, payload: {webhookRequest.Payload}");

//        // It is important to get the body from the request object to calculate the hash.
//        // Conversion back from the WebhookRequest object may result in a different string which will fail validation.
//        Request.Body.Position = 0;
//        using var reader = new StreamReader(Request.Body);
//        var body = reader.ReadToEnd();

//        var verified = DigitalSignature.Verify(digitalSignature, body, _authProfile.ClearBankPublicKey);

//        if (!verified)
//        {
//            return BadRequest("Incorrect signature");
//        }

//        // In the production system, the webhook should be placed into the internal queue for processing.
//        // The webhook should be responded to as quickly as possible and any heavy processing should be avoided.

//        var result = new WebhookResponse { Nonce = webhookRequest.Nonce };

//        var response = JsonSerializer.Serialize(result);
//        var signature = DigitalSignature.Generate(response, _authProfile.ClientPrivateKey);

//        Request.HttpContext.Response.Headers.Append("DigitalSignature", signature);

//        return Content(response);
//    }
//}
