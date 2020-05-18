using BankingSDK.Common.Enums;
using BankingSDK.Base.BerlinGroup.Models.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupPaymentStatusDto
    {
        public PaymentSatusISO20022 transactionStatus { get; set; }
        public string endToEndIdentification { get; set; }
        public BerlinGroupDebtorAccountDto debtorAccount { get; set; }
        public BerlinGroupInstructedAmount instructedAmount { get; set; }
        public string currencyOfTransfer { get; set; }
        public BerlinGroupExchangeRateInformation exchangeRateInformation { get; set; }
        public BerlinGroupCreditorAccountDto creditorAccount { get; set; }
        public string creditorAgent { get; set; }
        public string creditorAgentName { get; set; }
        public string creditorName { get; set; }
        public BerlinGroupCreditorAddress creditorAddress { get; set; }
        public string ultimateCreditor { get; set; }
        public string purposeCode { get; set; }
        public string chargeBearer { get; set; }
        public string remittanceInformationUnstructured { get; set; }
        public List<string> remittanceInformationUnstructuredArray { get; set; }
        public BerlinGroupRemittanceInformationStructured remittanceInformationStructured { get; set; }
        public string requestedExecutionDate { get; set; }
        public bool feesIndicator { get; set; }
        public BerlinGroupFeesAmount feesAmount { get; set; }
    }

    public class BerlinGroupDebtorAccountDto
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    public class BerlinGroupExchangeRateInformation
    {
        public string unitCurrency { get; set; }
        public string exchangeRate { get; set; }
        public string contractIdentification { get; set; }
        public string rateType { get; set; }
    }

    public class BerlinGroupCreditorAccountDto
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    public class BerlinGroupCreditorAddress
    {
        public string street { get; set; }
        public string buildingNumber { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }

    public class BerlinGroupRemittanceInformationStructured
    {
        public string reference { get; set; }
        public string referenceType { get; set; }
        public string referenceIssuer { get; set; }
    }

    public class BerlinGroupFeesAmount
    {
        public string currency { get; set; }
        public int amount { get; set; }
    }
}
