namespace CareMVC.Models.ViewModels
{
    public class PatientDashboardViewModel
    {
        public PatientProfileViewModel Profile { get; set; } = new();
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
        public List<AppointmentViewModel> PastAppointments { get; set; } = new();
    }

    public class PatientProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string CPR { get; set; } = "";
        public string PatientRefNumber { get; set; } = "";
        public string DateOfBirth { get; set; } = "";
        public string Gender { get; set; } = "";
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
    }
}
