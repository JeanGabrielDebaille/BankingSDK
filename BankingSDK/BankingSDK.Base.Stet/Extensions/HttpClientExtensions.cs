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
            var signingString = $"(request-target): {method.Method.ToLower()} {path}\n(created): {GetNumericTime(DateTime.Now)}\n(expires): {GetNumericTime(DateTime.Now.AddSeconds(40))}\nx-request-id: {client.DefaultRequestHeaders.GetValues("X-Request-ID").First()}\ndigest: {client.DefaultRequestHeaders.GetValues("Digest").First()}";
            var headerList = "(request-target) (created) (expires) host x-request-id digest";
            var signature = WebUtility.UrlEncode(Convert.ToBase64String(cert.GetRSAPrivateKey().SignData(Encoding.UTF8.GetBytes(signingString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)));

            var signatureHeader = $"signature:keyId=\"{keyId}\",algorithm=\"RS256\",headers=\"{headerList}\",signature=\"{signature}\"";
            client.DefaultRequestHeaders.Add("Signature", signatureHeader);
        }

        internal static int GetNumericTime(DateTime dateTime)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            TimeSpan result = dateTime.Subtract(dt);
            return Convert.ToInt32(result.TotalSeconds);
        }
    }
}
