namespace NewsAppServer.Models {
    public class NewsModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Thumbnail_path { get; set; } = null;
        public string? Attachments_path { get; set; } = null;
        public string HTML_body { get; set; }
        public string BBCode_body { get; set; }
        public string? Tags { get; set; } = null;
        public string Posted_by_Admin_username { get; set; }
        public DateTime Posted_on { get; set; } = DateTime.UtcNow;
        public bool IsFav { get; set; } = false;
    }
}
