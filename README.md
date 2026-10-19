# CreditWorks Vehicle Manager 

A small ASP.NET Core app for managing vehicles and their weight categories.
Vehicle categories are calculated from category ranges, so those ranges are applied to vehicles when the list view is displayed.

## Features

- Add and list vehicles
- sort vehicles by owner, manufacturer, year, or weight.
- Create, edit, and delete weight categories
- Assign an icon to each category
- Validate category ranges to prevent gaps/overlaps
- Store data in SQL Server

## Requirements

- .NET 10 SDK
- Docker Desktop with Docker Compose, or an available SQL Server instance

## Setup

### 1. Configure SQL Server

For the included Docker Compose setup, create a `.env` file next to `compose.yaml` with a SQL Server password:

```text
MSSQL_SA_PASSWORD=replace_with_password
```

Start SQL Server: 

`docker compose up -d`

### 2. Configure application connection string

Store the connection string in ASP.NET Core User Secrets. Replace the password with the same pw used in .env:

`dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CreditWorks;User Id=sa;Password=replace_with_password;Encrypt=True;TrustServerCertificate=True;" --project src/CreditWorks.Web`

### 3. Restore tools and create db

Restore the repo's local .NET tools:

`dotnet tool restore`

Set environment to Dev so EF Core can load the Development User Secrets

macOS | Linux:
`export ASPNETCORE_ENVIRONMENT=Development`

Powershell:
`$env:ASPNETCORE_ENVIRONMENT = "Development"`

Apply the db migrations

`dotnet ef database update --project src/CreditWorks.Web --startup-project src/CreditWorks.Web`

The app will apply pending migrations and seed initial data if tables are empty.

## Build, test, and run

`dotnet build`

### Unit tests (does not require SQL Server)

`dotnet test tests/CreditWorks.Tests`

### SQL Server Integration Test

SQL Server must be running and the `ConnectionStrings:DefaultConnection` User Secret must be configured

`dotnet test tests/CreditWorks.IntegrationTests`

To run both test projects simply run `dotnet test`





`dotnet run --project src/CreditWorks.Web --launch-profile http`

Open http://localhost:5184

To stop SQL Server container:

`docker compose down`

## Structure

The solution is split into four projects:

- `src/CreditWorks.Core` contains vehicle and category models, category resolution, configuration, validation, and deletion logic.
- `src/CreditWorks.Web` contains the Razor Pages interface, EF Core db context, migrations, and development seed data.
- `tests/CreditWorks.Tests` contains unit tests for category resolution, range validation, and deletion, vehicle validation, and sorting
- `tests/CreditWorks.IntegrationTests` contains a SQL Server integration test. Requires SQL Server and `ConnectionStrings:DefaultConnection` User Secret configured.


## Data Design

Manufacturers are stored separately from vehicles. Each vehicle references a manufacturer and stores its owner, year, and weight in KGs.

Vehicles don't store their category, the app decides it's category from the weight and current category ranges when the vehicle list is displayed. Category changes apply to existing vehicles automatically without updating vehicle records.

Each category contains name, iconkey, minimum weight, and optional max weight. The icon key refers to a bootstrap icon in the bootstrap icon catalog.

## Category ranges and boundaries

A category range is inclusive of the minmum weight and exclusive of the maximum weight.

```text
minimum weight <= vehicle weight < maximum weight
```

The final category has no max and includes everything at or above the minimum weight for that category.

Weights must be positive and support up to 2 d.p. The first category must start at 0.01kg.

The app validates the full category configuration to prevent gaps, overlaps, duplicate names, and invalid ranges. Creating a category splits an existing range. Deleting a category merges its range into a neighboring category.

## Initial data and environment

In the development environment, the application applies the pending migration and seeds the five required manufacturers and three initial weight categories when those tables are empty.

The app does not apply migrations or seed data outside of Development environment.

There is no authentication.


