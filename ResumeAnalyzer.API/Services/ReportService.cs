using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using ResumeAnalyzer.Core.Models;
using System.Text.Json;

namespace ResumeAnalyzer.API.Services
{
    public class ReportService
    {
        public byte[] GenerateReport(Analysis analysis, string fileName)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // ── Fonts ────────────────────────────────────────
            var boldFont = PdfFontFactory.CreateFont(
                StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(
                StandardFonts.HELVETICA);

            // ── Colors ───────────────────────────────────────
            var primaryColor = new DeviceRgb(13, 110, 253);
            var successColor = new DeviceRgb(25, 135, 84);
            var warningColor = new DeviceRgb(255, 193, 7);
            var dangerColor = new DeviceRgb(220, 53, 69);
            var infoColor = new DeviceRgb(13, 202, 240);
            var lightGrayColor = new DeviceRgb(248, 249, 250);
            var darkColor = new DeviceRgb(33, 37, 41);

            // ── Header ───────────────────────────────────────
            var header = new Paragraph("📄 Resume Analysis Report")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetFontColor(ColorConstants.WHITE)
                .SetBackgroundColor(primaryColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(15);
            document.Add(header);

            // ── File Info ────────────────────────────────────
            document.Add(new Paragraph($"Resume: {fileName}")
                .SetFont(regularFont)
                .SetFontSize(11)
                .SetFontColor(darkColor)
                .SetMarginTop(10));

            document.Add(new Paragraph(
                $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                .SetFont(regularFont)
                .SetFontSize(11)
                .SetFontColor(darkColor));

            // ── Scores Section ───────────────────────────────
            document.Add(new Paragraph("Scores")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetFontColor(primaryColor)
                .SetMarginTop(20));

            // ATS Score
            var atsColor = analysis.ATSScore >= 70 ? successColor :
                           analysis.ATSScore >= 50 ? warningColor : dangerColor;

            document.Add(new Paragraph(
                $"ATS Score: {analysis.ATSScore} / 100")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(atsColor)
                .SetBackgroundColor(lightGrayColor)
                .SetPadding(10)
                .SetMarginTop(5));

            // Job Match Score
            if (analysis.JobMatch != null)
            {
                document.Add(new Paragraph(
                    $"Job Match Score: {analysis.JobMatch.MatchScore} / 100")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(infoColor)
                    .SetBackgroundColor(lightGrayColor)
                    .SetPadding(10)
                    .SetMarginTop(5));
            }

            // ── Skills Section ───────────────────────────────
            AddSectionHeader(document, boldFont, primaryColor,
                "Skills Found");

            var skills = DeserializeList(analysis.SkillsJson);
            var skillsText = string.Join(" • ", skills);
            document.Add(new Paragraph(skillsText)
                .SetFont(regularFont)
                .SetFontSize(11)
                .SetFontColor(darkColor)
                .SetBackgroundColor(lightGrayColor)
                .SetPadding(10));

            // ── Strengths Section ────────────────────────────
            AddSectionHeader(document, boldFont, successColor,
                "Strengths");

            var strengths = DeserializeList(analysis.Strengths);
            foreach (var strength in strengths)
            {
                document.Add(new Paragraph($"✓  {strength}")
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetFontColor(darkColor)
                    .SetMarginLeft(10));
            }

            // ── Experience Summary ───────────────────────────
            AddSectionHeader(document, boldFont, infoColor,
                "Experience Summary");

            document.Add(new Paragraph(analysis.ExperienceSummary)
                .SetFont(regularFont)
                .SetFontSize(11)
                .SetFontColor(darkColor)
                .SetBackgroundColor(lightGrayColor)
                .SetPadding(10));

            // ── Suggestions Section ──────────────────────────
            AddSectionHeader(document, boldFont, warningColor,
                "Improvement Suggestions");

            var suggestions = DeserializeList(analysis.SuggestionsJson);
            for (int i = 0; i < suggestions.Count; i++)
            {
                document.Add(new Paragraph($"{i + 1}.  {suggestions[i]}")
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetFontColor(darkColor)
                    .SetMarginLeft(10)
                    .SetMarginTop(3));
            }

            // ── Missing Keywords Section ─────────────────────
            if (analysis.JobMatch != null)
            {
                AddSectionHeader(document, boldFont, dangerColor,
                    "Missing Keywords");

                var keywords = DeserializeList(
                    analysis.JobMatch.MissingKeywords);
                var keywordsText = string.Join(" • ", keywords);
                document.Add(new Paragraph(keywordsText)
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetFontColor(dangerColor)
                    .SetBackgroundColor(lightGrayColor)
                    .SetPadding(10));
            }

            // ── Footer ───────────────────────────────────────
            document.Add(new Paragraph(
                "\nGenerated by Resume Analyzer — Powered by Groq AI")
                .SetFont(regularFont)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(20));

            document.Close();
            return memoryStream.ToArray();
        }

        private void AddSectionHeader(Document document,
            PdfFont boldFont, Color color, string title)
        {
            document.Add(new Paragraph(title)
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(color)
                .SetMarginTop(15)
                .SetBorderBottom(new iText.Layout.Borders
                    .SolidBorder(color, 1)));
        }

        private List<string> DeserializeList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json)
                    ?? new List<string>();
            }
            catch { return new List<string>(); }
        }
    }
}