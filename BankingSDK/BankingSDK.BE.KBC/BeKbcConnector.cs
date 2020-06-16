using BankingSDK.Base.KBC;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.KBC
{
    public class BeKbcConnector : BaseKbcConnector
    {
        public BeKbcConnector(BankSettings settings) : base(settings, ConnectorType.BE_KBC)
        {
        }
    }
}
