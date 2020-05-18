using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.AT.ING
{
    public class AtIngConnector : BaseIngConnector
    {
        public AtIngConnector(BankSettings settings) : base(settings, "AT", ConnectorType.AT_ING)
        {
        }
    }
}
