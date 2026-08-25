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
5. Review and submit the record for delegated authority approval.
6. Open **Approvals** to approve and seal the cycle, or return it with required changes.

Submitted cycles are read-only while awaiting a decision. Returned cycles can be edited and resubmitted. Approval stores an immutable JSON snapshot and SHA-256 integrity hash.

## Method configuration

The indirect cost recovery factor `k` is **versioned configuration, not a constant**. It lives in
the `MethodConfigs` table (`Models/MethodConfig.cs`), seeded on first run as version `2026.1`
with `k = 1.35`. `RateEngine` is a pure function that takes a `MethodConfig` and never reads a
constant; `MethodConfigProvider` resolves the version in force, and a cycle stamps its
`MethodVersion` when it is sealed — so a record sealed in 2026 still reproduces its own figures
after the factor changes. See `docs/spec/architecture.md` §3, rules R5 and R6.

**To change the factor, add a new row and move `IsCurrent`.** Never edit an existing version:
sealed records point at it.

The application uses `ric-costing-v5.db`, created automatically on first run. It is excluded from
Git. The schema is created with `EnsureCreated()`, so **delete the local database file after
pulling a model change** — for example the `MethodConfigs` table and `RicCycle.MethodVersion`
added on 25 August 2026 — and it will be rebuilt on the next run.

## Demo sign-in

- Data entry user: `entry` / `Entry123!`
- Delegated approver: `approver` / `Approve123!`

Data entry users can create, edit and submit only their own cycles. Approvers have a separate approval queue and can return or approve submitted records. Both the Razor Page policies and POST handlers enforce the workflow; navigation visibility is not the security boundary. Replace the demo credentials and add institutional authentication before production use.

The custodian dashboard lists every cycle owned by the signed-in user with Draft, Submitted, Returned or Approved status. Approval decisions create persistent in-app notifications. The approver view shows line-level cost/funding sources, personnel assumptions, annual values, capacity, utilisation, rates and supporting evidence before a decision can be recorded.
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
dotnet restore src/CostingTool.csproj
dotnet run --project src/CostingTool.csproj
```

Open the localhost URL printed in the terminal. During development, changes can be watched with:

```bash
dotnet watch --project src/CostingTool.csproj
```

The current page contains representative dashboard data for UI development. It should later be
replaced by values supplied from page models, application services, and the server-side calculation
engine.
