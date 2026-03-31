# Viralis Classroom

A web application for managing classrooms, assignments, and student submissions.

**Live site:** https://viralis-app-hzd4ekgmdxfnh0ft.swedencentral-01.azurewebsites.net/

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express / LocalDB)
- [Entity Framework Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

Install the EF Core CLI tools if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Hristo-Dam/viralis-classroom.git
cd viralis-classroom/ViralisClassroom
```

### 2. Configure the connection string

The app uses [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to store the connection string locally. From the `Viralis.Web` project directory, run:

```bash
cd Viralis.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=ViralisClassroom;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Replace `YOUR_SERVER` with your SQL Server instance (e.g. `localhost`, `.\SQLEXPRESS`, or `(localdb)\mssqllocaldb`).

### 3. Apply migrations

Run the following command from the `Viralis.Web` directory to create the database and apply all migrations:

```bash
dotnet ef database update --project ../Viralis.Data/Viralis.Data.csproj
```

Or from the solution root (`ViralisClassroom/`):

```bash
dotnet ef database update --project Viralis.Data/Viralis.Data.csproj --startup-project Viralis.Web/Viralis.Web.csproj
```

### 4. Run the application

From the `Viralis.Web` directory:

```bash
dotnet run
```

The app will be available at `https://localhost:5001` (or the port shown in the terminal).

## Project Structure

```
ViralisClassroom/
├── Viralis.Web/        # ASP.NET Core MVC web app (entry point)
├── Viralis.Services/   # Business logic / service layer
├── Viralis.Data/       # EF Core DbContext, models, migrations
└── Viralis.Common/     # Shared view models and constants
```
