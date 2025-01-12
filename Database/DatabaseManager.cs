using Microsoft.Data.Sqlite;
using NewsAppServer.Models;

namespace NewsAppServer.Database {
	public class DatabaseManager {
		public static string _connectionString = "Data Source=";

		public static void Setup() {
            using (var connection = new SqliteConnection(_connectionString)) {
				connection.Open();

				var command = connection.CreateCommand();
				command.CommandText =
                            @"CREATE TABLE IF NOT EXISTS News 
(Id INTEGER PRIMARY KEY, 
Title VARCHAR(255) NOT NULL, 
Thumbnail_path VARCHAR, 
PDF_path VARCHAR, 
HTML_body VARCHAR NOT NULL,
Tags VARCHAR,
Posted_on_UTC_timezored DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS Admins (Password VARCHAR NOT NULL);
";
				command.ExecuteNonQuery();
			}
		}

		public async void AddNews(NewsForm news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                            @"INSERT INTO News 
(Title, Thumbnail_path, PDF_path, HTML_body, Tags) VALUES ($title, $thumbnail_path, $pdf_path, $html_body, $tags);";

                if (news.Thumbnail_path == null) {
                    command.CommandText = command.CommandText.Replace("$thumbnail_path", "null");
                } else {
                    command.Parameters.AddWithValue("$thumbnail_path", news.Thumbnail_path);
                }
                if (news.PDF_path == null) {
                    command.CommandText = command.CommandText.Replace("$pdf_path", "null");
                } else {
                    command.Parameters.AddWithValue("$pdf_path", news.PDF_path);
                }
                if (news.Tags == null) {
                    command.CommandText = command.CommandText.Replace("$tags", "null");
                } else {
                    command.Parameters.AddWithValue("$tags", news.Tags);
                }

                command.Parameters.AddWithValue("$title", news.Title);
                command.Parameters.AddWithValue("$html_body", news.HTML_body);
                await command.ExecuteNonQueryAsync();
            }
        }

		public async void RemoveNews(int newsID) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM News WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", newsID);
                await command.ExecuteNonQueryAsync();
            }
        }

		public async Task<List<NewsModel>> GetNews(int page, int amountPerPage) {
            // fist page is 0
			List<NewsModel> list = new List<NewsModel>();
            if (page < 1 || amountPerPage < 1) {
                return list;
            }
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM News ORDER BY Posted_on DESC LIMIT $amount OFFSET $page;";
                command.Parameters.AddWithValue("$page", page * amountPerPage);
                command.Parameters.AddWithValue("$amount", amountPerPage);
                using (var reader = await command.ExecuteReaderAsync()) {
                    while (reader.Read()) {
                        list.AddRange(reader.Cast<NewsModel>());
                    }
                }
            }
            return list;
        }

        public async Task<NewsModel?> GetNewsByID(int newsID) {
            NewsModel? news = null;
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM News WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", newsID);
                using (var reader = await command.ExecuteReaderAsync()) {
                    while (reader.Read()) {
                        news = reader.Cast<NewsModel>().First();
                    }
                }
            }
            return news;
        }

        public async void EditNews(NewsForm news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE News 
        SET Title = $title, Thumbnail_path = $thumbnail, PDF_path = $pdf, HTML_body = $html, Tags = $tags
        WHERE Id = $id";
                command.Parameters.AddWithValue("$title", news.Title);
                command.Parameters.AddWithValue("$id", news.Id);
                command.Parameters.AddWithValue("$thumbnail", news.Thumbnail_path);
                command.Parameters.AddWithValue("$pdf", news.PDF_path);
                command.Parameters.AddWithValue("$html", news.HTML_body);
                command.Parameters.AddWithValue("$tags", news.Tags);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<string>> GetAdminPasswords() {
            List<string> list = new List<string>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Admins;";
                using (var reader = await command.ExecuteReaderAsync()) {
                    while (reader.Read()) {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            return list;
        }

        public async void EditAdminPassword(string oldPass, string newPass) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Admins 
        SET Password = $newpass
        WHERE Password = $oldpass";
                command.Parameters.AddWithValue("$newpass", newPass);
                command.Parameters.AddWithValue("$oldpass", oldPass);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async void AddAdminPassword(string pass) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Admins VALUES ($pass)";
                command.Parameters.AddWithValue("$pass", pass);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async void RemoveAdmin(string pass) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Admins WHERE Password = $pass;";
                command.Parameters.AddWithValue("$pass", pass);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
