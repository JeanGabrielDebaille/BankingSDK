using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models
{
    public class TransactionsDto
    {
        public Pagination pagination { get; set; }
        public List<TransactionDto> transactions { get; set; }
    }

    public class TransactionAmount
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    public class TransactionDto
    {
        public string resourceId { get; set; }
        public string entryReference { get; set; }
        public TransactionAmount transactionAmount { get; set; }
        public string creditDebitIndicator { get; set; }
        public string status { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime transactionDate { get; set; }
        public List<string> remittanceInformation { get; set; }
    }

    public class Pagination
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int rowCount { get; set; }
        public int pageCount { get; set; }
    }
}
