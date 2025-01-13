
using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;

namespace NewsAppServer.Controllers {
    public class AdminController {
        public AdminController(WebApplication app) {
            app.MapPost("/admin/login", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword) => {

                try {
                        bool isAdmin = await CheckAdmin(db, adminPassword);
                        if (!isAdmin) {
                            throw new Exception();
                        }
                    return Results.Ok();

                } catch (Exception) {
                        return Results.Unauthorized();
                }

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/add", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword, [FromForm] string currentAdmin) => {

                    try {
                        bool isAdmin = await CheckAdmin(db, currentAdmin);
                        if (!isAdmin) {
                            throw new Exception();
                        }
                        db.AddAdminPassword(adminPassword);
                        return Results.Ok();

                    } catch (Exception) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/remove", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword, [FromForm] string currentAdmin) => {

                    try {
                        bool isAdmin = await CheckAdmin(db, currentAdmin);
                        if (!isAdmin) {
                            throw new Exception();
                        }
                        db.RemoveAdmin(adminPassword);
                        return Results.Ok();

                    } catch (Exception) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/edit", async (HttpContext http, DatabaseManager db,
                [FromForm] string oldAdminPassword, [FromForm] string currentAdmin,
                [FromForm] string newAdminPassword) => {

                    try {
                        bool isAdmin = await CheckAdmin(db, currentAdmin);
                        if (!isAdmin) {
                            throw new Exception();
                        }
                        db.EditAdminPassword(oldAdminPassword, newAdminPassword);
                        return Results.Ok();

                    } catch (Exception) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }

        private static async Task<bool> CheckAdmin(DatabaseManager db, string pass) {
            List<string> passwords = await db.GetAdminPasswords();
            Console.WriteLine(passwords.First());
            Console.WriteLine(pass);

            return passwords.Contains(pass);
        }
    }
}
