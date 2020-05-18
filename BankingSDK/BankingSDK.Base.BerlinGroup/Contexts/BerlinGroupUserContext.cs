using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Contexts
{
    public class BerlinGroupUserContext : IUserContext
    {
        public string UserId { get; set; }
        public List<BerlinGroupUserConsent> Consents { get; set; } = new List<BerlinGroupUserConsent>();
        public List<BerlinGroupConsentAccount> Accounts { get; set; } = new List<BerlinGroupConsentAccount>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class BerlinGroupUserConsent
    {
        public string ConsentId { get; set; }
        public DateTime ValidUntil { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidUntil { get; set; }
    }

    public class BerlinGroupConsentAccount
    {
        public string Id { get; set; }
        public string Iban { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string TransactionsConsentId { get; set; }
        public string BalancesConsentId { get; set; }
    }
}
