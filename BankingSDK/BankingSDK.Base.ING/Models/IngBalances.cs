using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngBalances
    {
        public List<BalanceDto> balances { get; set; }
        public IngAccount account { get; set; }
    }

    internal class BalanceAmountDto
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    internal class BalanceDto
    {
        public string balanceType { get; set; }
        public DateTime? lastChangeDateTime { get; set; }
        public BalanceAmountDto balanceAmount { get; set; }
    }
}
