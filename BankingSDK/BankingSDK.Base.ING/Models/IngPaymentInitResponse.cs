using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngPaymentInitResponse
    {
        public string transactionStatus { get; set; }
        public string paymentId { get; set; }
        public IngLinks _links { get; set; }
        public List<IngTppMessage> tppMessages { get; set; }
    }
    internal class IngLinks
    {
        public string scaRedirect { get; set; }
        public string self { get; set; }
        public string status { get; set; }
        public string delete { get; set; }
    }

    internal class IngTppMessage
    {
        public string category { get; set; }
        public string code { get; set; }
        public string text { get; set; }
        public string path { get; set; }
    }
}
