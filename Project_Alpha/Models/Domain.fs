namespace Konditerem.Web.Models

open System

[<CLIMutable>]
type User =
    { Id: int
      Name: string
      Email: string
      Phone: string
      MembershipType: string
      RegistrationDate: DateTime
      Active: bool }

[<CLIMutable>]
type OpeningHours =
    { Id: int
      DayOfWeek: string
      OpenTime: string
      CloseTime: string }

[<CLIMutable>]
type Capacity =
    { Id: int
      Date: DateTime
      MaxCapacity: int
      CurrentCount: int
      AvailableSpots: int
      IsFull: bool }

[<CLIMutable>]
type Booking =
    { Id: int
      UserId: int
      UserName: string
      BookingDate: DateTime
      BookingTime: string
      Status: string }

[<CLIMutable>]
type DashboardSummary =
    { TotalUsers: int
      ActiveUsers: int
      ActiveBookings: int
      CapacityToday: int
      AvailableToday: int }

[<CLIMutable>]
type HomePageViewModel =
    { Summary: DashboardSummary
      Users: User list
      OpeningHours: OpeningHours list
      Capacities: Capacity list
      Bookings: Booking list
      FlashMessage: string
      FlashError: string
      Today: string
      DaysOfWeek: string list
      MembershipTypes: string list
      BookingStatuses: string list }
