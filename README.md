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
- [Future improvements](#future-improvements)

## Task description
A company has a limited set of meeting rooms (resources), each with a fixed set of daily time slots. Multiple users may try to book the same slot at the same time — the system guarantees that exactly one request succeeds and the rest get a clear conflict, never a silent overwrite and never a 500 error.

## Business capabilities:
- view any meeting room and its schedule (free/booked slots)
- book a free slot
- view your own bookings (User) or all users' bookings (Admin), with pagination
- manage meeting rooms: create, edit, deactivate (Admin)
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
- **CQRS** — commands and queries via Mediator
- **Result pattern (FluentResults)** — expected business failures (booking conflict, attempting to delete a resource with upcoming bookings) are returned as Result.Fail(...)
- **Unique constraint as the concurrency mechanism** — the double-booking guarantee lives at the database level
- **Composition root in Infrastructure** — all service registration for the layer is extracted into Infrastructure/DependencyInjection.cs (AddInfrastructure())

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
│   └── Common/                      # Interfaces (IApplicationDbContext, IBookingNotifier, ICurrentUserService, IUserLookupService), Errors                                 
├── Infrastructure/                  # EF Core, Identity, SignalR, DI
│   ├── Persistence/                 # MeetingBookingDbContext, Configurations/, Migrations/
│   ├── Identity/                    # ApplicationUser, CurrentUserService, IdentitySeeder, UserLookupService
│   ├── Realtime/                    # BookingHub, SignalRBookingNotifier
│   └── DependencyInjection.cs       # AddInfrastructure()
├── Api/                             # ASP.NET Core MVC
│   ├── Controllers/                 # Account, Resources, TimeSlots, Bookings
│   ├── Views/                       # Razor Views (Home, Account, Resources, TimeSlots, Bookings)
│   ├── wwwroot/                     # site.css (custom design system), SignalR client JS
│   └── Program.cs
└── tests/
    └── MeetingBooking.Tests/
        ├── BookingConcurrencyTests.cs        # automated concurrency test (real SQL)
        ├── ResourcesHandlerTests.cs          # CRUD unit tests (EF Core InMemory)
        ├── BookingsHandlerScheduleTests.cs   # schedule unit test (EF Core InMemory)
        ├── BookSlotCommandValidatorTests.cs  # validator unit test
        └── TestDoubles/                      # fake dependency implementations for tests
```

## Concurrency control

**Requirement**: if two requests try to book the same slot at the same time, exactly one must succeed and the other must receive a clear conflict — never a 500 error, never a silent overwrite.
**Chosen approach**: a unique database constraint prevents duplicate bookings for the same resource and time slot. Constraint violations are caught as `DbUpdateException` and converted into a `BookingConflictError`; successful bookings trigger a SignalR notification.

## Real-time updates
`BookingHub (SignalR)` uses one group per resource `(resource-{id})`. After a successful booking, SignalRBookingNotifier notifies the relevant group — everyone currently viewing that resource's schedule sees the slot status update without refreshing the page.
Uses Azure SignalR Service in Default mode (not Serverless), connected via `Azure:SignalR:ConnectionString`.

## Roles and authorization
ASP.NET Core Identity, two roles:

| Role      | Capabilities                                                                                                    |
| --------- | --------------------------------------------------------------------------------------------------------------- |
| **User**  | View resources and schedules, book available slots, view own bookings                                           |
| **Admin** | All User capabilities, plus create/edit/delete resources, manage time slots, and view all bookings across users |

A resource with upcoming active bookings cannot be deleted — a business rule that prevents a room with confirmed future bookings from silently disappearing.

## Azure resources and deployment

| Resource                  | Name                         | Parameters                        |
| ------------------------- | ---------------------------- | --------------------------------- |
| **Resource Group**        | `rg-meeting-booking`         | Sweden Central                    |
| **Azure SQL Database**    | `meetingbooking-sql-kseniia` | Basic tier, 5 DTU, 2 GB           |
| **Azure SignalR Service** | `meetingbooking-signalr`     | Free F1, Default mode             |
| **Azure Web App**         | `meetingbooking-app`         | .NET 10, Free F1 App Service plan |

**Deployed application**: https://meetingbooking-app-etd4cxfneecsdkay.swedencentral-01.azurewebsites.net/
Connection strings and secrets are configured in App Service → Environment variables (Connection strings / Application settings), never stored in the repository.

## Getting started (local)
Requirements:
- .NET 10 SDK
- Access to a SQL Server / Azure SQL instance (LocalDB was not used — configuration targets Azure SQL)
```
# 1. Clone
git clone https://github.com/Ksenya-go/MeetingRoomBooking.git
cd MeetingRoomBooking

# 2. Configure the database connection (User Secrets)
cd Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:<your-server>.database.windows.net,1433;Database=<your-db>;User ID=<user>;Password=<password>;Encrypt=true;"
cd ..

# 3. Restore and build
dotnet restore
dotnet build

# 4. Apply migrations
dotnet ef database update --project Infrastructure --startup-project Api

# 5. Run
dotnet run --project Api
```
On first run, IdentitySeeder automatically creates the Admin/User roles and an admin account:
- Email: admin@meetingbooking.local
- Password: Admin123!
  
Time slots are added through the UI (/TimeSlots, Admin role) — the database is empty by default.

## Testing

dotnet test

| Test                            | Type                    | What it verifies                                                                      |
| ------------------------------- | ----------------------- | ------------------------------------------------------------------------------------- |
| `BookingConcurrencyTests`       | Concurrency (real SQL)  | 10 parallel requests for the same slot → exactly 1 success, 9 conflicts, 0 exceptions |
| `ResourcesHandlerTests`         | Unit (EF Core InMemory) | Resource CRUD, rejecting deletion when future bookings exist                          |
| `BookingsHandlerScheduleTests`  | Unit (EF Core InMemory) | Correctness of free/booked slot flags in the schedule                                 |
| `BookSlotCommandValidatorTests` | Unit                    | Rejecting past-dated bookings and empty IDs                                           |


## Setting up the concurrency test
The test runs against a real SQL Server / Azure SQL database.

The connection string is resolved from (in order): the TEST_DB_CONNECTION_STRING environment variable, testsettings.local.json (gitignored), or testsettings.json (committed, with a CHANGE_ME placeholder).

## About the use of Claude
The task specifies Claude Code, but development was carried out through the standard Claude.ai chat interface due to access limitations.

Claude was used for architecture design, implementation, CQRS and concurrency strategy, testing, debugging, and documentation. All generated code was reviewed, adapted to the project, and manually verified through builds, tests, and UI checks, following the conventions documented in CLAUDE.md.

## Future improvements
- Configure CI/CD with GitHub Actions using OIDC / federated identity once the required Azure permissions are available.
- Make `TimeSlot` configurable per resource to support different booking schedules for different rooms.
- Add email verification to the registration process.








