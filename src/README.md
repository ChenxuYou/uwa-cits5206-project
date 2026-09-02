# Research Infrastructure Costing Tool — application

ASP.NET Core Razor Pages, targeting .NET 10, with EF Core over SQLite in development.
Chosen and recorded in [ADR-001](../docs/decisions/adr-001-technology-stack.md).

---

## Getting it running in VS Code

**Once, on a new machine.**

1. **Install the .NET 10 SDK** — <https://dotnet.microsoft.com/download/dotnet/10.0>.
   Windows: the x64 installer. Apple Silicon (M1–M4): **Arm64**; Intel Mac: **x64**.
   Check it with `dotnet --info`; the SDK list must include a `10.0.x`.
2. **Install the VS Code extension.** Open the Extensions panel and install
   **C# Dev Kit** (`ms-dotnettools.csdevkit`) — it brings the C# extension and the debugger
   with it. VS Code offers this automatically the first time you open the repository,
   because it is listed in [`.vscode/extensions.json`](../.vscode/extensions.json).
3. **Open the repository folder itself** — the one containing `CostingTool.sln` — not
   `src/`. The solution ties the three projects together and the run configuration is
   written relative to the repository root.

**Every time.**

> Press <kbd>F5</kbd>.

That builds all three projects, starts the application in the Development environment, and
opens a browser at it. Sign in with the demo accounts below. Stop it with
<kbd>Shift</kbd>+<kbd>F5</kbd>.

If you prefer the terminal, the equivalent is:

```bash
dotnet run --project src/CostingTool.csproj
```

then open the `https://localhost:7267` address it prints. The first run shows a certificate
warning; `dotnet dev-certs https --trust` clears it for good.

**The other things you will want**, all from the command palette
(<kbd>Ctrl/Cmd</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> → *Tasks: Run Task*):

| Task | What it does |
| --- | --- |
| **build** | Builds the solution. Also <kbd>Ctrl/Cmd</kbd>+<kbd>Shift</kbd>+<kbd>B</kbd> |
| **watch** | Runs the app and reloads it as you save — the fastest loop for UI work |
| **test** | Runs the engine tests. Also `dotnet test` |
| **format** | Applies the formatting CI checks, so a pull request does not fail on whitespace |
| **reset local database** | Deletes the SQLite file; the schema rebuilds on the next run |

### When something does not work

| Symptom | Cause and fix |
| --- | --- |
| The sign-in page rejects `entry` / `Entry123!` | The app is not in the Development environment, so no demo accounts were seeded. Start it with <kbd>F5</kbd> or `dotnet run`, both of which read [`Properties/launchSettings.json`](Properties/launchSettings.json) |
| `SQLite Error 1: no such column` after pulling | The schema is built with `EnsureCreated()`, which cannot alter an existing database. Run the **reset local database** task and start again |
| Money renders as `¤100.00` | Should not happen — the app pins `en-AU` in `Program.cs`. If it does, say so: it means the culture configuration is not being applied |
| `dotnet` is not recognised after installing the SDK | Restart VS Code, or the terminal, so it picks up the new `PATH` |

### Demo sign-in

| Account | Username | Password | Sees |
| --- | --- | --- | --- |
| Platform custodian | `entry` | `Entry123!` | Their own cycles, the guided workflow, notifications |
| Delegated approver | `approver` | `Approve123!` | The approval queue, and every submitted record |

**These exist in the Development environment only.** They are seeded by `Program.cs` behind
an `IsDevelopment()` check, and the sign-in page prints them only there — so deploying to
staging cannot re-create the credentials that
[`risks.md` R14](../docs/project/risks.md) makes a gate on deploying. A staging or
production instance starts with no users, and accounts are provisioned deliberately.

---

## How the code is arranged

```
CostingTool.sln
├── src/
│   ├── CostingTool.Engine/     The calculation. No EF, no ASP.NET, no dependencies at all
│   │   ├── MethodConfig.cs         k, the rounding rule, and the version they belong to
│   │   ├── CapabilityRateInputs.cs What the engine needs to price one capability
│   │   └── RateEngine.cs           The three formulas, and the workings behind them
│   └── CostingTool.csproj      The web application
│       ├── Models/                 Entities, and the vocabulary of a cost entry
│       ├── Data/                   The DbContext
│       ├── Services/               The seam: cycle → engine inputs → page results
│       └── Pages/                  Razor Pages
└── tests/CostingTool.Engine.Tests/  References the engine and nothing else
```

**The engine is a separate project on purpose.** `architecture.md` §3 rule R7 says the
engine never sees a user, a session or a database row. Keeping it in its own project with
no package references makes that a fact the compiler enforces rather than a claim in a
document — and it lets the test project reference the arithmetic without dragging a web
application behind it.

`Services/RicCalculationService.cs` is the only class allowed to know both sides. It turns a
stored cycle into engine inputs — applying the platform-cost allocation, deciding which
rows are income, working out each capability's share — and turns the answers into something
a page can render.

### The workflow

1. **Start** — platform, pricing period, billable unit, capabilities.
2. **Costs** — Personnel, Equipment, Maintenance, Travel, Animal and Other items by year,
   plus the four non-variable income lines. A line is booked either to one capability or to
   the platform, never both.
3. **Capacity** — maximum capacity and forecast utilisation per user category. Forecast, not
   capacity: it is the divisor behind every rate.
4. **Rates** — three minimum sustainable rates per capability, with the figures behind each,
   plus proposed rates and the resulting balance.
5. **Review** — check and submit for delegated authority approval.
6. **Approvals** — the approver sees the workings, then approves and seals, or returns the
   cycle with required changes.

Submitted cycles are read-only while awaiting a decision; returned cycles can be edited and
resubmitted. Approval writes an immutable JSON snapshot — inputs, results **and the
workings** — with a SHA-256 integrity hash.

---

## Two things worth knowing before you change anything

### The engine refuses rather than guessing

If a capability has no forecast utilisation, `RateEngine` throws
`RateCalculationException` with a message written for a custodian, and the page shows that
message where the number would have been. It does **not** return zero. `$0.00 per hour`
reads like an answer, and a plausible wrong figure published for three to five years is the
harm this tool exists to prevent — [`architecture.md` §3](../docs/spec/architecture.md)
rule R4.

### The method is versioned; never edit a version

`k` (1.35 today), the rounding rule and the decimal places live in the `MethodConfigs`
table, seeded as version `2026.1`. A cycle stamps its `MethodVersion` when it is sealed, and
reopening a sealed record recalculates under **that** version. So:

> **To change the factor, add a new row and move `IsCurrent`. Never edit an existing
> version — sealed records point at it.**

Nothing in the engine reads a constant, and nothing in the UI states one either: the
"includes 35% indirect cost recovery" label is computed from the configuration in force.

---

## Tests

```bash
dotnet test
```

`tests/CostingTool.Engine.Tests` holds the golden file — the client's own worked example
from the guide, Step 3:

| Input | | Expected |
| --- | --- | --- |
| Operating costs $150,000 · UWA in-kind $20,000 · WA Gov $30,000 · 1,000 forecast hours | → | **$100.00** · **$162.00** · **$202.50** per hour |

These must reproduce **to the cent** or the build fails and nothing merges. The rest of the
suite covers the boundaries: zero and negative utilisation, negative costs, income exceeding
cost, a change to `k`, rounding at the half-cent, very large amounts, and determinism.

Figures come from the client's **guide**, never from the recorded walkthrough — see the
withdrawn fixtures note in [`architecture.md` §3](../docs/spec/architecture.md).

---

## Known gaps

Recorded here rather than discovered later.

| Gap | Where it is tracked |
| --- | --- |
| **`EnsureCreated()`, not migrations.** The schema cannot evolve, so a model change means deleting the local database. That is fine locally and unacceptable once the client has entered data — moving to EF Core migrations is a gate on the staging deployment | [`plan.md` M5](../docs/project/plan.md) |
| **No PDF export yet.** The sealed record exists as JSON with its hash; the client-facing PDF, showing the workings, is US-16 | [`user-stories.md`](../docs/spec/user-stories.md) |
| **Pay scales, capacity baselines and category lists are not in `MethodConfig` yet.** `k` and the rounding rule are; the rest of rule R5 is not, so the salary field carries a placeholder rather than a looked-up figure | [ADR-001 action 7](../docs/decisions/adr-001-technology-stack.md) |
| **`Amount` is the mean of the per-year figures.** Averaging a multi-year profile into one annual number is our decision, not the client's; it is commented where it happens and needs confirming | `Models/RicCycle.cs` |
| **The revenue projection divides the uplift back out** of the APFR and commercial proposed rates. Preserved from the spike and documented in `RateEngine`, but it carries no source marker in any client document | `Services/RicCalculationService.cs` |
| **`site.css` is minified** except the block at the end. Sections are expanded as they are next touched rather than in one unreviewable pass | — |
