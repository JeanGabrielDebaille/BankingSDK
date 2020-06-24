using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.Crelan
{
    public class BeCrelanConnector : BaseBerlinGroupConnector
    {
        public BeCrelanConnector(BankSettings settings) : base(settings, "https://api-sandbox.crelan.be", "https://api.crelan.be", ConnectorType.BE_CRELAN)
        {
        }
    }
}
