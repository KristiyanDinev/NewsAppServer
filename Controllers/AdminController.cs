
using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;

namespace NewsAppServer.Controllers {
    public class AdminController {
        public AdminController(WebApplication app) {
            app.MapPost("/adminlogin", (HttpContext http, DatabaseManager db, [FromForm] string adminPassword) => {


                try {
                    if (db.GetAdminPasswords().Contains(adminPassword)) {
                        return Results.Ok();
                    }
                    throw new Exception();
                } catch (Exception ex) {
                    return Results.Unauthorized();
                }

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }
    }
}
