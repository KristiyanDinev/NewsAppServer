using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NewsAppServer.Controllers;
using NewsAppServer.Database;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;

namespace NewsAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Microsoft.Data.Sqlite.Core
            // Microsoft.Data.Sqlite
            // SQLite
            // UybRuyibINbvcyrteTYCRTUVYIugcxtETYCRTUVigYCYR

            if (!File.Exists("wwwroot/")) {
                Directory.CreateDirectory("wwwroot");
                Directory.CreateDirectory("wwwroot\\attachment");
                Directory.CreateDirectory("wwwroot\\thumbnail");
            }


            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<DatabaseManager>();
            builder.Services.AddRateLimiter(_ => _
                .AddFixedWindowLimiter(policyName: "fixed", options => {
                    options.PermitLimit = 4;
                    options.Window = TimeSpan.FromSeconds(12);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                })
            );

            builder.WebHost.UseUrls([builder.Configuration.GetValue<string>("Urls") ?? "http://127.0.0.1:5000"]);

            /*
            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = builder.Configuration.GetValue<int>("https_port");
                options.RedirectStatusCode = 307;
            });

            builder.Services.AddAuthentication(
                CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate();

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.ConfigureHttpsDefaults(options =>
                    options.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
            });

            builder.WebHost.ConfigureKestrel(options =>
                options.ConfigureEndpointDefaults(listenOptions =>
                    listenOptions.UseHttps(new HttpsConnectionAdapterOptions {
                        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                        ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                        ServerCertificate = new X509Certificate2("public_privatekey.pfx", "newsapppassword")

                    })));

            builder.WebHost
                .UseUrls()
                .UseKestrel();
            
             "https_port": 5001,
  "Kestrel": {
    "EndPoints": {
      "Https": {
        "Url": "https://192.168.1.13:5001",
        "Certificate": {
          "Path": "public_privatekey.pfx",
          "Password": "newsapppassword"
        }
      }
    }
  }
             */

            var app = builder.Build();

            //app.UseHttpsRedirection();
            //app.UseAuthentication();
            //app.UseHsts();
            app.UseStaticFiles();
            app.UseRateLimiter();


            DatabaseManager._connectionString += 
                app.Configuration.GetValue<string>("SQLite_Location") 
                    ?? "database.sqlite";
            DatabaseManager._sysadmin_password += app.Configuration
                .GetValue<string>("Database_SystemAdmin_Password")
                    ?? "UybRuyibINbvcyrteTYCRTUVYIugcxtETYCRTUVigYCYR";
            DatabaseManager.Setup();

            new NewsController(app);
            new AdminController(app);

            app.Run();
        }
    }
}




