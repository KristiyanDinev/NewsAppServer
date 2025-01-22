using Microsoft.Data.Sqlite;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace NewsAppServer.Database {
	public class DatabaseManager {
		public static string _connectionString = "Data Source=";
		public static string _sysadmin_password = "";

		public static void Setup() {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                        @"CREATE TABLE IF NOT EXISTS News 
(Id INTEGER PRIMARY KEY, 
Title VARCHAR(255) NOT NULL, 
Thumbnail_path VARCHAR, 
Attachments_path VARCHAR, 
HTML_body VARCHAR NOT NULL,
BBCode_body VARCHAR NOT NULL,
Tags VARCHAR,
Posted_by_Admin_username VARCHAR NOT NULL,
Posted_on_UTC_timezoned DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS Admins (
Username VARCHAR NOT NULL UNIQUE,
Password VARCHAR NOT NULL UNIQUE,
Added_by VARCHAR NOT NULL,
Added_Date_UTC_timezoned DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);

INSERT OR IGNORE INTO Admins(Username, Password, Added_by) VALUES ('SystemAdmin', '" + _sysadmin_password + "', 'System Startup');";
            command.ExecuteNonQuery();
        }

		public async void AddNews(NewsModel news) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                        @"INSERT INTO News 
(Title, Thumbnail_path, Attachments_path, HTML_body, BBCode_body, Tags, Posted_by_Admin_username) VALUES ($title, $thumbnail_path, $attachments_path, $html_body, $bbcode_body, $tags, $admin_username);";

            if (news.Thumbnail_path == null) {
                command.CommandText = command.CommandText.Replace("$thumbnail_path", "null");

            } else {
                command.Parameters.AddWithValue("$thumbnail_path", news.Thumbnail_path);
            }

            if (news.Attachments_path == null) {
                command.CommandText = command.CommandText.Replace("$attachments_path", "null");

            } else {
                command.Parameters.AddWithValue("$attachments_path", news.Attachments_path);
            }

            if (news.Tags == null) {
                command.CommandText = command.CommandText.Replace("$tags", "null");

            } else {
                news.Tags = news.Tags.Replace("'", "''").Trim();                
                command.Parameters.AddWithValue("$tags", news.Tags);
            }

            command.Parameters.AddWithValue("$title", news.Title.Trim());
            command.Parameters.AddWithValue("$html_body", news.HTML_body);
            command.Parameters.AddWithValue("$bbcode_body", news.BBCode_body);
            command.Parameters.AddWithValue("$admin_username", news.Posted_by_Admin_username);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

		public async void RemoveNews(int newsID) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM News WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", newsID);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<NewsModel?> GetNewsByID(int newsID) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM News WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", newsID);
            await command.PrepareAsync();
            using var reader = await command.ExecuteReaderAsync();
            using DataTable dt = new DataTable();
            dt.Load(reader);
            return dt.Rows.Count > 0 ? ConvertToNews(dt.Rows[0]) : null;
        }

        public async void EditNews(NewsModel news) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE News 
        SET Title = $title, Thumbnail_path = $thumbnail, Attachments_path = $attachments, HTML_body = $html, BBCode_body = $bbcode, Tags = $tags
        WHERE Id = $id";
            command.Parameters.AddWithValue("$title", news.Title);
            command.Parameters.AddWithValue("$id", news.Id);
            command.Parameters.AddWithValue("$thumbnail", news.Thumbnail_path);
            command.Parameters.AddWithValue("$attachments", news.Attachments_path);
            command.Parameters.AddWithValue("$html", news.HTML_body);
            command.Parameters.AddWithValue("$bbcode", news.BBCode_body);

            if (news.Tags != null) {
                news.Tags = news.Tags.Replace("'", "\"");
            }
            command.Parameters.AddWithValue("$tags", news.Tags);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<AdminModel>> GetAdmins() {
            List<AdminModel> list = new List<AdminModel>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Admins;";

                using var reader = await command.ExecuteReaderAsync();
                using DataTable dt = new DataTable();
                dt.Load(reader);
                for (int i = 0; i < dt.Rows.Count; i++) {
                    list.Add(ConvertToAdmin(dt.Rows[i]));
                }
            }
            return list;
        }

        public async void EditAdmin(AdminModel oldAdmin, AdminModel newAdmin) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Admins 
        SET Username = $new_username, Password = $new_pass
        WHERE Username = $old_username";
            command.Parameters.AddWithValue("$new_username", newAdmin.Username);
            command.Parameters.AddWithValue("$new_pass", newAdmin.Password);
            command.Parameters.AddWithValue("$old_username", oldAdmin.Username);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async void AddAdmin(AdminModel admin) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Admins (Username, Password, Added_by) VALUES ($username, $pass, $added_by)";
            command.Parameters.AddWithValue("$username", admin.Username.Trim());
            command.Parameters.AddWithValue("$pass", admin.Password.Trim());
            command.Parameters.AddWithValue("$added_by", admin.Added_by);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async void RemoveAdmin(AdminModel admin) {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Admins WHERE Username = $username AND Password = $pass;";
            command.Parameters.AddWithValue("$username", admin.Username);
            command.Parameters.AddWithValue("$pass", admin.Password);
            await command.PrepareAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<NewsModel>> SearchNews(string? search, 
            string[] tags, string[] post_authors, int page, int amountPerPage) {
            List<NewsModel> list = new List<NewsModel>();
            using (var connection = new SqliteConnection(_connectionString)) {
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = CraftSearchCommand(ref search, tags, 
                    post_authors, page, amountPerPage);

                await command.PrepareAsync();

                using var reader = await command.ExecuteReaderAsync();
                using DataTable dt = new DataTable();
                dt.Load(reader);
                for (int i = 0; i < dt.Rows.Count; i++) {
                    list.Add(ConvertToNews(dt.Rows[i]));
                }
            }
            return list;
        }

        private static string CraftSearchCommand(ref string? search,
            string[] tags, string[] post_authors, int page, int amountPerPage) {
            string query = "SELECT * FROM News ";
            query += search != null || tags.Length > 0 || post_authors.Length > 0 ? " WHERE " : "";
            Craft_Title_And_Body_Command(ref query, ref search);
            Craft_Tags_Command(ref query, tags);
            Craft_PostAutors_Command(ref query, post_authors);
            Craft_Page_Command(ref query, page, amountPerPage);
            return query;

        }

        private static string Craft_Title_And_Body_Command(ref string query, 
            ref string? search) {
            if (search != null && search.Trim().Length > 0) {
                search = search.Trim().Replace("'", "''");
                query += @$" (Title LIKE '% {search} %' OR 
                        Title LIKE '{search} %' OR
                        Title LIKE '% {search}' OR
                        Title = '{search}' OR
                        HTML_body LIKE '% {search} %' OR
                        HTML_body LIKE '{search} %' OR
                        HTML_body LIKE '% {search}' OR
                        HTML_body = '{search}') ";
                /*
                query += @" (Title LIKE '% $search %' OR 
                        Title LIKE '$search %' OR
                        Title LIKE '% $search' OR
                        Title = $search OR
                        HTML_body LIKE '% $search %' OR
                        HTML_body LIKE '$search %' OR
                        HTML_body LIKE '% $search' OR
                        HTML_body = $search) ";*/
            }
            return query;
        }

        private static string Craft_Tags_Command(ref string query, string[] tags) {
            if (tags.Length > 0) {
                query += !query.Equals("SELECT * FROM News ") ? " AND ( " : " ( ";

                for (int i = 0; i < tags.Length - 1; i++) {
                    string tag = tags[i];
                    query += $""""
                    Tags LIKE '%;{tag};%' OR  
                    Tags LIKE '{tag};%' OR 
                    Tags LIKE '%;{tag}' OR 
                    Tags = '{tag}' OR 
                    """";
                }

                string lastTag = tags[tags.Length - 1];
                query += $"""
                        Tags LIKE '%;{lastTag};%' OR 
                        Tags LIKE '{lastTag};%' OR 
                        Tags LIKE '%;{lastTag}' OR 
                        Tags = '{lastTag}')
                        """;
            }
            return query;
        }

        private static string Craft_PostAutors_Command(ref string query, string[] post_authors) {
            if (post_authors.Length > 0) {
                query += !query.Equals("SELECT * FROM News ") ? " AND ( " : " ( ";

                for (int i = 0; i < post_authors.Length - 1; i++) {
                    string author = post_authors[i];
                    query += $" Posted_by_Admin_username = '{author}' OR";
                }

                string lastAuthor = post_authors[post_authors.Length - 1];
                query += $" Posted_by_Admin_username = '{lastAuthor}') ";
            }
            return query;
        }

        private static string Craft_Page_Command(ref string query, int page, int amountPerPage) {
            query += $" ORDER BY Posted_on_UTC_timezoned DESC LIMIT {amountPerPage} OFFSET {page * amountPerPage};";
            return query;
        }

        private static NewsModel ConvertToNews(DataRow row) {
            NewsModel newsModel = new NewsModel();
            newsModel.Title = Convert.ToString(row["Title"]);
            newsModel.Id = Convert.ToInt32(row["Id"]);
            newsModel.Thumbnail_path = Convert.ToString(row["Thumbnail_path"]);
            newsModel.Attachments_path = Convert.ToString(row["Attachments_path"]);
            newsModel.HTML_body = Convert.ToString(row["HTML_body"]);
            newsModel.BBCode_body = Convert.ToString(row["BBCode_body"]);
            newsModel.Tags = Convert.ToString(row["Tags"]);
            newsModel.Posted_by_Admin_username = Convert.ToString(row["Posted_by_Admin_username"]);

            newsModel.Posted_on_UTC_timezoned = DateTime.Parse(
                Convert.ToString(row["Posted_on_UTC_timezoned"]));

            return newsModel;
        }

        private static AdminModel ConvertToAdmin(DataRow row) {
            AdminModel admin = new AdminModel();
            admin.Username = Convert.ToString(row["Username"]);
            admin.Password = Convert.ToString(row["Password"]);
            admin.Added_by = Convert.ToString(row["Added_by"]);

            admin.Added_Date_UTC_timezoned = DateTime.Parse(
                Convert.ToString(row["Added_Date_UTC_timezoned"]));
            return admin;
        }
    }
}
