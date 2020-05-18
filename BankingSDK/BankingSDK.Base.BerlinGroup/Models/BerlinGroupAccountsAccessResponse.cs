using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupAccountsAccessResponse
    {
        public string consentStatus { get; set; }
        public string consentId { get; set; }
        public BerlinGroupAccountsAccessLinks _links { get; set; }
    }

    public class BerlinGroupAccountsAccessLinks
    {
        public BerlinGroupLink scaOAuth { get; set; }
        public BerlinGroupLink scaStatus { get; set; }
        public BerlinGroupLink status { get; set; }
        public BerlinGroupLink self { get; set; }
    }
}
