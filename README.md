# MeetingRoomBooking
A web application for booking meeting rooms with a guaranteed defense against double-booking and real-time slot status updates across all viewers — a test task built on ASP.NET Core.

## Contents
- [Task description](#task-description)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Concurrency control](#concurrency-control)
- [Real-time updates](#real-time-updates)
- [Roles and authorization](#roles-and-authorization)
- [Azure resources and deployment](#azure-resources-and-deployment)
- [Getting started (local)](#getting-started-(local))
- [Testing](#testing)
- [About the use of Claude](#about-the-use-of-Claude)
- [Known limitations](#Known-limitations)

## Task description
A company has a limited set of meeting rooms (resources), each with a fixed set of daily time slots. Multiple users may try to book the same slot at the same time — the system guarantees that exactly one request succeeds and the rest get a clear conflict, never a silent overwrite and never a 500 error.

## Business capabilities:
- view any resource and its schedule (free/booked slots)
- book a free slot
- view your own bookings (User) or all users' bookings (Admin), with pagination
- manage resources: create, edit, deactivate (Admin)
- manage time slots: create, delete (Admin)
- real-time slot status updates for everyone currently viewing that resource's schedule

## Technologies

| Category              | Technology                                                        |
| --------------------- | ----------------------------------------------------------------- |
| **.NET**              | .NET 10 / C#                                                      |
| **Web framework**     | ASP.NET Core MVC (Razor Views)                                    |
| **ORM**               | Entity Framework Core (Azure SQL / SQL Server)                    |
| **Mediator**          | Mediator (source-generated, martinothamar/Mediator)               |
| **Validation**        | FluentValidation                                                  |
| **Operation results** | FluentResults                                                     |
| **Real-time**         | Azure SignalR Service                                             |
| **Authentication**    | ASP.NET Core Identity (User/Admin roles)                          |
| **Frontend**          | Bootstrap 5, vanilla JS (SignalR client), X.PagedList             |
| **Tests**             | xUnit + FluentAssertions                                          |
| **Hosting**           | Azure Web App (App Service)                                       |

## Architecture
```
MeetingBooking.Domain             — core, no dependencies on other layers
    ↑
MeetingBooking.Application        — use cases (CQRS via Mediator), validation, Result pattern
    ↑
MeetingBooking.Infrastructure     — EF Core, Identity, SignalR hub, DI registration
    ↑
MeetingBooking.Api                — ASP.NET Core MVC (controllers, Razor Views, wwwroot)
```

## Patterns:
- **CQRS** — commands and queries via Mediator; one handler class per feature (BookingsHandler, ResourcesHandler, TimeSlotsHandler), not one class per single command
- **Result pattern (FluentResults)** — expected business failures (booking conflict, attempting to delete a resource with upcoming bookings) are returned as Result.Fail(...), not thrown as exceptions
- **Unique constraint as the concurrency mechanism** — the double-booking guarantee lives at the database level, not the application level (details below)
- **Composition root in Infrastructure** — all service registration for the layer is extracted into Infrastructure/DependencyInjection.cs (AddInfrastructure()); Program.cs is only responsible for the HTTP pipeline

## Domain model
```
Resource (a meeting room)
  Id, Name, Description, Capacity, IsActive

TimeSlot (a fixed time slot, shared across all resources)
  Id, StartTime, EndTime

Booking (a booking of a specific resource's slot on a specific date)
  Id, ResourceId, TimeSlotId, Date, UserId, CreatedAtUtc
  → unique index on (ResourceId, TimeSlotId, Date)
```

## Project structure
```
MeetingRoomBooking.sln
├── Domain/                          # Domain layer
│   ├── Resource                   
│   ├── TimeSlot
│   ├── Booking
├── Application/                     # Application layer (CQRS, validation)
│   ├── Bookings/                    # BookSlotCommand, GetResourceScheduleQuery, GetBookingsQuery, BookingsHandler
│   ├── Resources/                   # Create/Update/Delete/GetAll, ResourcesHandler
│   ├── TimeSlots/                   # Create/Delete/GetAll, TimeSlotsHandler
│   └── Common/                      # Interfaces (IApplicationDbContext, IBookingNotifier,
│                                     #   ICurrentUserService, IUserLookupService), Errors
│
├── Infrastructure/                  # EF Core, Identity, SignalR, DI
│   ├── Persistence/                 # MeetingBookingDbContext, Configurations/, Migrations/
│   ├── Identity/                    # ApplicationUser, CurrentUserService, IdentitySeeder, UserLookupService
│   ├── Realtime/                    # BookingHub, SignalRBookingNotifier
│   └── DependencyInjection.cs       # AddInfrastructure()
│
├── Api/                             # ASP.NET Core MVC
│   ├── Controllers/                 # Account, Resources, TimeSlots, Bookings
│   ├── Views/                       # Razor Views (Home, Account, Resources, TimeSlots, Bookings)
│   ├── wwwroot/                     # site.css (custom design system), SignalR client JS
│   └── Program.cs
│
└── tests/
    └── MeetingBooking.Tests/
        ├── BookingConcurrencyTests.cs        # automated concurrency test (real SQL)
        ├── ResourcesHandlerTests.cs          # CRUD unit tests (EF Core InMemory)
        ├── BookingsHandlerScheduleTests.cs   # schedule unit test (EF Core InMemory)
        ├── BookSlotCommandValidatorTests.cs  # validator unit test
        └── TestDoubles/                      # fake dependency implementations for tests
```

## Concurrency control


## Real-time updates
`BookingHub (SignalR)` uses one group per resource `(resource-{id})`. After a successful booking, SignalRBookingNotifier notifies the relevant group — everyone currently viewing that resource's schedule sees the slot status update without refreshing the page.
Uses Azure SignalR Service in Default mode (not Serverless), connected via `Azure:SignalR:ConnectionString`.

## Roles and authorization
ASP.NET Core Identity, two roles:

| Role      | Capabilities                                                                                                    |
| --------- | --------------------------------------------------------------------------------------------------------------- |
| **User**  | View resources and schedules, book available slots, view own bookings                                           |
| **Admin** | All User capabilities, plus create/edit/delete resources, manage time slots, and view all bookings across users |





