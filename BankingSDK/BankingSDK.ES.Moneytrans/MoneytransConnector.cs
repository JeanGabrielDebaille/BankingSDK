using BankingSDK.Common.Contexts;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Interfaces;
using BankingSDK.Common.Interfaces.Contexts;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Data;
using BankingSDK.Common.Models.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingSDK.ES.Moneytrans
{
    public class MoneytransConnector : IBankingConnector
    {
        public ConnectorType ConnectorType => throw new NotImplementedException();

        public string UserContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool UserContextChanged => throw new NotImplementedException();

        public IPagerContext CreatePageContext(byte limit)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<string>> CreatePaymentInitiationRequestAsync(PaymentInitiationRequest request)
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

        public Task<BankingResult<IUserContext>> RegisterUserAsync(string userId)
        {
            throw new NotImplementedException();
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
    }
}
