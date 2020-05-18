using BankingSDK.Base.BerlinGroup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankingSDK.LU.SocieteGenerale.Models
{
    internal class SocieteGeneraleTransactionsModel
    {
        public BerlinGroupAccountDto account { get; set; }
        public SocieteGeneraleTransactions transactions { get; set; }
    }

    internal class SocieteGeneraleCreditorAccount
    {
        public string iban { get; set; }
    }

    internal class SocieteGeneraleTransactions
    {
        public List<BerlinGroupTransactionDto> booked { get; set; } = new List<BerlinGroupTransactionDto>();
        public List<BerlinGroupTransactionDto> pending { get; set; } = new List<BerlinGroupTransactionDto>();
        public SocieteGeneraleLinksDto _links { get; set; }

        public uint PageTotal
        {
            get
            {
                if (string.IsNullOrEmpty(_links?.last))
                {
                    return 1;
                }

                return uint.Parse(HttpUtility.ParseQueryString((new Uri(_links.last)).Query).Get("page"));
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

    internal class SocieteGeneraleLinksDto
    {
        public string last { get; set; }
    }
}
