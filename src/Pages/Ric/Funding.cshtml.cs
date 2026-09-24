using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

public class FundingModel(CostingDbContext db) : RicPageModel(db)
{
    [BindProperty] public int CycleId { get; set; }

    /// <summary>The funding line being changed, or null while adding one (US-10: change any value).</summary>
    [BindProperty] public int? EditId { get; set; }

    [BindProperty] public string Category { get; set; } = CostEntry.IncomeCategories.UwaGpInKind;

    [BindProperty] public string? Description { get; set; }

    [BindProperty] public string? FundingBody { get; set; }

    [BindProperty] public string? Justification { get; set; }

    [BindProperty] public List<decimal> YearAmounts { get; set; } = [];

    /// <summary>The custodian has looked at an unusually large amount and says it is right (US-18).</summary>
    [BindProperty] public bool ConfirmLargeAmounts { get; set; }

    /// <summary>True when the form should offer the "I have checked these amounts" tick.</summary>
    public bool NeedsLargeAmountConfirmation { get; private set; }

    public int YearCount => Cycle.EndYear - Cycle.StartYear + 1;

    /// <param name="edit">A funding line of this cycle to open in the form for changing.</param>
    public async Task<IActionResult> OnGetAsync(int cycleId, int? edit = null)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        await RememberStepAsync(3);
        YearAmounts = Enumerable.Repeat(0m, YearCount).ToList();

        if (edit is not null)
        {
            if (!Cycle.IsEditable)
            {
                return RedirectToPage("/Ric/Review", new { cycleId });
            }

            if (Editable(edit.Value) is not { } item)
            {
                return NotFound();
            }

            EditId = item.Id;
            Category = item.Category;
            Description = item.Description;
            FundingBody = item.Supplier;
            Justification = item.Notes;
            var saved = item.YearAmounts.OrderBy(x => x.ProjectYear).Select(x => x.Amount).ToList();
            YearAmounts = Enumerable.Range(0, YearCount).Select(i => i < saved.Count ? saved[i] : 0m).ToList();
        }

        return Page();
    }

    public Task<IActionResult> OnPostAddAsync() => SaveAsync();

    /// <summary>Save changes to the line in <see cref="EditId"/>, held to the same checks as a new one.</summary>
    public Task<IActionResult> OnPostUpdateAsync() => SaveAsync();

    private async Task<IActionResult> SaveAsync()
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        RicCostEntry? existing = null;
        if (EditId is { } editId && (existing = Editable(editId)) is null)
        {
            return NotFound();
        }

        EntryChecks.ExplainUnreadableNumbers(
            ModelState,
            key => EntryChecks.YearIndex(key) is { } i ? $"{Cycle.StartYear + i} funding" : null,
            "an amount in dollars, such as 20000.00");
        Validate();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var amounts = Enumerable.Range(0, YearCount)
            .Select(i => i < YearAmounts.Count ? YearAmounts[i] : 0m)
            .ToList();

        if (existing is null)
        {
            existing = new RicCostEntry
            {
                RicCycleId = CycleId,
                RicCapabilityId = null,
                Scope = CostEntry.Scopes.Platform,
                CostType = CostEntry.Types.Income
            };
            Db.RicCostEntries.Add(existing);
        }
        else
        {
            Db.RicCostYearAmounts.RemoveRange(existing.YearAmounts);
        }

        existing.Category = Category;
        existing.Description = Description?.Trim();
        existing.Supplier = FundingBody?.Trim();
        existing.Notes = Justification?.Trim();
        existing.Amount = amounts.Average();
        existing.YearAmounts = amounts
            .Select((amount, i) => new RicCostYearAmount { ProjectYear = i + 1, Amount = amount })
            .ToList();

        RecordEdit();
        await Db.SaveChangesAsync();
        return RedirectToPage(new { cycleId = CycleId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!await Load(CycleId) || Editable(id) is not { } item)
        {
            return NotFound();
        }

        if (Cycle.IsEditable)
        {
            Db.Remove(item);
            RecordEdit();
            await Db.SaveChangesAsync();
        }

        return RedirectToPage(new { cycleId = CycleId });
    }

    /// <summary>A funding line of the loaded cycle — never a cost line, never another cycle's.</summary>
    private RicCostEntry? Editable(int id) => Cycle.Costs.FirstOrDefault(x => x.Id == id && x.IsIncome);

    private void Validate()
    {
        if (!CostEntry.IncomeCategories.All.Contains(Category))
        {
            ModelState.AddModelError(nameof(Category), "Select a valid funding source category.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Funding source name is required.");
        }

        if (string.IsNullOrWhiteSpace(Justification))
        {
            ModelState.AddModelError(
                nameof(Justification),
                "Justification is required: say where the funding comes from and how long it is committed for.");
        }

        var entered = YearAmounts.Take(YearCount).ToList();

        if (entered.Any(x => x < 0))
        {
            ModelState.AddModelError(string.Empty, "Funding amounts cannot be negative.");
        }

        if (ModelState.IsValid && !ConfirmLargeAmounts)
        {
            var large = EntryChecks.UnusuallyLarge(
                entered, Cycle.StartYear, "one funding source", EntryChecks.ConfirmIncomeAbove);

            foreach (var message in large)
            {
                ModelState.AddModelError(string.Empty, message);
            }

            NeedsLargeAmountConfirmation = large.Count > 0;
        }
    }

    private async Task<bool> Load(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return false;
        }

        CycleId = cycleId;
        return true;
    }
}
