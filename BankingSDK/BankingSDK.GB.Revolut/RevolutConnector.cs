using BankingSDK.Base.BerlinGroup.Models;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BankingSDK.GB.Revolut
{
    public class GbRevolutConnector : IBankingConnector
    {
        public string UserContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool UserContextChanged => throw new NotImplementedException();

        public ConnectorType ConnectorType => throw new NotImplementedException();

        public IPagerContext CreatePageContext(byte limit)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest model)
        {
            throw new NotImplementedException();
        }

        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            throw new NotImplementedException();
        }

        public async Task<BankingResult<List<Account>>> GetAccountsAsync()
        {
            var content = new StringContent(
                "{\"Data\":{\"Permissions\":[\"ReadAccountsDetail\",\"ReadBalances\",\"ReadBeneficiariesDetail\",\"ReadDirectDebits\",\"ReadProducts\",\"ReadStandingOrdersDetail\",\"ReadTransactionsCredits\"," +
                  "\"ReadTransactionsDebits\",\"ReadTransactionsDetail\",\"ReadOffers\",\"ReadPAN\",\"ReadParty\",\"ReadPartyPSU\",\"ReadScheduledPaymentsDetail\",\"ReadStatementsDetail\"]," +
    "\"ExpirationDateTime\":\"2020-05-02T00:00:00+00:00\",\"TransactionFromDateTime\":\"2017-05-03T00:00:00+00:00\",\"TransactionToDateTime\":\"2020-12-03T00:00:00+00:00\"},\"Risk\":{}}",
                Encoding.UTF8, "application/json");
            var token = await GetToken();
            var client = GetClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-fapi-financial-id", "001580000103UAvAAM");
            var result = await client.PostAsync("/account-access-consents", content);
            if (!result.IsSuccessStatusCode)
            {
                throw new ApiCallException(await result.Content.ReadAsStringAsync());
            }

            var temp = await result.Content.ReadAsStringAsync();

            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public IPagerContext RestorePagerContext(string json)
        {
            throw new NotImplementedException();
        }

        public Task SetUser(string userContext)
        {
            throw new NotImplementedException();
        }

        public Task SetUser(IUserContext userContext)
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

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(FlowContext flowContext, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalizeAsync(string flowContextJson, string queryString)
        {
            throw new NotImplementedException();
        }

        public IUserContext GetUserContext()
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            throw new NotImplementedException();
        }

        #region Private

        private async Task<BerlinGroupAccessData> GetToken()
        {
            var content = new StringContent($"client_id=rV1w0YYPf7_mOMaFKAJPIJxHCwiuefIYMo1j9UClT_E",
                   Encoding.UTF8, "application/x-www-form-urlencoded");
            var client = GetAuthClient();
            try
            {
                var result = await client.PostAsync($"/token", content);

                if (!result.IsSuccessStatusCode)
                {
                    throw new ApiUnauthorizedException(await result.Content.ReadAsStringAsync());
                }

                return JsonConvert.DeserializeObject<BerlinGroupAccessData>(await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                throw new Exception(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
            }
        }

        private HttpClient GetClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2("digitealqwac.p12", "digiteal"));
            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://sandbox-oba.revolut.com")
            };
            client.DefaultRequestHeaders.Add("TPP-Redirect-URI", "https://localhost:80");
            client.DefaultRequestHeaders.Add("PSU-IP-Address", "127.0.0.1");
            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

            return client;
        }

        private HttpClient GetAuthClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2("digitealqwac.p12", "digiteal"));
            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://sandbox-oba-auth.revolut.com")
            };
            client.DefaultRequestHeaders.Add("TPP-Redirect-URI", "https://localhost:80");
            client.DefaultRequestHeaders.Add("PSU-IP-Address", "127.0.0.1");
            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

            return client;
        }
        #endregion
    }
}
