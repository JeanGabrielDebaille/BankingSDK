using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.Nagelmackers
{
    public class BeNagelmackersConnector : BaseBerlinGroupConnector
    {
        public BeNagelmackersConnector(BankSettings settings) : base(settings, "https://openbankingsandbox.nagelmackers.be", "https://openbankingapi.nagelmackers.be", ConnectorType.BE_Nagelmackers)
        {
        }
    }
}
