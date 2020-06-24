using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.Bpost
{
    public class BeBpostConnector : BaseBerlinGroupConnector
    {
        public BeBpostConnector(BankSettings settings) : base(settings, "https://sandbox.psd2.bpostbank.be", "https://api.psd2.bpostbank.be", ConnectorType.BE_BPOST)
        {
        }
    }
}
