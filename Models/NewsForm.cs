namespace NewsAppServer.Models {
    public class NewsForm {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? PDF_path { get; set; } = null;
        public string? Thumbnail_path { get; set; } = null;
        public string HTML_body { get; set; }
        public string? Tags { get; set; } = null;
        public DateTime Posted_on_UTC_timezored { get; set; } = DateTime.UtcNow;
        public IFormFileCollection Files { get; set; }

        public bool DeleteThumbnail { get; set; } = false;
        public string DeletePDFs { get; set; } = "";
    }
}
