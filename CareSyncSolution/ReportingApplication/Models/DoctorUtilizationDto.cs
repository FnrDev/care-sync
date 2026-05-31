namespace ReportingApplication.Models
{
    public class DoctorUtilizationDto
    {
        public string Doctor { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
    }
}