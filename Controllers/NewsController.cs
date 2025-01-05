using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;

namespace NewsAppServer.Controllers {
    public class NewsController {

        public NewsController(WebApplication app) {
            // [FromForm] string bbcode

            app.MapGet("/news/{page:int}/{amount:int}", (HttpContext http, DatabaseManager db, 
                int page, int amount) => {
                    amount += 1;
                    return db.GetNews(page, amount);
            });

            app.MapPost("/news", (HttpContext http, [FromForm] News news) => {
                // BBCode.ConvertToHtml(bbcode)
            }).DisableAntiforgery();


            app.MapPost("/edit/news", (HttpContext http, [FromForm] News news) => {
                Console.WriteLine(news.Id);
                Console.WriteLine(news.Title);
            }).DisableAntiforgery();

            app.MapPost("/delete/news", (HttpContext http, [FromForm] int newsID) => {
                Console.WriteLine(newsID);
            }).DisableAntiforgery();
        }
    }
}
