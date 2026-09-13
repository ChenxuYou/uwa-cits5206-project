using System.Reflection;
using PdfSharp.Fonts;

namespace CostingTool.Pdf;

/// <summary>
/// Resolves every font request to the one face this project ships with.
///
/// <b>Why this class has to exist.</b> PDFsharp's platform-agnostic build has no font
/// of its own: on Linux and macOS its default resolver throws, because there is no
/// font-resolution strategy common to every operating system .NET runs on. A renderer
/// that works on a Windows laptop and throws on the Ubuntu staging server is exactly
/// the class of defect skills-audit gap G2 warns about — found on 2 October, when the
/// client is meant to be using the thing, rather than now.
///
/// So the face is embedded in the assembly and resolved from there. Two consequences,
/// both wanted: the server needs no fonts installed, and a record rendered on staging
/// is the same document as one rendered on a developer's machine.
///
/// DejaVu Sans is used under the Bitstream Vera licence — <c>Fonts/LICENCE-DejaVu.txt</c>,
/// which permits redistribution and embedding in a document.
/// </summary>
public sealed class EmbeddedFontResolver : IFontResolver
{
    /// <summary>The only family name this resolver reports. Ask for anything; get this.</summary>
    public const string FamilyName = "DejaVu Sans";

    private const string RegularFace = "DejaVuSans";
    private const string BoldFace = "DejaVuSans-Bold";

    private static readonly object Gate = new();

    /// <summary>
    /// Install the resolver, once per process.
    ///
    /// PDFsharp keeps the resolver in global state, so this is idempotent on purpose:
    /// every entry point that renders a document calls it, and the second call does
    /// nothing rather than replacing a resolver that is already in use.
    /// </summary>
    public static void Register()
    {
        lock (Gate)
        {
            GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();
        }
    }

    /// <summary>
    /// Map a requested family to a face we actually hold.
    ///
    /// Italic is simulated rather than embedded: the sealed record uses upright text
    /// throughout, and shipping two more font files to cover a style the document does
    /// not use would be 1.4 MB of repository for nothing.
    /// </summary>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        new(isBold ? BoldFace : RegularFace, false, isItalic);

    /// <summary>The face itself, read out of this assembly.</summary>
    public byte[]? GetFont(string faceName)
    {
        var resource = $"CostingTool.Pdf.Fonts.{faceName}.ttf";
        var assembly = typeof(EmbeddedFontResolver).GetTypeInfo().Assembly;

        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException(
                $"The font '{faceName}' is not embedded in this assembly. Expected the resource " +
                $"'{resource}'. Check the EmbeddedResource items in CostingTool.Pdf.csproj — a font " +
                "that is present on disk but not embedded fails here and nowhere earlier.");

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
