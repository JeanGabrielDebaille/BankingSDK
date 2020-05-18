//using BankingSDK.Common;
//using BankingSDK.Common.Contexts;
//using BankingSDK.Common.Enums;
//using BankingSDK.Common.Exceptions;
//using BankingSDK.Common.Interfaces;
//using BankingSDK.Common.Interfaces.Contexts;
//using BankingSDK.Common.Models;
//using BankingSDK.Common.Models.BerlinGroup.Requests;
//using BankingSDK.Common.Models.Data;
//using BankingSDK.Common.Models.Request;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Net.WebRequestMethods;

//namespace BankingSDK.DE.Sparkasse
//{
//    public class DeSparkasseConnector : IBankingConnector
//    {
//        private BerlinGroupUserContext _userContext;
//        private readonly string _sandboxUrl;
//        private readonly string _productionUrl;
//        private readonly BankSettings settings;
//        private readonly SdkApiConnector sdkApiConnector;

//        private string _url
//        {
//            get
//            {
//                return SdkApiSettings.IsSandbox ? _sandboxUrl : _productionUrl;
//            }
//        }

//        public string UserContext
//        {
//            get
//            {
//                return JsonConvert.SerializeObject(_userContext);
//            }
//            set
//            {
//                _userContext = JsonConvert.DeserializeObject<BerlinGroupUserContext>(value);
//            }
//        }

//        public bool UserContextChanged { get; private set; }

//        public ConnectorType ConnectorType => ConnectorType.BE_Sparkasse;

//        public DeSparkasseConnector(BankSettings settings, string sandboxUrl, string productionUrl)
//        {
//            _sandboxUrl = sandboxUrl;
//            _productionUrl = productionUrl;
//            this.settings = settings;
//            sdkApiConnector = new SdkApiConnector();
//        }

//        #region User
//        public async Task<BankingResult<IUserContext>> RegisterUser(string userId)
//        {
//            _userContext = new BerlinGroupUserContext
//            {
//                UserId = userId
//            };

//            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
//        }
//        #endregion

//        #region Accounts
//        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
//        {
//            return RequestAccountsAccessOption.Customizable;
//        }

//        public async Task<BankingResult<List<Account>>> GetAccounts()
//        {
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();

//            var data = _userContext.Accounts.Select(x => new Account
//            {
//                Id = x.Id,
//                Currency = x.Currency,
//                Iban = x.Iban,
//                Description = x.Description,
//                BalancesConsent = _userContext.Consents.Where(y => y.ConsentId == x.BalancesConsentId).Select(c => new ConsentInfo { ConsentId = c.ConsentId, ValidUntil = c.ValidUntil }).FirstOrDefault(),
//                TransactionsConsent = _userContext.Consents.Where(y => y.ConsentId == x.TransactionsConsentId).Select(c => new ConsentInfo { ConsentId = c.ConsentId, ValidUntil = c.ValidUntil }).FirstOrDefault()
//            }).ToList();

//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, "", 200, Http.Get, requestedAt, _userContext.UserId, null);

//            return new BankingResult<List<Account>>(ResultStatus.DONE, "", data, JsonConvert.SerializeObject(data));
//        }

//        //private async Task<List<AccountDto>> GetAccounts(string consentId, string token)
//        //{
//        //    var client = GetClient();
//        //    client.DefaultRequestHeaders.Add("Consent-ID", consentId);
//        //    client.DefaultRequestHeaders.Add("Authorization", $"{token}");
//        //    var url = $"/berlingroup/v1/accounts";
//        //    var result = await client.GetAsync(url);

//        //    if (!result.IsSuccessStatusCode)
//        //    {
//        //        throw new ApiCallException(await result.Content.ReadAsStringAsync());
//        //    }

//        //    string rawData = await result.Content.ReadAsStringAsync();
//        //    return JsonConvert.DeserializeObject<AccountsDto>(rawData).accounts;
//        //}

//        public async Task<BankingResult<string>> RequestAccountsAccess(AccountsAccessRequest model)
//        {
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();
//            var request = new AccountAccessRequest
//            {
//                access = new Access
//                {
//                    allPsd2 = (model.TransactionAccounts == null && model.BalanceAccounts == null) ? "allAccounts" : null,
//                    balances = model.BalanceAccounts?.Select(x => new AccountIban { iban = x }).ToList(),
//                    transactions = model.TransactionAccounts?.Select(x => new AccountIban { iban = x }).ToList()
//                },
//                combinedServiceIndicator = false,
//                frequencyPerDay = model.FrequencyPerDay,
//                recurringIndicator = true,
//                validUntil = DateTime.Today.AddDays(90).ToString("yyyy-MM-dd")
//            };
//            var payload = JsonConvert.SerializeObject(request);

//            var content = new StringContent(payload, Encoding.UTF8, "application/json");
//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
//            client.DefaultRequestHeaders.Add("TPP-Redirect-URI", SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl);
//            var url = "/berlingroup/v1/consents";
//            var result = await client.PostAsync(url, content);

//            if (!result.IsSuccessStatusCode)
//            {
//                var exceptionMessage = await result.Content.ReadAsStringAsync();
//                watch.Stop();
//                await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, exceptionMessage: exceptionMessage);
//                throw new ApiCallException(exceptionMessage);
//            }

//            string rawData = await result.Content.ReadAsStringAsync();
//            var accountAccessResult = JsonConvert.DeserializeObject<AccountsAccessResponse>(rawData);
//            var codeVerifier = Guid.NewGuid().ToString();
//            string codeChallenge;
//            using (SHA256 sha256Hash = SHA256.Create())
//            {
//                codeChallenge = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)));
//            }
//            var redirect = $"{accountAccessResult._links.scaOAuth.href}?scope=AIS:{accountAccessResult.consentId}&client_id={settings.NcaId}&state={model.FlowId}&redirect_uri={WebUtility.UrlEncode(SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl)}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&response_type=code&code_challenge_method=S256";

//            var flowContext = new FlowContext
//            {
//                Id = model.FlowId,
//                ConnectorType = ConnectorType,
//                FlowType = FlowType.AccountsAccess,
//                CodeVerifier = codeVerifier,
//                AccountAccessProperties = new AccountAccessProperties
//                {
//                    ConsentId = accountAccessResult.consentId,
//                    ValidUntil = DateTime.Today.AddDays(90).Date,
//                    BalanceAccounts = model.BalanceAccounts,
//                    TransactionAccounts = model.TransactionAccounts
//                }
//            };
//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null);

//            return new BankingResult<string>(ResultStatus.REDIRECT, url, redirect, rawData, flowContext: flowContext);
//        }

//        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalize(FlowContext flowContext, string queryString)
//        {
//            var query = HttpUtility.ParseQueryString(queryString);
//            var error = query.Get("error");
//            if (error != null)
//            {
//                throw new ApiCallException(query.Get("error_description"));
//            }
//            var code = query.Get("code");
//            var auth = await GetToken(code, flowContext.CodeVerifier);
//            var accounts = await GetAccounts(flowContext.AccountAccessProperties.ConsentId, $"{auth.token_type} {auth.access_token}");
//            bool fullAccess = flowContext.AccountAccessProperties.BalanceAccounts == null && flowContext.AccountAccessProperties.TransactionAccounts == null;

//            _userContext.Consents.Add(new UserConsent
//            {
//                ConsentId = flowContext.AccountAccessProperties.ConsentId,
//                ValidUntil = flowContext.AccountAccessProperties.ValidUntil,
//                RefreshToken = auth.refresh_token,
//                Token = auth.Token,
//                TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60)
//            });

//            foreach (var account in accounts)
//            {
//                var temp = _userContext.Accounts.FirstOrDefault(x => x.Id == account.resourceId);
//                if (temp != null)
//                {
//                    temp.BalancesConsentId = fullAccess || flowContext.AccountAccessProperties.BalanceAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : temp.BalancesConsentId;
//                    temp.TransactionsConsentId = fullAccess || flowContext.AccountAccessProperties.TransactionAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : temp.TransactionsConsentId;
//                }
//                else
//                {
//                    _userContext.Accounts.Add(new ConsentAccount
//                    {
//                        Id = account.resourceId,
//                        Iban = account.iban,
//                        Currency = account.currency,
//                        Description = account.name,
//                        BalancesConsentId = fullAccess || flowContext.AccountAccessProperties.BalanceAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : null,
//                        TransactionsConsentId = fullAccess || flowContext.AccountAccessProperties.TransactionAccounts.Any(y => account.iban == y) ? flowContext.AccountAccessProperties.ConsentId : null
//                    });
//                }
//            }

//            //cleanup
//            foreach (var consent in _userContext.Consents.Where(x => x.ValidUntil < DateTime.Now).ToList())
//            {
//                RemoveConsent(consent);
//            }

//            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
//        }

//        public async Task<BankingResult<IUserContext>> RequestAccountsAccessFinalize(string flowContextJson, string queryString)
//        {
//            return await RequestAccountsAccessFinalize(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
//        }

//        public async Task<BankingResult<List<BankingAccount>>> DeleteAccountAccess(string consentId)
//        {
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();
//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            var url = $"/berlingroup/v1/consents/{consentId}";
//            var result = await client.DeleteAsync(url);

//            if (!result.IsSuccessStatusCode)
//            {
//                var exceptionMessage = await result.Content.ReadAsStringAsync();
//                watch.Stop();
//                await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, exceptionMessage: exceptionMessage);
//                throw new ApiCallException(exceptionMessage);
//            }

//            var consent = _userContext.Consents.FirstOrDefault(x => x.ConsentId == consentId);
//            var data = _userContext.Accounts.Where(x => x.BalancesConsentId == consentId || x.TransactionsConsentId == consentId).Select(x => new BankingAccount
//            {
//                Iban = x.Iban,
//                Currency = x.Currency
//            }).ToList();

//            RemoveConsent(consent);
//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null);

//            return new BankingResult<List<BankingAccount>>(ResultStatus.DONE, url, data, JsonConvert.SerializeObject(data));
//        }

//        private void RemoveConsent(UserConsent consent)
//        {
//            if (consent?.ConsentId == null)
//            {
//                return;
//            }

//            _userContext.Accounts.Where(x => x.BalancesConsentId == consent.ConsentId).ToList().ForEach(x =>
//            {
//                x.BalancesConsentId = null;
//            });
//            _userContext.Accounts.Where(x => x.TransactionsConsentId == consent.ConsentId).ToList().ForEach(x =>
//            {
//                x.TransactionsConsentId = null;
//            });

//            _userContext.Accounts.RemoveAll(x => x.TransactionsConsentId == null && x.BalancesConsentId == null);
//            _userContext.Consents.Remove(consent);
//            UserContextChanged = true;
//        }
//        #endregion

//        #region Balances
//        public async Task<BankingResult<List<Balance>>> GetBalances(string accountId)
//        {
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();
//            var account = _userContext.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalide accountId");
//            var consent = _userContext.Consents.FirstOrDefault(x => x.ConsentId == account.BalancesConsentId && x.ValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalide or expired");
//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            client.DefaultRequestHeaders.Add("Consent-ID", consent.ConsentId);
//            if (consent.TokenValidUntil < DateTime.Now)
//            {
//                await RefreshToken(consent);
//            }
//            client.DefaultRequestHeaders.Add("Authorization", consent.Token);
//            var url = $"/berlingroup/v1/accounts/{accountId}/balances";
//            var result = await client.GetAsync(url);

//            if (!result.IsSuccessStatusCode)
//            {
//                var exceptionMessage = await result.Content.ReadAsStringAsync();
//                watch.Stop();
//                await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, exceptionMessage: exceptionMessage);
//                throw new ApiCallException(exceptionMessage);
//            }

//            string rawData = await result.Content.ReadAsStringAsync();
//            var model = JsonConvert.DeserializeObject<BalancesDto>(rawData);

//            var data = model.balances.Select(x => new Balance
//            {
//                BalanceAmount = new BalanceAmount
//                {
//                    Amount = x.balanceAmount.amount,
//                    Currency = x.balanceAmount.currency
//                },
//                BalanceType = x.balanceType,
//                ReferenceDate = x.referenceDate,
//                LastChangeDateTime = x.lastChangeDateTime
//            }).ToList();

//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, model.account.iban);

//            return new BankingResult<List<Balance>>(ResultStatus.DONE, url, data, rawData);
//        }
//        #endregion

//        #region Transactions
//        public async Task<BankingResult<List<Transaction>>> GetTransactions(string accountId, IPagerContext context = null)
//        {
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();
//            var account = _userContext.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalide accountId");
//            var consent = _userContext.Consents.FirstOrDefault(x => x.ConsentId == account.BalancesConsentId && x.ValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalide or expired");
//            BerlinGroupPagerContext pagerContext = (context as BerlinGroupPagerContext) ?? new BerlinGroupPagerContext();

//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            client.DefaultRequestHeaders.Add("Consent-ID", consent.ConsentId);
//            if (consent.TokenValidUntil < DateTime.Now)
//            {
//                await RefreshToken(consent);
//            }
//            client.DefaultRequestHeaders.Add("Authorization", consent.Token);
//            var url = $"/berlingroup/v1/accounts/{accountId}/transactions{pagerContext.GetRequestParams()}";
//            var result = await client.GetAsync(url);

//            if (!result.IsSuccessStatusCode)
//            {
//                var exceptionMessage = await result.Content.ReadAsStringAsync();
//                watch.Stop();
//                await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, exceptionMessage: exceptionMessage);
//                throw new ApiCallException(exceptionMessage);
//            }

//            string rawData = await result.Content.ReadAsStringAsync();
//            var model = JsonConvert.DeserializeObject<TransactionsModelDto>(rawData);
//            pagerContext.SetPage(pagerContext.GetNextPage());
//            pagerContext.SetPageTotal(model.transactions.PageTotal);

//            var data = model.transactions.all.Select(x => new Transaction
//            {
//                Id = x.transactionId,
//                Amount = x.transactionAmount.amount,
//                CounterpartReference = x.creditorAccount.iban,
//                CounterpartName = x.creditorName,
//                Currency = x.transactionAmount.currency,
//                Description = x.remittanceInformationUnstructured,
//                ExecutionDate = x.bookingDate,
//                ValueDate = x.valueDate
//            }).ToList();
//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, model.account.iban);

//            return new BankingResult<List<Transaction>>(ResultStatus.DONE, url, data, rawData, pagerContext);
//        }
//        #endregion

//        #region Payment
//        public async Task<BankingResult<string>> CreatePaymentInitiationRequest(PaymentInitiationRequest model)
//        {
//            var validate = model.Validate();
//            var requestedAt = DateTime.UtcNow;
//            var watch = Stopwatch.StartNew();
//            var paymentRequest = new PaymentRequest
//            {
//                creditorAccount = new Models.BerlinGroup.Requests.CreditorAccount
//                {
//                    iban = model.Recipient.Iban
//                },
//                creditorName = model.Recipient.Name,
//                debtorAccount = new DebtorAccount
//                {
//                    iban = model.Debtor.Iban,
//                    currency = model.Debtor.Currency
//                },
//                instructedAmount = new InstructedAmount
//                {
//                    amount = model.Amount,
//                    currency = model.Currency
//                },
//                endToEndIdentification = model.EndToEndId,
//                requestedExecutionDate = model.RequestedExecutionDate?.ToString("yyyy-MM-dd")
//            };

//            var content = new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json");
//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            client.DefaultRequestHeaders.Add("TPP-Explicit-Authorisation-Preferred", "false");
//            client.DefaultRequestHeaders.Add("PSU-IP-Address", model.PsuIp);
//            client.DefaultRequestHeaders.Add("TPP-Redirect-URI", SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl);
//            var url = $"/berlingroup/v1/payments/sepa-credit-transfers";
//            var result = await client.PostAsync(url, content);

//            if (!result.IsSuccessStatusCode)
//            {
//                var exceptionMessage = await result.Content.ReadAsStringAsync();
//                watch.Stop();
//                await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, exceptionMessage: exceptionMessage);
//                throw new ApiCallException(exceptionMessage);
//            }

//            var rawData = await result.Content.ReadAsStringAsync();
//            var paymentResult = JsonConvert.DeserializeObject<PaymentInitResponse>(rawData);
//            var codeVerifier = Guid.NewGuid().ToString();
//            string codeChallenge;
//            using (SHA256 sha256Hash = SHA256.Create())
//            {
//                codeChallenge = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)));
//            }
//            var redirect = $"{paymentResult._links.scaOAuth.href}?scope=PIS:{paymentResult.paymentId}&client_id={settings.NcaId}&state=test&redirect_uri={(SdkApiSettings.IsSandbox ? "http://localhost" : model.RedirectUrl)}&code_challenge={WebUtility.UrlEncode(codeChallenge)}&response_type=code&code_challenge_method=S256";


//            var flowContext = new FlowContext
//            {
//                Id = model.FlowId,
//                ConnectorType = ConnectorType,
//                FlowType = FlowType.Payment,
//                RedirectUrl = model.RedirectUrl,
//                CodeVerifier = codeVerifier,
//                PaymentProperties = new PaymentProperties
//                {
//                    PaymentId = paymentResult.paymentId
//                }
//            };
//            watch.Stop();
//            await sdkApiConnector.Log((int)ConnectorType, _url, (int)watch.ElapsedMilliseconds, url, (int)result.StatusCode, Http.Get, requestedAt, _userContext.UserId, null, model.Amount);

//            return new BankingResult<string>(ResultStatus.REDIRECT, url, redirect, rawData, flowContext: flowContext);
//        }

//        public async Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalize(FlowContext flowContext, string queryString)
//        {
//            var query = HttpUtility.ParseQueryString(queryString);
//            var error = query.Get("error");
//            if (error != null)
//            {
//                throw new ApiCallException(query.Get("error_description"));
//            }

//            var client = GetClient();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            var url = $"/berlingroup/v1/payments/sepa-credit-transfers/{flowContext.PaymentProperties.PaymentId}";
//            var result = await client.GetAsync(url);

//            if (!result.IsSuccessStatusCode)
//            {
//                throw new ApiCallException(await result.Content.ReadAsStringAsync());
//            }

//            var rawData = await result.Content.ReadAsStringAsync();
//            var paymentResult = JsonConvert.DeserializeObject<PaymentStatusDto>(rawData);

//            var data = new PaymentStatus
//            {
//                Amount = new BankingAccountInstructedAmount
//                {
//                    Amount = paymentResult.instructedAmount.amount,
//                    Currency = paymentResult.instructedAmount.currency
//                },
//                Creditor = new BankingAccount
//                {
//                    Iban = paymentResult.creditorAccount.iban,
//                    Currency = paymentResult.creditorAccount.currency
//                },
//                CreditorName = paymentResult.creditorName,
//                Debtor = new BankingAccount
//                {
//                    Iban = paymentResult.debtorAccount.iban,
//                    Currency = paymentResult.debtorAccount.currency
//                },
//                EndToEndIdentification = paymentResult.endToEndIdentification,
//                Status = paymentResult.transactionStatus
//            };

//            return new BankingResult<PaymentStatus>(ResultStatus.DONE, url, data, rawData);
//        }

//        public async Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalize(string flowContextJson, string queryString)
//        {
//            return await CreatePaymentInitiationRequestFinalize(JsonConvert.DeserializeObject<FlowContext>(flowContextJson), queryString);
//        }
//        #endregion

//        #region Pager
//        public IPagerContext RestorePagerContext(string json)
//        {
//            return JsonConvert.DeserializeObject<BerlinGroupPagerContext>(json);
//        }

//        public IPagerContext CreatePageContext(byte limit)
//        {
//            return new BerlinGroupPagerContext(limit);
//        }
//        #endregion

//        #region Private

//        private async Task<AccessData> GetToken(string authorizationCode, string codeVerifier)
//        {
//            var content = new StringContent($"client_id={settings.NcaId}&code={authorizationCode}&code_verifier={WebUtility.UrlEncode(codeVerifier)}&grant_type=authorization_code&redirect_uri=http://localhosta",
//                   Encoding.UTF8, "application/x-www-form-urlencoded");
//            var client = GetClient();
//            var result = await client.PostAsync($"/berlingroup/v1/token", content);

//            if (!result.IsSuccessStatusCode)
//            {
//                throw new ApiUnauthorizedException(await result.Content.ReadAsStringAsync());
//            }

//            return JsonConvert.DeserializeObject<AccessData>(await result.Content.ReadAsStringAsync());
//        }

//        private async Task RefreshToken(UserConsent consent)
//        {
//            var content = new StringContent($"client_id={settings.NcaId}&refresh_token={consent.RefreshToken}&grant_type=refresh_token",
//                   Encoding.UTF8, "application/x-www-form-urlencoded");
//            var client = GetClient();
//            var result = await client.PostAsync($"/berlingroup/v1/token", content);

//            if (!result.IsSuccessStatusCode)
//            {
//                throw new ApiUnauthorizedException(await result.Content.ReadAsStringAsync());
//            }

//            var auth = JsonConvert.DeserializeObject<AccessData>(await result.Content.ReadAsStringAsync());
//            consent.Token = auth.Token;
//            consent.TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60);
//            UserContextChanged = true;
//        }

//        private HttpClient GetClient()
//        {
//            HttpClientHandler handler = new HttpClientHandler();
//            handler.ClientCertificates.Add(settings.TlsCertificate);
//            HttpClient client = new HttpClient(handler)
//            {
//                BaseAddress = new Uri(_url)
//            };
//            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

//            return client;
//        }
//        #endregion
//    }
//}
