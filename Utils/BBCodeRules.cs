namespace NewsAppServer.Utils {
    public class BBCodeRules {
        public string BBCodeSyntax { get; set; }
        public string HtmlSyntax { get; set; }
        public string Fields { get; set; }

        public BBCodeRules() { }

        public BBCodeRules(string bBCodeSyntax, string htmlSyntax, string fields) {
            BBCodeSyntax = bBCodeSyntax;
            HtmlSyntax = htmlSyntax;
            Fields = fields;
        }
    }
}
