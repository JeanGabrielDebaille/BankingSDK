using BankingSDK.Base.Stet.Models.Requests;
using BankingSDK.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models
{
    public class PaymentStatusDto
    {
        public PaymentRequest2 paymentRequest { get; set; }
    }

    public class PaymentRequest2
    {
        public string resourceId { get; set; }
        public string paymentInformationId { get; set; }
        public DateTime creationDateTime { get; set; }
        public int numberOfTransactions { get; set; }
        public PaymentSatusISO20022 paymentInformationStatus { get; set; }
        public Party initiatingParty { get; set; }
        public PaymentTypeInformation paymentTypeInformation { get; set; }
        public Debtor debtor { get; set; }
        public Account debtorAccount { get; set; }
        public Beneficiary beneficiary { get; set; }
        public object purpose { get; set; }
        public object chargeBearer { get; set; }
        public List<CreditTransferTransaction> creditTransferTransaction { get; set; }
        public bool booking { get; set; }
        public string requestedExecutionDate { get; set; }
        public SupplementaryData supplementaryData { get; set; }
    }
}
