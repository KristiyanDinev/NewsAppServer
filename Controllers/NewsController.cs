using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.ComponentModel;
using System.Text;

namespace NewsAppServer.Controllers {
    public class NewsController {
        private readonly string wwwrootPath = "wwwroot";
        public NewsController(WebApplication app) {
            string thumbnailFileLocation = wwwrootPath + "\\thumbnail\\";
            string pdfFileLocation = wwwrootPath + "\\pdf\\";

            string pdfEndpoint = pdfFileLocation.Split("wwwroot").Last();
            string thumbnailEndpoint = thumbnailFileLocation.Split("wwwroot").Last();

            /*
             * 
             * public bool DeleteThumbnail { get; set; } = false;
        public string DeletePDFs { get; set; } = "";
             */

            app.MapGet("/news/{page:int}/{amount:int}", async (HttpContext http, 
                DatabaseManager db, 
                int page, int amount) => {
                    
                    page -= 1;
                    Dictionary<string, object> res = 
                        new Dictionary<string, object>();
                    try {
                        List<NewsModel> news = await db.GetNews(page, amount);
                        res.Add("News", news);
                        return res;
                    } catch (Exception) { 
                        return res;
                    }
                    
            });


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

            app.MapPost("/news/search", 
                async (HttpContext http, DatabaseManager db) => {
                    IFormCollection form = await http.Request.ReadFormAsync();
                    Dictionary<string, object> res =
                        new Dictionary<string, object>();
                    if (!form.ContainsKey("search")) {
                        return res;
                    }

                    string search = form["search"].ToString();
                    if (search.Replace(" ", "").Length == 0) {
                        return res;
                    }

                    List<string> fixTags = new List<string>();
                    if (form.ContainsKey("tags")) {
                        foreach (string tag in form["tags"].ToString().Split(';')) {
                            if (tag.Replace(" ", "").Length == 0) {
                                continue;
                            }
                            fixTags.Add(tag);
                        }
                    }
                    List<NewsModel> searchedNews = 
                        await db.SearchNews(search, fixTags.ToArray());
                    res.Add("News", searchedNews);
                    return res;
                });

            app.MapPost("/news", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                if (form.ContainsKey("AdminPass")) {
                    object passObj = form["AdminPass"];
                    if (passObj == null) {
                        return Results.Unauthorized();
                    }

                    bool isAdmin = await AdminController.CheckAdmin(db,
                        passObj.ToString());

                    if (!isAdmin) {
                        return Results.Unauthorized();
                    }
                } else {
                    return Results.Unauthorized();
                }
                NewsModel news = new NewsModel();
                foreach (string k in form.Keys) {
                    object value = form[k];

                    if (k.Equals("Title")) {
                        if (!IsRequirementFilled(value)) {
                            return Results.BadRequest();
                        }
                        news.Title = value.ToString();

                    } else if (k.Equals("Tags")) {
                        news.Tags = value?.ToString();

                    } else if (k.Equals("HTML_body")) {
                        if (!IsRequirementFilled(value)) {
                            return Results.BadRequest();
                        }

                        news.HTML_body = BBCode.ConvertToHtml(value.ToString());

                    } else if (k.Equals("Thumbnail")) {
                        if (!IsRequirementFilled(value)) {
                            continue;
                        }
                        string[] ThumbnailParts = value.ToString().Split(";");
                        string ThumbnailName = ThumbnailParts[0];
                        string ThumbnailData = ThumbnailParts[1];

                        if (ThumbnailName.Contains('\\') ||
                            ThumbnailName.Contains('/')) {
                                return Results.BadRequest();
                        }

                        if (ThumbnailData.EndsWith(',')) {
                            continue;
                        }

                        UploadFile(FromStringToUint8Array(ThumbnailData), 
                            thumbnailFileLocation + ThumbnailName);
                        news.Thumbnail_path = thumbnailEndpoint + ThumbnailName;


                    } else if (k.Equals("PDFs")) {
                        if (!IsRequirementFilled(value)) {
                            continue;
                        }

                        foreach (string pdfPart in value.ToString().Split(";")) {
                            if (pdfPart.Replace(" ", "").Length == 0) {
                                continue;
                            }
                            string[] pdfParts = pdfPart.Split(".");
                            string pdfName = pdfParts[0]+".pdf";
                            string pdfData = pdfParts[1].Remove(0,3);

                            if (pdfData.EndsWith(',')) {
                                continue;
                            }

                            UploadFile(FromStringToUint8Array(pdfData),
                                pdfFileLocation + pdfName);

                            news.PDF_path ??= "";
                            news.PDF_path += pdfEndpoint + pdfName + ";";
                        }
                    }
                }

                db.AddNews(news);
                return Results.Ok();

            })
                .DisableAntiforgery()
            .RequireRateLimiting("fixed");

            /*
            app.MapPost("/edit/news", (HttpContext http, DatabaseManager db) => {
            // PDF UpdateFiles(news, file, pdfFileLocation, pdfEndpoint, true);
            // Thumbnail UpdateFiles(news, file, thumbnailFileLocation, thumbnailEndpoint, false);
            

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

                }).DisableAntiforgery()
                .RequireRateLimiting("fixed");*/
            
            /*
            app.MapPost("/delete/news", (HttpContext http, DatabaseManager db) => {

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

                } catch (Exception) {
                    return Results.BadRequest();
                }
                return Results.Ok();


            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");*/
        }


        private static async void UploadFile(byte[] data, string location) {
            using FileStream fs = File.Create(location);
            await fs.WriteAsync(data);
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

        private async void UpdateFiles(NewsModel news, string FileName, 
            byte[] FileBytes,
            string fileLocationRoot, string endpointLocationRoot, bool isPDF) {
            
            string newFileLocation = fileLocationRoot + FileName;

            if (File.Exists(newFileLocation)) {
                byte[] currentContent = await File.ReadAllBytesAsync(newFileLocation);

                if (!currentContent.SequenceEqual(FileBytes)) {
                    File.Delete(newFileLocation);
                    UploadFile(FileBytes, newFileLocation);
                }

                return;
            }

            UploadFile(FileBytes, newFileLocation);

            if (isPDF) {
                news.PDF_path += endpointLocationRoot + FileName + ";";
                return;
            }

            File.Delete(wwwrootPath + news.Thumbnail_path);
            news.Thumbnail_path = endpointLocationRoot + FileName;
        }


        private static bool IsRequirementFilled(object obj) {
            return obj != null && obj.ToString()
                            .Replace(" ", "").Length > 0;
        }

        private static byte[] FromStringToUint8Array(string data) {
            string[] dataNumbers = data.Split(",");
            byte[] byteArray = new byte[dataNumbers.Length];
            for (int i = 0; i < dataNumbers.Length; i++) {
                byteArray[i] = Convert.ToByte(dataNumbers[i]);
            }
            return byteArray;
        }
    }
}
