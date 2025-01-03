package we.newsapp.newsappserver.database;

import jakarta.annotation.Nullable;
import org.springframework.lang.NonNull;
import we.newsapp.newsappserver.models.News;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class DatabaseManager {
    public String connection_string = "jdbc:sqlite:database.sqlite";

    @Nullable
    private Connection connect() {
        // connection string
        try {
            return DriverManager.getConnection(connection_string);
        } catch (Exception e) {
            System.out.println(e.getMessage());
            return null;
        }
    }

    public void setup() throws Exception {
        try(Connection connection = connect()) {
            if (connection == null) {
                throw new Exception("Can't connect to database");
            }
            connection.prepareStatement("CREATE TABLE IF NOT EXISTS News (Id INTEGER PRIMARY KEY, Title VARCHAR(255) NOT NULL, Thumbnail_base64 VARCHAR, PDF_path VARCHAR NOT NULL, HTML_body VARCHAR NOT NULL, Posted_on DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);")
                .executeUpdate();
            connection.prepareStatement("CREATE TABLE IF NOT EXISTS Admins (Password VARCHAR NOT NULL);")
                    .executeUpdate();
        }
    }

    public List<String> getAdminPasswords() throws Exception {
        try(Connection connection = connect()) {
            if (connection == null) {
                throw new Exception("Can't connect to database");
            }
            ResultSet resultSet = connection.prepareStatement("SELECT * FROM Admins;")
                    .executeQuery();
            List<String> passwords = new ArrayList<>();
            while (resultSet.next()) {
                passwords.add(resultSet.getString(1));
            }
            return passwords;
        }
    }

    public void addNews(News news) throws Exception {
        try(Connection connection = connect()) {
            if (connection == null) {
                throw new Exception("Can't connect to database");
            }
            PreparedStatement preparedStatement = connection.prepareStatement("INSERT INTO News (Title, Thumbnail_base64, PDF_path, HTML_body) VALUES (?, ?, ?, ?);");
            preparedStatement.setString(1, news.title);
            preparedStatement.setString(2, news.base64Thumbnail);
            preparedStatement.setString(3, news.pdf_path);
            preparedStatement.setString(4, news.html_body);
            preparedStatement.executeUpdate();
        }
    }

    public void deleteNews(int id) throws Exception {
        try(Connection connection = connect()) {
            if (connection == null) {
                throw new Exception("Can't connect to database");
            }
            PreparedStatement preparedStatement = connection.prepareStatement("DELETE FROM News WHERE Id = ?;");
            preparedStatement.setInt(1, id);
            preparedStatement.executeUpdate();
        }
    }
}
