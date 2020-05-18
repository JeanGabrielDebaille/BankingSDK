using BankingSDK.Base.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.NL.ING
{
    public class NlIngConnector : BaseIngConnector
    {
        public NlIngConnector(BankSettings settings) : base(settings, "NL", ConnectorType.NL_ING)
        {
        }
    }
}
