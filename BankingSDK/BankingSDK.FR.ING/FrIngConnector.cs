using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.FR.ING
{
    public class FrIngConnector : BaseIngConnector
    {
        public FrIngConnector(BankSettings settings) : base(settings, "FR", ConnectorType.FR_ING)
        {
        }
    }
}
