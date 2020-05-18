using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.CZ.ING
{
    public class CzIngConnector : BaseIngConnector
    {
        public CzIngConnector(BankSettings settings) : base(settings, "CZ", ConnectorType.CZ_ING)
        {
        }
    }
}
