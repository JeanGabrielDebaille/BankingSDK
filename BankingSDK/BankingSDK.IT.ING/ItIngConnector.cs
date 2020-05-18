using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.IT.ING
{
    public class ItIngConnector : BaseIngConnector
    {
        public ItIngConnector(BankSettings settings) : base(settings, "IT", ConnectorType.IT_ING)
        {
        }
    }
}
