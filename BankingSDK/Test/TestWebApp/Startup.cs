using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BankingSDK.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestWebApp.Models;

namespace TestWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBankingSdk();
            services.AddControllersWithViews();
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Storage.CallbackUrl = Configuration.GetValue<string>("CallbackUrl");
            
            SdkApiSettings.CompanyKey = Configuration.GetValue<string>("SdkCompanyKey");
            SdkApiSettings.ApplicationKey = Configuration.GetValue<string>("SdkApplicationKey");
            SdkApiSettings.Secret = Configuration.GetValue<string>("SdkSecret");
            SdkApiSettings.TppLegalName = Configuration.GetValue<string>("TppLegalName");
            SdkApiSettings.IsSandbox = Configuration.GetValue<bool>("Sandbox");

            DefaultBankSettings defaultBankSettings = new DefaultBankSettings();
            Configuration.GetSection("DefaultBankSettings").Bind(defaultBankSettings);

            X509Certificate2 tlsCertificate = null;
            X509Certificate2 signingCertificate = null;

            if (!string.IsNullOrEmpty(defaultBankSettings.TlsCertificateThumbprint))
            {
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    certStore.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        // Replace below with your certificate's thumbprint
                        defaultBankSettings.TlsCertificateThumbprint,
                        true);
                    // Get the first cert with the thumbprint
                    tlsCertificate = certCollection.OfType<X509Certificate2>().FirstOrDefault();

                    if (tlsCertificate is null)
                    {
                        throw new Exception(
                            $"Certificate with thumbprint {defaultBankSettings.TlsCertificateThumbprint} was not found");
                    }
                }
            }
            else
            {
                tlsCertificate = new X509Certificate2(defaultBankSettings.TlsCertificateName,
                    defaultBankSettings.TlsCertificatePassword);
            }

            if (!string.IsNullOrEmpty(defaultBankSettings.SigningCertificateThumbprint))
            {
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    certStore.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        // Replace below with your certificate's thumbprint
                        defaultBankSettings.SigningCertificateThumbprint,
                        true);
                    // Get the first cert with the thumbprint
                    signingCertificate = certCollection.OfType<X509Certificate2>().FirstOrDefault();

                    if (signingCertificate is null)
                    {
                        throw new Exception(
                            $"Certificate with thumbprint {defaultBankSettings.SigningCertificateThumbprint} was not found");
                    }
                }
            }
            else
            {
                signingCertificate = new X509Certificate2(defaultBankSettings.SigningCertificateName,
                    defaultBankSettings.SigningCertificatePassword);
            }

            Storage.BankSettings = new BankSettings
            {
                NcaId = defaultBankSettings.NcaId,
                TlsCertificate = tlsCertificate,
                SigningCertificate = signingCertificate,
                AppClientId = defaultBankSettings.AppClientId,
                AppClientSecret = defaultBankSettings.AppClientSecret,
                PemFileUrl = defaultBankSettings.PemFileUrl
            };

            // if (env.IsDevelopment())
            // {
                app.UseDeveloperExceptionPage();
            // }
            // else
            // {
                // app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            // }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
