using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupTransactionsModelDto
    {
        public BerlinGroupAccountDto account { get; set; }
        public BerlinGroupTransactionsDto transactions { get; set; }
    }

    public class BerlinGroupTransactionModelDto
    {
        public BerlinGroupTransactionDto transaction { get; set; }
    }

    public class BerlinGroupTransactionAmount
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    public class BerlinGroupCreditorAccount
    {
        public string iban { get; set; }
    }

    public class BerlinGroupTransactionDto
    {
        public string transactionId { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime valueDate { get; set; }
        public BerlinGroupTransactionAmount transactionAmount { get; set; }
        public string creditorName { get; set; }
        public BerlinGroupCreditorAccount creditorAccount { get; set; }
        public string remittanceInformationUnstructured { get; set; }
    }

    public class BerlinGroupTransactionsDto
    {
        public List<BerlinGroupTransactionDto> booked { get; set; } = new List<BerlinGroupTransactionDto>();
        public List<BerlinGroupTransactionDto> pending { get; set; } = new List<BerlinGroupTransactionDto>();
        public BerlinGroupLinks _links { get; set; }

        public uint PageTotal
        {
            get
            {
                if (_links?.last?.href == null)
                {
                    return 0;
                }

                return uint.Parse(HttpUtility.ParseQueryString((new Uri(_links.last.href)).Query).Get("page"));
            }
        }

        public List<BerlinGroupTransactionDto> all
        {
            get
            {
                var temp = new List<BerlinGroupTransactionDto>();
                temp.AddRange(booked);
                temp.AddRange(pending);
                return temp.OrderByDescending(x => x.valueDate).ToList();
            }
        }
    }

    public class BerlinGroupLinks
    {
        public BerlinGroupLink last { get; set; }
    }
}
