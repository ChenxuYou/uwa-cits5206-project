using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Approvals;

public class IndexModel(CostingDbContext db) : PageModel
{
    public List<RicCycle> Pending { get; private set; } = [];
    public List<RicCycle> Completed { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Pending = await db.RicCycles.AsNoTracking()
            .Where(x => x.Status == "Submitted")
            .OrderBy(x => x.SubmittedAtUtc)
            .ToListAsync();
        Completed = await db.RicCycles.AsNoTracking()
            .Where(x => x.Status == "Sealed" || x.Status == "Returned")
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(10)
            .ToListAsync();
    }
}
