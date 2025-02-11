using NewsAppServer.Controllers;
using NewsAppServer.Database;
using NewsAppServer.Models;
using System.Text;

namespace NewsAppServer.Utils {
    public class ControllerUtils {


        public static readonly string wwwrootPath = "wwwroot";

        public static readonly string thumbnail_FileLocation = wwwrootPath + "\\thumbnail\\";
        public static readonly string attachment_FileLocation = wwwrootPath + "\\attachment\\";

        public static readonly string attachment_Endpoint = attachment_FileLocation.Split("wwwroot").Last();
        public static readonly string thumbnail_Endpoint = thumbnail_FileLocation.Split("wwwroot").Last();

        public static async void UploadFile(byte[] data, string location) {
            using FileStream fs = File.Create(location);
            await fs.WriteAsync(data);
        }


        public static async Task<string?> UpdateFile(string? ThumbnailOldFilePath, string FileName,
            byte[] FileBytes,
            string fileLocationRoot, string endpointLocationRoot, bool isPDF) {

            if (FileName.Contains('/') || FileName.Contains('\\')) {
                return null;
            }

            string newFileLocation = fileLocationRoot + FileName;

            if (File.Exists(newFileLocation)) {
                byte[] currentContent = await File.ReadAllBytesAsync(newFileLocation);

                if (!currentContent.SequenceEqual(FileBytes)) {
                    File.Delete(newFileLocation);
                    UploadFile(FileBytes, newFileLocation);
                }
            }

            UploadFile(FileBytes, newFileLocation);

            if (isPDF) {
                return endpointLocationRoot + FileName + ";";
            }
            if (ThumbnailOldFilePath != null) {
                File.Delete(wwwrootPath + ThumbnailOldFilePath);
            }
            return endpointLocationRoot + FileName;
        }


        public static bool IsRequirementFilled(object? obj) {
            return obj != null && obj.ToString()
                            .Replace(" ", "").Length > 0;
        }

        public static byte[] FromStringToUint8Array(string data) {
            string[] dataNumbers = data.Split(",");
            byte[] byteArray = new byte[dataNumbers.Length];
            for (int i = 0; i < dataNumbers.Length; i++) {
                byteArray[i] = Convert.ToByte(dataNumbers[i]);
            }
            return byteArray;
        }


        public static async Task<bool> CheckAdminRequest(string AdminUsername, string AdminPassword,
            DatabaseManager db) {

            try {
                AdminModel loginAdmin = new AdminModel();
                loginAdmin.Username = AdminUsername;
                loginAdmin.Password = AdminPassword;

                AdminModel? admin = await AdminController.LoginAdmin(db, loginAdmin);
                return admin != null;
            } catch (Exception) {
                return false;
            }
        }


        public static List<string> SeperateValues(string? value) {
            List<string> list = new List<string>();
            if (value != null) {
                foreach (string tag in value.Split(';')) {
                    if (tag.Length == 0) {
                        continue;
                    }
                    list.Add(tag.Replace("'", "''").Trim());
                }
            }
            return list;
        }

        public static async Task<string?> UploadThumbnail(string thumbnail,
                string? oldThumbnail) {
            if (thumbnail.Length > 0) {
                string[] ThumbnailParts = thumbnail.Split(";");
                string ThumbnailName = ThumbnailParts[0];
                string ThumbnailData = ThumbnailParts[1];

                string byteData = Encoding.UTF8.GetString(
                    Convert.FromBase64String(ThumbnailData));

                if (ThumbnailName.Contains('\\') ||
                    ThumbnailName.Contains('/') ||
                    byteData.EndsWith(',') ||
                    byteData.StartsWith(',')) {
                    return null;
                }

                string? path = await UpdateFile(oldThumbnail,
                    ThumbnailName, FromStringToUint8Array(byteData),
                    thumbnail_FileLocation, thumbnail_Endpoint, false);

                return path;

            } else if (oldThumbnail != null) {
                try {
                    File.Delete(wwwrootPath + oldThumbnail);
                } catch (Exception) { }
            }
            return null;
        }

        public static async Task<string?> UploadAttachments(string attachmentValue) {
            string? Attachment_Path = null;
            if (attachmentValue.Length == 0) {
                return null;
            }
            foreach (string attachmetPart in attachmentValue.Split(";")) {
                if (attachmetPart.Length == 0) {
                    continue;
                }
                string[] attachmetParts = attachmetPart.Split("//");
                string attachmentName = attachmetParts[0];
                string attachmentData = attachmetParts[1];

                string byteData = Encoding.UTF8.GetString(
                    Convert.FromBase64String(attachmentData));

                if (attachmentName.Contains('\\') ||
                    attachmentName.Contains('/') ||
                    byteData.EndsWith(',') ||
                    byteData.StartsWith(',')) {
                    continue;
                }

                string? PDF_New_Path = await UpdateFile(null,
                    attachmentName, FromStringToUint8Array(byteData),
                    attachment_FileLocation, attachment_Endpoint, true);

                if (PDF_New_Path == null) {
                    continue;
                }
                Attachment_Path ??= "";
                Attachment_Path += attachment_Endpoint + attachmentName + ";";
            }
            return Attachment_Path;
        }

        public static void DeleteAttachments(string attachmentValue) {
            foreach (string attachmentPath in attachmentValue.Split(";")) {
                if (attachmentPath.Length == 0) {
                    continue;
                }
                try {
                    File.Delete(wwwrootPath + attachmentPath);
                } catch (Exception) { }
            }
        }

        public static async Task<string> GetIndexHTML(string path) {
            try {

                string htmlData = await File.ReadAllTextAsync(
                    Directory.GetCurrentDirectory()+
                    "\\wwwroot\\web\\html" +
                    path.Replace("/", "\\")+"\\Index.html");

                string HeaderImports = @"
    <meta charset=""utf-16"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<link href='../../../../web/css/main.css' rel=""stylesheet"" />
";


                string BodyImports = @"

            <script src=""https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js""></script>
<script src='../../../../web/js/main.js'></script>
";

                string TitleBar = @"
        
            <div class=""title-bar"">
        <p class=""title"">News / Posts / Studies</p>
    
        <div onclick='onClickOptions()' class=""menu"">
            <p>☰</p>
        </div>
    </div>
";

                htmlData = htmlData.Replace("{{HeaderImports}}", HeaderImports);
                htmlData = htmlData.Replace("{{BodyImports}}", BodyImports);
                htmlData = htmlData.Replace("{{TitleBar}}", TitleBar);
                return htmlData;

            } catch (Exception) {
                throw new Exception();
            }
        }

        public static NewsModel? Get_NewsModel_From_SearchSession(
            ISession session, int newsId) {

            return ConvertSessionToNewsModel(session, "SearchResults:" + newsId);
        }

        public static List<NewsModel> Get_NewsModels_From_SavedNewsSession(
            ISession session) {

            List<NewsModel> models = new List<NewsModel>();
            string v = session.GetString("SavedNewsIds") ?? "";


            try {

                foreach (string id in v.Split(';')) {
                    if (id.Length == 0) { continue; }
                    models.Add(ConvertSessionToNewsModel(session, "SavedNews:" + id));
                }
                return models;

            } catch (Exception) { 
                return models;
            }
        }

        public static void Add_NewsModel_To_SavedNewsSession(ref ISession session, 
            NewsModel item) {

            string v = session.GetString("SavedNewsIds") ?? "";

                session.SetString("SavedNewsIds", v + item.Id + ";");

                string key = "SavedNews:" + item.Id;

                session.SetString(key + ":id", item.Id.ToString());
                session.SetString(key + ":title", item.Title);
                session.SetString(key + ":attachments_path", item.Attachments_path ?? "");
                session.SetString(key + ":tags", item.Tags ?? "");
                session.SetString(key + ":posted_on", item.Posted_on.ToString());
                session.SetString(key + ":bbcode_body", item.BBCode_body);
                session.SetString(key + ":html_body", item.HTML_body);
                session.SetString(key + ":posted_by_admin_username", item.Posted_by_Admin_username);
                session.SetString(key + ":thumbnail_path", item.Thumbnail_path ?? "");
        }

        public static void Remove_NewsModel_From_SavedNewsSession(ref ISession session,
            int newsId) {

            string v = session.GetString("SavedNewsIds") ?? "";
            session.SetString("SavedNewsIds", v.Replace(newsId + ";", ""));

            string key = "SavedNews:" + newsId;
            session.Remove(key + ":id");
            session.Remove(key + ":title");
            session.Remove(key + ":attachments_path");
            session.Remove(key + ":tags");
            session.Remove(key + ":posted_on");
            session.Remove(key + ":bbcode_body");
            session.Remove(key + ":html_body");
            session.Remove(key + ":posted_by_admin_username");
            session.Remove(key + ":thumbnail_path");
        }


        private static NewsModel ConvertSessionToNewsModel(ISession session, string key) {
            NewsModel news = new NewsModel();
            news.Id = session.GetInt32(key+":id") ?? 0;
            news.HTML_body = session.GetString(key + ":html_body");
            news.BBCode_body = session.GetString(key + ":bbcode_body");
            news.Thumbnail_path = session.GetString(key + ":thumbnail_path");
            news.Tags = session.GetString(key + ":tags");
            news.Attachments_path = session.GetString(key + ":attachments_path");
            news.Posted_by_Admin_username = session.GetString(key + ":posted_by_admin_username");
            news.Posted_on = DateTime.Parse(session.GetString(key + ":posted_on"));
            return news;
        }

        public static void Add_News_To_SearchResults(ref ISession session,
            List<NewsModel> res) {

            foreach (NewsModel item in res) {

                string key = "SearchResults:" + item.Id;
                session.SetString(key + ":id", item.Id.ToString());
                session.SetString(key + ":title", item.Title);
                session.SetString(key + ":attachments_path", item.Attachments_path ?? "");
                session.SetString(key + ":tags", item.Tags ?? "");
                session.SetString(key + ":posted_on", item.Posted_on.ToString());
                session.SetString(key + ":bbcode_body", item.BBCode_body);
                session.SetString(key + ":html_body", item.HTML_body);
                session.SetString(key + ":posted_by_admin_username", item.Posted_by_Admin_username);
                session.SetString(key + ":thumbnail_path", item.Thumbnail_path ?? "");
            }


        }


        public static void _handleEmptyEntryInFile(ref string FileData, object model) {
            foreach (string property in
                    model.GetType().GetProperties().Select(f => f.Name).ToList()) {
                FileData = FileData.Replace("{{" + property + "}}", "");
            }
        }


        public static void _handleEntryInFile(ref string FileData, object model) {
            Type type = model.GetType();
            foreach (string property in
                    type.GetProperties().Select(f => f.Name).ToList()) {

                FileData = FileData.Replace("{{" + property + "}}",
                    Convert.ToString(type.GetProperty(property).GetValue(model)));
            }

        }
    }
}
