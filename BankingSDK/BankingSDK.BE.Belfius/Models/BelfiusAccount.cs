using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Belfius.Models
{
    public class BelfiusAccount
    {
        public string type { get; set; }
        public string iban { get; set; }
        public string currency { get; set; }
        public decimal balance { get; set; }
        public bool multicurrency { get; set; }
        public List<object> other_compartments { get; set; }
        public Links _links { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Link self { get; set; }
        public Link transactions { get; set; }
    }
}
