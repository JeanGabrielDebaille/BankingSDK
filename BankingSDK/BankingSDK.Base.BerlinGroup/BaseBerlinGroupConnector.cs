using BankingSDK.Base.BerlinGroup.Contexts;
using BankingSDK.Base.BerlinGroup.Models;
using BankingSDK.Base.BerlinGroup.Models.Requests;
using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Extension;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace BankingSDK.Base.BerlinGroup
{
    public abstract class BaseBerlinGroupConnector : SdkBaseConnector, IBankingConnector
    {
        private readonly Uri _sandboxUrl;
        private readonly Uri _productionUrl;
        private BerlinGroupUserContext _userContextLocal => (BerlinGroupUserContext)_userContext;

        private Uri _url => SdkApiSettings.IsSandbox ? _sandboxUrl : _productionUrl;

        public string UserContext
        {
            get => JsonConvert.SerializeObject(_userContext);
            set => _userContext = JsonConvert.DeserializeObject<BerlinGroupUserContext>(value);
        }

        protected BaseBerlinGroupConnector(BankSettings settings, string sandboxUrl, string productionUrl, ConnectorType connectorType) : base(settings, connectorType)
        {
            _sandboxUrl = new Uri(sandboxUrl);
            _productionUrl = SdkApiSettings.IsSandbox && string.IsNullOrEmpty(productionUrl) ? null : new Uri(productionUrl);
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
            return RequestAccountsAccessOption.Customizable;
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

                await LogAsync(_url, 200, Http.Get);

                return new BankingResult<List<Account>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        private async Task<List<BerlinGroupAccountDto>> GetAccountsAsync(string consentId, string token)
        {
            var client = GetClient();
            client.DefaultRequestHeaders.Add("Consent-ID", consentId);
            client.DefaultRequestHeaders.Add("Authorization", $"{token}");
            var result = await client.GetAsync("/berlingroup/v1/accounts");

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
                        allPsd2 = (model.TransactionAccounts == null && model.BalanceAccounts == null) ? "allAccounts" : null,
                        balances = model.BalanceAccounts?.Select(x => new BerlinGroupAccountIban { iban = x }).ToList(),
                        transactions = model.TransactionAccounts?.Select(x => new BerlinGroupAccountIban { iban = x }).ToList()
                    },
                    combinedServiceIndicator = false,
                    frequencyPerDay = model.FrequencyPerDay,
                    recurringIndicator = true,
                    validUntil = DateTime.Today.AddDays(90).ToString("yyyy-MM-dd")
                };
                var payload = JsonConvert.SerializeObject(request);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
                client.DefaultRequestHeaders.Add("TPP-Redirect-URI", SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl);
                var url = "/berlingroup/v1/consents";
                var result = await client.PostAsync(url, content);

                string rawData = await result.Content.ReadAsStringAsync();
                var accountAccessResult = JsonConvert.DeserializeObject<BerlinGroupAccountsAccessResponse>(rawData);
                var codeVerifier = Guid.NewGuid().ToString();
                string codeChallenge;
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    codeChallenge = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)));
                }
                var redirect = $"{accountAccessResult._links.scaOAuth.href}?scope=AIS:{accountAccessResult.consentId}&client_id={_settings.NcaId}&state={model.FlowId}&redirect_uri={WebUtility.UrlEncode(SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl)}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&response_type=code&code_challenge_method=S256";

                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.AccountsAccess,
                    CodeVerifier = codeVerifier,
                    AccountAccessProperties = new AccountAccessProperties
                    {
                        ConsentId = accountAccessResult.consentId,
                        ValidUntil = DateTime.Today.AddDays(90).Date,
                        BalanceAccounts = model.BalanceAccounts,
                        TransactionAccounts = model.TransactionAccounts
                    }
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, url, redirect, rawData, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Post, e.ToString());
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
                    throw new ApiCallException(query.Get("error_description"));
                }
                var code = query.Get("code");
                var auth = await GetToken(code, flowContext.CodeVerifier);
                var accounts = await GetAccountsAsync(flowContext.AccountAccessProperties.ConsentId, $"{auth.token_type} {auth.access_token}");
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
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Get, e.ToString());
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
                var url = $"/berlingroup/v1/consents/{consentId}";
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
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, "DELETE", e.ToString());
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
                var url = $"/berlingroup/v1/accounts/{accountId}/balances";
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
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Get, e.ToString());
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
                var url = $"/berlingroup/v1/accounts/{accountId}/transactions{pagerContext.GetRequestParams()}";
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<BerlinGroupTransactionsModelDto>(rawData);
                pagerContext.SetPage(pagerContext.GetNextPage());
                pagerContext.SetPageTotal(model.transactions.PageTotal);

                var data = model.transactions.all.Select(x => new Transaction
                {
                    Id = x.transactionId,
                    Amount = x.transactionAmount.amount,
                    CounterpartReference = x.creditorAccount.iban,
                    CounterpartName = x.creditorName,
                    Currency = x.transactionAmount.currency,
                    Description = x.remittanceInformationUnstructured,
                    ExecutionDate = x.bookingDate,
                    ValueDate = x.valueDate
                }).ToList();

                return new BankingResult<List<Transaction>>(ResultStatus.DONE, url, data, rawData, pagerContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Get, e.ToString());
                throw e;
            }
        }
        #endregion

        #region Payment
        public async Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest model)
        {
            var validate = model.Validate();
            try
            {
                var paymentRequest = new BerlinGroupPaymentRequest
                {
                    creditorAccount = new Models.Requests.BerlinGroupCreditorAccount
                    {
                        iban = model.Recipient.Iban
                    },
                    creditorName = model.Recipient.Name,
                    debtorAccount = new BerlinGroupDebtorAccount
                    {
                        iban = model.Debtor.Iban,
                        currency = model.Debtor.Currency
                    },
                    instructedAmount = new BerlinGroupInstructedAmount
                    {
                        amount = model.Amount,
                        currency = model.Currency
                    },
                    endToEndIdentification = model.EndToEndId,
                    requestedExecutionDate = model.RequestedExecutionDate?.ToString("yyyy-MM-dd")
                };

                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json");
                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("TPP-Explicit-Authorisation-Preferred", "false");
                client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
                client.DefaultRequestHeaders.Add("TPP-Redirect-URI", SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl);
                var url = $"/berlingroup/v1/payments/sepa-credit-transfers";
                var result = await client.PostAsync(url, content);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<BerlinGroupPaymentInitResponse>(rawData);
                var codeVerifier = Guid.NewGuid().ToString();
                string codeChallenge;
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    codeChallenge = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)));
                }
                var redirect = $"{paymentResult._links.scaOAuth.href}?scope=PIS:{paymentResult.paymentId}&client_id={_settings.NcaId}&state=test&redirect_uri={(SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl)}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&response_type=code&code_challenge_method=S256";


                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.Payment,
                    RedirectUrl = model.RedirectUrl,
                    CodeVerifier = codeVerifier,
                    PaymentProperties = new PaymentProperties
                    {
                        PaymentId = paymentResult.paymentId
                    }
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, url, redirect, rawData, flowContext: flowContext);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Post, e.ToString());
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
                    throw new ApiCallException(query.Get("error_description"));
                }

                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = $"/berlingroup/v1/payments/sepa-credit-transfers/{flowContext.PaymentProperties.PaymentId}";
                var result = await client.GetAsync(url);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<BerlinGroupPaymentStatusDto>(rawData);

                var data = new PaymentStatus
                {
                    Amount = new BankingAccountInstructedAmount
                    {
                        Amount = paymentResult.instructedAmount.amount,
                        Currency = paymentResult.instructedAmount.currency
                    },
                    Creditor = new BankingAccount
                    {
                        Iban = paymentResult.creditorAccount.iban,
                        Currency = paymentResult.creditorAccount.currency
                    },
                    CreditorName = paymentResult.creditorName,
                    Debtor = new BankingAccount
                    {
                        Iban = paymentResult.debtorAccount.iban,
                        Currency = paymentResult.debtorAccount.currency
                    },
                    EndToEndIdentification = paymentResult.endToEndIdentification,
                    Status = paymentResult.transactionStatus
                };

                return new BankingResult<PaymentStatus>(ResultStatus.DONE, url, data, rawData);
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync(_url, 500, Http.Get, e.ToString());
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
            return JsonConvert.DeserializeObject<BerlinGroupPagerContext>(json);
        }

        public IPagerContext CreatePageContext(byte limit)
        {
            return new BerlinGroupPagerContext(limit);
        }
        #endregion

        #region Private

        private async Task<BerlinGroupAccessData> GetToken(string authorizationCode, string codeVerifier)
        {
            var content = new StringContent($"client_id={_settings.NcaId}&code={authorizationCode}&code_verifier={WebUtility.UrlEncode(codeVerifier)}&grant_type=authorization_code&redirect_uri=http://localhosta",
                   Encoding.UTF8, "application/x-www-form-urlencoded");
            var client = GetClient();
            var result = await client.PostAsync($"/berlingroup/v1/token", content);

            return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
        }

        private async Task RefreshToken(BerlinGroupUserConsent consent)
        {
            var content = new StringContent($"client_id={_settings.NcaId}&refresh_token={consent.RefreshToken}&grant_type=refresh_token",
                   Encoding.UTF8, "application/x-www-form-urlencoded");
            var client = GetClient();
            var result = await client.PostAsync($"/berlingroup/v1/token", content);

            var auth = JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
            consent.Token = auth.Token;
            consent.TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60);
            UserContextChanged = true;
        }

        private SdkHttpClient GetClient()
        {
            var client = GetSdkClient(_url);
            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

            return client;
        }
        #endregion
    }
}