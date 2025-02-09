using Microsoft.AspNetCore.Mvc;
using NewsAppServer.Database;
using NewsAppServer.Models;
using NewsAppServer.Utils;
using System.Reflection;

namespace NewsAppServer.Controllers {
    public class WebController {

        public WebController(WebApplication app) {

            // main page / show news if has data
            app.MapGet("/", async (HttpContext context) => {
                try {

                    return Results.Content(await ControllerUtils.GetIndexHTML(""),
                        "text/html");

                } catch (Exception) { 
                    return Results.BadRequest();
                }
            });

            // page about this one post
            app.MapGet("/news/{id:int}", async (HttpContext context,
                DatabaseManager db, int id) => {
                try {

                       NewsModel? model = await db.GetNewsByID(id);

                        if (model == null) {

                            return Results.Redirect("/");
                        }
                    string a = await ControllerUtils.GetIndexHTML("/news");

                        ControllerUtils._handleEntryInFile(ref a, model);

                    return Results.Content(a, "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // search page
            app.MapGet("/search", async (HttpContext context) => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/search"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // options page
            app.MapGet("/options", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/options"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });


            //  about app
            app.MapGet("/options/aboutapp", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/aboutapp"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });


            // contact us
            app.MapGet("/options/contactus", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/contactus"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // saved news (page)
            app.MapGet("/options/savednews", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/savednews"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // saved news (json)
            app.MapPost("/options/savednews", (HttpContext context) => {

                Dictionary<string, object> res = new Dictionary<string, object>();
                
                res.Add("savednews",
                    ControllerUtils.Get_NewsModels_From_SavedNewsSession(context.Session));

                return res;


            }).DisableAntiforgery().RequireRateLimiting("fixed");

            // admin login (page)
            app.MapGet("/options/adminlogin", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/adminlogin"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // admin page
            app.MapGet("/admin", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/admin"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });

            // admin edit news page
            app.MapGet("/admin/editnews", async () => {
                try {

                    return Results.Content(
                        await ControllerUtils.GetIndexHTML("/adminedit_news"),
                        "text/html");

                } catch (Exception) {
                    return Results.BadRequest();
                }
            });
        }
    }
}
