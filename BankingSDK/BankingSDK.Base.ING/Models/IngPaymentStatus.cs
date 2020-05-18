using BankingSDK.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingSDK.Base.ING.Models
{
    internal class IngPaymentStatus
    {
        public PaymentSatusISO20022 transactionStatus { get; set; }
    }
}
