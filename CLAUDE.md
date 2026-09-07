# CLAUDE.md

This file provides guidance to Claude when working in this repository.

## Project overview

Meeting Room Booking System — a test task demonstrating safe concurrent booking of limited resources (meeting rooms) with real-time status updates.

Backend: ASP.NET Core (.NET 10), MVC (Razor Views)
Real-time: Azure SignalR Service
Database: Azure SQL Database
Frontend: ASP.NET Core MVC + Razor Views + Bootstrap 5 + vanilla JS (`@microsoft/signalr` client via CDN).

Served from the same Web App as the backend — no separate SPA project, no CORS setup needed.

## Architecture

Clean Architecture + DDD, CQRS via Mediator (source-generator based, martinothamar/Mediator).

* `Domain` — entities (`Resource`, `TimeSlot`, `Booking`), domain rules. No dependencies on other layers.
* `Application` — CQRS commands/queries and their handlers, interfaces (`IApplicationDbContext`, `IBookingNotifier`, `ICurrentUserService`, `IUserLookupService`, etc.), FluentValidation validators, FluentResults for expected failures. Depends only on Domain.
* `Infrastructure` — EF Core DbContext and configurations, Azure SQL access, SignalR hub implementation, ASP.NET Core Identity (`ApplicationUser`, roles, seeding). Depends on Application. Service registration is centralized in `Infrastructure/DependencyInjection.cs` (`AddInfrastructure()`), keeping `Program.cs` focused on the HTTP pipeline.
* `Api` — ASP.NET Core MVC project: controllers, Razor Views, `wwwroot` (Bootstrap 5, SignalR client JS), DI composition root, middleware, appsettings. No business logic here — controllers only call Mediator's `ISender` and return results based on `Result`/`Result<T>`.
* `tests/MeetingBooking.Tests` — xUnit tests: unit tests for handlers using EF Core InMemory (Resources, TimeSlots, schedule queries), a FluentValidation validator test, and the concurrency test against a real SQL Server/Azure SQL instance (see Testing below).

## Roles

* **User**: view resources and their schedules, book available slots, view their own bookings.
* **Admin**: everything a User can do, plus create/edit/delete resources, manage time slots, and view all bookings across all users.

## Concurrency control (critical design decision)

A slot must never be double-booked. This is enforced with a **unique database constraint** on `(ResourceId, TimeSlotId, Date)` in the `Bookings` table (see `Infrastructure/Persistence/Configurations/BookingConfiguration.cs`).

Flow in `BookingsHandler.Handle(BookSlotCommand ...)`:

1. Construct and add the new `Booking` entity to the DbContext.
2. Call `SaveChangesAsync`.
3. If the unique index is violated, EF Core throws `DbUpdateException` wrapping a `SqlException` with error number `2601` or `2627`. This is caught and converted into `Result.Fail(new BookingConflictError(...))`.
4. Otherwise, the save succeeds, a SignalR notification is sent, and `Result.Ok(bookingId)` is returned.

This was a deliberate choice over "check if free, then insert" (which is explicitly disallowed by the task — it doesn't prevent race conditions) and over transactional locking / optimistic concurrency with a `RowVersion` column.

The unique constraint approach was chosen because it pushes the guarantee to the database engine itself, requires no explicit locking code, and produces a well-defined, catchable failure mode under concurrent load.

This is verified directly by the automated concurrency test (see Testing).

If asked to change this mechanism, treat it as a deliberate architectural decision — update this file and explain the tradeoff in the commit message.

## Real-time updates

`BookingHub` (`Infrastructure/Realtime/BookingHub.cs`) uses one group per Resource (`resource-{id}`).

After a successful booking, or a resource change, the handler (via `SignalRBookingNotifier`) notifies the relevant group so every client currently viewing that resource's schedule updates without a page refresh.

The Razor View for a resource's schedule joins its group on load via the JS SignalR client and leaves it on unload.

Azure SignalR Service is used in Default mode (not Serverless), connected via `Azure:SignalR:ConnectionString` in configuration.

## Conventions

* FluentResults (`Result` / `Result<T>`) for expected/business failures in Application and Api layers. Exceptions are only for truly unexpected errors, not for control flow.
* One handler class per feature, handling multiple related commands/queries (e.g. `BookingsHandler`, `ResourcesHandler`, `TimeSlotsHandler`), not one handler class per single command.
* No business logic in controllers or in the Infrastructure layer.
* Business rules that go beyond simple CRUD (e.g. a Resource with upcoming bookings cannot be deleted) live in the Application handler, not in the controller or the domain entity's setters.
* XML doc comments on public Application-layer methods, especially command/query handlers, since that's where business rules live.

## Testing

* xUnit + FluentAssertions.
* Unit tests for `ResourcesHandler` and `BookingsHandler` queries use EF Core's InMemory provider (`tests/.../TestDoubles/InMemoryDbContextFactory.cs`) — acceptable for logic tests, but **NOT** used for the concurrency test, since InMemory does not enforce unique constraints the same way a real SQL engine does.
* The concurrency test (`BookingConcurrencyTests.cs`) fires multiple simultaneous booking requests at the same slot (via `Task.WhenAll`) against a real SQL Server / Azure SQL instance, and asserts that exactly one booking is created and the rest return a conflict result — never a `500` and never a duplicate row.
* The test's connection string is resolved from, in order:

  1. The `TEST_DB_CONNECTION_STRING` environment variable.
  2. `tests/MeetingBooking.Tests/testsettings.local.json` (gitignored, for a developer's own credentials).
  3. `testsettings.json` (committed, contains a `CHANGE_ME` placeholder).

See `README.md` for exact setup steps.

## How Claude was used in this project

**Note on tooling:** Although the task specifies Claude Code, development was carried out through Claude.ai due to access limitations:

* scaffolding Clean Architecture layers;
* writing Domain entities and EF Core configurations;
* designing and explaining the concurrency-control strategy;
* generating the CQRS commands/queries/handlers;
* writing the concurrency test and unit tests;
* debugging DI/SignalR/EF configuration issues;
* drafting this file and the README.

This is a deliberate, disclosed substitution — not an attempt to represent chat usage as terminal Claude Code usage.

Code generated this way was still reviewed, adapted, and committed manually after local verification (build, tests, manual UI checks), following the same architecture and conventions documented here.

## Azure resources

* Resource Group: `rg-meeting-booking` (Sweden Central).
* Azure SQL Database: `meetingbooking-sql-kseniia` on server `meetingbooking-sql-kseniia.database.windows.net` (Basic tier, 5 DTU, 2 GB, locally-redundant backups).
* Azure SignalR Service: `meetingbooking-signalr` (Free F1 tier, Default mode).
* Azure Web App: `meetingbooking-app` (.NET 10, Free F1 App Service plan), deployed at a unique default hostname (see README for the live URL).

Connection strings and secrets are configured in App Service → Environment variables → Connection strings / Application settings, never committed to the repo.

## Commands

* Build: `dotnet build`
* Run tests: `dotnet test`
* Run API locally: `dotnet run --project Api`
* Apply migrations: `dotnet ef database update --project Infrastructure --startup-project Api`

## Commit style

Atomic commits, imperative mood, short "why" in the body when the change isn't self-explanatory.

Example:

```text
Add unique constraint on (ResourceId, TimeSlotId, Date)

Prevents double-booking at the database level; booking handler catches
the constraint violation and returns a conflict result instead of a
silent overwrite or unhandled exception.
```
