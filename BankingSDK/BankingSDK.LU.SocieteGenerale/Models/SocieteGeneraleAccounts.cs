using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.LU.SocieteGenerale.Models
{
    internal class SocieteGeneraleAccounts
    {
        [JsonProperty("accounts")]
        public List<SocieteGeneraleAccount> Accounts { get; set; }
    }

    internal class SocieteGeneraleAccount
    {
        public string resourceId { get; set; }
        public string iban { get; set; }
        public string bban { get; set; }
        public string currency { get; set; }
        public string product { get; set; }
        public string cashAccountType { get; set; }
        public string status { get; set; }
        public string bic { get; set; }
        public string usage { get; set; }
    }
}
