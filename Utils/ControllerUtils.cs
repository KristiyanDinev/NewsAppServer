using NewsAppServer.Controllers;
using NewsAppServer.Database;
using NewsAppServer.Models;

namespace NewsAppServer.Utils {
    public class ControllerUtils {
        public static readonly string wwwrootPath = "wwwroot";

        public static readonly string thumbnailFileLocation = wwwrootPath + "\\thumbnail\\";
        public static readonly string pdfFileLocation = wwwrootPath + "\\pdf\\";

        public static readonly string pdfEndpoint = pdfFileLocation.Split("wwwroot").Last();
        public static readonly string thumbnailEndpoint = thumbnailFileLocation.Split("wwwroot").Last();

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


        public static async Task<bool> CheckAdminRequest(IFormCollection form,
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


        public static List<string> SeperateValues(string? value) {
            List<string> list = new List<string>();
            if (value != null) {
                foreach (string tag in value.Split(';')) {
                    if (tag.Length == 0) {
                        continue;
                    }
                    list.Add(tag.Replace("'", "\"").Trim());
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

                if (ThumbnailName.Contains('\\') ||
                    ThumbnailName.Contains('/') ||
                    ThumbnailData.EndsWith(',') ||
                    ThumbnailData.StartsWith(',')) {
                    return null;
                }

                string? path = await UpdateFile(oldThumbnail,
                    ThumbnailName, FromStringToUint8Array(ThumbnailData),
                    thumbnailFileLocation, thumbnailEndpoint, false);

                return path;
            }
            return null;
        }

        public static async Task<string?> UploadPDFs(string pdfValue) {
            string? PDF_path = null;
            foreach (string pdfPart in pdfValue.Split(";")) {
                if (pdfPart.Length == 0) {
                    continue;
                }
                string[] pdfParts = pdfPart.Split(".");
                string pdfName = pdfParts[0] + ".pdf";
                string pdfData = pdfParts[1].Remove(0, 3);

                if (pdfName.Contains('\\') ||
                    pdfName.Contains('/') ||
                    pdfData.EndsWith(',') ||
                    pdfData.StartsWith(',')) {
                    continue;
                }

                string? PDF_New_Path = await UpdateFile(null,
                    pdfName, FromStringToUint8Array(pdfData),
                    pdfFileLocation, pdfEndpoint, true);

                if (PDF_New_Path == null) {
                    continue;
                }
                PDF_path ??= "";
                PDF_path += pdfEndpoint + pdfName + ";";
            }
            return PDF_path;
        }

        public static void DeletePDFs(string pdfValue) {
            foreach (string pdfPath in pdfValue.Split(";")) {
                if (pdfPath.Length == 0) {
                    continue;
                }
                try {
                    File.Delete(wwwrootPath + pdfPath);
                } catch (Exception) { }
            }
        }
    }
}
