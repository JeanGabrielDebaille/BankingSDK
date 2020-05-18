using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.ES.ING
{
    public class EsIngConnector : BaseIngConnector
    {
        public EsIngConnector(BankSettings settings) : base(settings, "ES", ConnectorType.ES_ING)
        {
        }
    }
}
