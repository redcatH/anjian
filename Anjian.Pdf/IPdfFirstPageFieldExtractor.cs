using System.Collections.Generic;

namespace Anjian;

public interface IPdfFirstPageFieldExtractor
{
    PdfFirstPageExtractResult Extract(string pdfPath, IReadOnlyList<PdfFieldDefinition> fields);

    IReadOnlyList<PdfFirstPageExtractResult> ExtractDirectory(string directoryPath, IReadOnlyList<PdfFieldDefinition> fields);
}
