using BankingSDK.Base.Stet;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using System;

namespace BankingSDK.FR.CreeditAgricole
{
    public class FrCreditAgricole : BaseStetConnector
    {
        public FrCreditAgricole(BankSettings settings) : base(settings,
            new Uri("https://api.credit-agricole.fr/dsp2/v1"), 
            new Uri("https://sandbox-api.credit-agricole.fr/dsp2/v1"),
            ConnectorType.FR_ING)
        {
        }
    }
}
