# Konditerem Management System

A web-based gym administration system built with F# and ASP.NET Core MVC.

## Motivation

Managing a gym's daily operations — tracking members, schedules, capacity, and bookings — is often still done on paper or with scattered tools. This project digitizes that workflow into a single, clean web interface. It was originally developed as a Java application and later ported to F# to explore functional programming concepts such as immutable data modeling, pattern matching, and result-based error handling.

The Alpha version uses an in-memory data layer to demonstrate the full interface and domain logic without any external dependencies.

## Try Live

**Project Alpha:** [https://kondi.1elet.hu](https://kondi.1elet.hu)

## Screenshots

![Home](Home.jpg)
![Users](Users.jpg)
![Opening Hours](Openings.jpg)
![Capacity](Capacity.jpg)
![Bookings](Bookings.jpg)

## How to Build and Run

```bash
cd Project_Alpha
dotnet run
```

## Project Structure

```
due-fsharp-2026/
└── Project_Alpha/
    ├── Models/Domain.fs
    ├── Data/InMemoryGymRepository.fs
    ├── Controllers/HomeController.fs
    ├── Views/Home/Index.cshtml
    └── Program.fs
```

## Tech Stack

| Layer | Technology |
|-------|------------|
| Language | F# (.NET 10) |
| Framework | ASP.NET Core MVC |
| Data | In-memory |
| View | Razor + Bootstrap 5 |

## Features

- **Users** — create, update, delete; membership types: daily, monthly, quarterly, annual, student; active/inactive status
- **Opening Hours** — weekly schedule management
- **Capacity** — daily headcount tracking with real-time check-in/check-out
- **Bookings** — status management (confirmed, pending, cancelled)
