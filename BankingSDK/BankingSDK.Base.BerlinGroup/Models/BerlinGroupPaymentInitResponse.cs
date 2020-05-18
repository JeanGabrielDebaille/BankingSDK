using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupPaymentInitResponse
    {
        public string transactionStatus { get; set; }
        public string paymentId { get; set; }
        public BerlinGroupPaymentLinks _links { get; set; }
    }

    public class BerlinGroupLink
    {
        public string href { get; set; }
    }

    public class BerlinGroupPaymentLinks
    {
        public BerlinGroupLink scaOAuth { get; set; }
        public BerlinGroupLink scaStatus { get; set; }
        public BerlinGroupLink self { get; set; }
        public BerlinGroupLink status { get; set; }
    }
}
