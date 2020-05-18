using BankingSDK.Base.BerlinGroup;
using BankingSDK.Common;
using BankingSDK.Common.Enums;

namespace BankingSDK.BE.EuropaBank
{
    public class BeEuropaBankConnector : BaseBerlinGroupConnector
    {
        public BeEuropaBankConnector(BankSettings settings) : base(settings, "https://sandbox-api.tpp.europabank.be", "https://api.tpp.europabank.be", ConnectorType.BE_EuropaBank)
        {
        }
    }
}
