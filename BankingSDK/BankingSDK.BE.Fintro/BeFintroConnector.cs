using BankingSDK.Base.BNP;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.Fintro
{
    public class BeFintroConnector : BaseBnpConnector
    {
        public BeFintroConnector(BankSettings settings) : base(settings, new Uri("https://regulatory.api.fintro.be"), new Uri("https://services.fintro.be/SEPLJ04/sps/oauth/oauth20"), ConnectorType.BE_FINTRO)
        {
        }
    }
}
