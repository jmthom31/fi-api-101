namespace WebhooksReceiver.Models;

public sealed record WebhookRequest(string Type, int Version, object Payload, int Nonce);
