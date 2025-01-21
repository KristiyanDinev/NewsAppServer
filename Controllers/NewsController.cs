using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.ComponentModel;
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
                    //IFormCollection form = await http.Request.ReadFormAsync();
                    Dictionary<string, object> res =
                        new Dictionary<string, object>();
                    
                    try {/*
                        string? search = form["search"];

                        List<string> tags = ControllerUtils
                            .SeperateValues(form["tags"]);

                        List<string> post_authors = ControllerUtils
                            .SeperateValues(form["post_authors"]);;

                        List<NewsModel> searchedNews = await db.SearchNews(search, 
                            tags.ToArray(), post_authors.ToArray(), 
                            int.Parse(form["page"]) - 1, int.Parse(form["amount"]));*/

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


            app.MapPost("/news", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                bool isAdminReq = await ControllerUtils.CheckAdminRequest(form, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }

                if (!(form.ContainsKey("Title") && form.ContainsKey("Tags") &&
                    form.ContainsKey("HTML_body") &&
                    form.ContainsKey("Thumbnail") &&
                    form.ContainsKey("PDFs"))) {
                    return Results.BadRequest();
                }

                string? title = form["Title"];
                string? html_body = form["HTML_body"];

                if (!ControllerUtils.IsRequirementFilled(title) || 
                    !ControllerUtils.IsRequirementFilled(html_body)) {
                    return Results.BadRequest();
                }

                NewsModel news = new NewsModel();
                // ----------
                news.Posted_by_Admin_username = form["AdminUsername"];
                // ----------
                news.Title = title.Trim();
                // -----------
                string? tags = form["Tags"];
                news.Tags = tags;
                // -----------
                news.HTML_body = BBCode.ConvertToHtml(html_body);

                // ------------
                string? thumbnail = form["Thumbnail"];
                if (thumbnail != null) {
                    news.Thumbnail_path = await ControllerUtils.UploadThumbnail(thumbnail, null);
                }

                // ------------

                string? pdfs = form["PDFs"];
                if (pdfs != null && pdfs.Length > 0) {
                    news.PDF_path = await ControllerUtils.UploadPDFs(pdfs);
                }
                
                db.AddNews(news);
                return Results.Ok();

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            
            app.MapPost("/news/edit", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                bool isAdminReq = await ControllerUtils.CheckAdminRequest(form, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }

                if (!(form.ContainsKey("Id") && form.ContainsKey("Title") &&
                     form.ContainsKey("PDFs") && form.ContainsKey("Thumbnail")
                     && form.ContainsKey("Tags") && form.ContainsKey("HTML_body"))) {
                    return Results.BadRequest();
                }

                if (!int.TryParse(form["Id"], out int Id)) {
                    return Results.BadRequest();
                }

                NewsModel? dbNewsModel = await db.GetNewsByID(Id);
                if (dbNewsModel == null) {
                    return Results.BadRequest();
                }

                NewsModel newsModel = new NewsModel();


                string? Title = form["Title"];
                if (Title == null) {
                    return Results.BadRequest();
                }
                Title = Title.Trim();
                if (!dbNewsModel.Title.Equals(Title)) {
                    // Change Title
                    newsModel.Title = Title;
                }

                string? HTML_body = form["HTML_body"];
                if (HTML_body == null) {
                    return Results.BadRequest();
                }
                HTML_body = BBCode.ConvertToHtml(HTML_body);
                if (!dbNewsModel.HTML_body.Equals(HTML_body)) {
                    // Change HTML body
                    newsModel.HTML_body = HTML_body;
                }

                newsModel.Tags = form["Tags"];


                string? thumbnail = form["Thumbnail"];
                // dbNewsModel.Thumbnail_path -> endpoint path
                // thumbnail -> either same endpoint path or new data
                if (dbNewsModel.Thumbnail_path != thumbnail) {
                    if ((thumbnail == null || thumbnail.Length == 0) && 
                        (dbNewsModel.Thumbnail_path != null && 
                            dbNewsModel.Thumbnail_path.Length > 0)) {
                        try {
                            File.Delete(ControllerUtils.wwwrootPath + dbNewsModel.Thumbnail_path);
                        } catch (Exception) { }
                        newsModel.Thumbnail_path = null;

                    } else if (thumbnail != null && !(thumbnail.StartsWith(';') ||
                            thumbnail.EndsWith(';'))) {

                        string? path = await ControllerUtils.UploadThumbnail(thumbnail, dbNewsModel.Thumbnail_path);
                        newsModel.Thumbnail_path = path != null ? path : dbNewsModel.Thumbnail_path;

                    } else {
                        newsModel.Thumbnail_path = dbNewsModel.Thumbnail_path;
                    }
                }

                

                string? pdfsStr = form["PDFs"];
                if (dbNewsModel.PDF_path != pdfsStr) {
                    if ((pdfsStr == null || pdfsStr.Length == 0) &&
                        (dbNewsModel.PDF_path != null && dbNewsModel.PDF_path.Length > 0)) {
                        // delete all pdfs in  dbNewsModel.PDF_path
                        ControllerUtils.DeletePDFs(dbNewsModel.PDF_path);
                        newsModel.PDF_path = null;

                    } else if (pdfsStr != null && pdfsStr.Length > 0) {
                        // update PDFs
                        string? pdfPath = await ControllerUtils.UploadPDFs(pdfsStr);
                        newsModel.PDF_path = pdfPath != null ? pdfPath :
                            dbNewsModel.PDF_path;

                    }
                }

                db.EditNews(newsModel);
                return Results.Ok();

             }).DisableAntiforgery()
             .RequireRateLimiting("fixed");


            app.MapPost("/news/delete", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                bool isAdminReq = await ControllerUtils.CheckAdminRequest(form, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }

                if (!(form.ContainsKey("Id") && 
                    form.ContainsKey("Thumbnail") &&
                    form.ContainsKey("PDFs"))) {
                    return Results.BadRequest();
                }

                if (!int.TryParse(form["Id"], out int Id)) {
                    return Results.BadRequest();
                }

                string? thumbnail_path = form["Thumbnail"];
                if (ControllerUtils.IsRequirementFilled(thumbnail_path)) {
                    try { 
                        File.Delete(ControllerUtils.wwwrootPath + thumbnail_path);
                    } catch (Exception) { }
                }

                string? pdfs_path = form["PDFs"];
                if (ControllerUtils.IsRequirementFilled(pdfs_path)) {
                    ControllerUtils.DeletePDFs(pdfs_path);
                }

                db.RemoveNews(Id);
                return Results.Ok();
            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }
    }
}
