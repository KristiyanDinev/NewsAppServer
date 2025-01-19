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

                    string? search = null;
                    if (form.ContainsKey("search")) {
                        search = form["search"];
                        if (!IsRequirementFilled(search)) {
                            search = null;
                        }
                    }

                    List<string> fixTags = new List<string>();
                    if (form.ContainsKey("tags")) {
                        string? tagsValue = form["tags"];

                        if (IsRequirementFilled(tagsValue)) {
                            foreach (string tag in tagsValue.Split(';')) {
                                if (tag.Replace(" ", "").Length == 0) {
                                    continue;
                                }
                                fixTags.Add(tag);
                            }
                        }
                    }

                    if (search == null && fixTags.Count == 0) {
                        return res;
                    }

                    List<NewsModel> searchedNews = 
                        await db.SearchNews(search, fixTags.ToArray());
                    res.Add("News", searchedNews);
                    return res;
                });

            app.MapPost("/news", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                bool isAdminReq = await CheckAdminRequest(form, db);
                if (!isAdminReq) {
                    return Results.Unauthorized();
                }

                if (!(form.ContainsKey("Title") && form.ContainsKey("Tags") &&
                    form.ContainsKey("HTML_body") &&
                    form.ContainsKey("Thumbnail") &&
                    form.ContainsKey("PDFs"))) {
                    return Results.BadRequest();
                }

                if (!IsRequirementFilled(form["Title"]) || 
                    !IsRequirementFilled(form["HTML_body"])) {
                    return Results.BadRequest();
                }

                NewsModel news = new NewsModel();
                // ----------
                news.Posted_by_Admin_username = form["AdminUsername"].ToString();
                // ----------
                news.Title = form["Title"].ToString();
                // -----------
                string? tags = form["Tags"];
                news.Tags = tags?.ToString();
                // -----------
                news.HTML_body = BBCode.ConvertToHtml(form["HTML_body"].ToString());

                // ------------
                string thumbnail = form["Thumbnail"].ToString();
                if (thumbnail.Length > 0) {
                    string[] ThumbnailParts = form["Thumbnail"].ToString().Split(";");
                    string ThumbnailName = ThumbnailParts[0];
                    string ThumbnailData = ThumbnailParts[1];

                    if (ThumbnailName.Contains('\\') ||
                        ThumbnailName.Contains('/') || ThumbnailData.EndsWith(',')) {
                        return Results.BadRequest();
                    }

                    UploadFile(FromStringToUint8Array(ThumbnailData),
                        thumbnailFileLocation + ThumbnailName);
                    news.Thumbnail_path = thumbnailEndpoint + ThumbnailName;
                }

                // ------------

                string pdfs = form["PDFs"].ToString();
                if (pdfs.Length > 0) {
                    foreach (string pdfPart in form["PDFs"].ToString().Split(";")) {
                        if (pdfPart.Replace(" ", "").Length == 0 || pdfPart.Equals("null")) {
                            continue;
                        }
                        string[] pdfParts = pdfPart.Split(".");
                        string pdfName = pdfParts[0] + ".pdf";
                        string pdfData = pdfParts[1].Remove(0, 3);

                        if (pdfData.EndsWith(',')) {
                            continue;
                        }

                        UploadFile(FromStringToUint8Array(pdfData),
                            pdfFileLocation + pdfName);

                        news.PDF_path ??= "";
                        news.PDF_path += pdfEndpoint + pdfName + ";";
                    }
                }
                
                db.AddNews(news);
                return Results.Ok();

            })
                .DisableAntiforgery()
            .RequireRateLimiting("fixed");

            /*
            app.MapPost("/news/edit", (HttpContext http, DatabaseManager db) => {
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


            app.MapPost("/news/delete", async (HttpContext http, DatabaseManager db) => {
                IFormCollection form = await http.Request.ReadFormAsync();
                bool isAdminReq = await CheckAdminRequest(form, db);
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
                if (IsRequirementFilled(thumbnail_path)) {
                    File.Delete(wwwrootPath + thumbnail_path);
                }

                string? pdfs_path = form["PDFs"];
                if (IsRequirementFilled(pdfs_path)) {
                    foreach (string pdf in pdfs_path.Split(";")) {
                        if (pdf.Length == 0 || pdf.Equals("null")) {
                            continue;
                        }
                        File.Delete(wwwrootPath + pdf);
                    }
                }

                db.RemoveNews(Id);
                return Results.Ok();


            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
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


        private static bool IsRequirementFilled(object? obj) {
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


        private static async Task<bool> CheckAdminRequest(IFormCollection form, 
            DatabaseManager db) {

            try {
                AdminModel loginAdmin = new AdminModel();
                loginAdmin.Password = form["AdminPass"];
                loginAdmin.Username = form["AdminUsername"];

                AdminModel? admin = await AdminController.LoginAdmin(db, loginAdmin);
                return admin != null;
            } catch (Exception) { 
                return false;
            }
        }
    }
}
