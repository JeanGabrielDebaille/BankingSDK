using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BNP.Models
{
    public class BalancesDto
    {
        public List<BalanceDto> balances { get; set; }
    }

    public class BalanceAmountDto
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    public class BalanceDto
    {
        public string name { get; set; }
        public string balanceType { get; set; }
        public DateTime? referenceDate { get; set; }
        public BalanceAmountDto balanceAmount { get; set; }
    }
}
