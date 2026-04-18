namespace Konditerem.Web.Controllers
open Microsoft.AspNetCore.Mvc

type HomeController () =
    inherit Controller()
    member this.Index () = this.View()
    member this.Privacy () = this.View()
