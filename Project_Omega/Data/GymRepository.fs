namespace Konditerem.Web.Data

open System
open System.Globalization
open System.IO
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Configuration
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

type SqliteGymRepository(configuration: IConfiguration) =
    let dateFormat = "yyyy-MM-dd"
    let daysOfWeek =
        [ "Hétfő"
          "Kedd"
          "Szerda"
          "Csütörtök"
          "Péntek"
          "Szombat"
          "Vasárnap" ]

    let membershipTypes = [ "Napijegy"; "Havi"; "Negyedéves"; "Éves"; "Diák" ]
    let bookingStatuses = [ "active"; "cancelled"; "completed" ]

    let configuredPath =
        match configuration.["Database:Path"] with
        | null
        | "" -> Path.Combine("App_Data", "konditerem.db")
        | path -> path

    let databasePath =
        if Path.IsPathRooted(configuredPath) then
            configuredPath
        else
            Path.Combine(Directory.GetCurrentDirectory(), configuredPath)

    let connectionString = $"Data Source={databasePath}"

    let toDateString (value: DateTime) = value.ToString(dateFormat, CultureInfo.InvariantCulture)

    let parseDate (value: string) = DateTime.ParseExact(value, dateFormat, CultureInfo.InvariantCulture)

    let normalizeText (value: string) =
        if String.IsNullOrWhiteSpace(value) then
            ""
        else
            value.Trim()

    let validateRequired label value =
        if String.IsNullOrWhiteSpace(value) then
            Error $"A(z) {label} megadása kötelező."
        else
            Ok(normalizeText value)

    let validateEmail email =
        let normalized = normalizeText email

        if String.IsNullOrWhiteSpace(normalized) || not (normalized.Contains("@")) then
            Error "Érvényes email cím megadása kötelező."
        else
            Ok(normalized.ToLowerInvariant())

    let validateCapacity maxCapacity currentCount =
        if maxCapacity < 0 then
            Error "A maximális kapacitás nem lehet negatív."
        elif currentCount < 0 then
            Error "Az aktuális létszám nem lehet negatív."
        elif currentCount > maxCapacity then
            Error "Az aktuális létszám nem lehet nagyobb a maximális kapacitásnál."
        else
            Ok()

    let validateTimeRange openTime closeTime =
        let parseTime raw =
            match TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None) with
            | true, parsed -> Ok parsed
            | false, _ -> Error "Az időformátum legyen HH:mm."

        match parseTime openTime, parseTime closeTime with
        | Ok openAt, Ok closeAt when closeAt > openAt -> Ok()
        | Ok _, Ok _ -> Error "A zárási időnek később kell lennie, mint a nyitási időnek."
        | Error error, _ -> Error error
        | _, Error error -> Error error

    let ensureKnownDay dayOfWeek =
        if daysOfWeek |> List.contains dayOfWeek then
            Ok dayOfWeek
        else
            Error "Ismeretlen hétköznap érték."

    let ensureKnownStatus status =
        if bookingStatuses |> List.contains status then
            Ok status
        else
            Error "Ismeretlen foglalási státusz."

    let openConnection () =
        let connection = new SqliteConnection(connectionString)
        connection.Open()

        use pragma = connection.CreateCommand()
        pragma.CommandText <- "PRAGMA foreign_keys = ON;"
        pragma.ExecuteNonQuery() |> ignore

        connection

    let addParam (command: SqliteCommand) name value =
        command.Parameters.AddWithValue(name, value) |> ignore

    let executeNonQuery sql configure =
        use connection = openConnection ()
        use command = connection.CreateCommand()
        command.CommandText <- sql
        configure command
        command.ExecuteNonQuery()

    let executeScalarInt sql configure =
        use connection = openConnection ()
        use command = connection.CreateCommand()
        command.CommandText <- sql
        configure command
        command.ExecuteScalar() |> Convert.ToInt32

    let queryList sql configure projector =
        use connection = openConnection ()
        use command = connection.CreateCommand()
        command.CommandText <- sql
        configure command

        use reader = command.ExecuteReader()
        let results = ResizeArray<_>()

        while reader.Read() do
            results.Add(projector reader)

        List.ofSeq results

    let ensureUserExists userId =
        let count =
            executeScalarInt
                "SELECT COUNT(1) FROM users WHERE id = $id AND active = 1;"
                (fun command -> addParam command "$id" userId)

        count > 0

    let ensureBookingUnique bookingId userId bookingDate bookingTime =
        let sql =
            "SELECT COUNT(1) FROM bookings WHERE user_id = $userId AND booking_date = $bookingDate AND booking_time = $bookingTime AND status = 'active' AND id <> $bookingId;"

        let duplicates =
            executeScalarInt
                sql
                (fun command ->
                    addParam command "$userId" userId
                    addParam command "$bookingDate" (toDateString bookingDate)
                    addParam command "$bookingTime" bookingTime
                    addParam command "$bookingId" bookingId)

        duplicates = 0

    let readUsers () =
        queryList
            "SELECT id, name, email, phone, membership_type, registration_date, active FROM users ORDER BY id DESC;"
            ignore
            (fun reader ->
                { Id = reader.GetInt32(0)
                  Name = reader.GetString(1)
                  Email = reader.GetString(2)
                  Phone = reader.GetString(3)
                  MembershipType = reader.GetString(4)
                  RegistrationDate = parseDate (reader.GetString(5))
                  Active = reader.GetInt64(6) = 1L })

    let readOpeningHours () =
        queryList
            "SELECT id, day_of_week, open_time, close_time FROM opening_hours ORDER BY id ASC;"
            ignore
            (fun reader ->
                { Id = reader.GetInt32(0)
                  DayOfWeek = reader.GetString(1)
                  OpenTime = reader.GetString(2)
                  CloseTime = reader.GetString(3) })

    let readCapacities () =
        queryList
            "SELECT id, date, max_capacity, current_count FROM capacity ORDER BY date DESC;"
            ignore
            (fun reader ->
                let maxCapacity = reader.GetInt32(2)
                let currentCount = reader.GetInt32(3)

                { Id = reader.GetInt32(0)
                  Date = parseDate (reader.GetString(1))
                  MaxCapacity = maxCapacity
                  CurrentCount = currentCount
                  AvailableSpots = max 0 (maxCapacity - currentCount)
                  IsFull = currentCount >= maxCapacity })

    let readBookings () =
        queryList
            "SELECT b.id, b.user_id, u.name, b.booking_date, b.booking_time, b.status FROM bookings b INNER JOIN users u ON u.id = b.user_id ORDER BY b.booking_date DESC, b.booking_time ASC;"
            ignore
            (fun reader ->
                { Id = reader.GetInt32(0)
                  UserId = reader.GetInt32(1)
                  UserName = reader.GetString(2)
                  BookingDate = parseDate (reader.GetString(3))
                  BookingTime = reader.GetString(4)
                  Status = reader.GetString(5) })

    let buildSummary (users: User list) (capacities: Capacity list) (bookings: Booking list) =
        let today = DateTime.UtcNow.Date

        let todayCapacity =
            capacities
            |> List.tryFind (fun item -> item.Date.Date = today)
            |> Option.defaultValue
                { Id = 0
                  Date = today
                  MaxCapacity = 0
                  CurrentCount = 0
                  AvailableSpots = 0
                  IsFull = false }

        { TotalUsers = users.Length
          ActiveUsers = users |> List.filter (fun item -> item.Active) |> List.length
          ActiveBookings = bookings |> List.filter (fun item -> item.Status = "active") |> List.length
          CapacityToday = todayCapacity.CurrentCount
          AvailableToday = todayCapacity.AvailableSpots }

    let protectWrite work successMessage =
        try
            work ()
            Ok successMessage
        with ex ->
            Error ex.Message

    interface IGymRepository with
        member _.InitializeDatabase() =
            let folder = Path.GetDirectoryName(databasePath)

            if not (String.IsNullOrWhiteSpace(folder)) then
                Directory.CreateDirectory(folder) |> ignore

            use connection = openConnection ()
            use command = connection.CreateCommand()

            command.CommandText <-
                """
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    email TEXT NOT NULL UNIQUE,
                    phone TEXT NOT NULL DEFAULT '',
                    membership_type TEXT NOT NULL DEFAULT '',
                    registration_date TEXT NOT NULL,
                    active INTEGER NOT NULL DEFAULT 1 CHECK (active IN (0, 1))
                );

                CREATE TABLE IF NOT EXISTS opening_hours (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    day_of_week TEXT NOT NULL UNIQUE,
                    open_time TEXT NOT NULL,
                    close_time TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS capacity (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    date TEXT NOT NULL UNIQUE,
                    max_capacity INTEGER NOT NULL CHECK (max_capacity >= 0),
                    current_count INTEGER NOT NULL DEFAULT 0 CHECK (current_count >= 0 AND current_count <= max_capacity)
                );

                CREATE TABLE IF NOT EXISTS bookings (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    booking_date TEXT NOT NULL,
                    booking_time TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'active',
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_bookings_user_id ON bookings(user_id);
                CREATE INDEX IF NOT EXISTS idx_bookings_date ON bookings(booking_date);
                """

            command.ExecuteNonQuery() |> ignore

            for (dayOfWeek, openTime, closeTime) in
                [ ("Hétfő", "06:00", "22:00")
                  ("Kedd", "06:00", "22:00")
                  ("Szerda", "06:00", "22:00")
                  ("Csütörtök", "06:00", "22:00")
                  ("Péntek", "06:00", "22:00")
                  ("Szombat", "08:00", "20:00")
                  ("Vasárnap", "08:00", "18:00") ] do
                use seedCommand = connection.CreateCommand()
                seedCommand.CommandText <-
                    "INSERT INTO opening_hours (day_of_week, open_time, close_time) SELECT $dayOfWeek, $openTime, $closeTime WHERE NOT EXISTS (SELECT 1 FROM opening_hours WHERE day_of_week = $dayOfWeek);"

                addParam seedCommand "$dayOfWeek" dayOfWeek
                addParam seedCommand "$openTime" openTime
                addParam seedCommand "$closeTime" closeTime
                seedCommand.ExecuteNonQuery() |> ignore

        member _.GetHomePageViewModel(flashMessage, flashError) =
            let users = readUsers ()
            let openingHours = readOpeningHours ()
            let capacities = readCapacities ()
            let bookings = readBookings ()

            { Summary = buildSummary users capacities bookings
              Users = users
              OpeningHours = openingHours
              Capacities = capacities
              Bookings = bookings
              FlashMessage = flashMessage
              FlashError = flashError
              Today = toDateString DateTime.UtcNow.Date
              DaysOfWeek = daysOfWeek
              MembershipTypes = membershipTypes
              BookingStatuses = bookingStatuses }

        member _.AddUser(name, email, phone, membershipType, active) =
            protectWrite
                (fun () ->
                    let validatedName =
                        match validateRequired "név" name with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    let validatedEmail =
                        match validateEmail email with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    executeNonQuery
                        "INSERT INTO users (name, email, phone, membership_type, registration_date, active) VALUES ($name, $email, $phone, $membershipType, $registrationDate, $active);"
                        (fun command ->
                            addParam command "$name" validatedName
                            addParam command "$email" validatedEmail
                            addParam command "$phone" (normalizeText phone)
                            addParam command "$membershipType" (normalizeText membershipType)
                            addParam command "$registrationDate" (toDateString DateTime.UtcNow.Date)
                            addParam command "$active" (if active then 1 else 0))
                    |> ignore)
                "Felhasználó sikeresen létrehozva."

        member _.UpdateUser(id, name, email, phone, membershipType, active) =
            protectWrite
                (fun () ->
                    let validatedName =
                        match validateRequired "név" name with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    let validatedEmail =
                        match validateEmail email with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    let affected =
                        executeNonQuery
                            "UPDATE users SET name = $name, email = $email, phone = $phone, membership_type = $membershipType, active = $active WHERE id = $id;"
                            (fun command ->
                                addParam command "$id" id
                                addParam command "$name" validatedName
                                addParam command "$email" validatedEmail
                                addParam command "$phone" (normalizeText phone)
                                addParam command "$membershipType" (normalizeText membershipType)
                                addParam command "$active" (if active then 1 else 0))

                    if affected = 0 then
                        invalidOp "A kiválasztott felhasználó nem található."
                    else
                        ())
                "Felhasználó sikeresen frissítve."

        member _.DeleteUser(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "DELETE FROM users WHERE id = $id;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A kiválasztott felhasználó nem található."
                    else
                        ())
                "Felhasználó törölve."

        member _.AddOpeningHours(dayOfWeek, openTime, closeTime) =
            protectWrite
                (fun () ->
                    let validDay =
                        match ensureKnownDay (normalizeText dayOfWeek) with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    match validateTimeRange (normalizeText openTime) (normalizeText closeTime) with
                    | Ok () -> ()
                    | Error error -> invalidOp error

                    executeNonQuery
                        "INSERT INTO opening_hours (day_of_week, open_time, close_time) VALUES ($dayOfWeek, $openTime, $closeTime);"
                        (fun command ->
                            addParam command "$dayOfWeek" validDay
                            addParam command "$openTime" (normalizeText openTime)
                            addParam command "$closeTime" (normalizeText closeTime))
                    |> ignore)
                "Nyitvatartás létrehozva."

        member _.UpdateOpeningHours(id, dayOfWeek, openTime, closeTime) =
            protectWrite
                (fun () ->
                    let validDay =
                        match ensureKnownDay (normalizeText dayOfWeek) with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    match validateTimeRange (normalizeText openTime) (normalizeText closeTime) with
                    | Ok () -> ()
                    | Error error -> invalidOp error

                    let affected =
                        executeNonQuery
                            "UPDATE opening_hours SET day_of_week = $dayOfWeek, open_time = $openTime, close_time = $closeTime WHERE id = $id;"
                            (fun command ->
                                addParam command "$id" id
                                addParam command "$dayOfWeek" validDay
                                addParam command "$openTime" (normalizeText openTime)
                                addParam command "$closeTime" (normalizeText closeTime))

                    if affected = 0 then
                        invalidOp "A kiválasztott nyitvatartás nem található."
                    else
                        ())
                "Nyitvatartás frissítve."

        member _.DeleteOpeningHours(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "DELETE FROM opening_hours WHERE id = $id;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A kiválasztott nyitvatartás nem található."
                    else
                        ())
                "Nyitvatartás törölve."

        member _.AddCapacity(date, maxCapacity, currentCount) =
            protectWrite
                (fun () ->
                    match validateCapacity maxCapacity currentCount with
                    | Ok () -> ()
                    | Error error -> invalidOp error

                    executeNonQuery
                        "INSERT INTO capacity (date, max_capacity, current_count) VALUES ($date, $maxCapacity, $currentCount);"
                        (fun command ->
                            addParam command "$date" (toDateString date)
                            addParam command "$maxCapacity" maxCapacity
                            addParam command "$currentCount" currentCount)
                    |> ignore)
                "Kapacitás rekord létrehozva."

        member _.UpdateCapacity(id, date, maxCapacity, currentCount) =
            protectWrite
                (fun () ->
                    match validateCapacity maxCapacity currentCount with
                    | Ok () -> ()
                    | Error error -> invalidOp error

                    let affected =
                        executeNonQuery
                            "UPDATE capacity SET date = $date, max_capacity = $maxCapacity, current_count = $currentCount WHERE id = $id;"
                            (fun command ->
                                addParam command "$id" id
                                addParam command "$date" (toDateString date)
                                addParam command "$maxCapacity" maxCapacity
                                addParam command "$currentCount" currentCount)

                    if affected = 0 then
                        invalidOp "A kiválasztott kapacitás rekord nem található."
                    else
                        ())
                "Kapacitás rekord frissítve."

        member _.DeleteCapacity(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "DELETE FROM capacity WHERE id = $id;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A kiválasztott kapacitás rekord nem található."
                    else
                        ())
                "Kapacitás rekord törölve."

        member _.IncrementCapacity(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "UPDATE capacity SET current_count = current_count + 1 WHERE id = $id AND current_count < max_capacity;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A létszám nem növelhető tovább ehhez a naphoz."
                    else
                        ())
                "Beléptetés rögzítve."

        member _.DecrementCapacity(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "UPDATE capacity SET current_count = current_count - 1 WHERE id = $id AND current_count > 0;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A létszám nem csökkenthető tovább ehhez a naphoz."
                    else
                        ())
                "Kiléptetés rögzítve."

        member _.AddBooking(userId, bookingDate, bookingTime, status) =
            protectWrite
                (fun () ->
                    let validStatus =
                        match ensureKnownStatus (normalizeText status) with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    match validateRequired "foglalási idő" bookingTime with
                    | Ok _ -> ()
                    | Error error -> invalidOp error

                    if not (ensureUserExists userId) then
                        invalidOp "Csak aktív felhasználóhoz lehet foglalást rögzíteni."

                    if not (ensureBookingUnique 0 userId bookingDate bookingTime) then
                        invalidOp "A felhasználónak már van aktív foglalása erre az időpontra."

                    executeNonQuery
                        "INSERT INTO bookings (user_id, booking_date, booking_time, status) VALUES ($userId, $bookingDate, $bookingTime, $status);"
                        (fun command ->
                            addParam command "$userId" userId
                            addParam command "$bookingDate" (toDateString bookingDate)
                            addParam command "$bookingTime" (normalizeText bookingTime)
                            addParam command "$status" validStatus)
                    |> ignore)
                "Foglalás létrehozva."

        member _.UpdateBooking(id, userId, bookingDate, bookingTime, status) =
            protectWrite
                (fun () ->
                    let validStatus =
                        match ensureKnownStatus (normalizeText status) with
                        | Ok value -> value
                        | Error error -> invalidOp error

                    match validateRequired "foglalási idő" bookingTime with
                    | Ok _ -> ()
                    | Error error -> invalidOp error

                    if not (ensureUserExists userId) then
                        invalidOp "Csak aktív felhasználóhoz lehet foglalást menteni."

                    if validStatus = "active" && not (ensureBookingUnique id userId bookingDate bookingTime) then
                        invalidOp "A felhasználónak már van aktív foglalása erre az időpontra."

                    let affected =
                        executeNonQuery
                            "UPDATE bookings SET user_id = $userId, booking_date = $bookingDate, booking_time = $bookingTime, status = $status WHERE id = $id;"
                            (fun command ->
                                addParam command "$id" id
                                addParam command "$userId" userId
                                addParam command "$bookingDate" (toDateString bookingDate)
                                addParam command "$bookingTime" (normalizeText bookingTime)
                                addParam command "$status" validStatus)

                    if affected = 0 then
                        invalidOp "A kiválasztott foglalás nem található."
                    else
                        ())
                "Foglalás frissítve."

        member _.DeleteBooking(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "DELETE FROM bookings WHERE id = $id;"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A kiválasztott foglalás nem található."
                    else
                        ())
                "Foglalás törölve."

        member _.CancelBooking(id) =
            protectWrite
                (fun () ->
                    let affected =
                        executeNonQuery
                            "UPDATE bookings SET status = 'cancelled' WHERE id = $id AND status <> 'cancelled';"
                            (fun command -> addParam command "$id" id)

                    if affected = 0 then
                        invalidOp "A foglalás nem mondható le vagy már le van mondva."
                    else
                        ())
                "Foglalás lemondva."