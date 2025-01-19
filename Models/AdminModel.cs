namespace NewsAppServer.Models {
    public class AdminModel {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Added_by { get; set; }
        public DateTime Added_Date_UTC_timezoned { get; set; } = DateTime.UtcNow;
    }
}
