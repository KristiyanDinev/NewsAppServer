
using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;

namespace NewsAppServer.Controllers {
    public class AdminController {
        public AdminController(WebApplication app) {
            app.MapPost("/admin/login", (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword) => {

                try {
                    CheckAdmin(db, adminPassword);
                    return Results.Ok();

                } catch (Exception ex) {
                    return Results.Unauthorized();
                }

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/add", (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword, [FromForm] string currentAdmin) => {

                    try {
                        CheckAdmin(db, currentAdmin);
                        db.AddAdminPassword(adminPassword);
                        return Results.Ok();

                    } catch (Exception ex) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/remove", (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword, [FromForm] string currentAdmin) => {

                    try {
                        CheckAdmin(db, currentAdmin);
                        db.RemoveAdmin(adminPassword);
                        return Results.Ok();

                    } catch (Exception ex) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/edit", (HttpContext http, DatabaseManager db,
                [FromForm] string oldAdminPassword, [FromForm] string currentAdmin,
                [FromForm] string newAdminPassword) => {

                    try {
                        CheckAdmin(db, currentAdmin);
                        db.EditAdminPassword(oldAdminPassword, newAdminPassword);
                        return Results.Ok();

                    } catch (Exception ex) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }

        private async void CheckAdmin(DatabaseManager db, string pass) {
            List<string> passwords = await db.GetAdminPasswords();
            if (!passwords.Contains(pass)) {
                throw new Exception();
            }
        }
    }
}
