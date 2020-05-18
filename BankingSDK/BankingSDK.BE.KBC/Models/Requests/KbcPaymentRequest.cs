using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.KBC.Models.Requests
{
    internal class KbcPaymentRequest
    {
        public KbcCreditorAccount creditorAccount { get; set; }
        public string creditorName { get; set; }
        public KbcDebtorAccount debtorAccount { get; set; }
        public KbcInstructedAmount instructedAmount { get; set; }
        public string endToEndIdentification { get; set; }
        public string requestedExecutionDate { get; set; }
        public string remittanceInformationUnstructured { get; set; } = "";
    }
    internal class KbcCreditorAccount
    {
        public string iban { get; set; }
    }

    internal class KbcDebtorAccount
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    internal class KbcInstructedAmount
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }
}
