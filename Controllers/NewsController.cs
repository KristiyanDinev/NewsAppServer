using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Utils;

namespace NewsAppServer.Controllers {
    public class NewsController {

        public NewsController(WebApplication app) { 
            app.MapGet("/news/{limit:int?}", (HttpContext http, DatabaseManager db, 
                int limit = 0) => {
                    return db.GetNews(limit);
            });

            app.MapPost("/news", (HttpContext http, [FromForm] string bbcode) => {
                Console.WriteLine(BBCode.ConvertToHtml(bbcode));
            }).DisableAntiforgery();
        }
    }
}
