namespace Konditerem.Web

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Konditerem.Web.Data

module Program =
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation() |> ignore
        builder.Services.AddSingleton<IGymRepository, InMemoryGymRepository>() |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error") |> ignore
            app.UseHsts() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseAuthorization() |> ignore

        app.MapControllerRoute(
            name = "default",
            pattern = "{controller=Home}/{action=Index}/{id?}") |> ignore

        app.Run()
        0
