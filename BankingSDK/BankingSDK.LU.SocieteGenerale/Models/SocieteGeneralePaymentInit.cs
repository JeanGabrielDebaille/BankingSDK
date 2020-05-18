using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.LU.SocieteGenerale.Models
{
    internal class SocieteGeneralePaymentInit
    {
        public string transactionStatus { get; set; }
        public string paymentId { get; set; }
        public SocieteGeneralePaymentLinks _links { get; set; }
    }

    internal class SocieteGeneralePaymentLinks
    {
        public string scaRedirect { get; set; }
        public string scaOAuth { get; set; }
        public string scaStatus { get; set; }
        public string self { get; set; }
        public string status { get; set; }
    }
}
