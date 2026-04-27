# CsvSyncAsyncBenchmark

A small .NET 8 sample that benchmarks synchronous vs asynchronous database reads when loading CSV data into a local SQL Server (LocalDB) database and exposes two minimal HTTP endpoints for on-demand measurements.

--

## Features

- Seed a database from a CSV file (data.csv) on first run
- Console benchmark comparing synchronous DbContext.ToList() vs asynchronous ToListAsync()
- Minimal HTTP endpoints:
  - GET /db-sync  — runs a synchronous read benchmark
  - GET /db-async — runs an asynchronous read benchmark

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (or change the connection string in appsettings.json to a server you control)

## CSV format

Place a file named `data.csv` in the project root (same folder as Program.cs). Expected CSV format (header + rows):

```
Id,Name,Email
1,John Doe,john@example.com
2,Jane Smith,jane@example.com
...
```

The seeder will skip the header and parse rows with at least three comma-separated fields (Id, Name, Email).

## Setup & Run

1. Clone the repository
2. Open the solution in Visual Studio or run from command line

From command line (PowerShell):

```powershell
dotnet restore
dotnet build
dotnet run --project CsvSyncAsyncBenchmark.csproj
```

Notes:
- The app will run database migrations on startup. If the database is empty and a `data.csv` file is present, it will seed the Users table.
- If you do not have LocalDB, update `appsettings.json` connection string `DefaultConnection` to a valid SQL Server instance.

## Endpoints

- GET / returns a small welcome message
- GET /db-sync  — performs a synchronous read and returns { Count, TimeTakenMs }
- GET /db-async — performs an asynchronous read and returns { Count, TimeTakenMs }

Example (curl):

```powershell
curl http://localhost:5000/db-sync
curl http://localhost:5000/db-async
```

## Screenshots

Add screenshots to a folder named `Screenshots` at the repository root. The README will reference the images below — replace the filenames with your real screenshots if they differ.

![Console run screenshot](Screenshots/console-run.png)
![Benchmark results screenshot](Screenshots/benchmark-results.png)
![Database seeded screenshot](Screenshots/db-seeded.png)

If images do not display in your environment, ensure the `Screenshots` folder is committed to the repo and the files are named exactly as referenced above.

## Tips

- For larger benchmarks, use a bigger CSV (tens/hundreds of thousands of rows) and ensure the database has appropriate indexing.
- The project currently seeds using the synchronous CsvService.ReadCsvSync; you can extend Program.cs to use CsvServiceAsync if you want to seed asynchronously.

## License

MIT

