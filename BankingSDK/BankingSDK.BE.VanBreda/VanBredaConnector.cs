using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.VanBreda
{
    public class BeVanBredaConnector : BaseBerlinGroupConnector
    {
        public BeVanBredaConnector(BankSettings settings) : base(settings, "https://xs2a-sandbox.bankvanbreda.be", "https://xs2a-api.bankvanbreda.be", ConnectorType.BE_VanBreda)
        {
        }
    }
}
