using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace CostingTool.Pdf;

/// <summary>
/// Turns a sealed snapshot into the document a custodian can attach to a Freedom of
/// Information response.
///
/// <b>What the document has to survive.</b> Someone reads it in 2030, asks "why does this
/// cost $50 an hour?", and must be able to answer from the page in front of them without
/// the application, the database or us. So every rate is printed beside the arithmetic
/// that produced it, the method version and factor are named, and the integrity hash is
/// on the last page. That is US-16 and US-17, and it is the client's own request of
/// 20 August 2026 that the record show "the workings for the calculator".
///
/// The renderer is deliberately dull: it reads a <see cref="SealedRecord"/> and writes it
/// out. It calculates nothing. If a figure is not in the snapshot it does not appear.
/// </summary>
public static class SealedRecordPdf
{
    private const string Face = EmbeddedFontResolver.FamilyName;

    // The one accent. Matches --red-600 in the presentation style guide, so a printed
    // record and a deck put in front of the same client look like one project.
    private static readonly Color Accent = new(0xC8, 0x10, 0x2E);
    private static readonly Color Ink = new(0x0B, 0x0B, 0x0C);
    private static readonly Color Muted = new(0x5C, 0x55, 0x4D);
    private static readonly Color Panel = new(0xF1, 0xED, 0xE7);
    private static readonly Color Rule = new(0xDD, 0xD6, 0xCC);

    /// <summary>
    /// Render a stored snapshot.
    /// </summary>
    /// <param name="snapshotJson">The cycle's <c>SnapshotJson</c>, exactly as sealed.</param>
    /// <param name="snapshotHash">The cycle's <c>SnapshotHash</c>, printed for verification.</param>
    public static byte[] Render(string snapshotJson, string? snapshotHash) =>
        Render(SealedRecord.Parse(snapshotJson), snapshotHash);

    /// <summary>Render a parsed snapshot.</summary>
    public static byte[] Render(SealedRecord record, string? snapshotHash)
    {
        ArgumentNullException.ThrowIfNull(record);

        EmbeddedFontResolver.Register();

        var renderer = new PdfDocumentRenderer { Document = Build(record, snapshotHash) };
        renderer.RenderDocument();

        using var buffer = new MemoryStream();
        renderer.PdfDocument.Save(buffer, false);
        return buffer.ToArray();
    }

    /// <summary>
    /// The document model, before it becomes bytes. Public so a test can assert on the
    /// document without rendering one.
    /// </summary>
    public static Document Build(SealedRecord record, string? snapshotHash)
    {
        ArgumentNullException.ThrowIfNull(record);

        var cycle = record.Cycle;
        var platformName = string.IsNullOrWhiteSpace(cycle?.PlatformName) ? "Research platform" : cycle!.PlatformName!;

        var document = new Document();
        document.Info.Title = $"Costing record — {platformName}";
        document.Info.Subject = "Sealed research infrastructure costing and pricing record";
        document.Info.Author = "UWA Research Infrastructure Costing Tool";

        var normal = document.Styles["Normal"]!;
        normal.Font.Name = Face;
        normal.Font.Size = 9.5;
        normal.Font.Color = Ink;
        normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.Orientation = Orientation.Portrait;
        section.PageSetup.TopMargin = Unit.FromCentimeter(2.0);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2.0);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2.2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(2.2);

        AddRunningHead(section, platformName, cycle);
        AddFooter(section, snapshotHash);

        AddTitleBlock(section, record, platformName, cycle);
        AddMethodBlock(section, record);

        // US-13: every justification sits beside the figures it explains — the costing
        // assumptions and each line's note with the costs, the utilisation assumptions and each
        // deduction's note with the capacity, the pricing justification after the balance.
        AddCostsAndIncome(section, record);
        AddCapacity(section, record, cycle?.BillableUnit);

        foreach (var capability in record.Capabilities)
        {
            AddCapability(section, capability, cycle?.BillableUnit);
        }

        AddPlatformSummary(section, record, cycle?.BillableUnit);
        AddJustification(section, cycle);
        AddIntegrityBlock(section, record, snapshotHash);

        return document;
    }

    // ----------------------------------------------------------------------------------
    // Page furniture
    // ----------------------------------------------------------------------------------

    private static void AddRunningHead(Section section, string platformName, SealedCycle? cycle)
    {
        var head = section.Headers.Primary.AddParagraph();
        head.Format.Font.Size = 7.5;
        head.Format.Font.Color = Muted;
        head.Format.SpaceAfter = Unit.FromPoint(6);
        head.Format.Borders.Bottom.Width = 0.5;
        head.Format.Borders.Bottom.Color = Rule;
        head.AddText($"{platformName}  ·  sealed costing record");

        if (cycle is not null && cycle.StartYear > 0)
        {
            head.AddText($"  ·  {cycle.StartYear}–{cycle.EndYear}");
        }
    }

    private static void AddFooter(Section section, string? snapshotHash)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Font.Size = 7.5;
        footer.Format.Font.Color = Muted;
        footer.Format.Alignment = ParagraphAlignment.Left;

        if (!string.IsNullOrWhiteSpace(snapshotHash))
        {
            // The first twelve characters are enough to match a page against the record
            // at a glance; the whole hash is printed once, on the last page.
            footer.AddText($"SHA-256 {snapshotHash[..Math.Min(12, snapshotHash.Length)]}…    ");
        }

        footer.AddText("Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();
    }

    // ----------------------------------------------------------------------------------
    // Content blocks
    // ----------------------------------------------------------------------------------

    private static void AddTitleBlock(Section section, SealedRecord record, string platformName, SealedCycle? cycle)
    {
        var eyebrow = section.AddParagraph("SEALED COSTING RECORD");
        eyebrow.Format.Font.Size = 7.5;
        eyebrow.Format.Font.Bold = true;
        eyebrow.Format.Font.Color = Accent;
        eyebrow.Format.SpaceAfter = Unit.FromPoint(2);

        var title = section.AddParagraph(platformName);
        title.Format.Font.Size = 20;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = Unit.FromPoint(2);

        var period = section.AddParagraph(
            cycle is null || cycle.StartYear == 0
                ? "Pricing period not recorded"
                : $"Pricing period {cycle.StartYear}–{cycle.EndYear}  ·  billed in {(cycle.BillableUnit ?? "units").ToLower(RecordFormat.Culture)}");
        period.Format.Font.Size = 10;
        period.Format.Font.Color = Muted;
        period.Format.SpaceAfter = Unit.FromPoint(10);
        period.Format.Borders.Bottom.Width = 1.5;
        period.Format.Borders.Bottom.Color = Accent;

        var facts = KeyValueTable(section);
        AddFact(facts, "Prepared by", cycle?.CreatedByDisplay);
        AddFact(facts, "Submitted for approval", $"{cycle?.SubmittedBy ?? "—"}, {RecordFormat.Timestamp(cycle?.SubmittedAtUtc)}");
        AddFact(facts, "Approved by delegated authority", $"{cycle?.ApprovedBy ?? "—"}, {RecordFormat.Timestamp(cycle?.ApprovedAtUtc)}");
        AddFact(facts, "Rates effective from", RecordFormat.Date(cycle?.EffectiveDateUtc));
        AddFact(facts, "Sealed", RecordFormat.Timestamp(record.SealedAtUtc));

        if (!string.IsNullOrWhiteSpace(cycle?.ApprovalComment))
        {
            AddFact(facts, "Approver's comment", cycle!.ApprovalComment);
        }

        Space(section, 10);
    }

    private static void AddMethodBlock(Section section, SealedRecord record)
    {
        var method = record.Method;

        Heading(section, "How these rates were calculated");

        var lead = section.AddParagraph(
            $"Version {method?.Version ?? record.MethodVersion ?? "—"} of the University's costing method, as it stood when this " +
            "record was sealed. A later change to the method does not change this record: it is " +
            "recalculated under the version named here, not under today's.");
        lead.Format.Font.Size = 9;
        lead.Format.Font.Color = Muted;
        lead.Format.SpaceAfter = Unit.FromPoint(6);

        var facts = KeyValueTable(section);
        AddFact(facts, "Indirect cost recovery (k)", method is null
            ? "—"
            : $"{method.IndirectCostRecovery.ToString("0.00", RecordFormat.Culture)} — a {(method.IndirectCostRecovery - 1m) * 100m:0.#}% uplift");
        AddFact(facts, "Rounding", method is null
            ? "—"
            : $"{method.RateDecimals} decimal places, {Humanise(method.MidpointRule)} at an exact half");
        AddFact(facts, "Source of the method", method?.Source);

        if (method?.Formulas is { } formulas)
        {
            Space(section, 6);
            var table = FormulaTable(section);
            AddFormula(table, "UWA researcher", formulas.UwaResearcher);
            AddFormula(table, "APFR", formulas.Apfr);
            AddFormula(table, "Commercial", formulas.Commercial);
        }

        Space(section, 10);
    }

    private static void AddCapability(Section section, SealedCapability capability, string? billableUnit)
    {
        var result = capability.Result;

        Heading(section, capability.Name ?? result?.CapabilityName ?? "Capability");

        if (result is null)
        {
            var missing = section.AddParagraph(
                "No rates were stored for this capability. A record is never sealed around a " +
                "capability that could not be priced, so a snapshot in this state means the " +
                "stored record is damaged — raise it rather than re-sealing.");
            missing.Format.Font.Color = Accent;
            Space(section, 8);
            return;
        }

        // ---- The three rates, and what was actually proposed --------------------------
        var rates = section.AddTable();
        rates.Borders.Width = 0;
        rates.AddColumn(Unit.FromCentimeter(5.6));
        rates.AddColumn(Unit.FromCentimeter(5.4));
        rates.AddColumn(Unit.FromCentimeter(5.4));

        var header = rates.AddRow();
        header.Shading.Color = Panel;
        header.TopPadding = Unit.FromPoint(3);
        header.BottomPadding = Unit.FromPoint(3);
        HeaderCell(header.Cells[0], "Rate");
        HeaderCell(header.Cells[1], "Minimum sustainable", right: true);
        HeaderCell(header.Cells[2], "Proposed and charged", right: true);

        AddRateRow(rates, "UWA researcher", result.DisplayUwaRate, result.ProposedUwaRate, billableUnit);
        AddRateRow(rates, "APFR", result.DisplayApfrRate, result.ProposedApfrRate, billableUnit);
        AddRateRow(rates, "Commercial", result.DisplayCommercialRate, result.ProposedCommercialRate, billableUnit);

        Space(section, 8);

        // ---- The workings -------------------------------------------------------------
        var workingsLead = section.AddParagraph("The figures behind those rates");
        workingsLead.Format.Font.Size = 9;
        workingsLead.Format.Font.Bold = true;
        workingsLead.Format.SpaceAfter = Unit.FromPoint(4);

        var facts = KeyValueTable(section);
        AddFact(facts, "Operating cost booked to this capability", RecordFormat.Money(result.CapabilityOperatingCost));
        AddFact(facts, "Share of platform-level cost", RecordFormat.Money(result.AllocatedPlatformCost));
        AddFact(facts, "Total operating cost (C)", RecordFormat.Money(result.TotalOperatingCost));
        AddFact(facts, "UWA non-variable income", RecordFormat.Money(result.UwaIncome));
        AddFact(facts, "Non-UWA non-variable income", RecordFormat.Money(result.NonUwaIncome));
        AddFact(facts, "Forecast utilisation (U)", RecordFormat.Quantity(result.ForecastUtilisation, billableUnit));

        if (capability.Workings is { } workings)
        {
            Space(section, 6);
            var arithmetic = FormulaTable(section);
            AddFormula(arithmetic, "UWA researcher", workings.UwaResearcher);
            AddFormula(arithmetic, "APFR", workings.Apfr);
            AddFormula(arithmetic, "Commercial", workings.Commercial);
        }

        Space(section, 10);
    }

    private static void AddPlatformSummary(Section section, SealedRecord record, string? billableUnit)
    {
        var platform = record.Platform;
        if (platform is null)
        {
            return;
        }

        Heading(section, "The platform, at the proposed rates");

        var facts = KeyValueTable(section);
        // Schema 1.2 added the workbook's other lines. A record sealed under 1.1 does not
        // carry them, and they are not back-filled here: a figure this document never held
        // is not a figure the approver saw.
        var carriesTheBalanceLines = platform.NetCostToRecover is not null;

        AddFact(facts, "Total operating cost", RecordFormat.Money(platform.TotalOperatingCost));

        if (carriesTheBalanceLines)
        {
            AddFact(facts, "Less non-variable income", RecordFormat.Money(platform.TotalIncome ?? 0m));
            AddFact(facts, "Cost to recover from usage", RecordFormat.Money(platform.NetCostToRecover!.Value));
            AddFact(facts, "Billed to users at the proposed rates", RecordFormat.Money(platform.GrossForecastRevenue ?? 0m));
            AddFact(facts, "Less University overheads recovered", RecordFormat.Money(platform.OverheadsRecovered ?? 0m));
        }

        AddFact(
            facts,
            carriesTheBalanceLines ? "Retained by the platform" : "Forecast revenue at the proposed rates",
            RecordFormat.Money(platform.ForecastRevenue));
        AddFact(facts, "Forecast balance", RecordFormat.Balance(platform.ForecastBalance), emphasis: true);

        if (platform.FullEconomicCostBalance is not null)
        {
            AddFact(facts, "Against full economic cost", RecordFormat.Balance(platform.FullEconomicCostBalance.Value));
        }

        var note = section.AddParagraph(carriesTheBalanceLines
            ? "The forecast balance measures what the platform retains at the proposed rates against "
              + "its operating cost less non-variable income — the money usage actually has to recover. "
              + "The line beneath it measures the same revenue against full economic cost, with no "
              + "income deducted, and is negative wherever recurrent funding carries part of the cost. "
              + "A deficit is not an error; it is the amount that will not be recovered at the rates "
              + "proposed, and the record says why it was accepted."
            : "A deficit here is not necessarily an error: the UWA researcher rate is set below full "
              + "cost by design, because non-variable income has already been deducted from it. What "
              + "the figure shows is how much of the platform's cost is not recovered from usage at "
              + "the rates proposed.");
        note.Format.Font.Size = 8.5;
        note.Format.Font.Color = Muted;
        note.Format.SpaceBefore = Unit.FromPoint(4);

        Space(section, 10);
    }

    // ----------------------------------------------------------------------------------
    // The inputs, with what explains them (US-13)
    // ----------------------------------------------------------------------------------

    private static void AddCostsAndIncome(Section section, SealedRecord record)
    {
        var costingAssumptions = record.Cycle?.CostingAssumptions;
        if (record.Costs.Count == 0 && string.IsNullOrWhiteSpace(costingAssumptions))
        {
            return;
        }

        Heading(section, "Operating costs and non-variable income");
        Lead(section, "Every line as sealed, with the note or justification entered against it. Amounts are the "
                      + "annual average over the pricing period, GST exclusive.");

        if (!string.IsNullOrWhiteSpace(costingAssumptions))
        {
            Label(section, "Costing assumptions");
            Quote(section, costingAssumptions!);
        }

        foreach (var capability in record.Capabilities)
        {
            var lines = record.Costs.Where(x => x.RicCapabilityId == capability.Id).ToList();
            if (lines.Count > 0)
            {
                Label(section, $"{capability.Name ?? "Capability"} — booked to this capability");
                LineTable(section, lines);
            }
        }

        var platform = record.Costs.Where(x => x.RicCapabilityId is null).ToList();
        if (platform.Count > 0)
        {
            var ways = record.Capabilities.Count;
            Label(section, $"Platform level — split evenly across the {ways} {(ways == 1 ? "capability" : "capabilities")}");
            LineTable(section, platform);
        }

        Space(section, 8);
    }

    private static void LineTable(Section section, IEnumerable<SealedCostLine> lines)
    {
        var table = section.AddTable();
        table.Borders.Width = 0;
        table.AddColumn(Unit.FromCentimeter(6.8));
        table.AddColumn(Unit.FromCentimeter(6.2));
        table.AddColumn(Unit.FromCentimeter(3.4));

        foreach (var line in lines.OrderBy(x => x.IsIncome).ThenBy(x => x.Id))
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(2);

            var name = line.Description ?? line.PersonnelName ?? line.Category ?? "Line";
            var item = row.Cells[0].AddParagraph(line.Position is null ? name : $"{name} ({line.Position})");
            item.Format.Font.Size = 9;

            var kind = line.IsIncome ? $"Income · {line.Category}" : line.Category ?? "";
            if (line.FloorArea is { } area && line.FloorAreaRate is { } rate)
            {
                kind += $" · {area:#,##0.##} m² × {RecordFormat.Money(rate)} per m²";
            }

            var category = row.Cells[1].AddParagraph(kind);
            category.Format.Font.Size = 8.5;
            category.Format.Font.Color = Muted;

            var amount = row.Cells[2].AddParagraph(line.IsIncome ? $"−{RecordFormat.Money(line.Amount)}" : RecordFormat.Money(line.Amount));
            amount.Format.Font.Size = 9;
            amount.Format.Alignment = ParagraphAlignment.Right;

            // The line's own explanation, directly beneath it.
            var explanation = table.AddRow();
            explanation.BottomPadding = Unit.FromPoint(3);
            explanation.Borders.Bottom.Width = 0.25;
            explanation.Borders.Bottom.Color = Rule;
            explanation.Cells[0].MergeRight = 2;
            var note = explanation.Cells[0].AddParagraph(
                string.IsNullOrWhiteSpace(line.Notes)
                    ? "No note recorded."
                    : $"{(line.IsIncome ? "Justification" : "Note")}: {line.Notes}");
            note.Format.Font.Size = 8.5;
            note.Format.Font.Color = Muted;
            note.Format.LeftIndent = Unit.FromPoint(6);
        }

        Space(section, 4);
    }

    private static void AddCapacity(Section section, SealedRecord record, string? billableUnit)
    {
        var assumptions = record.Cycle?.UtilisationAssumptions;
        if (record.Capabilities.Count == 0)
        {
            return;
        }

        Heading(section, "Capacity and forecast use");
        Lead(section, "Every rate is divided by forecast use, not by capacity. Capacity is the ceiling the forecast "
                      + "is judged against.");

        if (!string.IsNullOrWhiteSpace(assumptions))
        {
            Label(section, "Utilisation assumptions");
            Quote(section, assumptions!);
        }

        var method = record.Method;

        foreach (var capability in record.Capabilities)
        {
            Label(section, capability.Name ?? "Capability");
            var facts = KeyValueTable(section);

            // Schema 1.3 records how the capacity was built. An older record holds only the
            // figure, and says so rather than inventing a derivation.
            if (capability.CapacityBaseline is { } baseline && capability.CapacityBaselineAmount is { } amount)
            {
                var (label, basis) = baseline switch
                {
                    "Machine" => ("Machine availability", method?.MachineAvailableDays is { } days
                        ? $"{days:#,##0.##} days ({method.MachineAvailabilityBasis})" : null),
                    "Staff" => ("Staff availability", method?.StaffAvailableDays is { } days
                        ? $"{days:#,##0.##} {method.StaffAvailabilityBasis}" : null),
                    _ => ("Stated baseline", capability.StatedBaselineNote)
                };

                AddFact(facts, label, RecordFormat.Quantity(amount, billableUnit), note: basis);

                foreach (var deduction in capability.CapacityDeductions)
                {
                    AddFact(
                        facts,
                        $"Less {deduction.Kind?.ToLowerInvariant()}",
                        $"−{RecordFormat.Quantity(deduction.Amount, billableUnit)}",
                        note: deduction.Note);
                }

                if (capability.IsStaffReliant == true && capability.StaffCapacity is { } cap)
                {
                    AddFact(
                        facts,
                        "Staff cap — a person must be present",
                        $"{capability.StaffFte:0.###} FTE: {RecordFormat.Quantity(cap, billableUnit)}");
                }
            }

            AddFact(facts, "Usable capacity", RecordFormat.Quantity(capability.MaximumCapacity, billableUnit), emphasis: true);

            var forecast = capability.ForecastUwaUse + capability.ForecastApfrUse + capability.ForecastCommercialUse;
            AddFact(
                facts,
                "Forecast use",
                $"{RecordFormat.Quantity(forecast, billableUnit)} — UWA {capability.ForecastUwaUse:#,##0.##}, "
                + $"APFR {capability.ForecastApfrUse:#,##0.##}, commercial {capability.ForecastCommercialUse:#,##0.##}");

            if (capability.MaximumCapacity > 0)
            {
                AddFact(facts, "Forecast as a share of capacity", $"{forecast / capability.MaximumCapacity * 100m:0.#}%");
            }

            if (!string.IsNullOrWhiteSpace(capability.AboveCapacityReason))
            {
                AddFact(facts, "Why the forecast exceeds capacity", capability.AboveCapacityReason);
            }

            Space(section, 4);
        }

        Space(section, 6);
    }

    private static void AddJustification(Section section, SealedCycle? cycle)
    {
        if (cycle is null ||
            (string.IsNullOrWhiteSpace(cycle.PricingJustification)
             && string.IsNullOrWhiteSpace(cycle.BenchmarkNotes)))
        {
            return;
        }

        // The utilisation assumptions used to sit here as well. They now head the capacity
        // section, beside the figures they explain.
        Heading(section, "Why these rates were proposed");

        if (!string.IsNullOrWhiteSpace(cycle.PricingJustification))
        {
            Quote(section, cycle.PricingJustification!);
        }

        if (!string.IsNullOrWhiteSpace(cycle.BenchmarkNotes))
        {
            var label = section.AddParagraph("Benchmarking");
            label.Format.Font.Size = 9;
            label.Format.Font.Bold = true;
            label.Format.SpaceBefore = Unit.FromPoint(6);
            label.Format.SpaceAfter = Unit.FromPoint(2);
            Quote(section, cycle.BenchmarkNotes!);
        }

        Space(section, 10);
    }

    private static void AddIntegrityBlock(Section section, SealedRecord record, string? snapshotHash)
    {
        Heading(section, "Integrity");

        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Size = 8.5;
        paragraph.Format.Font.Color = Muted;
        paragraph.Format.Shading.Color = Panel;
        paragraph.Format.Borders.Left.Width = 2;
        paragraph.Format.Borders.Left.Color = Accent;
        paragraph.Format.LeftIndent = Unit.FromPoint(8);
        paragraph.Format.SpaceBefore = Unit.FromPoint(2);

        paragraph.AddText(
            "This document was produced from the sealed snapshot of the record, not from the live " +
            "database, so it says what was approved rather than what the system holds today. The " +
            "snapshot is stored with the SHA-256 hash below; recomputing the hash over the stored " +
            "snapshot reproduces it exactly if neither has been altered.");
        paragraph.AddLineBreak();
        paragraph.AddLineBreak();
        paragraph.AddText($"Snapshot schema {record.SchemaVersion ?? "—"}   ·   method version {record.MethodVersion ?? "—"}");
        paragraph.AddLineBreak();

        var hash = paragraph.AddFormattedText(string.IsNullOrWhiteSpace(snapshotHash) ? "(not recorded)" : snapshotHash);
        hash.Font.Size = 8;
        hash.Font.Color = Ink;
    }

    // ----------------------------------------------------------------------------------
    // Small builders
    // ----------------------------------------------------------------------------------

    private static void Heading(Section section, string text)
    {
        var heading = section.AddParagraph(text);
        heading.Format.Font.Size = 12;
        heading.Format.Font.Bold = true;
        heading.Format.SpaceBefore = Unit.FromPoint(6);
        heading.Format.SpaceAfter = Unit.FromPoint(5);
        heading.Format.Borders.Bottom.Width = 0.5;
        heading.Format.Borders.Bottom.Color = Rule;
        heading.Format.KeepWithNext = true;
    }

    private static void Quote(Section section, string text)
    {
        var quote = section.AddParagraph(text);
        quote.Format.Font.Size = 9;
        quote.Format.LeftIndent = Unit.FromPoint(8);
        quote.Format.Borders.Left.Width = 2;
        quote.Format.Borders.Left.Color = Rule;
        quote.Format.SpaceAfter = Unit.FromPoint(4);
    }

    private static void Space(Section section, double points)
    {
        var spacer = section.AddParagraph();
        spacer.Format.SpaceAfter = Unit.FromPoint(points);
    }

    private static Table KeyValueTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Width = 0;
        table.Rows.LeftIndent = 0;
        table.AddColumn(Unit.FromCentimeter(7.4));
        table.AddColumn(Unit.FromCentimeter(9.0));
        return table;
    }

    private static void Lead(Section section, string text)
    {
        var lead = section.AddParagraph(text);
        lead.Format.Font.Size = 8.5;
        lead.Format.Font.Color = Muted;
        lead.Format.SpaceAfter = Unit.FromPoint(6);
    }

    private static void Label(Section section, string text)
    {
        var label = section.AddParagraph(text);
        label.Format.Font.Size = 9;
        label.Format.Font.Bold = true;
        label.Format.SpaceBefore = Unit.FromPoint(6);
        label.Format.SpaceAfter = Unit.FromPoint(3);
        label.Format.KeepWithNext = true;
    }

    private static void AddFact(Table table, string label, string? value, bool emphasis = false, string? note = null)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(1.5);
        row.BottomPadding = Unit.FromPoint(1.5);
        row.Borders.Bottom.Width = 0.25;
        row.Borders.Bottom.Color = Rule;

        var key = row.Cells[0].AddParagraph(label);
        key.Format.Font.Size = 9;
        key.Format.Font.Color = Muted;

        var text = row.Cells[1].AddParagraph(string.IsNullOrWhiteSpace(value) ? "—" : value);
        text.Format.Font.Size = 9;
        text.Format.Font.Bold = emphasis;

        if (!string.IsNullOrWhiteSpace(note))
        {
            var why = row.Cells[1].AddParagraph(note);
            why.Format.Font.Size = 8.5;
            why.Format.Font.Color = Muted;
        }
    }

    private static Table FormulaTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Width = 0;
        table.AddColumn(Unit.FromCentimeter(3.6));
        table.AddColumn(Unit.FromCentimeter(12.8));
        return table;
    }

    private static void AddFormula(Table table, string label, string? formula)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(2);
        row.BottomPadding = Unit.FromPoint(2);
        row.Shading.Color = Panel;

        var key = row.Cells[0].AddParagraph(label);
        key.Format.Font.Size = 8.5;
        key.Format.Font.Color = Muted;

        var text = row.Cells[1].AddParagraph(string.IsNullOrWhiteSpace(formula) ? "—" : formula);
        text.Format.Font.Size = 8.5;
    }

    private static void AddRateRow(Table table, string label, decimal calculated, decimal proposed, string? billableUnit)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(3);
        row.BottomPadding = Unit.FromPoint(3);
        row.Borders.Bottom.Width = 0.25;
        row.Borders.Bottom.Color = Rule;

        var name = row.Cells[0].AddParagraph(label);
        name.Format.Font.Size = 9.5;

        var minimum = row.Cells[1].AddParagraph(RecordFormat.Rate(calculated, billableUnit));
        minimum.Format.Font.Size = 9.5;
        minimum.Format.Alignment = ParagraphAlignment.Right;

        var charged = row.Cells[2].AddParagraph(RecordFormat.Rate(proposed, billableUnit));
        charged.Format.Font.Size = 9.5;
        charged.Format.Font.Bold = true;
        charged.Format.Alignment = ParagraphAlignment.Right;

        // Colour is never the only carrier: a rate below the sustainable one is marked in
        // words as well, in the cell beside it.
        if (proposed < calculated)
        {
            charged.AddText("  below cost");
            charged.Format.Font.Color = Accent;
        }
    }

    private static void HeaderCell(Cell cell, string text, bool right = false)
    {
        var paragraph = cell.AddParagraph(text);
        paragraph.Format.Font.Size = 7.5;
        paragraph.Format.Font.Bold = true;
        paragraph.Format.Font.Color = Muted;
        paragraph.Format.Alignment = right ? ParagraphAlignment.Right : ParagraphAlignment.Left;
    }

    private static string Humanise(string? midpointRule) => midpointRule switch
    {
        "AwayFromZero" => "rounding half away from zero",
        "ToEven" => "rounding half to even",
        null or "" => "rounding rule not recorded",
        _ => midpointRule
    };
}
