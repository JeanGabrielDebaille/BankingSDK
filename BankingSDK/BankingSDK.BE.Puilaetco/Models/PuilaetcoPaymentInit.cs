using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Puilaetco.Models
{
    internal class PuilaetcoPaymentInit
    {
        public string transactionStatus { get; set; }
        public string paymentId { get; set; }
        public PuilaetcoLinks _links { get; set; }
    }
}
