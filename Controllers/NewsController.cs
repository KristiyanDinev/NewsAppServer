using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.ComponentModel;
using System.Net.Mail;
using System.Text;

namespace NewsAppServer.Controllers {
    public class NewsController {
        
        public NewsController(WebApplication app) {

            app.MapGet("/news/id/{newsID:int}", async (HttpContext http,
                DatabaseManager db,
                int newsID) => {
                    Dictionary<string, object> res =
                        new Dictionary<string, object>();
                    try {
                        NewsModel? news = await db.GetNewsByID(newsID);
                        res.Add("News", news);
                        return res;
                    } catch (Exception) { 
                        return null;
                    }
                });


            app.MapPost("/news/search", async (HttpContext http, DatabaseManager db,
                [FromForm] string tags, [FromForm] string search, 
                [FromForm] string post_authors, [FromForm] int page,
                [FromForm] int amount) => {

                    Dictionary<string, object> res =
                        new Dictionary<string, object>();
                    
                    try {

                        List<string> tagsList = ControllerUtils
                            .SeperateValues(tags);

                        List<string> post_authorsList = ControllerUtils
                            .SeperateValues(post_authors); ;

                        List<NewsModel> searchedNews = await db.SearchNews(search,
                            tagsList.ToArray(), post_authorsList.ToArray(),
                            page - 1, amount);

                        res.Add("News", searchedNews);
                        return res;

                    } catch (Exception) {
                        return res;
                    }

                    
                }).DisableAntiforgery();


            app.MapPost("/news", async (HttpContext http, DatabaseManager db,
                [FromForm] string AdminUsername, [FromForm] string AdminPassword,
                [FromForm] string Title,
                [FromForm] string HTML_body,
                [FromForm] string Tags,
                [FromForm] string Thumbnail,
                [FromForm] string Attachments) => {

                bool isAdminReq = await ControllerUtils
                    .CheckAdminRequest(AdminUsername, AdminPassword, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }


                NewsModel news = new NewsModel();
                news.Posted_by_Admin_username = AdminUsername;
                news.Title = Title.Trim();
                news.Tags = Tags;
                news.BBCode_body = HTML_body;
                news.HTML_body = BBCode.ConvertToHtml(HTML_body);
                news.Thumbnail_path = await ControllerUtils.UploadThumbnail(Thumbnail, null);
                news.Attachments_path = await ControllerUtils.UploadAttachments(Attachments);

                db.AddNews(news);
                return Results.Ok();

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");


            app.MapPost("/news/edit", async(HttpContext http, DatabaseManager db,
                 [FromForm] string AdminUsername, [FromForm] string AdminPassword,
                 [FromForm] int Id, [FromForm] string Title,
                 [FromForm] string HTML_body,
                 [FromForm] string Tags,
                 [FromForm] string Thumbnail,
                 [FromForm] string NewAttachments,
                 [FromForm] bool DeleteThumbnail,
                 [FromForm] string DeleteAttachments) => {
                
                bool isAdminReq = await ControllerUtils
                     .CheckAdminRequest(AdminUsername, AdminPassword, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }


                NewsModel? dbNewsModel = await db.GetNewsByID(Id);
                if (dbNewsModel == null) {
                    return Results.BadRequest();
                }

                NewsModel newsModel = new NewsModel();

                newsModel.Id = Id;
                newsModel.Title = Title.Trim();
                
                newsModel.HTML_body = BBCode.ConvertToHtml(HTML_body);

                newsModel.Tags = Tags;

                newsModel.BBCode_body = HTML_body;


               newsModel.Thumbnail_path = await ControllerUtils.UploadThumbnail(Thumbnail, 
                    DeleteThumbnail && dbNewsModel.Thumbnail_path != null ? 
                    dbNewsModel.Thumbnail_path : null);


                if (dbNewsModel.Attachments_path != null && DeleteAttachments.Length > 0) {
                        foreach (string p in DeleteAttachments.Split(";")) {
                             if (p.Length == 0) {
                                 continue;
                             }
                             try {
                                 File.Delete(ControllerUtils.wwwrootPath + p);
                                 dbNewsModel.Attachments_path = 
                                 dbNewsModel.Attachments_path.Replace(p+";", "");
                             } catch (Exception) { }
                         
                         }
                     }
                newsModel.Attachments_path = dbNewsModel.Attachments_path;
                newsModel.Attachments_path += await ControllerUtils
                     .UploadAttachments(NewAttachments);

                db.EditNews(newsModel);
                return Results.Ok();

             }).DisableAntiforgery()
             .RequireRateLimiting("fixed");


            app.MapPost("/news/delete", async (HttpContext http, DatabaseManager db,
                [FromForm] string AdminUsername, [FromForm] string AdminPassword,
                 [FromForm] int Id,
                 [FromForm] string Thumbnail,
                 [FromForm] string Attachments) => {

                bool isAdminReq = await ControllerUtils
                     .CheckAdminRequest(AdminUsername, AdminPassword, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }


                if (ControllerUtils.IsRequirementFilled(Thumbnail)) {
                    try { 
                        File.Delete(ControllerUtils.wwwrootPath + Thumbnail);
                    } catch (Exception) { }
                }

                if (ControllerUtils.IsRequirementFilled(Attachments)) {
                    ControllerUtils.DeleteAttachments(Attachments);
                }

                db.RemoveNews(Id);
                return Results.Ok();

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }
    }
}
