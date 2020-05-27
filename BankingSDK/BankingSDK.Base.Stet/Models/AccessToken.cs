using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models
{
    public class AccessToken
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public DateTime expires { get; set; }
        public string refresh_token { get; set; }
    }
}
