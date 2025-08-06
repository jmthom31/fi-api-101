using System;
using System.Security.Cryptography;
using System.Text;

namespace WebhooksReceiver;

public static class DigitalSignature
{
    public static string Generate(string text, string privateKey)
    {
        using var signingAlgorithm = RSA.Create();

        signingAlgorithm.ImportFromPem(privateKey);

        var signatureBytes = signingAlgorithm.SignData(Encoding.UTF8.GetBytes(text), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signatureBytes);
    }

    public static bool Verify(string digitalSignature, string text, string publicKey)
    {
        var signatureBytes = Convert.FromBase64String(digitalSignature);

        using var signingAlgorithm = RSA.Create();

        signingAlgorithm.ImportFromPem(publicKey);

        return signingAlgorithm.VerifyData(Encoding.UTF8.GetBytes(text), signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
