using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using BankingSDK;
using BankingSDK.Base;
using BankingSDK.BE.KBC;
using BankingSDK.BE.ING;
using BankingSDK.Common;
using BankingSDK.Common.Enums;
using BankingSDK.Common.Models;
using BankingSDK.Common.Models.Request;
using Microsoft.Extensions.DependencyInjection;
using BankingSDK.Common.Interfaces.Contexts;
using System.Collections.Generic;
using BankingSDK.Common.Models.Data;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddBankingSdk();

            // Setting general settings of BankingSDk
            BankSettings generalBankSettings = new BankSettings();
            generalBankSettings.NcaId = "VALID_NCA_ID";
            generalBankSettings.TlsCertificate = new X509Certificate2("ING-eidas_tls.pfx", "bankingsdk");
            generalBankSettings.SigningCertificate = new X509Certificate2("ING-eidas_signing.pfx", "bankingsdk");


            SdkApiSettings.CompanyKey = "f954a771-509b-43c9-a064-0a3f9c9bb9d3";
            SdkApiSettings.ApplicationKey = "4d972ae0-13cf-44ad-a20a-9ff44b884ce7";
            SdkApiSettings.Secret = "MMH74H6OCI5CHPX1OS56PI6HSEGAVZ9PU8V8KTXL87ADBB23VJX1OFDDPAOZA53MIPRI4ZQ54T8SI2FRLMPMCIBP5RSKE7LCEX5WTU952164HV5W46NYQQKAOCMNHF07I5NU7T71NTTKLHHJW5QHUWWXN4HQHWYWB9VLHKZK9WL669ZH5D0LZSMVI0HCLQT157JLV5CLGFI7L00OFAVE5N7SW9CRDW9SCUA9ZNV9APEUJPKS936K5P5SA1YV3VIT";
            SdkApiSettings.TppLegalName = "EXTHAND";
            SdkApiSettings.IsSandbox = true;

            BankingSDK.BE.ING.BeIngConnector bankConnector = new BeIngConnector(generalBankSettings);
            //BankingSDK.BE.KBC.BeKbcConnector bankConnector = new BeKbcConnector(generalBankSettings);

            string userId = Guid.NewGuid().ToString();
            string userContect = (bankConnector.RegisterUserAsync(userId).Result).GetData().ToJson();

            string callBackUrl = "https://developer.bankingsdk.com/callback";

            AccountsAccessRequest accountsAccessRequest = new AccountsAccessRequest {
                FlowId = SdkApiSettings.ApplicationKey,
                FrequencyPerDay = 4,
                RedirectUrl = callBackUrl,
                PsuIp ="127.0.0.1", 
                SingleAccount= "BE91732047678076"
            };

            BankingResult<string> bankingResult = bankConnector.RequestAccountsAccessAsync(accountsAccessRequest).Result;

            if (bankingResult.GetStatus() == ResultStatus.REDIRECT)
            {
                // We get the flow context.
                var flowContext = bankingResult.GetFlowContext();
                string redirectUrlOnTheBank = bankingResult.GetData();

                var psi = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe");
                psi.Arguments = redirectUrlOnTheBank;
                Process.Start(psi);


                Console.WriteLine("QueryString received?");
                string queryString = Console.ReadLine();

                BankingResult<IUserContext> result = bankConnector.RequestAccountsAccessFinalizeAsync(flowContext,queryString).Result;

                if(result.GetStatus() == ResultStatus.DONE)
                {
                    Console.WriteLine("Ok. Cool.");

                    BankingResult<List<Account>> accounts = bankConnector.GetAccountsAsync().Result;
                    foreach(Account account in accounts.GetData())
                    {
                        Console.WriteLine("Account " + account.Iban);

                        BankingResult<List<Balance>> resultBalances = bankConnector.GetBalancesAsync(account.Iban).Result;
                        if (resultBalances.GetStatus() == ResultStatus.DONE)
                        {
                            List<Balance> accountBalances = resultBalances.GetData();
                            foreach(Balance balance in accountBalances)
                            {
                                Console.WriteLine("  Balance : " + balance.BalanceAmount + " " + balance.ReferenceDate?.ToString("dd MMM yyyy"));
                            }
                        }
                    }
                }

            }
            Console.ReadLine();
            return;
        }
    }
}
