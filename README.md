# Konditerem Management System

A web-based gym administration system built with F# and ASP.NET Core MVC. 

This repository contains two projects: an initial in-memory prototype (**Project Alpha**) and a final application with persistent database storage (**Project Omega**).

## Motivation

Managing a gym's daily operations (tracking members, schedules, capacity, and bookings) is often done on paper or with scattered tools. This project digitizes that workflow into a single web interface. 

It was originally developed as a Java application and later ported to F# to explore functional programming concepts such as immutable data modeling, pattern matching, and result-based error handling. The user interface is optimized for desktop environments, targeting reception desk usage where data tables require larger screen real estate.

---

## Project Structure
* [**Project Omega**](./Project_Omega) — The final submission with SQLite database, strict validation, and Hungarian localization.
* [**Project Alpha**](./Project_Alpha) — The early in-memory prototype.

---

## Core Features
* **Users:** Create, update, delete; membership types (daily, monthly, quarterly, annual, student); active/inactive status.
* **Opening Hours:** Weekly schedule management.
* **Capacity:** Daily headcount tracking with real-time check-in/check-out.
* **Bookings:** Status management (Aktív / Lemondva / Befejezett).

---

## Project Omega (Final Version)

This is the production-ready state of the application. It replaces the in-memory data layer with an SQLite database and introduces strict validation.

**Try Live:** [https://kondi.1elet.hu/omega](https://kondi.1elet.hu/omega)

### Key Technical Details
* **Storage:** SQLite database using ADO.NET in F#.
* **Validation:** 
  * Server-side F# Regex validation for emails (requires `@` and a valid TLD).
  * Server-side validation for phone numbers (8–15 digits, numeric only).
  * Client-side HTML5 pattern constraints as a first line of defense.
* **Hungarian Localization:** The entire UI is localized to Hungarian — all labels, buttons, navigation items, error messages, and status values (e.g. *Aktív*, *Lemondva*, *Befejezett*) are displayed in Hungarian, as the system targets Hungarian gym reception staff.
* **Database Seeding:** The application automatically creates the database and populates a default 7-day opening hours schedule on the first run.

### Database Schema

The database uses a normalized relational schema with four tables:

```mermaid
erDiagram
    users {
        int id PK
        string full_name
        string email
        string phone
        string membership_type
        string status
        string registration_date
    }
    bookings {
        int id PK
        int user_id FK
        string booking_date
        string start_time
        string end_time
        string status
    }
    opening_hours {
        int id PK
        string day_of_week
        string open_time
        string close_time
    }
    capacity_log {
        int id PK
        string date
        int max_capacity
        int current_count
    }
    users ||--o{ bookings : "has"
```

> **Extensibility:** The schema is designed so that future features — such as a `payments` table or detailed `access_logs` — can be easily linked to the `users` table via foreign keys.

### Screenshots (Omega)
<details>
<summary>Click to view screenshots</summary>
<br>

**Dashboard**
![Dashboard](Project_Omega/docs/main.jpg)

**User Management**
![User Management](Project_Omega/docs/users.jpg)

**Opening Hours**
![Opening Hours](Project_Omega/docs/opening.jpg)

**Capacity Tracking**
![Capacity Tracking](Project_Omega/docs/capacity.jpg)

**Booking Management**
![Booking Management](Project_Omega/docs/booking.jpg)

</details>

---

## Project Alpha (Initial Prototype)

The Alpha version uses an in-memory data layer to demonstrate the domain logic without external dependencies. The UI in this prototype is in English.

**Try Live:** [https://kondi.1elet.hu/alpha](https://kondi.1elet.hu/alpha)

### Screenshots (Alpha)
<details>
<summary>Click to view screenshots</summary>
<br>

![Dashboard](Project_Alpha/docs/Home.jpg)
![Users Management](Project_Alpha/docs/Users.jpg)
![Opening Hours](Project_Alpha/docs/Openings.jpg)
![Capacity Tracking](Project_Alpha/docs/Capacity.jpg)
![Bookings Management](Project_Alpha/docs/Bookings.jpg)

</details>

---

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| **Language** | F# (.NET 8.0) |
| **Framework** | ASP.NET Core MVC |
| **Database** | SQLite (`Microsoft.Data.Sqlite`) |
| **View** | Razor (`.cshtml`) + Vanilla CSS + Bootstrap Grid |

```text
due-fsharp-2026/
├── Project_Alpha/          # Initial prototype (In-memory)
└── Project_Omega/          # Final submission (SQLite)
    ├── App_Data/           # Database file
    ├── Models/Domain.fs    # F# Records & Types
    ├── Data/               # SQLite Gym Repository
    ├── Controllers/        # ASP.NET MVC Controllers
    ├── Views/              # Razor Pages (UI)
    └── Program.fs          # Application Entry Point
```

## How to Build and Run

### Running Project Omega (Final)
```bash
cd Project_Omega
dotnet run
```

### Running Project Alpha
```bash
cd Project_Alpha
dotnet run
```
