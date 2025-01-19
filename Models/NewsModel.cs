namespace NewsAppServer.Models {
    public class NewsModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Thumbnail_path { get; set; } = null;
        public string? PDF_path { get; set; } = null;
        public string HTML_body { get; set; }
        public string? Tags { get; set; } = null;
        public string Posted_by_Admin_username { get; set; }
        public DateTime Posted_on_UTC_timezoned { get; set; } = DateTime.UtcNow;
    }
}
