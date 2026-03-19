using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Xceed.Words.NET;
using System.Text;

namespace ResumeAnalyzer.Infrastructure.Services
{
    public class ResumeParserService
    {
        public string ParseResume(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".pdf" => ParsePdf(filePath),
                ".docx" => ParseDocx(filePath),
                _ => throw new NotSupportedException(
                    $"File type '{extension}' is not supported. Please upload PDF or DOCX.")
            };
        }

        private string ParsePdf(string filePath)
        {
            var sb = new StringBuilder();

            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(
                    pdfDoc.GetPage(i), strategy);
                sb.AppendLine(pageText);
            }

            return sb.ToString();
        }

        private string ParseDocx(string filePath)
        {
            using var doc = DocX.Load(filePath);
            var sb = new StringBuilder();

            foreach (var paragraph in doc.Paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph.Text))
                    sb.AppendLine(paragraph.Text);
            }

            return sb.ToString();
        }
    }
}