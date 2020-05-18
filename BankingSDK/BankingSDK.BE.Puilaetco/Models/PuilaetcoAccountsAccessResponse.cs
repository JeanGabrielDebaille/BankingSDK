using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.BE.Puilaetco.Models
{
    internal class PuilaetcoAccountsAccessResponse
    {
        public string consentStatus { get; set; }
        public string consentId { get; set; }
        public PuilaetcoLinks _links { get; set; }
    }

    internal class PuilaetcoLinks
    {
        public string scaRedirect { get; set; }
        public string scaOAuth { get; set; }
        public string startAuthorisation { get; set; }
        public string scaStatus { get; set; }
        public string status { get; set; }
        public string self { get; set; }
    }

    internal class PuilaetcoAuthorizationDto
    {
        public string scaStatus { get; set; }
        public PuilaetcoLinks _links { get; set; }
    }
}
