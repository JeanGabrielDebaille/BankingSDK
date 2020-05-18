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

        public Task<BankingResult<string>> CreatePaymentInitiationRequest(PaymentInitiationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalize(FlowContext flowContext, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<PaymentStatus>> CreatePaymentInitiationRequestFinalize(string flowContextJson, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<BankingAccount>>> DeleteAccountAccess(string consentId)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Account>>> GetAccounts()
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Balance>>> GetBalances(string accountId)
        {
            throw new NotImplementedException();
        }

        public RequestAccountsAccessOption GetRequestAccountsAccessOption()
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<List<Transaction>>> GetTransactions(string accountId, IPagerContext context = null)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RegisterUser(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<string>> RequestAccountsAccess(AccountsAccessRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RequestAccountsAccessFinalize(FlowContext flowContext, string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<BankingResult<IUserContext>> RequestAccountsAccessFinalize(string flowContextJson, string queryString)
        {
            throw new NotImplementedException();
        }

        public IPagerContext RestorePagerContext(string json)
        {
            throw new NotImplementedException();
        }
    }
}
