# Costing Tool dashboard

ASP.NET Core Razor Pages MVP for the Research Infrastructure Costing & Pricing Tool.

## RIC workflow

The dashboard starts a guided server-backed workflow:

1. Define the platform, pricing period, billable unit and capabilities.
2. Record detailed Personnel, Equipment, Maintenance, Travel, Animal and Other items by year,
   together with non-variable income. Personnel records retain funding, fellowship, employment,
   salary, superannuation and workload details.
3. Enter maximum capacity and forecast annual utilisation for UWA, APFR and commercial users.
4. Calculate the three minimum sustainable rates in C# and record proposed rates and evidence.
5. Review and submit the immutable record for approval.

The application uses `ric-costing-v2.db`, created automatically on first run. It is excluded from Git.
The earlier project-costing prototype remains under `Pages/Costs`, but the dashboard now routes to
the RIC workflow under `Pages/Ric`.

## Requirements

- .NET 10 SDK

Check the installed version:

```bash
dotnet --info
```

If macOS reports `command not found`, install the .NET 10 SDK from:
https://dotnet.microsoft.com/download/dotnet/10.0

Choose the **Arm64** installer for Apple Silicon (M1/M2/M3/M4), or **x64** for an Intel Mac.

## Run

From the repository root:

```bash
dotnet restore src/CostingTool/CostingTool.csproj
dotnet run --project src/CostingTool/CostingTool.csproj
```

Open the localhost URL printed in the terminal. During development, changes can be watched with:

```bash
dotnet watch --project src/CostingTool/CostingTool.csproj
```

The current page contains representative dashboard data for UI development. It should later be
replaced by values supplied from page models, application services, and the server-side calculation
engine.
