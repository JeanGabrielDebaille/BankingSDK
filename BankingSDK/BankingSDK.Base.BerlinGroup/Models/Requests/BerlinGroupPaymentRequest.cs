using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models.Requests
{
    public class BerlinGroupPaymentRequest
    {
        public BerlinGroupCreditorAccount creditorAccount { get; set; }
        public BerlinGroupCreditorAddress creditorAddress { get; set; }
        public string creditorAgent { get; set; }
        public string creditorName { get; set; }
        public string dayOfExecution { get; set; }
        public BerlinGroupDebtorAccount debtorAccount { get; set; }
        public DateTime? endDate { get; set; }
        public string endToEndIdentification { get; set; }
        public string executionRule { get; set; }
        public string frequency { get; set; }
        public BerlinGroupInstructedAmount instructedAmount { get; set; }
        public string remittanceInformationUnstructured { get; set; }
        public string requestedExecutionDate { get; set; }
        public DateTime? startDate { get; set; }
    }

    public class BerlinGroupCreditorAccount
    {
        public string bban { get; set; }
        public string currency { get; set; }
        public string iban { get; set; }
        public string maskedPan { get; set; }
        public string msisdn { get; set; }
    }

    public class BerlinGroupCreditorAddress
    {
        public string buildingNumber { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string postalCode { get; set; }
        public string street { get; set; }
    }

    public class BerlinGroupDebtorAccount
    {
        public string bban { get; set; }
        public string currency { get; set; }
        public string iban { get; set; }
        public string maskedPan { get; set; }
        public string msisdn { get; set; }
    }

    public class BerlinGroupInstructedAmount
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }
}
