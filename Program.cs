using Microsoft.AspNetCore.RateLimiting;
using NewsAppServer.Controllers;
using NewsAppServer.Database;
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

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
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




