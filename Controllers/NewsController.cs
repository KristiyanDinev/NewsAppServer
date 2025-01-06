using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;

namespace NewsAppServer.Controllers {
    public class NewsController {
        public NewsController(WebApplication app) {

            string thumbnailFileLocation = app.Configuration.GetValue<string>("Thumbnails_Location") 
                ?? Directory.GetCurrentDirectory() + "\\thumbnail\\";
            string pdfFileLocation = app.Configuration.GetValue<string>("PDFs_Location") 
                ?? Directory.GetCurrentDirectory() + "\\pdf\\";

            string pdfEndpoint = pdfFileLocation.Split("wwwroot").Last();
            string thumbnailEndpoint = thumbnailFileLocation.Split("wwwroot").Last();

            app.MapGet("/news/{page:int}/{amount:int}", async (HttpContext http, 
                DatabaseManager db, 
                int page, int amount) => {
                    
                    page -= 1;
                    try {
                        List<NewsModel> news = await db.GetNews(page, amount);
                        return news;
                    } catch (Exception e) { 
                        return new List<NewsModel>();
                    }
                    
            });


            app.MapGet("/news/id/{newsID:int}", async (HttpContext http,
                DatabaseManager db,
                int newsID) => {

                    try {
                        NewsModel? news = await db.GetNewsByID(newsID);
                        return news;
                    } catch (Exception e) { 
                        return null;
                    }
                    

                });


            app.MapPost("/news", (HttpContext http, DatabaseManager db,
                [FromForm] NewsForm news) => {
                    if (news.HTML_body.Length == 0 || news.Title.Length == 0) {
                        return Results.BadRequest();
                    }
                    try {
                        news.HTML_body = BBCode.ConvertToHtml(news.HTML_body);

                        foreach (IFormFile file in news.Files) {
                            if (file.FileName.ToLower().EndsWith(".pdf")) {
                                UploadFile(file, pdfFileLocation);
                                news.PDF_path += pdfEndpoint + file.FileName + ";";

                            } else if (file.FileName.EndsWith(".png") || file.FileName.EndsWith(".jpeg") ||
                                    file.FileName.EndsWith(".jpg") || file.FileName.EndsWith(".apng") ||
                                    file.FileName.EndsWith(".svg")) {
                                UploadFile(file, thumbnailFileLocation);
                                news.Thumbnail_path = thumbnailEndpoint + file.FileName;
                            }

                        }

                        db.AddNews(news);

                    } catch (Exception ex) {
                        return Results.BadRequest();
                    }
                    return Results.Ok();

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");


            app.MapPost("/edit/news", (HttpContext http, DatabaseManager db, 
                [FromForm] NewsForm news) => {

                    if (news.HTML_body.Length == 0 || news.Title.Length == 0) {
                        return Results.BadRequest();
                    }

                    try {
                        db.EditNews(news);
                    } catch (Exception ex) { 
                        return Results.BadRequest();
                    }
                        return Results.Ok();

                }).DisableAntiforgery()
                .RequireRateLimiting("fixed");

            app.MapPost("/delete/news", (HttpContext http, DatabaseManager db, 
                [FromForm] int newsID) => {

                try {
                    db.RemoveNews(newsID);
                } catch (Exception ex) {
                    return Results.BadRequest();
                }
                return Results.Ok();


            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }


        private async void UploadFile(IFormFile file, string location) {
            using FileStream fs = File.Create(location + file.FileName);
            await file.CopyToAsync(fs);
        }
    }
}
