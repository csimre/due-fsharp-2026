namespace Konditerem.Web.Data

open System
open System.Globalization
open Konditerem.Web.Models

type IGymRepository =
    abstract member InitializeDatabase: unit -> unit
    abstract member GetHomePageViewModel: string * string -> HomePageViewModel
    abstract member AddUser: string * string * string * string * bool -> Result<string, string>
    abstract member UpdateUser: int * string * string * string * string * bool -> Result<string, string>
    abstract member DeleteUser: int -> Result<string, string>
    abstract member AddOpeningHours: string * string * string -> Result<string, string>
    abstract member UpdateOpeningHours: int * string * string * string -> Result<string, string>
    abstract member DeleteOpeningHours: int -> Result<string, string>
    abstract member AddCapacity: DateTime * int * int -> Result<string, string>
    abstract member UpdateCapacity: int * DateTime * int * int -> Result<string, string>
    abstract member DeleteCapacity: int -> Result<string, string>
    abstract member IncrementCapacity: int -> Result<string, string>
    abstract member DecrementCapacity: int -> Result<string, string>
    abstract member AddBooking: int * DateTime * string * string -> Result<string, string>
    abstract member UpdateBooking: int * int * DateTime * string * string -> Result<string, string>
    abstract member DeleteBooking: int -> Result<string, string>
    abstract member CancelBooking: int -> Result<string, string>

type InMemoryGymRepository() =
    let users = ref []
    let openingHours = ref []
    let capacities = ref []
    let bookings = ref []

    let dateFormat = "yyyy-MM-dd"
    let daysOfWeek = [ "Hétfő"; "Kedd"; "Szerda"; "Csütörtök"; "Péntek"; "Szombat"; "Vasárnap" ]
    let membershipTypes = [ "Napijegy"; "Havi"; "Negyedéves"; "Éves"; "Diák" ]
    let bookingStatuses = [ "active"; "cancelled"; "completed" ]

    let toDateString (value: DateTime) = value.ToString(dateFormat, CultureInfo.InvariantCulture)

    let nextId items getId =
        if List.isEmpty items then 1
        else (items |> List.map getId |> List.max) + 1

    let buildSummary (u: User list) (c: Capacity list) (b: Booking list) =
        let today = DateTime.UtcNow.Date
        let todayCapacity =
            c
            |> List.tryFind (fun item -> item.Date.Date = today)
            |> Option.defaultValue
                { Id = 0; Date = today; MaxCapacity = 0; CurrentCount = 0; AvailableSpots = 0; IsFull = false }

        { TotalUsers = u.Length
          ActiveUsers = u |> List.filter (fun item -> item.Active) |> List.length
          ActiveBookings = b |> List.filter (fun item -> item.Status = "active") |> List.length
          CapacityToday = todayCapacity.CurrentCount
          AvailableToday = todayCapacity.AvailableSpots }

    interface IGymRepository with
        member _.InitializeDatabase() =
            // Dummy kezdeti adatok a prototipushoz
            users.Value <- [
                { Id = 1; Name = "Kovács Péter"; Email = "kovacs.peter@email.com"; Phone = "+36301234567"; MembershipType = "Havi"; RegistrationDate = DateTime.UtcNow.AddDays(-30.0); Active = true }
                { Id = 2; Name = "Nagy Anna"; Email = "nagy.anna@email.com"; Phone = "+36209876543"; MembershipType = "Éves"; RegistrationDate = DateTime.UtcNow.AddDays(-150.0); Active = true }
                { Id = 3; Name = "Szabó Gábor"; Email = "szabo.gabor@email.com"; Phone = "+36701112233"; MembershipType = "Napijegy"; RegistrationDate = DateTime.UtcNow.AddDays(-2.0); Active = false }
            ]
            
            openingHours.Value <- [
                { Id = 1; DayOfWeek = "Hétfő"; OpenTime = "06:00"; CloseTime = "22:00" }
                { Id = 2; DayOfWeek = "Kedd"; OpenTime = "06:00"; CloseTime = "22:00" }
                { Id = 3; DayOfWeek = "Szerda"; OpenTime = "06:00"; CloseTime = "22:00" }
                { Id = 4; DayOfWeek = "Csütörtök"; OpenTime = "06:00"; CloseTime = "22:00" }
                { Id = 5; DayOfWeek = "Péntek"; OpenTime = "06:00"; CloseTime = "22:00" }
                { Id = 6; DayOfWeek = "Szombat"; OpenTime = "08:00"; CloseTime = "20:00" }
                { Id = 7; DayOfWeek = "Vasárnap"; OpenTime = "08:00"; CloseTime = "20:00" }
            ]
            
            let today = DateTime.UtcNow.Date
            capacities.Value <- [
                { Id = 1; Date = today; MaxCapacity = 50; CurrentCount = 12; AvailableSpots = 38; IsFull = false }
                { Id = 2; Date = today.AddDays(1.0); MaxCapacity = 50; CurrentCount = 0; AvailableSpots = 50; IsFull = false }
            ]
            
            bookings.Value <- [
                { Id = 1; UserId = 1; UserName = "Kovács Péter"; BookingDate = today; BookingTime = "17:00"; Status = "active" }
                { Id = 2; UserId = 2; UserName = "Nagy Anna"; BookingDate = today; BookingTime = "18:30"; Status = "active" }
            ]

        member _.GetHomePageViewModel(flashMessage, flashError) =
            let u = users.Value
            let oh = openingHours.Value
            let c = capacities.Value
            let b = bookings.Value
            { Today = toDateString DateTime.UtcNow
              Summary = buildSummary u c b
              Users = u
              OpeningHours = oh
              Capacities = c
              Bookings = b
              DaysOfWeek = daysOfWeek
              MembershipTypes = membershipTypes
              BookingStatuses = bookingStatuses
              FlashMessage = flashMessage
              FlashError = flashError }

        member _.AddUser(name, email, phone, membershipType, active) =
            let newUser = { Id = nextId users.Value (fun u -> u.Id); Name = name; Email = email; Phone = phone; MembershipType = membershipType; RegistrationDate = DateTime.UtcNow; Active = active }
            users.Value <- newUser :: users.Value
            Ok "Felhasználó sikeresen létrehozva."

        member _.UpdateUser(id, name, email, phone, membershipType, active) =
            match users.Value |> List.tryFind (fun u -> u.Id = id) with
            | Some user ->
                let updatedUser = { user with Name = name; Email = email; Phone = phone; MembershipType = membershipType; Active = active }
                users.Value <- users.Value |> List.map (fun u -> if u.Id = id then updatedUser else u)
                Ok "Felhasználó sikeresen frissítve."
            | None -> Error "Felhasználó nem található."

        member _.DeleteUser(id) =
            users.Value <- users.Value |> List.filter (fun u -> u.Id <> id)
            Ok "Felhasználó törölve."

        member _.AddOpeningHours(dayOfWeek, openTime, closeTime) =
            let newHours = { Id = nextId openingHours.Value (fun o -> o.Id); DayOfWeek = dayOfWeek; OpenTime = openTime; CloseTime = closeTime }
            openingHours.Value <- newHours :: openingHours.Value
            Ok "Nyitvatartás sikeresen létrehozva."

        member _.UpdateOpeningHours(id, dayOfWeek, openTime, closeTime) =
            match openingHours.Value |> List.tryFind (fun o -> o.Id = id) with
            | Some _ ->
                let updatedHours = { Id = id; DayOfWeek = dayOfWeek; OpenTime = openTime; CloseTime = closeTime }
                openingHours.Value <- openingHours.Value |> List.map (fun o -> if o.Id = id then updatedHours else o)
                Ok "Nyitvatartás sikeresen frissítve."
            | None -> Error "Nyitvatartás nem található."

        member _.DeleteOpeningHours(id) =
            openingHours.Value <- openingHours.Value |> List.filter (fun o -> o.Id <> id)
            Ok "Nyitvatartás törölve."

        member _.AddCapacity(date, maxCapacity, currentCount) =
            let newCapacity = { Id = nextId capacities.Value (fun c -> c.Id); Date = date; MaxCapacity = maxCapacity; CurrentCount = currentCount; AvailableSpots = maxCapacity - currentCount; IsFull = currentCount >= maxCapacity }
            capacities.Value <- newCapacity :: capacities.Value
            Ok "Kapacitás sikeresen rögzítve."

        member _.UpdateCapacity(id, date, maxCapacity, currentCount) =
            match capacities.Value |> List.tryFind (fun c -> c.Id = id) with
            | Some _ ->
                let updatedCapacity = { Id = id; Date = date; MaxCapacity = maxCapacity; CurrentCount = currentCount; AvailableSpots = maxCapacity - currentCount; IsFull = currentCount >= maxCapacity }
                capacities.Value <- capacities.Value |> List.map (fun c -> if c.Id = id then updatedCapacity else c)
                Ok "Kapacitás sikeresen frissítve."
            | None -> Error "Kapacitás nem található."

        member _.DeleteCapacity(id) =
            capacities.Value <- capacities.Value |> List.filter (fun c -> c.Id <> id)
            Ok "Kapacitás törölve."

        member _.IncrementCapacity(id) =
            match capacities.Value |> List.tryFind (fun c -> c.Id = id) with
            | Some c when c.CurrentCount < c.MaxCapacity ->
                let updatedCapacity = { c with CurrentCount = c.CurrentCount + 1; AvailableSpots = c.MaxCapacity - (c.CurrentCount + 1); IsFull = (c.CurrentCount + 1) >= c.MaxCapacity }
                capacities.Value <- capacities.Value |> List.map (fun cap -> if cap.Id = id then updatedCapacity else cap)
                Ok "Létszám megnövelve."
            | Some _ -> Error "A terem megtelt!"
            | None -> Error "Kapacitás nem található."

        member _.DecrementCapacity(id) =
            match capacities.Value |> List.tryFind (fun c -> c.Id = id) with
            | Some c when c.CurrentCount > 0 ->
                let updatedCapacity = { c with CurrentCount = c.CurrentCount - 1; AvailableSpots = c.MaxCapacity - (c.CurrentCount - 1); IsFull = false }
                capacities.Value <- capacities.Value |> List.map (fun cap -> if cap.Id = id then updatedCapacity else cap)
                Ok "Létszám csökkentve."
            | Some _ -> Error "A létszám már 0."
            | None -> Error "Kapacitás nem található."

        member _.AddBooking(userId, bookingDate, bookingTime, status) =
            match users.Value |> List.tryFind (fun u -> u.Id = userId) with
            | Some user ->
                let newBooking = { Id = nextId bookings.Value (fun b -> b.Id); UserId = userId; UserName = user.Name; BookingDate = bookingDate; BookingTime = bookingTime; Status = status }
                bookings.Value <- newBooking :: bookings.Value
                Ok "Foglalás sikeresen létrehozva."
            | None -> Error "Felhasználó nem található."

        member _.UpdateBooking(id, userId, bookingDate, bookingTime, status) =
            match bookings.Value |> List.tryFind (fun b -> b.Id = id), users.Value |> List.tryFind (fun u -> u.Id = userId) with
            | Some _, Some user ->
                let updatedBooking = { Id = id; UserId = userId; UserName = user.Name; BookingDate = bookingDate; BookingTime = bookingTime; Status = status }
                bookings.Value <- bookings.Value |> List.map (fun b -> if b.Id = id then updatedBooking else b)
                Ok "Foglalás sikeresen frissítve."
            | None, _ -> Error "Foglalás nem található."
            | _, None -> Error "Felhasználó nem található."

        member _.DeleteBooking(id) =
            bookings.Value <- bookings.Value |> List.filter (fun b -> b.Id <> id)
            Ok "Foglalás törölve."

        member _.CancelBooking(id) =
            match bookings.Value |> List.tryFind (fun b -> b.Id = id) with
            | Some b ->
                let updatedBooking = { b with Status = "cancelled" }
                bookings.Value <- bookings.Value |> List.map (fun bk -> if bk.Id = id then updatedBooking else bk)
                Ok "Foglalás lemondva."
            | None -> Error "Foglalás nem található."
