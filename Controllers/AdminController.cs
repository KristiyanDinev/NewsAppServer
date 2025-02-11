using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;

namespace NewsAppServer.Controllers {
    public class AdminController {
        public AdminController(WebApplication app) {
            app.MapPost("/admin/login", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword, [FromForm] string adminUsername) => {

                try {
                        AdminModel loginAdmin = new AdminModel();
                        loginAdmin.Password = adminPassword;
                        loginAdmin.Username = adminUsername;
                        AdminModel? adminModel = await LoginAdmin(db, loginAdmin);
                        
                        return adminModel;

                    } catch (Exception) {
                        return null;
                }

            }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/add", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword,
                [FromForm] string adminUsername,

                [FromForm] string currentAdminUsername, 
                [FromForm] string currentAdminPassword) => {

                    try {
                        AdminModel loginAdmin = new AdminModel();
                        loginAdmin.Password = currentAdminPassword;
                        loginAdmin.Username = currentAdminUsername;

                        AdminModel? adminModel = await LoginAdmin(db, loginAdmin) ?? throw new Exception();

                        AdminModel newAdmin = new AdminModel();
                        newAdmin.Username = adminUsername;
                        newAdmin.Password = adminPassword;
                        newAdmin.Added_by = adminModel.Username;

                        bool did = await db.AddAdmin(newAdmin);
                        return did ? Results.Ok() : Results.Unauthorized();

                    } catch (Exception) {
                        return Results.Unauthorized();

                    } 

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/remove", async (HttpContext http, DatabaseManager db,
                [FromForm] string adminPassword,
                [FromForm] string adminUsername,

                [FromForm] string currentAdminPassword,
                [FromForm] string currentAdminUsername) => {

                    try {
                        AdminModel loginAdmin = new AdminModel();
                        loginAdmin.Password = currentAdminPassword;
                        loginAdmin.Username = currentAdminUsername;

                        AdminModel? adminModel = await LoginAdmin(db, loginAdmin) ?? throw new Exception();

                        AdminModel deleteAdmin = new AdminModel();
                        deleteAdmin.Username = adminUsername;
                        deleteAdmin.Password = adminPassword;
                        db.RemoveAdmin(deleteAdmin);
                        return Results.Ok();

                    } catch (Exception) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");

            app.MapPost("/admin/edit", async (HttpContext http, DatabaseManager db,
                [FromForm] string currentAdminPassword,
                [FromForm] string currentAdminUsername,
                [FromForm] string newAdminPassword,
                [FromForm] string oldAdminPassword,
                [FromForm] string oldAdminUsername,
                [FromForm] string newAdminUsername) => {

                    try {
                        AdminModel loginAdmin = new AdminModel();
                        loginAdmin.Password = currentAdminPassword;
                        loginAdmin.Username = currentAdminUsername;

                        AdminModel? adminModel = await LoginAdmin(db, loginAdmin) ?? throw new Exception();

                        AdminModel oldAdmin = new AdminModel();
                        oldAdmin.Password = oldAdminPassword;
                        oldAdmin.Username = oldAdminUsername;

                        AdminModel newAdmin = new AdminModel();
                        newAdmin.Password = newAdminPassword;
                        newAdmin.Username = newAdminUsername;

                        db.EditAdmin(oldAdmin, newAdmin);
                        return Results.Ok();

                    } catch (Exception) {
                        return Results.Unauthorized();
                    }

                }).DisableAntiforgery()
            .RequireRateLimiting("fixed");
        }

        public static async Task<AdminModel?> LoginAdmin(DatabaseManager db, AdminModel admin) {
            List<AdminModel> admins = await db.GetAdmins();
            foreach (AdminModel a in admins) {
                if (a.Username.Equals(admin.Username) && a.Password.Equals(admin.Password)) {
                    return a;
                }
            }
            return null;
        }
    }
}
