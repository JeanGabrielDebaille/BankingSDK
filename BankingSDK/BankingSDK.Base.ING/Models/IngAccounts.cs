using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngAccounts
    {
        public List<IngAccount> accounts { get; set; }
    }

    internal class IngAccount
    {
        public string resourceId { get; set; }
        public string product { get; set; }
        public string iban { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
    }
}
