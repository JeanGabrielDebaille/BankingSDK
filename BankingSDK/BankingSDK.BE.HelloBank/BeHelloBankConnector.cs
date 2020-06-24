using BankingSDK.Base.BNP;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.HelloBank
{
    public class BeHelloBankConnector : BaseBnpConnector
    {
        public BeHelloBankConnector(BankSettings settings) : base(settings, new Uri("https://regulatory.api.hellobank.be"), new Uri("https://services.hellobank.be/SEPLJ04/sps/oauth/oauth20"), ConnectorType.BE_HELLO_BANK)
        {
        }
    }
}
