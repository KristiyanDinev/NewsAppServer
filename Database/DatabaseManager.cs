using Microsoft.Data.Sqlite;
using NewsAppServer.Models;
using System.Data;
using System.Data.Common;

namespace NewsAppServer.Database {
	public class DatabaseManager {
		public static string _connectionString = "Data Source=";

		public static void Setup() {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
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

		public async void AddNews(NewsModel news) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
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
                if (news.Tags != null) {
                    news.Tags = news.Tags.Replace("'", "\"");
                }
                command.Parameters.AddWithValue("$tags", news.Tags);
            }

            command.Parameters.AddWithValue("$title", news.Title);
            command.Parameters.AddWithValue("$html_body", news.HTML_body);
            await command.ExecuteNonQueryAsync();
        }

		public async void RemoveNews(int newsID) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM News WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", newsID);
            await command.ExecuteNonQueryAsync();
        }

		public async Task<List<NewsModel>> GetNews(int page, int amountPerPage) {
            // fist page is 0
			List<NewsModel> list = new List<NewsModel>();
            if (page < 1 || amountPerPage < 1) {
                return list;
            }
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM News ORDER BY Posted_on DESC LIMIT $amount OFFSET $page;";
                command.Parameters.AddWithValue("$page", page * amountPerPage);
                command.Parameters.AddWithValue("$amount", amountPerPage);
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) {
                    list.AddRange(reader.Cast<NewsModel>());
                }
            }
            return list;
        }

        public async Task<NewsModel?> GetNewsByID(int newsID) {
            NewsModel? news = null;
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM News WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", newsID);
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) {
                    news = reader.Cast<NewsModel>().First();
                }
            }
            return news;
        }

        public async void EditNews(NewsModel news) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE News 
        SET Title = $title, Thumbnail_path = $thumbnail, PDF_path = $pdf, HTML_body = $html, Tags = $tags
        WHERE Id = $id";
            command.Parameters.AddWithValue("$title", news.Title);
            command.Parameters.AddWithValue("$id", news.Id);
            command.Parameters.AddWithValue("$thumbnail", news.Thumbnail_path);
            command.Parameters.AddWithValue("$pdf", news.PDF_path);
            command.Parameters.AddWithValue("$html", news.HTML_body);

            if (news.Tags != null) {
                news.Tags = news.Tags.Replace("'", "\"");
            }
            command.Parameters.AddWithValue("$tags", news.Tags);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<string>> GetAdminPasswords() {
            List<string> list = new List<string>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Admins;";
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) {
                    list.Add(reader.GetString(0));
                }
            }
            return list;
        }

        public async void EditAdminPassword(string oldPass, string newPass) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Admins 
        SET Password = $newpass
        WHERE Password = $oldpass";
            command.Parameters.AddWithValue("$newpass", newPass);
            command.Parameters.AddWithValue("$oldpass", oldPass);
            await command.ExecuteNonQueryAsync();
        }

        public async void AddAdminPassword(string pass) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Admins VALUES ($pass)";
            command.Parameters.AddWithValue("$pass", pass);
            await command.ExecuteNonQueryAsync();
        }

        public async void RemoveAdmin(string pass) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Admins WHERE Password = $pass;";
            command.Parameters.AddWithValue("$pass", pass);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<NewsModel>> SearchNews(string search, 
            string[] tags) {
            List<NewsModel> list = new List<NewsModel>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = @"
                SELECT * 
                FROM News
                WHERE (Title LIKE '% $search %' OR 
                Title LIKE '$search %' OR
                Title LIKE '% $search' OR
                Title = $search OR
                HTML_body LIKE '% $search %' OR
                HTML_body LIKE '$search %' OR
                HTML_body LIKE '% $search' OR
                HTML_body = $search)";


                if (tags.Length > 0) {
                    command.CommandText += " AND ( ";
                    
                    for (int i = 0; i < tags.Length - 1; i++) {
                        string tag = tags[i].Replace("'", "\"");
                        command.CommandText += $""""
                    Tags LIKE '%;{tag};%' OR  
                    Tags LIKE '{tag};%' OR 
                    Tags LIKE '%;{tag}' OR 
                    Tags = '{tag}' OR 
                    """";
                    }

                    string lastTag = tags[tags.Length - 1].Replace("'", "\"");
                    command.CommandText += $"""
                        Tags LIKE '%;{lastTag};%' OR 
                        Tags LIKE '{lastTag};%' OR 
                        Tags LIKE '%;{lastTag}' OR 
                        Tags = '{lastTag}')
                        """;
                }
                

                command.Parameters.AddWithValue("$search", search);

                using var reader = await command.ExecuteReaderAsync();
                using DataTable dt = new DataTable();
                dt.Load(reader);
                for (int i = 0; i < dt.Rows.Count; i++) {
                    DataRow row = dt.Rows[i];

                    NewsModel newsModel = new NewsModel();
                    newsModel.Title = Convert.ToString(row["Title"]);
                    newsModel.Id = Convert.ToInt32(row["Id"]);
                    newsModel.Thumbnail_path = Convert.ToString(row["Thumbnail_path"]);
                    newsModel.PDF_path = Convert.ToString(row["PDF_path"]);
                    newsModel.HTML_body = Convert.ToString(row["HTML_body"]);
                    newsModel.Tags = Convert.ToString(row["Tags"]);

                    newsModel.Posted_on_UTC_timezored = DateTime.Parse(Convert.ToString(row["Posted_on_UTC_timezored"]));

                    list.Add(newsModel);
                }
            }
            return list;
        }
    }
}
