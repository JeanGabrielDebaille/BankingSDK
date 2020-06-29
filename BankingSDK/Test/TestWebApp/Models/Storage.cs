using BankingSDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestWebApp.Controllers;

namespace TestWebApp.Models
{
    public class Storage
    {
        public static IConfiguration Configuration;
        public static BankSettings BankSettings { get; set; }

        public static SdkBaseConnector Connector { get; set; }
        public static string CallbackUrl { get; set; }
        public static string FinalizeUrl { get; set; }
        public static string SCAUrl { get; set; }
    }
}
