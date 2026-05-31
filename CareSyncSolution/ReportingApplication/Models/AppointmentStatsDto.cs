namespace ReportingApplication.Models
{
    public class AppointmentStatsDto
    {
        public int Total { get; set; }
        public int TodayCount { get; set; }
        public List<StatusCountDto> ByStatus { get; set; } = new();
    }
}