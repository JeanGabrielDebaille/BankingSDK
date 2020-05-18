using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngTransactionsModel
    {
        public IngTransactions transactions { get; set; }
        public IngAccount account { get; set; }
    }

    internal class IngDebtorAccount
    {
        public string bban { get; set; }
        public string iban { get; set; }
        public string bic { get; set; }
    }

    internal class IngRemittanceInformationStructured
    {
        public string reference { get; set; }
    }

    internal class IngTransactionAmount
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    internal class IngBooked
    {
        public string transactionType { get; set; }
        public IngDebtorAccount debtorAccount { get; set; }
        public IngRemittanceInformationStructured remittanceInformationStructured { get; set; }
        public IngTransactionAmount transactionAmount { get; set; }
        public DateTime? bookingDate { get; set; }
        public string debtorName { get; set; }
        public string endToEndId { get; set; }
        public string transactionId { get; set; }
    }

    internal class IngTransactions
    {
        public List<IngBooked> booked { get; set; }
    }
}
