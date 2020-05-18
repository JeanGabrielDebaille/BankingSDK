using BankingSDK.Common.Interfaces.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Belfius.Contexts
{
    public class BelfiusUserContext : IUserContext
    {
        public string UserId { get; set; }
        public List<ConsentAccount> Accounts { get; set; } = new List<ConsentAccount>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ConsentAccount
    {
        public string Id { get; set; }
        public string Iban { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public UserToken Token { get; set; }
    }

    public class UserToken
    {
        public DateTime ValidUntil { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidUntil { get; set; }
    }
}
