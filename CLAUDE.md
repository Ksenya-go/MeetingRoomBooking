# CLAUDE.md

This file provides guidance to Claude Code when working in this repository.

## Project overview

Meeting Room Booking System — a test task demonstrating safe concurrent booking of limited resources (meeting rooms) with real-time status updates.

Backend: ASP.NET Core (.NET 10)
Real-time: Azure SignalR Service
Database: Azure SQL Database
Frontend: TBD (see /src/Web or /src/Client once decided)


## Architecture

Clean Architecture + DDD, CQRS via Mediator.

- `Domain` — entities (Resource, TimeSlot, Booking, ApplicationUser), value objects, domain rules. No dependencies on other layers.
- `Application` — CQRS commands/queries and their handlers, interfaces (IBookingRepository, IBookingNotifier, etc.), FluentValidation validators, FluentResults for expected failures. Depends only on Domain.
- `Infrastructure` — EF Core DbContext and configurations, Azure SQL access, SignalR hub implementation, ASP.NET Core Identity setup.Depends on Application.
-  `Api` — ASP.NET Core MVC project: controllers, Razor Views, wwwroot (Bootstrap 5, SignalR client JS), DI composition root, middleware, appsettings. No business logic here — controllers only call MediatR and return results.
- `tests/MeetingBooking.Tests` — xUnit tests: unit tests for handlers,integration tests via WebApplicationFactory, and the concurrency test(see below).

## Roles

- **User**: view resources and their schedules, book available slots.
- **Admin**: everything a User can do, plus create/edit/delete resources,
  view all bookings across all users.

## Concurrency control (critical design decision)

A slot must never be double-booked. This is enforced with a **unique database constraint** on (ResourceId, TimeSlotId, Date) in the Bookings table, combined with a database transaction around the booking command.
Flow in `BookSlotCommandHandler`:
1. Begin a transaction.
2. Insert the new Booking row.
3. If the unique constraint is violated (SqlException / DbUpdateException), catch it, roll back, and return `Result.Fail(new BookingConflictError())`.
4. Otherwise commit and return `Result.Ok(bookingId)`.

Do NOT implement booking as "check if slot is free, then insert" as two separate unprotected steps — this is explicitly disallowed by the task requirements and does not prevent race conditions.

If asked to change this mechanism (e.g. to RowVersion-based optimistic concurrency), treat it as a deliberate architectural decision — update this file and explain the tradeoff in the commit message.


Real-time updates

BookingHub (SignalR) uses one group per Resource (resource-{id}). After a successful booking (or admin resource change), the handler notifies the relevant group so every client currently viewing that resource's schedule updates without a page refresh. The Razor View for a resource's schedule joins its group on load via the JS SignalR client.

## Conventions
- FluentResults (`Result` / `Result<T>`) for expected/business failures in Application and Api layers. Exceptions are only for truly unexpected errors, not for control flow.
- One handler class per feature, handling multiple related commands/queries (not one handler class per single command).
- No business logic in controllers or in the Infrastructure layer.
- XML doc comments on public Application-layer methods, especially command/query handlers, since that's where business rules live.

## Testing
- xUnit + FluentAssertions.
- Integration tests use `WebApplicationFactory`.
- The concurrency test fires multiple simultaneous booking requests at the same slot (via `Task.WhenAll`) against a real (or Testcontainers-based) SQL instance, and asserts that exactly one booking is created and the rest return a conflict result — never a 500 and never a duplicate row. This test must be runnable with a plain `dotnet test`.

## How Claude Code is used in this project
Claude Code is used throughout development for: scaffolding CQRS commands/ handlers, writing EF Core entity configurations and migrations, generating xUnit tests (including the concurrency test), reviewing SignalR hub code, and drafting documentation. Commit messages describe what changed and why; non-obvious architectural decisions (e.g. the concurrency strategy above) are documented here rather than only in commit history.


## Azure resources

- Resource Group: rg-meeting-booking (Sweden Central)
- Azure SQL Database: meetingbooking-db (Basic tier, 5 DTU)
- Azure SignalR Service: meetingbooking-signalr (Free tier, Default mode)
- Azure Web App: meetingbooking-app (.NET 10, Free F1 plan)

Connection strings and secrets are configured in App Service →
Configuration → Application settings, never committed to the repo.



## Commands
- Build: `dotnet build`
- Run tests: `dotnet test`
- Run API locally: `dotnet run --project Api`
- Apply migrations: `dotnet ef database update --project Infrastructure --startup-project Api`

## Commit style
Atomic commits, imperative mood, short "why" in the body when the change
isn't self-explanatory. Example:
    Add unique constraint on (ResourceId, TimeSlotId, Date)
    Prevents double-booking at the database level; booking handler catches
    the constraint violation and returns a conflict result instead of a
    silent overwrite or unhandled exception.