using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models
{
    public class AccountsDto
    {
        public List<AccountDto> accounts { get; set; }
    }

    public class AccountId
    {
        public string iban { get; set; }
    }

    public class AccountDto
    {
        public string resourceId { get; set; }
        public string bicFi { get; set; }
        public string name { get; set; }
        public string usage { get; set; }
        public string cashAccountType { get; set; }
        public string product { get; set; }
        public string currency { get; set; }
        public AccountId accountId { get; set; }
        public string psuStatus { get; set; }
    }
}
