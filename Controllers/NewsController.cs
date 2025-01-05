namespace NewsAppServer.Controllers {
    public class NewsController {

        public NewsController(WebApplication app) { 
            app.MapGet("/news/{limit:int?}", (HttpContext http, int limit = 0) => { 
                
            });

            app.MapPost("/news", (HttpContext http, string bbcode) => {
                Console.WriteLine(bbcode);
            });
        }
    }
}
