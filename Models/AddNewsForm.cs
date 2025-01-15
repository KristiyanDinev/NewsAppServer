namespace NewsAppServer.Models {
    public class AddNewsForm {
        public string Title { get; set; }
        public string HTML_body { get; set; }
        public string? Tags { get; set; } = null;

    }
}
