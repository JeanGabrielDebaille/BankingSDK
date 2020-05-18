using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.DE.ING
{
    public class DeIngConnector : BaseIngConnector
    {
        public DeIngConnector(BankSettings settings) : base(settings, "DE", ConnectorType.DE_ING)
        {
        }
    }
}
