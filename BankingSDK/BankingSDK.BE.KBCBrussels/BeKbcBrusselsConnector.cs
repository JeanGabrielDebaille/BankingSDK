using BankingSDK.Base.KBC;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.BE.KBCBrussels
{
    public class BeKbcBrusselsConnector : BaseKbcConnector
    {
        public BeKbcBrusselsConnector(BankSettings settings) : base(settings, ConnectorType.BE_KBC_BRUSSELS)
        {
        }
    }
}
