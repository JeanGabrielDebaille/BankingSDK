using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Belfius.Models.Requests
{
    public class BelfiusPaymentRequest
    {
        public OriginAccount origin_account { get; set; }
        public RemoteAccount remote_account { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string communication { get; set; }
        public string communication_type { get; set; }
        public string execution_date { get; set; }
        public string payment_id { get; set; }
        public string payment_treatment_type { get; set; }
    }

    public class OriginAccount
    {
        public string iban { get; set; }
    }

    public class RemoteAccount
    {
        public string name { get; set; }
        public string iban { get; set; }
        public string bic { get; set; }
    }
}
