using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;

namespace NewsAppServer.Controllers {
    public class NewsController {

        public NewsController(WebApplication app) {
            // [FromForm] string bbcode

            app.MapGet("/news/{page:int}/{amount:int}", async (HttpContext http, 
                DatabaseManager db, 
                int page, int amount) => {
                    
                    page -= 1;
                    List<News> news = await db.GetNews(page, amount);
                    return news;
                    
            });

            app.MapPost("/news", (HttpContext http, DatabaseManager db, 
                [FromForm] News news) => {
                    // BBCode.ConvertToHtml(bbcode)
                    try {
                        news.HTML_body = BBCode.ConvertToHtml(news.HTML_body);
                        db.AddNews(news);
                    } catch (Exception ex) {
                        return Results.BadRequest();
                    }
                    return Results.Ok();
            }).DisableAntiforgery();


            app.MapPost("/edit/news", (HttpContext http, DatabaseManager db, 
                [FromForm] News news) => {

                    try {
                        db.EditNews(news);
                    } catch (Exception ex) { 
                        return Results.BadRequest();
                    }
                        return Results.Ok();

                }).DisableAntiforgery();

            app.MapPost("/delete/news", (HttpContext http, DatabaseManager db, 
                [FromForm] int newsID) => {

                try {
                    db.RemoveNews(newsID);
                } catch (Exception ex) {
                    return Results.BadRequest();
                }
                return Results.Ok();


            }).DisableAntiforgery();
        }
    }
}
