using System.Collections.Generic;

namespace BankingSDK.Base.BerlinGroup.Models.Requests
{
    public class BerlinGroupAccountAccessRequest
    {
        public BerlinGroupAccess access { get; set; }
        public bool combinedServiceIndicator { get; set; }
        public int frequencyPerDay { get; set; }
        public bool recurringIndicator { get; set; }
        public string validUntil { get; set; }
    }

    public class BerlinGroupAccess
    {
        public string allPsd2 { get; set; }
        public List<BerlinGroupAccountIban> balances { get; set; }
        public List<BerlinGroupAccountIban> transactions { get; set; }
    }


    public class BerlinGroupAccountIban
    {
        public string iban { get; set; }
    }
}
