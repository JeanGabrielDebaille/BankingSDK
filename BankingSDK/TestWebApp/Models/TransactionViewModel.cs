using System.Collections.Generic;
using BankingSDK.Common.Models.Data;

namespace TestWebApp.Models
{
    public class TransactionListViewModel
    {
        public string AccountId { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}