using BankingSDK.Base.KBC;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.CBC
{
    public class BeCbcConnector : BaseKbcConnector
    {
        public BeCbcConnector(BankSettings settings) : base(settings, ConnectorType.BE_CBC)
        {
        }
    }
}
