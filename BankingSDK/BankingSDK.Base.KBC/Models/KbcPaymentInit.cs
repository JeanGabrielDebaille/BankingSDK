using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.KBC.Models
{
    public class KbcPaymentInit
    {
        public string transactionStatus { get; set; }
        public string paymentId { get; set; }
        public Links _links { get; set; }
    }

    public class Links
    {
        public string scaRedirect { get; set; }
        public string status { get; set; }
    }
}
