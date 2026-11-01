# TwitchUniConnect

> **For educational purposes only.** This project is a learning exercise for software design and API development with .NET. It is not production-ready and is not presented as a complete or secure social platform. Do not use it to process real or sensitive data.

## Overview

A sample social platform built with .NET 10 and ASP.NET Core Web API. It focuses on layering and selected Domain-Driven Design (DDD) concepts, including entities, aggregate roots, and domain events, with Entity Framework Core for persistence.

## Implemented Features

- **Profiles and friendships:** Create and retrieve profiles; send, accept, or reject friend requests.
- **Posts:** Create, retrieve, and edit post text.
- **Comments and interactions:** Add comments and replies; add or remove interactions such as likes, love, and laughter.
- **Conversations and messages:** Create conversations, list inbox conversations and messages with pagination, and send, edit, or delete messages.
- **Architecture:** Separate API, application, domain, and data-access layers, with shared building blocks such as `Entity`, `ValueObject`, `AggregateRoot`, and `IDomainEvent`.
- **Persistence:** EF Core with SQL Server and a social data model; domain events are dispatched through a logging component.
- **API documentation and error handling:** Swagger in development and a general exception handler for API responses.
- **Tests:** Tests cover workflows, the persistence model, and some shared-kernel components.

## Available API Routes

The current API includes these route groups:

- `api/profiles` — Create and retrieve profiles and manage friend requests.
- `api/posts` — Posts, comments, replies, and interactions.
- `api/conversations` — Conversations, inboxes, and messages.

When running in the development environment, Swagger provides details about request and response schemas.

## Requirements and Running the Project

- Install the .NET 10 SDK.
- Use SQL Server or LocalDB. The default connection string is in `Frontends/Web/TwitchUniConnect.Social.Api/appsettings.json`; adjust it for your local environment as needed.

Run the API from the repository root:

```powershell
dotnet run --project Frontends/Web/TwitchUniConnect.Social.Api/TwitchUniConnect.Social.Api.csproj
```

In development, the application creates the database using `EnsureCreated`. Swagger is enabled only in this environment.

Run the tests with:

```powershell
dotnet test TwitchUniConnect.slnx
```

## Project Limitations

This project is **for learning and experimentation only**. It does not currently include the guarantees or reviews required for production use, such as integrated authentication and authorization, secrets management, abuse prevention, operational database migrations, monitoring, or security testing. Review the code and configuration and address these areas before using it outside an educational environment.
