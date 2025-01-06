namespace NewsAppServer.Models {
    public class NewsModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Thumbnail_path { get; set; }
        public string PDF_path { get; set; }
        public string HTML_body { get; set; }
        public string Tags { get; set; }
        public DateTime Posted_on_UTC_timezored { get; set; } = DateTime.UtcNow;
    }
}
