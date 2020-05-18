using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Puilaetco.Models
{
    internal class PuilaetcoAccounts
    {
        public List<PuilaetcoAccount> accounts { get; set; }
    }

    internal class PuilaetcoAccount
    {
        public string resourceId { get; set; }
        public string name { get; set; }
        public string accountType { get; set; }
        public string bic { get; set; }
        public string currency { get; set; }
        public string cashAccountType { get; set; }
        public string iban { get; set; }
        public string bban { get; set; }
        public string msisdn { get; set; }
        public string maskedPan { get; set; }
    }
}
