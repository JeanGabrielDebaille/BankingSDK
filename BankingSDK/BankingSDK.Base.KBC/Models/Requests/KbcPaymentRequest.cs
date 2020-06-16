using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.KBC.Models.Requests
{
    public class KbcPaymentRequest
    {
        public KbcCreditorAccount creditorAccount { get; set; }
        public string creditorName { get; set; }
        public KbcDebtorAccount debtorAccount { get; set; }
        public KbcInstructedAmount instructedAmount { get; set; }
        public string endToEndIdentification { get; set; }
        public string requestedExecutionDate { get; set; }
        public string remittanceInformationUnstructured { get; set; } = "";
    }
    public class KbcCreditorAccount
    {
        public string iban { get; set; }
    }

    public class KbcDebtorAccount
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    public class KbcInstructedAmount
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }
}
