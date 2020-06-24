using BankingSDK.Base.BerlinGroup.Contexts;
using BankingSDK.Common;
using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BankingSDK.ES.Moneytrans
{
    public class MoneytransConnector : SdkBaseConnector, IBankingConnector
    {
        private BerlinGroupUserContext _userContextLocal => (BerlinGroupUserContext)_userContext;

        public MoneytransConnector(BankSettings settings) :base(settings, 1000)
        {

        }


        public string UserContext
        {
            get => JsonConvert.SerializeObject(_userContext);
            set => _userContext = JsonConvert.DeserializeObject<BerlinGroupUserContext>(value);
        }


        public IPagerContext CreatePageContext(byte limit)
        {
            throw new NotImplementedException();
        }

        public async Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest request)
        {
            var token = await GetToken();
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

        public Task<BankingResult<List<BankingAccount>>> DeleteAccountAccessAsync(string consentId)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Account>>> GetAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Balance>>> GetBalancesAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Transaction>>> GetTransactionsAsync(string accountId, IPagerContext context = null)
        {
            throw new NotImplementedException();
        }

        public async Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            _userContext = new BerlinGroupUserContext
            {
                UserId = userId
            };

            return new BankingResult<IUserContext>(ResultStatus.DONE, null, _userContext, JsonConvert.SerializeObject(_userContext));
        }

        public Task<BankingResult<string>> RequestAccountsAccessAsync(AccountsAccessRequest request)
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

        public IPagerContext RestorePagerContext(string json)
        {
            throw new NotImplementedException();
        }


        private async Task<string> GetToken()
        {
            var content = new StringContent($"grant_type=client_credentials&client_id={_settings.AppClientId}&scope=pisp", Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpClientHandler handler = new HttpClientHandler();

            handler.ClientCertificates.Add(_settings.TlsCertificate);
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://pac-api-sandbox.moneytrans.eu");
            var result = await client.PostAsync("/v1/token", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            return await result.Content.ReadAsStringAsync();
        }
    }
}
