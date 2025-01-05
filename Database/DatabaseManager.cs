using Microsoft.Data.Sqlite;
using NewsAppServer.Models;

namespace NewsAppServer.Database {
	public class DatabaseManager {
		private static readonly string _connectionString = "Data Source=database.sqlite";

		public static void Setup() {
            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            using (var connection = new SqliteConnection(_connectionString)) {
				connection.Open();

				var command = connection.CreateCommand();
				command.CommandText =
                            @"CREATE TABLE IF NOT EXISTS News 
(Id INTEGER PRIMARY KEY, 
Title VARCHAR(255) NOT NULL, 
Thumbnail_base64 VARCHAR, 
PDF_path VARCHAR NOT NULL, 
HTML_body VARCHAR NOT NULL,
Tags VARCHAR,
Posted_on DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS Admins (Password VARCHAR NOT NULL);
";
				command.ExecuteNonQuery();
			}
		}

		public async void AddNews(News news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                            @"INSERT INTO News 
(Title, Thumbnail_base64, PDF_path, HTML_body, Tags) VALUES ($title, $thumbnailbase64, $pdf_path, $html_body, $tags);";
                command.Parameters.AddWithValue("$title", news.Title);
                command.Parameters.AddWithValue("$thumbnailbase64", news.Thumbnail_base64);
                command.Parameters.AddWithValue("$pdf_path", news.PDF_path);
                command.Parameters.AddWithValue("$html_body", news.HTML_body);
                command.Parameters.AddWithValue("$tags", news.Tags);
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

		public async Task<List<News>> GetNews(int page, int amountPerPage) {
            // fist page is 0
			List<News> list = new List<News>();
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
                        foreach (News news in reader.Cast<News>()) {
                            list.Add(news);
                        }
                    }
                }
            }
            return list;
        }

        public async void EditNews(News news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE News 
        SET Title = $title, Thumbnail_base64 = $thumbnail, PDF_path = $pdf, HTML_body = $html, Tags = $tags
        WHERE Id = $id";
                command.Parameters.AddWithValue("$title", news.Title);
                command.Parameters.AddWithValue("$id", news.Id);
                command.Parameters.AddWithValue("$thumbnail", news.Thumbnail_base64);
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
    }
}
