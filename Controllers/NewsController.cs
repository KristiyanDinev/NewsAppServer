using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;

namespace NewsAppServer.Controllers {
    public class NewsController {
        
        public NewsController(WebApplication app) {
            /*
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
                        return res;
                    }
                }).RequireRateLimiting("fixed");*/

            app.MapPost("/news/latest", async (HttpContext http,
                DatabaseManager db, [FromForm] int page,
                [FromForm] int amount) => {

                    Dictionary<string, object> res =
                        new Dictionary<string, object>();
                    page -= 1;
                    try {
                        List<NewsModel> news = await db.GetLatestNews(page, amount);

                        ISession session = http.Session;
                        ControllerUtils.Add_News_To_SearchResults(ref session,
                            news);

                        await session.CommitAsync();


                        res.Add("News", news);
                        return res;

                    } catch (Exception) {
                        return null;
                    }
                }).DisableAntiforgery()
                .RequireRateLimiting("fixed");


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
                            .SeperateValues(post_authors);

                        List<NewsModel> searchedNews = await db.SearchNews(search,
                            tagsList.ToArray(), post_authorsList.ToArray(),
                            page - 1, amount);

                        // is_fav
                        ISession session = http.Session;
                        List<NewsModel> savedNews =
                        ControllerUtils.Get_NewsModels_From_SavedNewsSession(session);

                        foreach (NewsModel news in searchedNews) {
                            if (savedNews.Find(n => n.Id == news.Id) != null) {
                                news.IsFav = true;
                            }
                        }

                        ControllerUtils.Add_News_To_SearchResults(ref session,
                            searchedNews);

                        await session.CommitAsync();

                        res.Add("News", searchedNews);
                        return res;

                    } catch (Exception) {
                        return res;
                    }

                    
                }).DisableAntiforgery()
                .RequireRateLimiting("fixed");


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

                     bool toDel = DeleteThumbnail && dbNewsModel.Thumbnail_path != null;

                    if (toDel || Thumbnail.Length > 0) {
                         newsModel.Thumbnail_path = await ControllerUtils
                            .UploadThumbnail(Thumbnail,
                                toDel ? dbNewsModel.Thumbnail_path : null);
                     } else {
                         newsModel.Thumbnail_path = dbNewsModel.Thumbnail_path;
                     }


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
                 string? attachPath = await ControllerUtils
                      .UploadAttachments(NewAttachments);
                     if (attachPath != null && newsModel.Attachments_path != null) {
                         newsModel.Attachments_path += attachPath;

                     } else if (attachPath != null
                     && newsModel.Attachments_path == null) {
                         newsModel.Attachments_path = attachPath;

                     }

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
