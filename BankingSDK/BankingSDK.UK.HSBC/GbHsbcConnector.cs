using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using BankingSDK.UK.Hsbc.Contexts;
using BankingSDK.UK.Hsbc.Models;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace BankingSDK.UK.Hsbc
{
    public class GbHsbcConnector : SdkBaseConnector, IBankingConnector
    {
        private HsbcUserContext _userContextLocal => (HsbcUserContext)_userContext;

        private readonly string _countryCode;
        private readonly Uri _sandboxUrl = new Uri("http://sandbox.hsbc.com/psd2/obie");
        private readonly Uri _productionUrl = new Uri("https://api.ing.com");


        private Uri apiUrl => SdkApiSettings.IsSandbox ? _sandboxUrl : _productionUrl;

        public string UserContext
        {
            get => JsonConvert.SerializeObject(_userContext);
            set => _userContext = JsonConvert.DeserializeObject<HsbcUserContext>(value);
        }

        public GbHsbcConnector(BankSettings settings, string countryCode, ConnectorType connectorType) : base(settings, connectorType)
        {
            _countryCode = countryCode;
        }

        public GbHsbcConnector(BankSettings settings, string countryCode, int connectorID) : base(settings, connectorID)
        {
            _countryCode = countryCode;
        }

        #region User
        public async Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            _userContext = new HsbcUserContext
            {
                UserId = userId
            };

            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
        }
        #endregion

        #region Accounts

        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            return RequestAccountsAccessOption.NotCustomizable;
        }

        public async Task<BankingResult<List<Account>>> GetAccountsAsync()
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                //return new BankingResult<List<Account>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private async Task<List<HsbcUserContext>> GetAccountsAsync(string token)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<string>> RequestAccountsAccessAsync(AccountsAccessRequest model)
        {
            try
            {
                FlowContext flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType.BE_BNP,
                    FlowType = FlowType.AccountsAccess,
                    RedirectUrl = model.RedirectUrl
                };
                var brand = ConnectorType == ConnectorType.BE_HELLO_BANK ? "hb" : (ConnectorType == ConnectorType.BE_FINTRO ? "fintro" : "bnppf");
                var redirect = $"{apiUrl}/psd2/obie/v3.1/as/token.oauth2?response_type=code&client_id={_settings.AppClientId}&redirect_uri={WebUtility.UrlEncode($"{model.RedirectUrl}")}&scope=aisp&state={model.FlowId}&brand={brand}";
                return new BankingResult<string>(ResultStatus.REDIRECT, "", redirect, null, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(FlowContext flowContext, string queryString)
        {
            throw new Exception("NOT IMPLEMENTED");
        }

        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(string flowContextJson, string queryString)
        {
            return await RequestAccountsAccessFinalizeAsync(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
        }

        public async Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                //return new BankingResult<List<BankingAccount>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private void RemoveToken(UserAccessToken accessToken)
        {
            if (accessToken?.RefreshAccessToken == null)
            {
                return;
            }

            _userContextLocal.Accounts.RemoveAll(x => x.RefreshAccessToken == accessToken.RefreshAccessToken);
            _userContextLocal.Tokens.Remove(accessToken);
            UserContextChanged = true;
        }
        #endregion

        #region Balances
        public async Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                //return new BankingResult<List<Balance>>(ResultStatus.DONE, url, data, rawData);
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }
        #endregion

        #region Transactions
        public async Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                //return new BankingResult<List<Transaction>>(ResultStatus.DONE, url, data, rawData, pagerContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }
        #endregion

        #region Payment
        public async Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest model)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                //return new BankingResult<string>(ResultStatus.REDIRECT, url, paymentResult._links.scaRedirect, rawData, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(FlowContext flowContext, string queryString)
        {
            try
            {
                throw new Exception("NOT IMPLEMENTED");
                // return new BankingResult<PaymentStatus>(ResultStatus.DONE, url, data, rawData);
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(string flowContextJson, string queryString)
        {
            return await CreatePaymentInitiationRequestFinalizeAsync(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
        }
        #endregion

        #region Pager
        public IPagerContext RestorePagerContext(string json)
        {
            return JsonConvert.DeserializeObject<HsbcPagerContext>(json);
        }

        public IPagerContext CreatePageContext(byte limit)
        {
            return new HsbcPagerContext(limit);
        }
        #endregion

        #region Private


        public static async Task<string> CreateJWTAsync(X509Certificate2 signingCert)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            X509SecurityKey privateKey = new X509SecurityKey(signingCert);
            // Create JWToken
            var token = tokenHandler.CreateJwtSecurityToken(issuer: "6ffc4352-661f-49df-8666-3d2f05196584",
                audience: "https://sandbox.hsbc.com/psd2/obie/v3.1/as/token.oauth2",
                subject: new System.Security.Claims.ClaimsIdentity(new List<Claim> { new Claim("sub", "6ffc4352-661f-49df-8666-3d2f05196584") }),
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials:
                new SigningCredentials(privateKey,
                        SecurityAlgorithms.RsaSha256));

            return tokenHandler.WriteToken(token);
        }

        private async Task<HsbcAccessData> GetClientToken()
        {
            var jwt = await CreateJWTAsync(_settings.SigningCertificate);
            //var content = new StringContent(jwt, Encoding.UTF8, "application/jwt");
            HttpClientHandler handler = new HttpClientHandler();

            handler.ClientCertificates.Add(_settings.TlsCertificate);
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://sandbox.hsbc.com");
            //var result = await client.PostAsync("/psd2/obie/v3.1/register", content);

            //if (!result.IsSuccessStatusCode)
            //{
            //    throw new Exception(await result.Content.ReadAsStringAsync());
            //}

            var content = new StringContent($"grant_type=client_credentials&scope=accounts&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={jwt}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var result = await client.PostAsync("/psd2/obie/v3.1/as/token.oauth2", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }


            string rvalue = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HsbcAccessData>(rvalue);
        }

        private async Task<HsbcAccessData> GetCustomerToken(string token, string authorizationCode)
        {
            throw new Exception("NOT IMPLEMENTED");
            //return JsonConvert.DeserializeObject<HsbcAccessData>(await result.Content.ReadAsStringAsync());
        }

        private async Task<HsbcAccessData> GetRefreshToken(string token, string refreshToken)
        {
            throw new Exception("NOT IMPLEMENTED");
            //return JsonConvert.DeserializeObject<HsbcAccessData>(await result.Content.ReadAsStringAsync());
        }

        private SdkHttpClient GetClient(string payload = "")
        {
            SdkHttpClient client = GetSdkClient(apiUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'", new CultureInfo("en-US")));
            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
            using (SHA256 sha256Hash = SHA256.Create())
            {
                client.DefaultRequestHeaders.Add("Digest", "SHA-256=" + Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(payload))));
            }
            return client;
        }
        #endregion
    }
}
