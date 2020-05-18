using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupBalancesDto
    {
        public BerlinGroupAccountDto account { get; set; }
        public List<BerlinGroupBalanceDto> balances { get; set; }
    }

    public class BerlinGroupBalanceAmountDto
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    public class BerlinGroupBalanceDto
    {
        public BerlinGroupBalanceAmountDto balanceAmount { get; set; }
        public DateTime? lastChangeDateTime { get; set; }
        public DateTime? referenceDate { get; set; }
        public string balanceType { get; set; }
    }
}
