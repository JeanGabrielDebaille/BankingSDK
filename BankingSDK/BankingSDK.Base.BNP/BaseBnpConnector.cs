using BankingSDK.Base.BNP.Contexts;
using BankingSDK.Base.BNP.Extensions;
using BankingSDK.Base.BNP.Models;
using BankingSDK.Base.BNP.Models.Requests;
using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Account = BankingSDK.Common.Models.Data.Account;

namespace BankingSDK.Base.BNP
{
    public abstract class BaseBnpConnector : SdkBaseConnector , IBankingConnector
    {
        private BnpUserContext _userContextLocal => (BnpUserContext)_userContext;
        private readonly Uri _apiUrlSandbox = new Uri("https://sandbox.api.bnpparibasfortis.com");
        private readonly Uri _authUrlSandbox = new Uri("https://sandbox.auth.bnpparibasfortis.com");
        private readonly Uri _apiUrlProduction;
        private readonly Uri _authUrlProduction;

        private Uri apiUrl => SdkApiSettings.IsSandbox ? _apiUrlSandbox : _apiUrlProduction;

        private Uri authUrl => SdkApiSettings.IsSandbox ? _authUrlSandbox : _apiUrlProduction;

        private Uri authUrl2 => SdkApiSettings.IsSandbox ? _authUrlSandbox : _authUrlProduction;

        public string UserContext
        {
            get
            {
                return JsonConvert.SerializeObject(_userContext);
            }
            set
            {
                _userContext = JsonConvert.DeserializeObject<BnpUserContext>(value);
            }
        }




        public BaseBnpConnector(BankSettings settings, Uri productionUrl, Uri productionAuthUrl, ConnectorType connectorType):base(settings,connectorType)
        {
            _apiUrlProduction = productionUrl;
            _authUrlProduction= productionAuthUrl;

        }

        #region User
        public async Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            _userContext = new BnpUserContext
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
                    BalancesConsent = _userContextLocal.Tokens.Where(y => y.AccessToken == x.AccessToken).Select(c => new ConsentInfo { ConsentId = c.AccessToken, ValidUntil = c.TokenValidUntil }).FirstOrDefault(),
                    TransactionsConsent = _userContextLocal.Tokens.Where(y => y.AccessToken == x.AccessToken).Select(c => new ConsentInfo { ConsentId = c.AccessToken, ValidUntil = c.TokenValidUntil }).FirstOrDefault()
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

        private async Task<List<AccountDto>> GetAccounts(string token)
        {
            try
            {
                var client =GetClient();
                client.DefaultRequestHeaders.Add("Authorization", token);

                var url = $"/v1/accounts{(SdkApiSettings.IsSandbox ? (ConnectorType == ConnectorType.BE_HelloBank ? "?brand=hb" : (ConnectorType == ConnectorType.BE_Fintro ? "?brand=fintro" : "")) : "bnppf")}";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, _settings.PemFileUrl);
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<AccountsDto>(rawData);

                return model.accounts;
            }
            catch (ApiCallException e) { throw e; }
            catch (ApiUnauthorizedException e) { throw e; }
            catch (PagerException e) { throw e; }
            catch (SdkUnauthorizedException e) { throw e; }
            catch (Exception e)
            {
                await LogAsync( apiUrl, 500, Http.Get, e.ToString());
                throw e;
            }
        }

        public async Task<BankingResult<string>> RequestAccountsAccessAsync(AccountsAccessRequest model)
        {
            FlowContext flowContext = new FlowContext
            {
                Id = model.FlowId,
                ConnectorType = ConnectorType.BE_BNP,
                FlowType = FlowType.AccountsAccess,
                RedirectUrl = model.RedirectUrl
            };
            var brand = ConnectorType == ConnectorType.BE_HelloBank ? "hb" : (ConnectorType == ConnectorType.BE_Fintro ? "fintro" : "bnppf");
            var redirect = $"{authUrl2}/authorize?response_type=code&client_id={_settings.AppClientId}&redirect_uri={WebUtility.UrlEncode($"{model.RedirectUrl}")}&scope=aisp&state={model.FlowId}&brand={brand}";
            return new BankingResult<string>(ResultStatus.REDIRECT, "", redirect, null, flowContext: flowContext);
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
            var auth = await GetToken(code, flowContext.RedirectUrl);
            var accounts = await GetAccounts($"{auth.token_type} {auth.access_token}");

            _userContextLocal.Tokens.Add(new UserAccessToken
            {
                TokenType = auth.token_type,
                AccessToken = auth.access_token,
                TokenValidUntil = DateTime.Now.AddSeconds(auth.expires_in - 60)
            });

            foreach (var account in accounts)
            {
                var temp = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == account.resourceId);
                if (temp != null)
                {
                    temp.AccessToken = auth.access_token;
                }
                else
                {
                    _userContextLocal.Accounts.Add(new UserAccount
                    {
                        Id = account.resourceId,
                        Iban = account.accountId.iban,
                        Currency = account.currency,
                        Description = account.name,
                        AccessToken = auth.access_token
                    });
                }
            }

            //cleanup
            foreach (var accessToken in _userContextLocal.Tokens.Where(x => x.TokenValidUntil < DateTime.Now).ToList())
            {
                RemoveToken(accessToken);
            }

            UserContextChanged = true;
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
                var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.AccessToken == consentId);
                var data = _userContextLocal.Accounts.Where(x => x.AccessToken == consentId).Select(x => new BankingAccount
                {
                    Iban = x.Iban,
                    Currency = x.Currency
                }).ToList();
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
            if (accessToken?.AccessToken == null)
            {
                return;
            }

            _userContextLocal.Accounts.RemoveAll(x => x.AccessToken == accessToken.AccessToken);
            _userContextLocal.Tokens.Remove(accessToken);
            UserContextChanged = true;
        }
        #endregion

        #region Balances
        public async Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {

            try
            {
                var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalid accountId");
                var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.AccessToken == account.AccessToken && x.TokenValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalid or expired");
                var client = GetClient();
                client.DefaultRequestHeaders.Add("Authorization", accessToken.FullToken);


                var url = $"/v1/accounts/{accountId}/balances";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, _settings.PemFileUrl);
                var result = await client.GetAsync(url);

                string rawData = await result.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<BalancesDto>(rawData);

                var data = model.balances.Select(x => new Balance
                {
                    BalanceAmount = new BalanceAmount
                    {
                        Amount = x.balanceAmount.amount,
                        Currency = x.balanceAmount.currency
                    },
                    BalanceType = x.balanceType,
                    ReferenceDate = x.referenceDate
                }).ToList();

                return new BankingResult<List<Balance>>(ResultStatus.DONE, url, data, rawData);
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

        #region Transactions
        public async Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            try
            {
                var account = _userContextLocal.Accounts.FirstOrDefault(x => x.Id == accountId) ?? throw new ApiCallException("Invalide accountId");
                var accessToken = _userContextLocal.Tokens.FirstOrDefault(x => x.AccessToken == account.AccessToken && x.TokenValidUntil > DateTime.Now) ?? throw new ApiCallException("Consent invalide or expired");
                BnpPagerContext pagerContext = (context as BnpPagerContext) ?? new BnpPagerContext();

                var client = GetClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", accessToken.FullToken);

                var url = $"/v1/accounts/{accountId}/transactions{pagerContext.GetRequestParams()}";

                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, _settings.PemFileUrl);
                var result = await client.GetAsync(url);

                List<Transaction> data = new List<Transaction>();
                string rawData = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    var model = JsonConvert.DeserializeObject<TransactionsDto>(rawData);
                    pagerContext.SetTotal((uint)model.pagination.rowCount);
                    pagerContext.SetPageTotal((uint)model.pagination.pageCount);

                    data = model.transactions?.Select(x => new Transaction
                    {
                        Id = x.entryReference,
                        Amount = x.transactionAmount.amount,
                        Currency = x.transactionAmount.currency,
                        ExecutionDate = x.bookingDate,
                        ValueDate = x.valueDate
                    }).ToList();
                }

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
                var request = new PaymentRequest
                {
                    paymentInformationId = "MyPmtInfIdtt",
                    creationDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    requestedExecutionDate = model.RequestedExecutionDate?.ToString("yyyy-MM-dd"),
                    numberOfTransactions = 1,
                    initiatingParty = new Party
                    {
                        name = SdkApiSettings.TppLegalName,
                        postalAddress = SdkApiSettings.IsSandbox ? new PostalAddress
                        {
                            country = "FR",
                            addressLine = new List<string>
                            {
                            }
                        } : null,
                        organisationId = SdkApiSettings.IsSandbox ? new OrganisationId
                        {
                            identification = "12FR5",
                            issuer = "ACPR",
                            schemeName = "CPAN"
                        } : null
                    },
                    paymentTypeInformation = new PaymentTypeInformation
                    {
                        serviceLevel = "SEPA"
                    },
                    debtor = new Debtor
                    {
                        name = model.Debtor.Name,
                        postalAddress = SdkApiSettings.IsSandbox ? new PostalAddress
                        {
                            country = "FR",
                            addressLine = new List<string>
                            {
                            }
                        } : null,
                        privateId = SdkApiSettings.IsSandbox ? new OrganisationId
                        {
                            identification = "FD37G",
                            issuer = "BICXYYTTZZZ",
                            schemeName = "BANK"
                        } : null
                    },
                    debtorAccount = new Models.Requests.Account
                    {
                        iban = model.Debtor.Iban
                    },
                    beneficiary = new Beneficiary
                    {
                        creditor = new Party
                        {
                            name = model.Recipient.Name,
                            postalAddress = SdkApiSettings.IsSandbox ? new PostalAddress
                            {
                                country = "FR",
                                addressLine = new List<string>
                                {
                                }
                            } : null,
                            organisationId = SdkApiSettings.IsSandbox ? new OrganisationId
                            {
                                identification = "852126789",
                                schemeName = "SREN",
                                issuer = "FR"
                            } : null
                        },
                        creditorAccount = new Models.Requests.Account
                        {
                            iban = model.Recipient.Iban
                        }
                    },
                    creditTransferTransaction = new List<CreditTransferTransaction>
                {
                    new CreditTransferTransaction
                    {
                        paymentId=new PaymentId
                        {
                            instructionId=model.EndToEndId,
                            endToEndId=model.EndToEndId
                        },
                        instructedAmount= new InstructedAmount
                        {
                            amount=model.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                            currency=model.Currency
                        },
                        remittanceInformation= new List<string>
                        {
                        }
                    }
                },
                    supplementaryData = new SupplementaryData
                    {
                        acceptedAuthenticationApproach = new List<string>
                    {
                        "REDIRECT",
                        "DECOUPLED"
                    },
                        successfulReportUrl = model.RedirectUrl + $"?flowId={model.FlowId}",
                        unsuccessfulReportUrl = model.RedirectUrl + $"?flowId={model.FlowId}"
                    }
                };

                var payload = JsonConvert.SerializeObject(request);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var url = $"/v1/payment-requests";
                var client = GetClient();

                var token = await GetClientToken(model.RedirectUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"{token.token_type} {token.access_token}");
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Post, url, _settings.PemFileUrl);
                var result = await client.PostAsync(url, content);

                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<PaymentInitResponse>(rawData);
                var flowContext = new FlowContext
                {
                    Id = model.FlowId,
                    ConnectorType = ConnectorType,
                    FlowType = FlowType.Payment,
                    PaymentProperties = new PaymentProperties
                    {
                        PaymentId = result.Headers.Location.ToString().Split('/').Last()
                    }
                };

                return new BankingResult<string>(ResultStatus.REDIRECT, url, paymentResult._links.consentApproval.href, rawData, flowContext: flowContext);
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

                var client = GetClient();

                var token = await GetClientToken(flowContext.RedirectUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"{token.token_type} {token.access_token}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = $"/v1/payment-requests/{flowContext.PaymentProperties.PaymentId}";
                client.SignRequest(_settings.SigningCertificate, HttpMethod.Get, url, _settings.PemFileUrl);
                var result = await client.GetAsync(url);



                var rawData = await result.Content.ReadAsStringAsync();
                var paymentResult = JsonConvert.DeserializeObject<PaymentStatusDto>(rawData);

                var data = new PaymentStatus
                {
                    Amount = new BankingAccountInstructedAmount
                    {
                        Amount = decimal.Parse(paymentResult.paymentRequest.creditTransferTransaction.First().instructedAmount.amount),
                        Currency = paymentResult.paymentRequest.creditTransferTransaction.First().instructedAmount.currency
                    },
                    Creditor = new BankingAccount
                    {
                        Iban = paymentResult.paymentRequest.beneficiary.creditorAccount.iban
                    },
                    CreditorName = paymentResult.paymentRequest.beneficiary.creditor.name,
                    Debtor = new BankingAccount
                    {
                        Iban = paymentResult.paymentRequest.debtorAccount.iban
                    },
                    EndToEndIdentification = paymentResult.paymentRequest.creditTransferTransaction.First().paymentId.endToEndId,
                    Status = paymentResult.paymentRequest.paymentInformationStatus
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
        public IPagerContext CreatePageContext(byte limit)
        {
            return new BnpPagerContext(limit);
        }

        public IPagerContext RestorePagerContext(string json)
        {
            return JsonConvert.DeserializeObject<BnpPagerContext>(json);
        }
        #endregion

        #region Private

        private async Task<AccessToken> GetToken(string authCode, string redirectUrl)
        {
            var content = new StringContent($"grant_type=authorization_code&code={authCode}&client_id={_settings.AppClientId}&redirect_uri={redirectUrl}&scope=aisp&client_secret={_settings.AppClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var client = GetAuthClient();
            var result = await client.PostAsync("/token", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject<AccessToken>(await result.Content.ReadAsStringAsync());
        }

        private async Task<AccessToken> GetClientToken(string redirectUrl)
        {
            var content = new StringContent($"grant_type=client_credentials&client_id={_settings.AppClientId}&redirect_uri={redirectUrl}&scope=pisp&client_secret={_settings.AppClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var client = GetAuthClient();
            var result = await client.PostAsync("/token", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject<AccessToken>(await result.Content.ReadAsStringAsync());
        }

        private SdkHttpClient GetClient(string payload = "")
        {
            SdkHttpClient sdkHttpClient = GetSdkClient(apiUrl);
            sdkHttpClient.DefaultRequestHeaders.Add("X-Openbank-Organization", "c0eb70e9-02ba-42f6-9736-bd4244c19c18");
            sdkHttpClient.DefaultRequestHeaders.Add("X-Openbank-Stet-Version", "1.4.0.47.develop");
            sdkHttpClient.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
            using (SHA256 sha256Hash = SHA256.Create())
            {
                sdkHttpClient.DefaultRequestHeaders.Add("Digest", "SHA-256=" + Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(payload))));
            }
            return sdkHttpClient;
        }

        private SdkHttpClient GetAuthClient()
        {
            SdkHttpClient sdkHttpClient = GetSdkClient(authUrl);
            sdkHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            sdkHttpClient.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

            if (SdkApiSettings.IsSandbox)
            {
                sdkHttpClient.DefaultRequestHeaders.Add("X-Openbank-Organization", "c0eb70e9-02ba-42f6-9736-bd4244c19c18");
            }
            sdkHttpClient.DefaultRequestHeaders.Add("X-Openbank-Stet-Version", "1.4.0.47.develop");

            return sdkHttpClient;
        }

        #endregion
    }
}
