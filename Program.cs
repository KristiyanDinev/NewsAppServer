using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NewsAppServer.Controllers;
using NewsAppServer.Database;
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
            // 1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3
            // INSERT INTO Admins VALUES ('1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3');

            if (!File.Exists("wwwroot/")) {
                Directory.CreateDirectory("wwwroot");
                Directory.CreateDirectory("wwwroot\\pdf");
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

           var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            //app.UseHsts();
            app.UseRateLimiter();
            

            DatabaseManager._connectionString += 
                app.Configuration.GetValue<string>("SQLite_Location") 
                    ?? "database.sqlite";
            DatabaseManager.Setup();

            new NewsController(app);
            new AdminController(app);

            app.Run();
        }
    }
}




