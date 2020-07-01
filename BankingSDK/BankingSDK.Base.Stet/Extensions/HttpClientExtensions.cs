using BankingSDK.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BankingSDK.Base.Stet.Extensions
{
    internal static class HttpClientExtensions
    {
        internal static void SignRequest(this SdkHttpClient client, X509Certificate2 cert, HttpMethod method, string path, string keyId)
        {
            var created = $"{GetNumericTime(DateTime.UtcNow)}";
            var expires = $"{GetNumericTime(DateTime.UtcNow.AddSeconds(55))}";
            var signingString = $"(request-target): {method.Method.ToLower()} {path}\n(created): {created}\n(expires): {expires}\nhost: regulatory.api.bnpparibasfortis.be\nx-request-id: {client.DefaultRequestHeaders.GetValues("X-Request-ID").First()}\ndigest: {client.DefaultRequestHeaders.GetValues("Digest").First()}";
            var headerList = "(request-target) (created) (expires) host x-request-id digest";
            var signature = Convert.ToBase64String(cert.GetRSAPrivateKey().SignData(Encoding.UTF8.GetBytes(signingString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            var signatureHeader = $"keyId=\"{keyId}\",algorithm=\"RS256\",headers=\"{headerList}\",(created)={created},(expires)={expires},signature=\"{signature}\"";
            client.DefaultRequestHeaders.Add("Signature", signatureHeader);
        }

        internal static int GetNumericTime(DateTime dateTime)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan result = dateTime.Subtract(dt);
            return Convert.ToInt32(result.TotalSeconds);
        }
    }
}
