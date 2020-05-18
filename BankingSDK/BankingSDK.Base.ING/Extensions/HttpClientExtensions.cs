using BankingSDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BankingSDK.Base.ING.Extensions
{
    internal static class HttpClientExtensions
    {
        internal static void SignRequest(this SdkHttpClient client, X509Certificate2 cert, HttpMethod method, string path, string headerName, string keyId, bool addSig = false)
        {
            var signingString = $"(request-target): {method.Method.ToLower()} {path}\ndate: {client.DefaultRequestHeaders.GetValues("Date").First()}\ndigest: {client.DefaultRequestHeaders.GetValues("Digest").First()}\nx-request-id: {client.DefaultRequestHeaders.GetValues("X-Request-ID").First()}";
            var headerList = "(request-target) date digest x-request-id";
            var signature = Convert.ToBase64String(cert.GetRSAPrivateKey().SignData(Encoding.UTF8.GetBytes(signingString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            var signatureHeader = (addSig ? "Signature " : "") + $"keyId=\"{keyId}\",algorithm=\"rsa-sha256\",headers=\"{headerList}\",signature=\"{signature}\"";
            client.DefaultRequestHeaders.Add(headerName, signatureHeader);
        }
    }
}
