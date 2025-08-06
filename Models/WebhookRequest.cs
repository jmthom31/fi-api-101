namespace WebhooksReceiver.Models;

public sealed class WebhookRequest
{
    public string Type { get; set; }

    public int Version { get; set; }

    public object Payload { get; set; }

    public int Nonce { get; set; }
}
