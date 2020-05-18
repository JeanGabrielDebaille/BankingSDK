using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.AXA
{
    public class BeAxaConnector : BaseBerlinGroupConnector
    {
        public BeAxaConnector(BankSettings settings) : base(settings, "https://api-dailybanking-sandbox.axabank.be", "https://api-dailybanking.axabank.be", ConnectorType.BE_AXA)
        {
        }
    }
}
