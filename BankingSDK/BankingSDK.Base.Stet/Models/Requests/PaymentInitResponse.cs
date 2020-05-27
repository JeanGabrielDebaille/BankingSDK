using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.Stet.Models.Requests
{
    public class PaymentInitResponse
    {
        public Links _links { get; set; }
        public string appliedAuthenticationApproach { get; set; }
    }
    public class ConsentApproval
    {
        public string href { get; set; }
    }

    public class Links
    {
        public ConsentApproval consentApproval { get; set; }
    }
}
