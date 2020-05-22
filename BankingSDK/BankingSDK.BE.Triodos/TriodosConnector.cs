using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Exceptions;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BankingSDK.BE.Triodos
{
    public class BeTriodosConnector : IBankingConnector
    {
        public string UserContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        #region User

        public Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public bool UserContextChanged
        {
            get =>throw new NotImplementedException();
        }

        public ConnectorType ConnectorType => throw new NotImplementedException();

        #endregion

        #region Accounts
        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            throw new NotImplementedException();
        }

        public async Task<BankingResult<List<Account>>> GetAccountsAsync()
        {
            var client = GetClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await client.GetAsync("/xs2a-bg/nl/onboarding/v1");
            if (!result.IsSuccessStatusCode)
            {
                throw new ApiCallException(await result.Content.ReadAsStringAsync());
            }

            throw new NotImplementedException();
        }

        public Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<string>> RequestAccountsAccessAsync(AccountsAccessRequest model)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(FlowContext flowContext, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RequestAccountsAccessFinalizeAsync(string flowContextJson, string queryString)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Balances

        public Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Transactions

        public Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Payment

        public Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest model)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(FlowContext flowContext, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(string flowContextJson, string queryString)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Pager

        public IPagerContext CreatePageContext(byte limit)
        {
            throw new NotImplementedException();
        }

        public IPagerContext RestorePagerContext(string json)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private

        //private async Task<AccessData> GetToken(string authorizationCode, string codeChallenge)
        //{
        //    string codeVerifier;
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        codeVerifier = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(codeChallenge)));
        //    }

        //    var content = new StringContent($"client_id={settings.NcaId}&code={authorizationCode}&code_verifier={codeVerifier}&grant_type=authorization_code&redirect_uri=http://localhosta",
        //           Encoding.UTF8, "application/x-www-form-urlencoded");
        //    var client = GetClient();
        //    var result = await client.PostAsync($"/berlingroup/v1/token", content);

        //    if (!result.IsSuccessStatusCode)
        //    {
        //        throw new ApiUnauthorizedException(await result.Content.ReadAsStringAsync());
        //    }

        //    return JsonConvert.DeserializeObject<AccessData>(await result.Content.ReadAsStringAsync());
        //}

        private HttpClient GetClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2("digitealqwaccn.p12", "test");
            handler.ClientCertificates.Add(cert);
            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://xs2a-sandbox.triodos.com")
            };
            //client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

            return client;
        }
        #endregion
    }
}
