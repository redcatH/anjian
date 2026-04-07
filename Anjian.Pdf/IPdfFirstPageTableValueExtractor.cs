namespace Anjian;

public interface IPdfFirstPageTableValueExtractor
{
    PdfFirstPageTableValueResult Extract(string pdfPath, string rowLabel, string columnLabel);
}
