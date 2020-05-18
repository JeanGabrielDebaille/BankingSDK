using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.LU.SocieteGenerale.Models
{
    internal class SocieteGeneraleAccountsAccessResponse
    {
        public string consentStatus { get; set; }
        public string consentId { get; set; }
        public SocieteGeneraleLinks _links { get; set; }
    }

    internal class SocieteGeneraleLinks
    {
        public string scaRedirect { get; set; }
        public string scaOAuth { get; set; }
        public string startAuthorisation { get; set; }
        public string scaStatus { get; set; }
        public string status { get; set; }
        public string self { get; set; }
    }
}
