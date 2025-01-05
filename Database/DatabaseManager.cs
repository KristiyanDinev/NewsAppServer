using Microsoft.Data.Sqlite;
using NewsAppServer.Models;

namespace NewsAppServer.Database {
	public class DatabaseManager {
		private static readonly string _connectionString = "Data Source=database.db";

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
Posted_on DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS Admins (Password VARCHAR NOT NULL);
";
				command.ExecuteNonQuery();
			}
		}

		public void AddNews(News news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                            @"INSERT INTO News 
(Title, Thumbnail_base64, PDF_path, HTML_body) VALUES ($title, $thumbnailbase64, $pdf_path, $html_body);";
                command.Parameters.AddWithValue("$title", news.Title);
                command.Parameters.AddWithValue("$thumbnailbase64", news.Thumbnail_base64);
                command.Parameters.AddWithValue("$pdf_path", news.PDF_path);
                command.Parameters.AddWithValue("$html_body", news.HTML_body);
                command.ExecuteNonQuery();
            }
        }

		public void RemoveNews(News news) {
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM News WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", news.Id);
                command.ExecuteNonQuery();
            }
        }

		public List<News> GetNews(int limit) { 
			List<News> list = new List<News>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                var command = connection.CreateCommand();
                if (limit > 0) {
                    command.CommandText = "SELECT * FROM News LIMIT $limit";
                    command.Parameters.AddWithValue("$limit", limit);
                } else {
                    command.CommandText = "SELECT * FROM News";
                }
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        foreach (News news in reader.Cast<News>()) {
                            list.Add(news);
                        }
                    }
                }
            }
            return list;
        }
	}
}
