using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.BerlinGroup.Models
{
    public class BerlinGroupAccountsDto
    {
        public List<BerlinGroupAccountDto> accounts { get; set; }
    }

    public class BerlinGroupAccountDto
    {
        public string resourceId { get; set; }
        public string name { get; set; }
        public string accountType { get; set; }
        public string bic { get; set; }
        public string currency { get; set; }
        public string cashAccountType { get; set; }
        public string iban { get; set; }
        public string bban { get; set; }
        public string msisdn { get; set; }
        public string maskedPan { get; set; }
    }
}
