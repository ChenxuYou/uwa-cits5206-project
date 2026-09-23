using System.Collections;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Pdf.IO;
using Xunit;

namespace CostingTool.Pdf.Tests;

/// <summary>
/// The export, held to the same standard as the engine.
///
/// <c>GoldenFileTests</c> proves the arithmetic reproduces the client's worked example to
/// the cent. That guarantee is worth nothing if the document handed to the client shows a
/// different figure, so the same numbers — <b>$100.00 / $162.00 / $202.50</b> — are
/// asserted again here, on the way out.
///
/// The fixture is a sealed snapshot in the shape <c>Approvals/Details.cshtml.cs</c>
/// writes, schema 1.2, built around that worked example.
/// </summary>
public class SealedRecordPdfTests
{
    private static readonly string FixturePath =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "sealed-record-2026.1.json");

    private static string Snapshot() => File.ReadAllText(FixturePath);

    private const string Hash = "9F2C4A1E8B7D6053E1AA0C7F2B4D8E6190C3A5B7D9E1F3058A2C4E6081B3D5F7";

    // ----------------------------------------------------------------------------------
    // The golden file, on the way out
    // ----------------------------------------------------------------------------------

    [Theory]
    [InlineData("$100.00 per hour")]
    [InlineData("$162.00 per hour")]
    [InlineData("$202.50 per hour")]
    public void The_clients_worked_example_reaches_the_document_to_the_cent(string rate)
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains(rate, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_rate_is_printed_beside_the_arithmetic_that_produced_it()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        // US-17 — "why does it cost $50 an hour?" is answered from the page itself.
        Assert.Contains("(150000 - 50000) / 1000 = 100", text, StringComparison.Ordinal);
        Assert.Contains("((150000 - 30000) / 1000) * 1.35 = 162.00", text, StringComparison.Ordinal);
        Assert.Contains("(150000 / 1000) * 1.35 = 202.50", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_method_version_and_factor_are_named_on_the_document()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        // A record recalculated under a later method version is a different record, so the
        // one it was sealed under has to be readable years later, off the page.
        Assert.Contains("2026.1", text, StringComparison.Ordinal);
        Assert.Contains("1.35", text, StringComparison.Ordinal);
        Assert.Contains("rounding half away from zero", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_integrity_hash_is_printed_in_full()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains(Hash, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_capability_in_the_snapshot_appears()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains("Confocal microscope", text, StringComparison.Ordinal);
        Assert.Contains("Cryo-electron microscope", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_rate_proposed_below_the_sustainable_one_is_said_in_words_not_only_in_colour()
    {
        // The cryo-EM commercial rate is proposed at $240.00 against a calculated $270.00.
        // Colour is never the only carrier of meaning — a monochrome print must still say so.
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains("below cost", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_deficit_is_written_as_a_deficit()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains("($56,500.00) deficit", text, StringComparison.Ordinal);
    }

    // ----------------------------------------------------------------------------------
    // Supersession (F22)
    // ----------------------------------------------------------------------------------

    [Fact]
    public void A_record_that_replaces_another_names_it_with_its_hash()
    {
        const string oldHash = "0A1B2C3D4E5F60718293A4B5C6D7E8F90A1B2C3D4E5F60718293A4B5C6D7E8F9";
        var snapshot = Snapshot()
            .Replace("\"SchemaVersion\": \"1.2\"", "\"SchemaVersion\": \"1.3\"", StringComparison.Ordinal)
            .Replace(
                "\"EffectiveDateUtc\": \"2027-01-01T00:00:00Z\",",
                "\"EffectiveDateUtc\": \"2027-01-01T00:00:00Z\", \"Supersedes\": { \"Id\": 3, \"PlatformName\": \"Microscopy & Characterisation Platform\", "
                + "\"StartYear\": 2023, \"EndYear\": 2025, \"SealedAtUtc\": \"2022-11-30T03:00:00Z\", \"SnapshotHash\": \"" + oldHash + "\" },",
                StringComparison.Ordinal);

        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(snapshot), Hash));

        Assert.Contains("Supersedes", text, StringComparison.Ordinal);
        Assert.Contains("2023–2025", text, StringComparison.Ordinal);
        Assert.Contains(oldHash, text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_record_sealed_before_supersession_was_recorded_says_nothing_about_it()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.DoesNotContain("Supersedes", text, StringComparison.Ordinal);
    }

    // ----------------------------------------------------------------------------------
    // It is a real PDF
    // ----------------------------------------------------------------------------------

    [Fact]
    public void Rendering_produces_a_readable_pdf()
    {
        var bytes = SealedRecordPdf.Render(Snapshot(), Hash);

        Assert.True(bytes.Length > 1000, "A record of two capabilities cannot be under a kilobyte.");
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

        using var stream = new MemoryStream(bytes);
        var pdf = PdfReader.Open(stream, PdfDocumentOpenMode.InformationOnly);

        Assert.True(pdf.PageCount >= 1);
        Assert.Contains("Microscopy", pdf.Info.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void Rendering_twice_produces_the_same_length()
    {
        // Not a full determinism check — a PDF carries a creation date — but enough to
        // catch a renderer that accumulates state between calls, which is the failure
        // mode of a static font resolver installed more than once.
        var first = SealedRecordPdf.Render(Snapshot(), Hash);
        var second = SealedRecordPdf.Render(Snapshot(), Hash);

        Assert.Equal(first.Length, second.Length);
    }

    // ----------------------------------------------------------------------------------
    // It refuses rather than guessing
    // ----------------------------------------------------------------------------------

    [Fact]
    public void An_unsealed_cycle_is_refused_with_a_message_a_custodian_can_read()
    {
        var error = Assert.Throws<SealedRecordFormatException>(() => SealedRecord.Parse(""));

        Assert.Contains("approved", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_schema_version_this_renderer_does_not_know_is_refused()
    {
        var future = Snapshot().Replace("\"SchemaVersion\": \"1.2\"", "\"SchemaVersion\": \"2.0\"", StringComparison.Ordinal);

        var error = Assert.Throws<SealedRecordFormatException>(() => SealedRecord.Parse(future));

        Assert.Contains("2.0", error.Message, StringComparison.Ordinal);
    }

    // ----------------------------------------------------------------------------------
    // Schema 1.4 — who sealed it, and every input (US-15, US-16)
    // ----------------------------------------------------------------------------------

    private static readonly string Schema14Path =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "sealed-record-2026.1-schema-1.4.json");

    private static string Schema14Text() =>
        AllText(SealedRecordPdf.Build(SealedRecord.Parse(File.ReadAllText(Schema14Path)), Hash));

    [Fact]
    public void The_document_names_who_sealed_the_record_and_when()
    {
        var text = Schema14Text();

        Assert.Contains("Sealed by", text, StringComparison.Ordinal);
        Assert.Contains("Dr Mei Chen, 16 September 2026, 02:15 UTC", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_record_sealed_before_1_4_names_the_approver_as_the_sealer()
    {
        // Sealing has always been the approver's act; 1.4 only started naming it separately.
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(Snapshot()), Hash));

        Assert.Contains("Sealed by", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Capacity baseline", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Each_proposed_rate_is_printed_with_its_variance_from_the_calculated_one()
    {
        var text = Schema14Text();

        Assert.Contains("Variance", text, StringComparison.Ordinal);
        Assert.Contains("matches", text, StringComparison.Ordinal);
        // Cryo-EM commercial: calculated $270.00, proposed $240.00.
        Assert.Contains("$30.00 below (11.1%)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_cost_and_income_line_is_printed()
    {
        var text = Schema14Text();

        Assert.Contains("The costs and income entered", text, StringComparison.Ordinal);
        Assert.Contains("Cryo-EM service contract", text, StringComparison.Ordinal);
        Assert.Contains("Platform administration", text, StringComparison.Ordinal);
        Assert.Contains("less $40,000.00", text, StringComparison.Ordinal);
        Assert.Contains("From the 2025 ledger", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_capacity_behind_each_forecast_is_printed()
    {
        var text = Schema14Text();

        Assert.Contains("Machine: 1,882.5 hours", text, StringComparison.Ordinal);
        Assert.Contains("Less maintenance", text, StringComparison.Ordinal);
        Assert.Contains("112.5 hours — 15 days planned service", text, StringComparison.Ordinal);
        Assert.Contains("0.5 FTE must be present", text, StringComparison.Ordinal);
        Assert.Contains("Usable capacity", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_document_says_it_is_to_be_retained_for_audit()
    {
        Assert.Contains("for audit and review", Schema14Text(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_schema_1_4_record_renders_to_a_pdf()
    {
        var bytes = SealedRecordPdf.Render(File.ReadAllText(Schema14Path), Hash);

        Assert.True(bytes.Length > 1000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    /// <summary>
    /// A record sealed before 18 September 2026 carries schema 1.1, which has no line for
    /// the cost net of income and measures its balance against full operating cost. It is
    /// still rendered, and rendered as it was sealed: the lines it does not carry are left
    /// out rather than printed as zero, and the note under the figure is the one that
    /// matches the measure it used.
    /// </summary>
    [Fact]
    public void A_record_sealed_under_the_previous_schema_still_renders_as_it_was_sealed()
    {
        var text = AllText(SealedRecordPdf.Build(SealedRecord.Parse(SealedUnderSchema11), Hash));

        Assert.Contains("($56,500.00) deficit", text, StringComparison.Ordinal);
        Assert.Contains("Forecast revenue at the proposed rates", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Cost to recover from usage", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Against full economic cost", text, StringComparison.Ordinal);
    }

    private const string SealedUnderSchema11 = """
        {
          "SchemaVersion": "1.1",
          "SealedAtUtc": "2026-09-16T02:20:00Z",
          "MethodVersion": "2026.1",
          "Method": {
            "Version": "2026.1",
            "IndirectCostRecovery": 1.35,
            "RateDecimals": 2,
            "MidpointRule": "AwayFromZero",
            "Source": "UWA Costing & Pricing Guide, Step 3"
          },
          "Cycle": {
            "Id": 1,
            "PlatformName": "Microscopy",
            "StartYear": 2026,
            "EndYear": 2027,
            "BillableUnit": "Hours",
            "CreatedByDisplay": "Priya Lal"
          },
          "Platform": {
            "TotalOperatingCost": 250000,
            "ForecastRevenue": 193500,
            "ForecastBalance": -56500
          },
          "Capabilities": []
        }
        """;

    [Fact]
    public void Damaged_json_is_refused_rather_than_half_rendered()
    {
        Assert.Throws<SealedRecordFormatException>(() => SealedRecord.Parse("{ this is not json"));
    }

    // ----------------------------------------------------------------------------------
    // Reading the document back
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Every piece of text in the document, flattened.
    ///
    /// Asserting against the document model rather than against extracted PDF text keeps
    /// these tests about content — did the right figure reach the page — and out of the
    /// business of parsing PDF operators.
    /// </summary>
    private static string AllText(Document document)
    {
        var builder = new StringBuilder();

        foreach (Section section in document.Sections)
        {
            Collect(section.Headers.Primary.Elements, builder);
            Collect(section.Elements, builder);
            Collect(section.Footers.Primary.Elements, builder);
        }

        return builder.ToString();
    }

    private static void Collect(IEnumerable elements, StringBuilder builder)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case Text text:
                    builder.Append(text.Content);
                    break;

                case FormattedText formatted:
                    Collect(formatted.Elements, builder);
                    break;

                case Paragraph paragraph:
                    Collect(paragraph.Elements, builder);
                    builder.Append('\n');
                    break;

                case Table table:
                    foreach (Row row in table.Rows)
                    {
                        foreach (Cell cell in row.Cells)
                        {
                            Collect(cell.Elements, builder);
                            builder.Append('\t');
                        }

                        builder.Append('\n');
                    }

                    break;
            }
        }
    }
}
