using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BNP.Models.Requests
{
    public class PaymentRequest
    {
        public string paymentInformationId { get; set; }
        public string creationDateTime { get; set; }
        public string requestedExecutionDate { get; set; }
        public int numberOfTransactions { get; set; }
        public Party initiatingParty { get; set; }
        public PaymentTypeInformation paymentTypeInformation { get; set; }
        public Debtor debtor { get; set; }
        public Account debtorAccount { get; set; }
        //public Agent debtorAgent { get; set; }
        public Beneficiary beneficiary { get; set; }
        //public Party ultimateCreditor { get; set; }
        //public string purpose { get; set; }
        //public string chargeBearer { get; set; }
        //public string paymentInformationStatus { get; set; }
        //public string statusReasonInformation { get; set; }
        //public string fundsAvailability { get; set; }
        //public bool? booking { get; set; }
        public List<CreditTransferTransaction> creditTransferTransaction { get; set; }
        public SupplementaryData supplementaryData { get; set; }
    }

    public class PostalAddress
    {
        public string country { get; set; }
        public List<string> addressLine { get; set; }
    }

    public class OrganisationId
    {
        public string identification { get; set; }
        public string schemeName { get; set; }
        public string issuer { get; set; }
    }

    public class PaymentTypeInformation
    {
        //public string instructionPriority { get; set; }
        public string serviceLevel { get; set; }
        //public string localInstrument { get; set; }
        //public string categoryPurpose { get; set; }
    }

    public class Account
    {
        public string iban { get; set; }
        //public OrganisationId others { get; set; }
    }

    public class ClearingSystemMemberId
    {
        public string clearingSystemId { get; set; }
        public string memberId { get; set; }
    }

    public class Agent
    {
        public string bicFi { get; set; }
        public ClearingSystemMemberId clearingSystemMemberId { get; set; }
        public string name { get; set; }
        public PostalAddress postalAddress { get; set; }
    }

    public class Party
    {
        public string name { get; set; }
        public PostalAddress postalAddress { get; set; }
        public OrganisationId organisationId { get; set; }
    }

    public class Debtor
    {
        public string name { get; set; }
        public PostalAddress postalAddress { get; set; }
        public OrganisationId privateId { get; set; }
    }

    public class Beneficary
    {
        public string id { get; set; }
        //public bool? isTrusted { get; set; }
        //public Agent creditorAgent { get; set; }
        public Party creditor { get; set; }
        public Account creditorAccount { get; set; }
    }

    public class PaymentId
    {
        //public string resourceId { get; set; }
        public string instructionId { get; set; }
        public string endToEndId { get; set; }
    }

    public class InstructedAmount
    {
        public string currency { get; set; }
        public string amount { get; set; }
    }

    public class Beneficiary
    {
        //public bool? isTrusted { get; set; }
        //public Agent creditorAgent { get; set; }
        public Party creditor { get; set; }
        public Account creditorAccount { get; set; }
    }

    public class CreditTransferTransaction
    {
        public PaymentId paymentId { get; set; }
        //public DateTime? requestedExecutionDate { get; set; }
        //public string endDate { get; set; }
        //public string executionRule { get; set; }
        //public string frequency { get; set; }
        public InstructedAmount instructedAmount { get; set; }
        //public string id { get; set; }
        //public Beneficiary beneficiary { get; set; }
        //public Party ultimateCreditor { get; set; }
        //public List<int> regulatoryReportingCodes { get; set; }
        public List<string> remittanceInformation { get; set; }
        //public string transactionStatus { get; set; }
        //public string statusReasonInformation { get; set; }
    }

    public class SupplementaryData
    {
        public List<string> acceptedAuthenticationApproach { get; set; }
        //public string appliedAuthenticationApproach { get; set; }
        //public string scaHint { get; set; }
        public string successfulReportUrl { get; set; }
        public string unsuccessfulReportUrl { get; set; }
    }
}
