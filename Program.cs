using Microsoft.AspNetCore.Builder;
using NewsAppServer.Controllers;

namespace NewsAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            new NewsController(app);
            new AdminController(app);

            app.Run();
        }
    }
}
