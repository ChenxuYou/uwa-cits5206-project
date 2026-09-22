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
| Administrator | `admin` | `Admin123!` | Every cycle, whoever owns it, read-only — and the accounts themselves |

**These exist in the Development environment only.** They are seeded by `Program.cs` behind
an `IsDevelopment()` check, and the sign-in page prints them only there — so deploying to
staging cannot re-create the credentials that
[`risks.md` R14](../docs/project/risks.md) makes a gate on deploying. A staging or
production instance starts with no users, and accounts are provisioned deliberately.

**Provisioning the first account.** With no users and no sign-up page, a fresh staging
database would lock out the administrator who is meant to create the accounts. Supply
`Bootstrap__AdminUserName` and `Bootstrap__AdminPassword` (and optionally
`Bootstrap__AdminDisplayName`) as environment variables at first start: one administrator is
created, only while the table is empty, and only if the password meets the policy below.
Everyone else is then created from **Accounts** inside the application. Never commit these
values.

**What an administrator can and cannot do.** They see every cycle in the application and
administer accounts — create, deactivate, reset a password. They cannot edit, submit or seal
another person's cycle: those actions write a name into the record, and US-02, US-15 and
US-16 depend on that name being the person who did the work. The role set beyond these three
is [Q4](../docs/spec/requirements.md#9-open-questions), still open with the client. Accounts
are deactivated rather than deleted, because their names appear on the records they created.

Passwords are never stored in plain text. ASP.NET Core's `PasswordHasher<AppUser>` creates a
new random salt for every password and stores a versioned PBKDF2 hash containing its salt and
work factor. The application currently uses Identity V3 format with 210,000 iterations. A
successful login transparently upgrades an older hash when its work factor is no longer
current.

The password rules live in one place, `Services/PasswordPolicy.cs`, so the change-password
page and the administrator's create-and-reset screens cannot come to disagree about them.
(The demo passwords above are shorter than the policy requires: they are development-only,
typed constantly while building, and printed on the development sign-in page.)

Five failed attempts lock an account for 15 minutes. Authentication cookies are HTTP-only,
use `SameSite=Lax`, expire after two hours and carry a security stamp checked against the
database. Changing a password rotates that stamp, invalidating older sessions. Signed-in
users can change their password from the user area; new passwords require at least 12
characters with uppercase, lowercase, number and symbol.

---

## How the code is arranged

```
CostingTool.sln
├── src/
│   ├── CostingTool.Engine/     The calculation. No EF, no ASP.NET, no dependencies at all
│   │   ├── MethodConfig.cs         k, the rounding rule, and the version they belong to
│   │   ├── CapabilityRateInputs.cs What the engine needs to price one capability
│   │   └── RateEngine.cs           The three formulas, and the workings behind them
│   ├── CostingTool.Pdf/        The sealed-record PDF. MigraDoc, and nothing else
│   │   ├── SealedRecord.cs         The snapshot, read back — schema 1.1
│   │   ├── SealedRecordPdf.cs      Rates beside the arithmetic that produced them
│   │   ├── EmbeddedFontResolver.cs Why the font travels with the assembly
│   │   └── Fonts/                  DejaVu Sans, embedded. Licence beside it
│   └── CostingTool.csproj      The web application
│       ├── Models/                 Entities, and the vocabulary of a cost entry
│       ├── Data/                   The DbContext
│       ├── Services/               The seam: cycle → engine inputs → page results
│       └── Pages/                  Razor Pages
├── tests/CostingTool.Engine.Tests/  References the engine and nothing else
└── tests/CostingTool.Pdf.Tests/     References the renderer and nothing else
```

### Who owns which part

Agreed on 15 September 2026 — [`plan.md` §3](../docs/project/plan.md). Ask the owner first, and
never approve a pull request in your own layer.

| Part of the tree | Layer | Owner |
| --- | --- | --- |
| `Pages/**/*.cshtml.cs`, `Services/` (except the calculation), `Models/`, `Data/`, CI | General backend | Chenxu You |
| `CostingTool.Engine/`, `Services/RicCalculationService.cs`, the workings in `CostingTool.Pdf/` | Backend — calculation | Wenmin Luo |
| Server, CD, DNS, TLS, release | Backend — deployment | Dai Lam La La |
| `Pages/**/*.cshtml`, `wwwroot/css/` | Front end | Yichen Zhao |
| `Pages/Account/`, `Services/CurrentUser.cs`, the authorisation policies in `Program.cs` | Authentication | Jaswanth Vericherla |

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
2. **Costs** — operating costs by the client's workbook categories: directly incurred per
   capability, directly allocated and indirect (floor area × rate per m²) at platform level.
   A cost is booked either to one capability or to the platform, never both; platform costs
   are split evenly across the capabilities, and a running total per capability reconciles
   to the platform total.
3. **Funding** — the four non-variable funding lines, recorded separately from operating
   costs with their source, commitment period and effect on each rate.
4. **Capacity** — usable capacity built from the machine or staff baseline in the method
   configuration, less itemised deductions and capped by staff FTE where a person must be
   present; then forecast utilisation per user category. Forecast, not capacity: it is the
   divisor behind every rate, and a forecast above capacity needs a reason.
5. **Rates** — three minimum sustainable rates per capability, with the figures behind each,
   plus proposed rates and the resulting balance.
6. **Review** — check and submit for delegated authority approval.
7. **Approvals** — the approver sees the workings, then approves and seals, or returns the
   cycle with required changes.

Every completed step is a link in the step bar, and any answer can be changed on the way back
(US-10): step 1 reopens on the cycle to rename, add or remove capabilities, recorded cost and
funding lines open for editing in place, and the capacity and rates steps save before going
back. Removing a capability or changing the billable unit asks first, because it clears what
later steps hold; the pricing period can move but not change length once lines exist. A step
with typed but unsaved changes warns before it is left by a link.

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

**`dotnet restore --force-evaluate` once, first.** `CostingTool.Pdf` adds a package
reference, and both projects are pinned with a lockfile — a plain restore against a stale
`packages.lock.json` fails with NU1004 rather than explaining itself.

`tests/CostingTool.Engine.Tests` holds the golden file — the client's own worked example
from the guide, Step 3:

| Input | | Expected |
| --- | --- | --- |
| Operating costs $150,000 · UWA in-kind $20,000 · WA Gov $30,000 · 1,000 forecast hours | → | **$100.00** · **$162.00** · **$202.50** per hour |

These must reproduce **to the cent** or the build fails and nothing merges. The rest of the
suite covers the boundaries: zero and negative utilisation, negative costs, income exceeding
cost, a change to `k`, rounding at the half-cent, very large amounts, and determinism.

`tests/CostingTool.Pdf.Tests` then asserts the **same three figures on the way out** — a
correct engine behind a document that prints something else is not worth much.

Figures come from the client's **guide**, never from the recorded walkthrough — see the
withdrawn fixtures note in [`architecture.md` §3](../docs/spec/architecture.md).

---

## Known gaps

Recorded here rather than discovered later.

| Gap | Where it is tracked |
| --- | --- |
| **`EnsureCreated()`, not migrations.** The schema cannot evolve, so a model change means deleting the local database. That is fine locally and unacceptable once the client has entered data — moving to EF Core migrations is a gate on the staging deployment | [`plan.md` M5](../docs/project/plan.md) |
| **The PDF export is a spike, not finished work.** `src/CostingTool.Pdf` renders a sealed record and the custodian can download it from the review page; it has not been reviewed by a second member, the approver has no link to it yet, and nobody has printed one on A4. US-16 closes in S5 | [ADR-002](../docs/decisions/adr-002-pdf-generation.md), follow-on actions |
| **Pay scales, capacity baselines and category lists are not in `MethodConfig` yet.** `k` and the rounding rule are; the rest of rule R5 is not, so the salary field carries a placeholder rather than a looked-up figure | [ADR-001 action 7](../docs/decisions/adr-001-technology-stack.md) |
| **`Amount` is the mean of the per-year figures.** Averaging a multi-year profile into one annual number is our decision, not the client's; it is commented where it happens and needs confirming | `Models/RicCycle.cs` |
| **The revenue projection divides the uplift back out** of the APFR and commercial proposed rates. Preserved from the spike and documented in `RateEngine`, but it carries no source marker in any client document | `Services/RicCalculationService.cs` |
| **`site.css` is minified** except the block at the end. Sections are expanded as they are next touched rather than in one unreviewable pass | — |
