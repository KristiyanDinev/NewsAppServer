package we.newsapp.newsappserver.models;


import java.time.ZonedDateTime;

public class News {
    public int id;
    public String title;
    public String base64Thumbnail;
    public String pdf_path;
    public String html_body;
    public ZonedDateTime posted_on_zonedDateTime;
}
