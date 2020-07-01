using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class DefaultBankSettings
    {
        public string NcaId { get; set; }
        public string AppClientId { get; set; }
        public string AppClientSecret { get; set; }
        public string TlsCertificateName { get; set; }
        public string TlsCertificateThumbprint { get; set; }
        public string TlsCertificatePassword { get; set; }
        public string SigningCertificateName { get; set; }
        public string SigningCertificateThumbprint { get; set; }
        public string SigningCertificatePassword { get; set; }
        public string PemFileUrl { get; set; }
    }
}
