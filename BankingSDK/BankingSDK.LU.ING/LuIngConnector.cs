using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.LU.ING
{
    public class LuIngConnector : BaseIngConnector
    {
        public LuIngConnector(BankSettings settings) : base(settings, "LU", ConnectorType.LU_ING)
        {
        }
    }
}
