using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.RO.ING
{
    public class RoIngConnector : BaseIngConnector
    {
        public RoIngConnector(BankSettings settings) : base(settings, "RO", ConnectorType.RO_ING)
        {
        }
    }
}
