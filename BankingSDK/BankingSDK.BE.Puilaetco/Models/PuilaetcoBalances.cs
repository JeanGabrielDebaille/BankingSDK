using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Puilaetco.Models
{
    internal class PuilaetcoBalances
    {
        public PuilaetcoAccount account { get; set; }
        public List<PuilaetcoBalance> balances { get; set; }
    }

    internal class PuilaetcoBalanceAmount
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    internal class PuilaetcoBalance
    {
        public PuilaetcoBalanceAmount balanceAmount { get; set; }
        public DateTime? lastChangeDateTime { get; set; }
        public DateTime? referenceDate { get; set; }
        public string balanceType { get; set; }
    }
}
