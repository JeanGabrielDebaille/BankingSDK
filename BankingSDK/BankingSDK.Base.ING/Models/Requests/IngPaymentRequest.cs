using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models.Requests
{
    internal class IngPaymentRequest
    {
        //internal string endToEndIdentification { get; set; }
        public IngDebtorAccount debtorAccount { get; set; }
        public IngInstructedAmount instructedAmount { get; set; }
        public IngCreditorAccount creditorAccount { get; set; }
        //internal string creditorAgent { get; set; }
        public string creditorName { get; set; }
        //internal CreditorAddress creditorAddress { get; set; }
        //internal string chargeBearer { get; set; }
        //internal string remittanceInformationUnstructured { get; set; }
        //internal ClearingSystemMemberIdentification clearingSystemMemberIdentification { get; set; }
        //internal string debtorName { get; set; }
        //internal string debtorAgent { get; set; }
        //internal string instructionPriority { get; set; }
        //internal string serviceLevelCode { get; set; }
        //internal string localInstrumentCode { get; set; }
        //internal string categoryPurposeCode { get; set; }
        //internal string requestedExecutionDate { get; set; }
        //internal string startDate { get; set; }
        //internal string frequency { get; set; }
        
    }

    internal class IngDebtorAccount
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    internal class IngInstructedAmount
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    internal class IngCreditorAccount
    {
        public string iban { get; set; }
        //internal string bban { get; set; }
        //internal string currency { get; set; }
    }

    internal class IngCreditorAddress
    {
        public string street { get; set; }
        public string buildingNumber { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }

    internal class IngClearingSystemMemberIdentification
    {
        public string clearingSystemIdentificationCode { get; set; }
        public string memberIdentification { get; set; }
    }
}
