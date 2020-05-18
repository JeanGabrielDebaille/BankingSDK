using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Belfius.Models
{
    public class BelfiusTransactions
    {
        public Embedded _embedded { get; set; }
        public Links _links { get; set; }
    }

    public class BelfiusTransaction
    {
        public string transaction_ref { get; set; }
        public string counterparty_info { get; set; }
        public string communication { get; set; }
        public string communication_type { get; set; }
        public string remittance_info { get; set; }
        public string counterparty_account { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public DateTime execution_date { get; set; }
    }

    public class Embedded
    {
        public List<BelfiusTransaction> transactions { get; set; }
        public string next_page_key { get; set; }
    }
}
