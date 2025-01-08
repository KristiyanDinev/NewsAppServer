using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.ComponentModel;

namespace NewsAppServer.Controllers {
    public class NewsController {
        private readonly string wwwrootPath = "wwwroot";
        public NewsController(WebApplication app) {
            string thumbnailFileLocation = wwwrootPath + "\\thumbnail\\";
            string pdfFileLocation = wwwrootPath + "\\pdf\\";

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

                    if (news.Tags != null && news.Tags.Length == 0) {
                        news.Tags = null;
                    }

                    try {
                        news.HTML_body = BBCode.ConvertToHtml(news.HTML_body);

                        foreach (IFormFile file in news.Files) {
                            string FileName = file.FileName.ToLower();
                            if (FileName.Contains("\\") || FileName.Contains("/")) {
                                return Results.BadRequest();
                            }
                            if (FileName.EndsWith(".pdf")) {
                                UploadFile(file, pdfFileLocation + file.FileName);
                                news.PDF_path ??= "";
                                news.PDF_path += pdfEndpoint + file.FileName + ";";

                            } else if (FileName.EndsWith(".png") || FileName.EndsWith(".jpeg") ||
                                    FileName.EndsWith(".jpg") || FileName.EndsWith(".apng") ||
                                    FileName.EndsWith(".svg")) {
                                UploadFile(file, thumbnailFileLocation + file.FileName);
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

                    if (news.Tags != null && news.Tags.Length == 0) {
                        news.Tags = null;
                    }

                    try {

                        news.HTML_body = BBCode.ConvertToHtml(news.HTML_body);

                        foreach (IFormFile file in news.Files) {
                            string FileName = file.FileName.ToLower();
                            if (FileName.Contains("\\") || FileName.Contains("/")) { 
                                return Results.BadRequest();
                            }

                            if (FileName.EndsWith(".pdf")) {
                                UpdateFiles(news, file, 
                                    pdfFileLocation, pdfEndpoint, true);

                            } else if (FileName.EndsWith(".png") || FileName.EndsWith(".jpeg") ||
                                    FileName.EndsWith(".jpg") || FileName.EndsWith(".apng") ||
                                    FileName.EndsWith(".svg")) {
                                UpdateFiles(news, file, 
                                    thumbnailFileLocation, 
                                    thumbnailEndpoint, false);
                                
                            }
                        }

                        string thumbnailDeletePath = wwwrootPath + news.Thumbnail_path;
                        if (news.DeleteThumbnail && File.Exists(thumbnailDeletePath)) {
                            File.Delete(thumbnailDeletePath);
                            news.Thumbnail_path = "";
                        }

                        if (news.DeletePDFs.Length > 0 && news.PDF_path != null) {
                            foreach (string filePath in news.DeletePDFs.Split(';')) { 
                                if (filePath.Length == 0) {
                                    continue;
                                }
                            
                                string fullFilePath = wwwrootPath + filePath;
                                if (File.Exists(fullFilePath)) {
                                    File.Delete(fullFilePath);
                                }
                                news.PDF_path = news.PDF_path.Replace(filePath+";", "");
                            }
                        }

                        db.EditNews(news);

                    } catch (Exception ex) { 
                        return Results.BadRequest();
                    }
                        return Results.Ok();

                }).DisableAntiforgery()
                .RequireRateLimiting("fixed");


            app.MapPost("/delete/news", (HttpContext http, DatabaseManager db, 
                [FromForm] NewsForm news) => {

                try {

                    if (news.PDF_path != null) {

                            foreach (string pdf in news.PDF_path.Split(";")) {
                                if (pdf.Length == 0) {
                                    continue;
                                }
                                File.Delete(wwwrootPath + pdf);
                            }
                    }
                        if (news.Thumbnail_path != null && news.Thumbnail_path.Length > 0) {
                            File.Delete(wwwrootPath + news.Thumbnail_path);
                        }


                    db.RemoveNews(news.Id);

                } catch (Exception ex) {
                    return Results.BadRequest();
                }
                return Results.Ok();


            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }


        private async void UploadFile(IFormFile file, string location) {
            using (FileStream fs = File.Create(location)) {
                await file.CopyToAsync(fs);
            }
        }

        [Description(@"Reads the bytes of the stream. The input stream is not closed.
        It doesn't close the Stream if it is MemoryStream, 
but it copies the bytes from the input stream to new temp MemoryStream 
and it reads it from the new temp MemoryStream, then it closes the new temp MemoryStream.")]
        private byte[] ReadAllBytes(Stream inStream) {
            if (inStream is MemoryStream inMemoryStream) {
                return inMemoryStream.ToArray();
            }

            byte[] bytes = new byte[inStream.Length];
            using (var outStream = new MemoryStream()) {
                inStream.CopyTo(outStream);
                bytes = outStream.ToArray();
            }
            return bytes;
        }

        private async void UpdateFiles(NewsForm news, IFormFile file, 
            string fileLocationRoot, string endpointLocationRoot, bool isPDF) {
            
            string newFileLocation = fileLocationRoot + file.FileName;

            if (File.Exists(newFileLocation)) {
                byte[] currentContent = await File.ReadAllBytesAsync(newFileLocation);

                bool isDiff = false;
                using (Stream stream = file.OpenReadStream()) {
                    isDiff = !currentContent.SequenceEqual(ReadAllBytes(stream));
                }

                if (isDiff) {
                    File.Delete(newFileLocation);
                    UploadFile(file, newFileLocation);
                }

                return;
            }

            UploadFile(file, newFileLocation);

            if (isPDF) {
                news.PDF_path += endpointLocationRoot + file.FileName + ";";
                return;
            }

            File.Delete(wwwrootPath + news.Thumbnail_path);
            news.Thumbnail_path = endpointLocationRoot + file.FileName;
        }
    }
}
