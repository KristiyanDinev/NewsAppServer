
using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;

namespace NewsAppServer.Controllers {
    public class AdminController {
        public AdminController(WebApplication app) {
            app.MapPost("/adminlogin", async (HttpContext http, DatabaseManager db, [FromForm] string adminPassword) => {


                try {
                    List<string> passwords = await db.GetAdminPasswords();
                    if (passwords.Contains(adminPassword)) {
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
