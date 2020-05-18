using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.ING
{
    public class BeIngConnector : BaseIngConnector
    {
        public BeIngConnector(BankSettings settings) : base(settings, "BE", ConnectorType.BE_ING)
        {
        }
    }
}
