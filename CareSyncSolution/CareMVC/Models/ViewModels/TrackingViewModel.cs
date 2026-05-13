namespace CareMVC.Models.ViewModels
{
    public class TrackingViewModel
    {
        public string CPR { get; set; } = "";
        public string RefNumber { get; set; } = "";
        public TrackingResultViewModel? Result { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TrackingResultViewModel
    {
        public TrackingPatient Patient { get; set; } = new();
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
        public List<TrackingVisit> RecentVisits { get; set; } = new();
    }

    public class TrackingPatient
    {
        public int Id { get; set; }
        public string CPR { get; set; } = "";
        public string PatientRefNumber { get; set; } = "";
    }

    public class TrackingVisit
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Doctor { get; set; } = "";
        public string Diagnosis { get; set; } = "";
        public string? DoctorNotes { get; set; }
    }
}
