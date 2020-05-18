using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.PL.ING
{
    public class PlIngConnector : BaseIngConnector
    {
        public PlIngConnector(BankSettings settings) : base(settings, "PL", ConnectorType.PL_ING)
        {
        }
    }
}
