using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.KBC.Contexts
{
    public class KbcUserContext : IUserContext
    {
        public string UserId { get; set; }
        public List<KbcUserConsent> Consents { get; set; } = new List<KbcUserConsent>();
        public List<KbcConsentAccount> Accounts { get; set; } = new List<KbcConsentAccount>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class KbcUserConsent
    {
        public string ConsentId { get; set; }
        public DateTime ValidUntil { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidUntil { get; set; }
    }

    public class KbcConsentAccount
    {
        public string Id { get; set; }
        public string Iban { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string TransactionsConsentId { get; set; }
        public string BalancesConsentId { get; set; }
    }
}
