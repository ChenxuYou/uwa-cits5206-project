using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;
public class RatesModel(CostingDbContext db, RicCalculationService calculator) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;
    public Dictionary<int,CapabilityRateResult> Results { get; private set; }=[];
    [BindProperty] public int CycleId {get;set;}
    [BindProperty] public List<RateInput> Inputs {get;set;}=[];
    [BindProperty] public string? BenchmarkNotes {get;set;}
    [BindProperty] public string? PricingJustification {get;set;}
    public async Task<IActionResult> OnGetAsync(int cycleId){if(!await Load(cycleId))return NotFound();Inputs=Cycle.Capabilities.Select(c=>new RateInput(c.Id,c.ProposedUwaRate,c.ProposedApfrRate,c.ProposedCommercialRate)).ToList();BenchmarkNotes=Cycle.BenchmarkNotes;PricingJustification=Cycle.PricingJustification;return Page();}
    public async Task<IActionResult> OnPostAsync(){if(!await Load(CycleId))return NotFound();if(Cycle.Status!="Draft")return RedirectToPage("/Ric/Review",new{cycleId=CycleId});foreach(var x in Inputs){if(new[]{x.Uwa,x.Apfr,x.Commercial}.Any(v=>v<0))ModelState.AddModelError(string.Empty,"Proposed rates cannot be negative.");}if(!ModelState.IsValid)return Page();foreach(var x in Inputs){var c=Cycle.Capabilities.First(y=>y.Id==x.Id);c.ProposedUwaRate=x.Uwa;c.ProposedApfrRate=x.Apfr;c.ProposedCommercialRate=x.Commercial;}Cycle.BenchmarkNotes=BenchmarkNotes;Cycle.PricingJustification=PricingJustification;Cycle.UpdatedAtUtc=DateTime.UtcNow;await db.SaveChangesAsync();return RedirectToPage("/Ric/Review",new{cycleId=CycleId});}
    private async Task<bool> Load(int id){Cycle=(await db.RicCycles.Include(x=>x.Capabilities).Include(x=>x.Costs).FirstOrDefaultAsync(x=>x.Id==id))!;if(Cycle is null)return false;CycleId=id;Results=Cycle.Capabilities.ToDictionary(x=>x.Id,x=>calculator.Calculate(Cycle,x));return true;}
    public class RateInput
    {
        public RateInput() { }
        public RateInput(int id, decimal uwa, decimal apfr, decimal commercial) => (Id, Uwa, Apfr, Commercial) = (id, uwa, apfr, commercial);
        public int Id { get; set; }
        public decimal Uwa { get; set; }
        public decimal Apfr { get; set; }
        public decimal Commercial { get; set; }
    }
}
