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

    }
}
