using ResumeAnalzer.Web.Models;

namespace ResumeAnalzer.Web
{
    public static class AppState
    {
        public static AnalysisResult? AnalysisResult { get; set; }
        public static int CurrentResumeId { get; set; }
        public static string Token { get; set; } = string.Empty;
    }
}