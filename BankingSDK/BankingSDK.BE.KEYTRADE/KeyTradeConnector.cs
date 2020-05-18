using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.KEYTRADE
{
    public class BeKeyTradeConnector : BaseBerlinGroupConnector
    {
        public BeKeyTradeConnector(BankSettings settings) : base(settings, "https://psd2.api.sandbox.keytradebank.be", "https://psd2.api.keytradebank.be", ConnectorType.BE_KeyTrade)
        {
        }
    }
}
