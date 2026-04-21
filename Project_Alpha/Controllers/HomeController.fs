namespace Konditerem.Web.Controllers

open System
open System.Diagnostics
open Microsoft.AspNetCore.Mvc
open Konditerem.Web.Models
open Konditerem.Web.Data

type HomeController(gymRepository: IGymRepository) =
    inherit Controller()

    [<HttpGet>]
    member this.Index() =
        let msg = match this.TempData.["FlashMessage"] with | null -> "" | v -> string v
        let err = match this.TempData.["FlashError"] with | null -> "" | v -> string v
        let model = gymRepository.GetHomePageViewModel(msg, err)
        this.View(model)

    [<HttpPost>]
    member this.CreateUser(name: string, email: string, phone: string, membershipType: string, active: bool) =
        match gymRepository.AddUser(name, email, phone, membershipType, active) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.UpdateUser(id: int, name: string, email: string, phone: string, membershipType: string, active: bool) =
        match gymRepository.UpdateUser(id, name, email, phone, membershipType, active) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.DeleteUser(id: int) =
        match gymRepository.DeleteUser(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.CreateOpeningHours(dayOfWeek: string, openTime: string, closeTime: string) =
        match gymRepository.AddOpeningHours(dayOfWeek, openTime, closeTime) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.UpdateOpeningHours(id: int, dayOfWeek: string, openTime: string, closeTime: string) =
        match gymRepository.UpdateOpeningHours(id, dayOfWeek, openTime, closeTime) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.DeleteOpeningHours(id: int) =
        match gymRepository.DeleteOpeningHours(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.CreateCapacity(date: DateTime, maxCapacity: int, currentCount: int) =
        match gymRepository.AddCapacity(date, maxCapacity, currentCount) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.UpdateCapacity(id: int, date: DateTime, maxCapacity: int, currentCount: int) =
        match gymRepository.UpdateCapacity(id, date, maxCapacity, currentCount) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.DeleteCapacity(id: int) =
        match gymRepository.DeleteCapacity(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.IncrementCapacity(id: int) =
        match gymRepository.IncrementCapacity(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.DecrementCapacity(id: int) =
        match gymRepository.DecrementCapacity(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.CreateBooking(userId: int, bookingDate: DateTime, bookingTime: string, status: string) =
        match gymRepository.AddBooking(userId, bookingDate, bookingTime, status) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.UpdateBooking(id: int, userId: int, bookingDate: DateTime, bookingTime: string, status: string) =
        match gymRepository.UpdateBooking(id, userId, bookingDate, bookingTime, status) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.DeleteBooking(id: int) =
        match gymRepository.DeleteBooking(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<HttpPost>]
    member this.CancelBooking(id: int) =
        match gymRepository.CancelBooking(id) with
        | Ok msg -> this.TempData.["FlashMessage"] <- msg
        | Error err -> this.TempData.["FlashError"] <- err
        this.RedirectToAction("Index")

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error() =
        let reqId = if isNull Activity.Current then this.HttpContext.TraceIdentifier else Activity.Current.Id
        this.View( { RequestId = reqId } )