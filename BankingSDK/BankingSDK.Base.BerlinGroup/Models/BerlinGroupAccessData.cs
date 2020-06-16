using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupAccessData
    {
        public string Token
        {
            get
            {
                return $"{token_type[0].ToString().ToUpper() + token_type.Substring(1).ToLower()} {access_token}";
            }
        }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string client_id { get; set; }
    }
}
