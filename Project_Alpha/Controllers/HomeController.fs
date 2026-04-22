namespace Konditerem.Web.Controllers

open System
open System.Globalization
open Microsoft.AspNetCore.Mvc
open Konditerem.Web.Data

module private Parsing =
    let tryParseDate value =
        match DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None) with
        | true, parsed -> Ok parsed
        | false, _ -> Error "A dátum formátuma legyen yyyy-MM-dd."

type HomeController(repository: IGymRepository) as this =
    inherit Controller()

    member private _.RedirectWithMessage(key: string, message: string) =
        this.TempData.[key] <- message
        this.RedirectToAction("Index")

    member private this.RedirectSuccess message = this.RedirectWithMessage("FlashMessage", message)
    member private this.RedirectError message = this.RedirectWithMessage("FlashError", message)

    member _.Index() =
        let flashMessage = match this.TempData.["FlashMessage"] with | null -> "" | value -> string value
        let flashError = match this.TempData.["FlashError"] with | null -> "" | value -> string value
        repository.GetHomePageViewModel(flashMessage, flashError) |> this.View

    [<HttpPost>]
    member this.CreateUser(name: string, email: string, phone: string, membershipType: string, active: bool) =
        match repository.AddUser(name, email, phone, membershipType, active) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.UpdateUser(id: int, name: string, email: string, phone: string, membershipType: string, active: bool) =
        match repository.UpdateUser(id, name, email, phone, membershipType, active) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.DeleteUser(id: int) =
        match repository.DeleteUser(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.CreateOpeningHours(dayOfWeek: string, openTime: string, closeTime: string) =
        match repository.AddOpeningHours(dayOfWeek, openTime, closeTime) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.UpdateOpeningHours(id: int, dayOfWeek: string, openTime: string, closeTime: string) =
        match repository.UpdateOpeningHours(id, dayOfWeek, openTime, closeTime) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.DeleteOpeningHours(id: int) =
        match repository.DeleteOpeningHours(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.CreateCapacity(date: string, maxCapacity: int, currentCount: int) =
        match Parsing.tryParseDate date with
        | Error error -> this.RedirectError error
        | Ok parsedDate ->
            match repository.AddCapacity(parsedDate, maxCapacity, currentCount) with
            | Ok message -> this.RedirectSuccess message
            | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.UpdateCapacity(id: int, date: string, maxCapacity: int, currentCount: int) =
        match Parsing.tryParseDate date with
        | Error error -> this.RedirectError error
        | Ok parsedDate ->
            match repository.UpdateCapacity(id, parsedDate, maxCapacity, currentCount) with
            | Ok message -> this.RedirectSuccess message
            | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.DeleteCapacity(id: int) =
        match repository.DeleteCapacity(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.IncrementCapacity(id: int) =
        match repository.IncrementCapacity(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.DecrementCapacity(id: int) =
        match repository.DecrementCapacity(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.CreateBooking(userId: int, bookingDate: string, bookingTime: string, status: string) =
        match Parsing.tryParseDate bookingDate with
        | Error error -> this.RedirectError error
        | Ok parsedDate ->
            match repository.AddBooking(userId, parsedDate, bookingTime, status) with
            | Ok message -> this.RedirectSuccess message
            | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.UpdateBooking(id: int, userId: int, bookingDate: string, bookingTime: string, status: string) =
        match Parsing.tryParseDate bookingDate with
        | Error error -> this.RedirectError error
        | Ok parsedDate ->
            match repository.UpdateBooking(id, userId, parsedDate, bookingTime, status) with
            | Ok message -> this.RedirectSuccess message
            | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.DeleteBooking(id: int) =
        match repository.DeleteBooking(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    [<HttpPost>]
    member this.CancelBooking(id: int) =
        match repository.CancelBooking(id) with
        | Ok message -> this.RedirectSuccess message
        | Error error -> this.RedirectError error

    member this.Privacy() = this.View()
