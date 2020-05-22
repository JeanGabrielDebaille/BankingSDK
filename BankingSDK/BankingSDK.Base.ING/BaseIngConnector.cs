using BankingSDK.Base.BerlinGroup.Models;
using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using BankingSDK.Base.ING.Contexts;
using BankingSDK.Base.ING.Extensions;
using BankingSDK.Base.ING.Models;
using BankingSDK.Base.ING.Models.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace BankingSDK.Base.ING
{
    public abstract class BaseIngConnector : SdkBaseConnector, IBankingConnector
    {
        private IngUserContext _userContextLocal => (IngUserContext)_userContext;

        private readonly string _countryCode;
        private readonly Uri _sandboxUrl = new Uri("https://api.sandbox.ing.com");
        private readonly Uri _productionUrl = new Uri("https://api.ing.com");

        private Uri apiUrl => SdkApiSettings.IsSandbox ? _sandboxUrl : _productionUrl;

        public string UserContext
        {
            get => JsonConvert.SerializeObject(_userContext);
            set => _userContext = JsonConvert.DeserializeObject<IngUserContext>(value);
        }

        public BaseIngConnector(BankSettings settings, string countryCode, ConnectorType connectorType)
            : base(settings, connectorType)
        {
            _countryCode = countryCode;
        }

        #region User
        public async Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            _userContext = new IngUserContext
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
                var data = _userContextLocal.Accounts.Select(x => new Account
                {
                    Id = x.Id,
                    Currency = x.Currency,
                    Iban = x.Iban,
                    Description = x.Description,
                    BalancesConsent = _userContextLocal.Tokens.Where(y => y.RefreshAccessToken == x.RefreshAccessToken).Select(c => new ConsentInfo { ConsentId = c.AccessToken, ValidUntil = c.TokenValidUntil }).FirstOrDefault(),
                    TransactionsConsent = _userContextLocal.Tokens.Where(y => y.RefreshAccessToken == x.RefreshAccessToken).Select(c => new ConsentInfo { ConsentId = c.AccessToken, ValidUntil = c.TokenValidUntil }).FirstOrDefault()
                }).ToList();
                return new BankingResult<List<Account>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private async Task<List<IngAccount>> GetAccountsAsync(string token)
        {
            try
            {
                var clientToken = await GetClientToken();
                var client = GetClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                var url = "/v2/accounts";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", clientToken.client_id);
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<IngAccounts>(rawData);

                return model.accounts;
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
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
                var clientAuth = await GetClientToken();
                var client = GetClient();
                client.DefaultRequestHeaders.Add("Authorization", clientAuth.Token);
                var scope = "payment-accounts:balances:view%20payment-accounts:transactions:view";
                var url = $"/oauth2/authorization-server-url?scope={scope}&redirect_uri={model.RedirectUrl}&response_type=code&country_code={_countryCode}";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", clientAuth.client_id);
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var redirect = JsonConvert.DeserializeObject<IngRedirect>(rawData).location + $"?client_id={clientAuth.client_id}&scope={scope}&redirect_uri={model.RedirectUrl}&state={model.FlowId}";

                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.AccountsAccess,
                    RedirectUrl = model.RedirectUrl
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, "", redirect, redirect, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(FlowContext flowContext, string queryString)
        {
            var query = HttpUtility.ParseQueryString(queryString);
            var error = query.Get("error");
            if (error != null)
            {
                await LogAsync(apiUrl, 500, Http.Get, query.Get("error_description"));
                throw new ApiCallException(query.Get("error_description"));
            }

            var code = query.Get("code");
            var clientToken = await GetClientToken();
            var customerToken = await GetCustomerToken($"{clientToken.token_type} {clientToken.access_token}", code);
            var accounts = await GetAccountsAsync(customerToken.Token);

            _userContextLocal.Tokens.Add(new UserAccessToken
            {
                TokenType = customerToken.token_type,
                AccessToken = customerToken.access_token,
                TokenValidUntil = DateTime.Now.AddSeconds(customerToken.expires_in - 60),
                RefreshAccessToken = customerToken.refresh_token,
                RefreshTokenValidUntil = DateTime.Now.AddSeconds(customerToken.refresh_token_expires_in - 60)
            });

            foreach (var account in accounts)
            {
                var temp = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == account.resourceId);
                if (temp != null)
                {
                    temp.RefreshAccessToken = customerToken.refresh_token;
                }
                else
                {
                    _userContextLocal.Accounts.Add(new UserAccount
                    {
                        Id = account.resourceId,
                        Iban = account.iban,
                        Currency = account.currency,
                        Description = account.name,
                        RefreshAccessToken = customerToken.refresh_token
                    });
                }
            }

            //cleanup
            foreach (var accessToken in _userContextLocal.Tokens.Where(x => x.TokenValidUntil < DateTime.Now).ToList())
            {
                RemoveToken(accessToken);
            }

            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
        }

        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(string flowContextJson, string queryString)
        {
            return await RequestAccountsAccessFinalizeAsync(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
        }

        public async Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            try
            {
                var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.RefreshAccessToken == consentId);
                var data = _userContextLocal.Accounts.Where(x => x.RefreshAccessToken == consentId).Select(x => new BankingAccount
                {
                    Iban = x.Iban,
                    Currency = x.Currency
                }).ToList();

                RemoveToken(accessToken);
                return new BankingResult<List<BankingAccount>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
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
            var clientToken = await GetClientToken();
            string token;

            if (SdkApiSettings.IsSandbox)
            {
                var customerToken = await GetCustomerToken($"{clientToken.token_type} {clientToken.access_token}", "8b6cd77a-aa44-4527-ab08-a58d70cca286");
                token = $"{customerToken.token_type} {customerToken.access_token}";
            }
            else
            {
                var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalid accountId");
                var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.RefreshAccessToken == account.RefreshAccessToken && x.RefreshTokenValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalid or expired");
                if (accessToken.TokenValidUntil > DateTime.Now)
                {
                    token = accessToken.FullToken;
                }
                else
                {
                    var refresh = await GetRefreshToken($"{clientToken.token_type} {clientToken.access_token}", _userContextLocal.Tokens.First().RefreshAccessToken);
                    token = $"{refresh.token_type} {refresh.access_token}";
                }
            }

            var client = GetClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var url = $"/v3/accounts/{accountId}/balances{(SdkApiSettings.IsSandbox ? "?balanceTypes=interimBooked" : "")}";
            client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", clientToken.client_id);
            var result = await client.GetAsync(url);

            string rawData = await result.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<IngBalances>(rawData);

            var data = model.balances.Select(x => new Balance
            {
                BalanceAmount = new BalanceAmount
                {
                    Amount = x.balanceAmount.amount,
                    Currency = x.balanceAmount.currency
                },
                BalanceType = x.balanceType,
                LastChangeDateTime = x.lastChangeDateTime
            }).ToList();

            return new BankingResult<List<Balance>>(ResultStatus.DONE, url, data, rawData);
        }
        #endregion

        #region Transactions
        public async Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            try
            {
                IngPagerContext pagerContext = (context as IngPagerContext) ?? new IngPagerContext();
                var clientToken = await GetClientToken();
                string token;

                if (SdkApiSettings.IsSandbox)
                {
                    var customerToken = await GetCustomerToken($"{clientToken.token_type} {clientToken.access_token}", "8b6cd77a-aa44-4527-ab08-a58d70cca286");
                    token = $"{customerToken.token_type} {customerToken.access_token}";
                }
                else
                {
                    var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalid accountId");
                    var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.RefreshAccessToken == account.RefreshAccessToken && x.RefreshTokenValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalid or expired");
                    if (accessToken.TokenValidUntil > DateTime.Now)
                    {
                        token = accessToken.FullToken;
                    }
                    else
                    {
                        var refresh = await GetRefreshToken($"{clientToken.token_type} {clientToken.access_token}", _userContextLocal.Tokens.First().RefreshAccessToken);
                        token = $"{refresh.token_type} {refresh.access_token}";
                    }
                }

                var client = GetClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                var url = $"/v2/accounts/{accountId}/transactions{pagerContext.GetRequestParams()}";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", clientToken.client_id);
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<IngTransactionsModel>(rawData);

                var data = model.transactions.booked.Select(x => new Transaction
                {
                    Id = x.transactionId,
                    Currency = x.transactionAmount.currency,
                    Amount = x.transactionAmount.amount,
                    CounterpartReference = x.debtorAccount.iban,
                    ExecutionDate = x.bookingDate
                }).ToList();

                return new BankingResult<List<Transaction>>(ResultStatus.DONE, url, data, rawData, pagerContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
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
                string payload;
                StringContent content;
                if (SdkApiSettings.IsSandbox)
                {
                    payload = "{\"instructedAmount\":{\"amount\":\"1\",\"currency\":\"EUR\"},\"creditorAccount\":{\"iban\":\"AT861921125678901234\"},\"creditorName\":\"Laura Musterfrau\"}";//JsonConvert.SerializeObject(paymentRequest);//
                    content = new StringContent(payload, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                }
                else
                {
                    var paymentRequest = new IngPaymentRequest
                    {
                        creditorAccount = new Models.Requests.IngCreditorAccount
                        {
                            iban = model.Recipient.Iban
                        },
                        debtorAccount = new Models.Requests.IngDebtorAccount
                        {
                            iban = model.Debtor.Iban,
                            currency = model.Debtor.Currency
                        },
                        creditorName = model.Recipient.Name,
                        instructedAmount = new Models.Requests.IngInstructedAmount
                        {
                            amount = model.Amount.ToString(),
                            currency = model.Currency
                        },
                    };
                    payload = JsonConvert.SerializeObject(paymentRequest);
                    content = new StringContent(payload, Encoding.UTF8, "application/json");
                }

                var url = $"/v1/payments/sepa-credit-transfers";
                var client = GetClient(payload);
                var token = await GetClientToken();
                client.DefaultRequestHeaders.Add("Authorization", $"{token.token_type} {token.access_token}");
                client.DefaultRequestHeaders.Add("TPP-Redirect-URI", SdkApiSettings.IsSandbox ? "https://example.com/redirect" : model.RedirectUrl);
                client.DefaultRequestHeaders.Add("PSU-IP-Address", SdkApiSettings.IsSandbox ? "37.44.220.0" : model.PsuIp);
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Post, url, "Signature", token.client_id);

                var result = await client.PostAsync(url, content);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<Models.IngPaymentInitResponse>(rawData);
                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.Payment,
                    PaymentProperties = new PaymentProperties
                    {
                        PaymentId = paymentResult.paymentId
                    },
                    RedirectUrl = model.RedirectUrl
                };
                return new BankingResult<string>(ResultStatus.REDIRECT, url, paymentResult._links.scaRedirect, rawData, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
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
                var query = HttpUtility.ParseQueryString(queryString);
                var error = query.Get("error");
                if (error != null)
                {
                    await LogAsync(apiUrl, 500, Http.Get, query.Get("error_description"));
                    throw new ApiCallException(query.Get("error_description"));
                }

                var url = $"/v1/payments/sepa-credit-transfers/{flowContext.PaymentProperties.PaymentId}";
                var client = GetClient();
                var token = await GetClientToken();
                client.DefaultRequestHeaders.Add("Authorization", $"{token.token_type} {token.access_token}");
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", token.client_id);
                var result = await client.GetAsync(url);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<IngPayment>(rawData);

                url = $"/v1/payments/sepa-credit-transfers/{flowContext.PaymentProperties.PaymentId}/status";
                client.DefaultRequestHeaders.Remove("Signature");
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, "Signature", token.client_id);
                result = await client.GetAsync(url);

                rawData = await result.Content.ReadAsStringAsync();
                var paymentStatusResult = JsonConvert.DeserializeObject<Models.IngPaymentStatus>(rawData);

                var data = new PaymentStatus
                {
                    Amount = new BankingAccountInstructedAmount
                    {
                        Amount = paymentResult.instructedAmount.amount,
                        Currency = paymentResult.instructedAmount.currency
                    },
                    Creditor = new BankingAccount
                    {
                        Iban = paymentResult.creditorAccount.iban
                    },
                    CreditorName = paymentResult.creditorName,
                    Status = paymentStatusResult.transactionStatus
                };

                return new BankingResult<PaymentStatus>(ResultStatus.DONE, url, data, rawData);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
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
            return JsonConvert.DeserializeObject<IngPagerContext>(json);
        }

        public IPagerContext CreatePageContext(byte limit)
        {
            return new IngPagerContext(limit);
        }
        #endregion

        #region Private
        private async Task<BerlinGroupAccessData> GetClientToken()
        {
            var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            var payload = await content.ReadAsStringAsync();

            var client = GetClient(payload);
            client.DefaultRequestHeaders.Add("TPP-Signature-Certificate", $"-----BEGIN CERTIFICATE-----{Convert.ToBase64String(_settings.SigningCertificate.RawData)}-----END CERTIFICATE-----");
            client.SignRequest(_settings.SigningCertificate, HttpMethod.Post, "/oauth2/token", "Authorization", $"SN={_settings.SigningCertificate.SerialNumber},CA={_settings.SigningCertificate.Issuer}", true);
            var result = await client.PostAsync("/oauth2/token", content);

            return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
        }

        private async Task<BerlinGroupAccessData> GetCustomerToken(string token, string authorizationCode)
        {
            var content = new StringContent($"grant_type=authorization_code&code={authorizationCode}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var payload = await content.ReadAsStringAsync();

            var client = GetClient(payload);
            client.DefaultRequestHeaders.Add("Authorization", token);
            client.SignRequest(_settings.SigningCertificate, HttpMethod.Post, "/oauth2/token", "Signature", "5ca1ab1e-c0ca-c01a-cafe-154deadbea75");
            var result = await client.PostAsync("/oauth2/token", content);

            return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
        }

        private async Task<BerlinGroupAccessData> GetRefreshToken(string token, string refreshToken)
        {
            var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var payload = await content.ReadAsStringAsync();

            var client = GetClient(payload);
            client.DefaultRequestHeaders.Add("Authorization", token);
            client.SignRequest(_settings.SigningCertificate, HttpMethod.Post, "/oauth2/token", "Signature", "5ca1ab1e-c0ca-c01a-cafe-154deadbea75");
            var result = await client.PostAsync("/oauth2/token", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
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
