using BankingSDK.Base.BerlinGroup.Models.Requests;
using BankingSDK.Common.Enums;
using System.Collections.Generic;

namespace BankingSDK.BE.Puilaetco.Models
{
    internal class PuilaetcoPaymentStatus
    {
        public PaymentSatusISO20022 transactionStatus { get; set; }
        public string endToEndIdentification { get; set; }
        public PuilaetcoDebtorAccountDto debtorAccount { get; set; }
        public BerlinGroupInstructedAmount instructedAmount { get; set; }
        public string currencyOfTransfer { get; set; }
        public PuilaetcoExchangeRateInformation exchangeRateInformation { get; set; }
        public PuilaetcoCreditorAccountDto creditorAccount { get; set; }
        public string creditorAgent { get; set; }
        public string creditorAgentName { get; set; }
        public string creditorName { get; set; }
        public PuilaetcoCreditorAddress creditorAddress { get; set; }
        public string ultimateCreditor { get; set; }
        public string purposeCode { get; set; }
        public string chargeBearer { get; set; }
        public string remittanceInformationUnstructured { get; set; }
        public List<string> remittanceInformationUnstructuredArray { get; set; }
        public PuilaetcoRemittanceInformationStructured remittanceInformationStructured { get; set; }
        public string requestedExecutionDate { get; set; }
        public bool feesIndicator { get; set; }
        public PuilaetcoFeesAmount feesAmount { get; set; }
    }

    internal class PuilaetcoDebtorAccountDto
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    internal class PuilaetcoExchangeRateInformation
    {
        public string unitCurrency { get; set; }
        public string exchangeRate { get; set; }
        public string contractIdentification { get; set; }
        public string rateType { get; set; }
    }

    internal class PuilaetcoCreditorAccountDto
    {
        public string iban { get; set; }
        public string currency { get; set; }
    }

    internal class PuilaetcoCreditorAddress
    {
        public string street { get; set; }
        public string buildingNumber { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
    }

    internal class PuilaetcoRemittanceInformationStructured
    {
        public string reference { get; set; }
        public string referenceType { get; set; }
        public string referenceIssuer { get; set; }
    }

    internal class PuilaetcoFeesAmount
    {
        public string currency { get; set; }
        public int amount { get; set; }
    }
}
