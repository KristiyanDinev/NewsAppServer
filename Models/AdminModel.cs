namespace NewsAppServer.Models {
    public class AdminModel {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Added_by { get; set; }
        public DateTime Added_Date { get; set; } = DateTime.UtcNow;
    }
}
