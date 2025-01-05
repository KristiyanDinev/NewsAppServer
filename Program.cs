using NewsAppServer.Controllers;
using NewsAppServer.Database;

namespace NewsAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Microsoft.Data.Sqlite.Core
            // SQLite
            // SQLitePCLRaw.core

            DatabaseManager.Setup();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<DatabaseManager>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            new NewsController(app);
            new AdminController(app);

            app.Run();
        }
    }
}




