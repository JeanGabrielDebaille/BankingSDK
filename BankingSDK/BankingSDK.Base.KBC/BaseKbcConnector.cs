using BankingSDK.Base.BerlinGroup.Contexts;
using BankingSDK.Base.BerlinGroup.Models;
using BankingSDK.Base.BerlinGroup.Models.Requests;
using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using BankingSDK.Base.KBC.Models;
using BankingSDK.Base.KBC.Models.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace BankingSDK.Base.KBC
{
    public class BaseKbcConnector : SdkBaseConnector, IBankingConnector
    {
        private BerlinGroupUserContext _userContextLocal => (BerlinGroupUserContext)_userContext;
        private readonly Uri _sandboxUrl = new Uri("https://be.psd2.sandbox.kbc-group.com");
        private readonly Uri _productionUrl = new Uri("https://openapi.kbc-group.com");

        private Uri apiUrl => SdkApiSettings.IsSandbox ? _sandboxUrl : _productionUrl;
        private string RedirectUrl => SdkApiSettings.IsSandbox ? "https://be.psd2.sandbox.kbc-group.com" : "https://idp.kbc.com";

        public string UserContext
        {
            get
            {
                return JsonConvert.SerializeObject(_userContext);
            }
            set
            {
                _userContext = JsonConvert.DeserializeObject<BerlinGroupUserContext>(value);
            }
        }

        public BaseKbcConnector(BankSettings settings, ConnectorType connectorType) : base(settings, connectorType)
        {
        }

        #region User
        public async Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            _userContext = new BerlinGroupUserContext
            {
                UserId = userId
            };

            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
        }

        #endregion

        #region Accounts
        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            return RequestAccountsAccessOption.SingleAccount;
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
                    BalancesConsent = _userContextLocal.Consents.Where(y => y.ConsentId == x.BalancesConsentId).Select(c => new ConsentInfo { ConsentId = c.ConsentId, ValidUntil = c.ValidUntil }).FirstOrDefault(),
                    TransactionsConsent = _userContextLocal.Consents.Where(y => y.ConsentId == x.TransactionsConsentId).Select(c => new ConsentInfo { ConsentId = c.ConsentId, ValidUntil = c.ValidUntil }).FirstOrDefault()
                }).ToList();

                return new BankingResult<List<Account>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private async Task<List<BerlinGroupAccountDto>> GetAccountsAsync(string consentId, string token)
        {
            var client = GetClient();
            client.DefaultRequestHeaders.Add("Consent-ID", consentId);
            client.DefaultRequestHeaders.Add("Authorization", $"{token}");
            var url = $"/psd2/v2/accounts";
            var result = await client.GetAsync(url);

            string rawData = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BerlinGroupAccountsDto>(rawData).accounts;
        }

        public async Task<BankingResult<string>> RequestAccountsAccessAsync(AccountsAccessRequest model)
        {
            try
            {
                var request = new BerlinGroupAccountAccessRequest
                {
                    access = new BerlinGroupAccess
                    {
                        balances = new List<BerlinGroupAccountIban> { new BerlinGroupAccountIban { iban = model.SingleAccount } },
                        transactions = new List<BerlinGroupAccountIban> { new BerlinGroupAccountIban { iban = model.SingleAccount } }
                    },
                    combinedServiceIndicator = false,
                    frequencyPerDay = model.FrequencyPerDay,
                    recurringIndicator = true,
                    validUntil = DateTime.Today.AddDays(89).ToString("yyyy-MM-dd")
                };

                var payload = JsonConvert.SerializeObject(request);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
                client.DefaultRequestHeaders.Add("TPP-Redirect-URI", model.RedirectUrl);
                var url = "/psd2/v2/consents";
                var result = await client.PostAsync(url, content);

                string rawData = await result.Content.ReadAsStringAsync();
                var accountAccessResult = JsonConvert.DeserializeObject<BerlinGroupAccountsAccessResponse>(rawData);
                //var codeVerifier = "WtTEuIaHve9RS_mMK6P99Z_RWL5cLTOqAJ2ar2BeN9g";
                var codeVerifier = Guid.NewGuid().ToString();
                string codeChallenge;
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    codeChallenge = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier))).Replace("=", "").Replace("+", "-").Replace("/", "_");//"IObWtymAvqW35KPIr8Gsl8jbKJUoL7Dx_EijWCvkwEM";
                }

                string mainCompany;
                switch(ConnectorType)
                {
                    case ConnectorType.BE_KBC:
                        mainCompany = "0001";
                        break;
                    case ConnectorType.BE_CBC:
                        mainCompany = "0002";
                        break;
                    case ConnectorType.BE_KBC_BRUSSELS:
                        mainCompany = "0001&company=9998";
                        break;
                    default:
                        throw new Exception("Unknown connector type");
                }

                // to specify the language add &language=NL
                //var redirect = $"{apiUrl}/ASK/oauth/authorize/1?client_id={_settings.NcaId}&redirect_uri={WebUtility.UrlEncode(model.RedirectUrl)}&response_type=code&scope={WebUtility.UrlEncode($"AIS:{accountAccessResult.consentId}")}&state={model.FlowId}&language=NL&mainCompany={mainCompany}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&code_challenge_method=S256";
                var redirect = $"{RedirectUrl}/ASK/oauth/authorize/1?client_id={_settings.NcaId}&redirect_uri={WebUtility.UrlEncode(model.RedirectUrl)}&response_type=code&scope={WebUtility.UrlEncode($"AIS:{accountAccessResult.consentId}")}&state={model.FlowId}&mainCompany={mainCompany}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&code_challenge_method=S256";

                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.AccountsAccess,
                    CodeVerifier = codeVerifier,
                    AccountAccessProperties = new AccountAccessProperties
                    {
                        ConsentId = accountAccessResult.consentId,
                        ValidUntil = DateTime.Today.AddDays(89).Date,
                        BalanceAccounts = model.BalanceAccounts,
                        TransactionAccounts = model.TransactionAccounts
                    }
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, url, redirect, rawData, flowContext: flowContext);
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
            try
            {
                var query = HttpUtility.ParseQueryString(queryString);
                var error = query.Get("error");
                if (error != null)
                {
                    await LogAsync(apiUrl, 500, Http.Get, query.Get("error_description"));
                    throw new ApiCallException(query.Get("error_description"));
                }
                var code = query.Get("code");
                var auth = await GetToken(code, flowContext.CodeVerifier, flowContext.RedirectUrl);
                var accounts = await GetAccountsAsync(flowContext.AccountAccessProperties.ConsentId, auth.Token);
                bool fullAccess = flowContext.AccountAccessProperties.BalanceAccounts == null && flowContext.AccountAccessProperties.TransactionAccounts == null;

                _userContextLocal.Consents.Add(new BerlinGroupUserConsent
                {
                    ConsentId = flowContext.AccountAccessProperties.ConsentId,
                    ValidUntil = flowContext.AccountAccessProperties.ValidUntil,
                    RefreshToken = auth.refresh_token,
                    Token = auth.Token,
                    TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60)
                });

                foreach (var account in accounts)
                {
                    var temp = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == account.resourceId);
                    if (temp != null)
                    {
                        temp.BalancesConsentId = fullAccess || flowContext.AccountAccessProperties.BalanceAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : temp.BalancesConsentId;
                        temp.TransactionsConsentId = fullAccess || flowContext.AccountAccessProperties.TransactionAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : temp.TransactionsConsentId;
                    }
                    else
                    {
                        _userContextLocal.Accounts.Add(new BerlinGroupConsentAccount
                        {
                            Id = account.resourceId,
                            Iban = account.iban,
                            Currency = account.currency,
                            Description = account.name,
                            BalancesConsentId = fullAccess || flowContext.AccountAccessProperties.BalanceAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : null,
                            TransactionsConsentId = fullAccess || flowContext.AccountAccessProperties.TransactionAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : null
                        });
                    }
                }

                //cleanup
                foreach (var consent in _userContextLocal.Consents.Where(x => x.ValidUntil < DateTime.Now).ToList())
                {
                    RemoveConsent(consent);
                }

                return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(string flowContextJson, string queryString)
        {
            return await RequestAccountsAccessFinalizeAsync(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
        }

        public async Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            try
            {
                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = $"/psd2/v2/consents/{consentId}";
                var result = await client.DeleteAsync(url);

                var consent = _userContextLocal.Consents.FirstOrDefault(x => x.ConsentId == consentId);
                var data = _userContextLocal.Accounts.Where(x => x.BalancesConsentId == consentId || x.TransactionsConsentId == consentId).Select(x => new BankingAccount
                {
                    Iban = x.Iban,
                    Currency = x.Currency
                }).ToList();

                RemoveConsent(consent);

                return new BankingResult<List<BankingAccount>>(ResultStatus.DONE, url, data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private void RemoveConsent(BerlinGroupUserConsent consent)
        {
            if (consent?.ConsentId == null)
            {
                return;
            }

            _userContextLocal.Accounts.Where(x => x.BalancesConsentId == consent.ConsentId).ToList().ForEach(x =>
            {
                x.BalancesConsentId = null;
            });
            _userContextLocal.Accounts.Where(x => x.TransactionsConsentId == consent.ConsentId).ToList().ForEach(x =>
            {
                x.TransactionsConsentId = null;
            });

            _userContextLocal.Accounts.RemoveAll(x => x.TransactionsConsentId == null && x.BalancesConsentId == null);
            _userContextLocal.Consents.Remove(consent);
            UserContextChanged = true;
        }
        #endregion

        #region Balances
        public async Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {
            try
            {
                var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalide accountId");
                var consent = _userContextLocal.Consents.FirstOrDefault(x => x.ConsentId == account.BalancesConsentId && x.ValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalide or expired");
                var client = GetClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Consent-ID", consent.ConsentId);
                if (consent.TokenValidUntil < DateTime.Now)
                {
                    await RefreshToken(consent);
                }
                client.DefaultRequestHeaders.Add("Authorization", consent.Token);
                var url = $"/psd2/v2/accounts/{accountId}/balances";
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<BerlinGroupBalancesDto>(rawData);

                var data = model.balances.Select(x => new Balance
                {
                    BalanceAmount = new BalanceAmount
                    {
                        Amount = x.balanceAmount.amount,
                        Currency = x.balanceAmount.currency
                    },
                    BalanceType = x.balanceType,
                    ReferenceDate = x.referenceDate,
                    LastChangeDateTime = x.lastChangeDateTime
                }).ToList();

                return new BankingResult<List<Balance>>(ResultStatus.DONE, url, data, rawData);
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
                var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalide accountId");
                var consent = _userContextLocal.Consents.FirstOrDefault(x => x.ConsentId == account.BalancesConsentId && x.ValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalide or expired");
                BerlinGroupPagerContext pagerContext = (context as BerlinGroupPagerContext) ?? new BerlinGroupPagerContext();

                var client = GetClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Consent-ID", consent.ConsentId);
                if (consent.TokenValidUntil < DateTime.Now)
                {
                    await RefreshToken(consent);
                }
                client.DefaultRequestHeaders.Add("Authorization", consent.Token);
                var url = $"/psd2/v2/accounts/{accountId}/transactions{pagerContext.GetRequestParams()}";
                url = $"/psd2/v2/accounts/{accountId}/transactions";
                var result = await client.GetAsync(url);


                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<BerlinGroupTransactionsModelDto>(rawData);
                pagerContext.SetPage(pagerContext.GetNextPage());
                pagerContext.SetPageTotal(model.transactions.PageTotal);

                var data = model.transactions.all.Select(x => new Transaction
                {
                    Id = x.transactionId,
                    Amount = x.transactionAmount.amount,
                    CounterpartReference = x.creditorAccount?.iban,
                    CounterpartName = x.creditorName,
                    Currency = x.transactionAmount.currency,
                    Description = x.remittanceInformationUnstructured,
                    ExecutionDate = x.bookingDate,
                    ValueDate = x.valueDate
                }).ToList();

                return new BankingResult<List<Transaction>>(ResultStatus.DONE, url, data, rawData, pagerContext);
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
                var paymentRequest = new KbcPaymentRequest
                {
                    creditorAccount = new Models.Requests.KbcCreditorAccount
                    {
                        iban = model.Recipient.Iban
                    },
                    creditorName = model.Recipient.Name,
                    debtorAccount = new Models.Requests.KbcDebtorAccount
                    {
                        iban = model.Debtor.Iban,
                        currency = model.Debtor.Currency
                    },
                    instructedAmount = new Models.Requests.KbcInstructedAmount
                    {
                        amount = model.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                        currency = model.Currency
                    },
                    endToEndIdentification = model.EndToEndId,
                    requestedExecutionDate = model.RequestedExecutionDate?.ToString("yyyy-MM-dd"),
                    remittanceInformationUnstructured = model.remittanceInformationUnstructured
                };

                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json");
                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
                client.DefaultRequestHeaders.Add("TPP-Redirect-URI", $"{model.RedirectUrl}?flowId={model.FlowId}");
                var url = $"/psd2/v2/payments/sepa-credit-transfers";
                var result = await client.PostAsync(url, content);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<KbcPaymentInit>(rawData);

                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.Payment,
                    PaymentProperties = new PaymentProperties
                    {
                        PaymentId = paymentResult.paymentId
                    }
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, url, paymentResult._links.scaRedirect, rawData, flowContext: flowContext);
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
            var query = HttpUtility.ParseQueryString(queryString);
            var error = query.Get("error");
            if (error != null)
            {
                await LogAsync(apiUrl, 500, Http.Get, query.Get("error_description"));
                throw new ApiCallException(query.Get("error_description"));
            }

            var client = GetClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("PSU-IP-Address", "80.80.0.0"); // shouldn't be mandatory but bug in KBC side code
            var url = $"/psd2/v2/payments/sepa-credit-transfers/{flowContext.PaymentProperties.PaymentId}/status";
            var result = await client.GetAsync(url);

            var rawData = await result.Content.ReadAsStringAsync();
            var paymentResult = JsonConvert.DeserializeObject<BerlinGroupPaymentStatusDto>(rawData);

            var data = new PaymentStatus
            {
                //Amount = new BankingAccountInstructedAmount
                //{
                //    Amount = paymentResult.instructedAmount.amount,
                //    Currency = paymentResult.instructedAmount.currency
                //},
                //Creditor = new BankingAccount
                //{
                //    Iban = paymentResult.creditorAccount.iban,
                //    Currency = paymentResult.creditorAccount.currency
                //},
                //CreditorName = paymentResult.creditorName,
                //Debtor = new BankingAccount
                //{
                //    Iban = paymentResult.debtorAccount.iban,
                //    Currency = paymentResult.debtorAccount.currency
                //},
                //EndToEndIdentification = paymentResult.endToEndIdentification,
                Status = paymentResult.transactionStatus
            };

            return new BankingResult<PaymentStatus>(ResultStatus.DONE, url, data, rawData);

        }

        public async Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(string flowContextJson, string queryString)
        {
            return await CreatePaymentInitiationRequestFinalizeAsync(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
        }
        #endregion

        #region Pager
        public IPagerContext RestorePagerContext(string json)
        {
            return JsonConvert.DeserializeObject<BerlinGroupPagerContext>(json);
        }

        public IPagerContext CreatePageContext(byte limit)
        {
            return new BerlinGroupPagerContext(limit);
        }
        #endregion

        #region Private

        private async Task<BerlinGroupAccessData> GetToken(string authorizationCode, string codeVerifier, string redirectUrl)
        {
            //var content = new StringContent($"client_id={_settings.NcaId}&grant_type=authorization_code&redirect_uri={WebUtility.UrlEncode(redirectUrl)}&code={authorizationCode}&code_verifier={WebUtility.UrlEncode(codeVerifier)}", Encoding.UTF8, );
            var query = $"client_id={_settings.NcaId}&grant_type=authorization_code&redirect_uri={WebUtility.UrlEncode(redirectUrl)}&code={authorizationCode}&code_verifier={WebUtility.UrlEncode(codeVerifier)}";

            var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            var client = GetClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await client.PostAsync($"/ASK/oauth/token/1", content);

            return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
        }

        private async Task RefreshToken(BerlinGroupUserConsent consent)
        {
            try
            {
                var content = new StringContent($"client_id={_settings.NcaId}&grant_type=refresh_token&refresh_token={consent.RefreshToken}",
                       Encoding.UTF8, "application/x-www-form-urlencoded");

                var client = GetClient();
                var result = await client.PostAsync($"/ASK/oauth/token/1", content);

                var auth = JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
                consent.Token = auth.Token;
                consent.TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60);
                consent.RefreshToken = auth.refresh_token;
            } catch(Exception e)
            {
                // An error occured in refreshing, this is not recoverable. Set the date to yesterday to show it invalid
                consent.TokenValidUntil = DateTime.Now.AddDays(-1);
                throw e;
            } finally
            {
                UserContextChanged = true;
            }
        }

        private SdkHttpClient GetClient()
        {
            SdkHttpClient client = GetSdkClient(apiUrl);
            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
            return client;
        }
        #endregion
    }
}
