using BankingSDK.Base.BNP;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.BNP
{
    public class BeBnpConnector : BaseBnpConnector
    {
        public BeBnpConnector(BankSettings settings) : base(settings, new Uri("https://regulatory.api.bnpparibasfortis.be"), new Uri("https://services.bnpparibasfortis.be/SEPLJ04/sps/oauth/oauth20"), ConnectorType.BE_BNP)
        {
        }
    }
}
