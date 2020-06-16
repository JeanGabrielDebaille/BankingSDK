using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models
{
    public class AccessToken
    {
        private string _token_type { get; set; }
        public string token_type 
        { 
            get {
                return $"{_token_type[0].ToString().ToUpper() + _token_type.Substring(1).ToLower()}";
            }
            set { _token_type = value;} 
        }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public DateTime expires { get; set; }
        public string refresh_token { get; set; }
    }
}
