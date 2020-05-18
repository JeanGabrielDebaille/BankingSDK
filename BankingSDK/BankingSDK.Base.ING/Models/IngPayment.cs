using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngPayment
    {
        public IngInstructedAmount instructedAmount { get; set; }
        public IngCreditorAccount creditorAccount { get; set; }
        public string creditorName { get; set; }
    }

    internal class IngInstructedAmount
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    internal class IngCreditorAccount
    {
        public string iban { get; set; }
    }
}
