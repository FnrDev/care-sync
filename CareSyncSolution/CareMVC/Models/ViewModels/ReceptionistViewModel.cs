namespace CareMVC.Models.ViewModels
{
    public class ReceptionistDashboardViewModel
    {
        public List<AppointmentViewModel> TodayAppointments { get; set; } = new();
        public int RequestedCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CheckedInCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class PatientSearchResult
    {
        public int PatientProfileId { get; set; }
        public string FullName { get; set; } = "";
        public string CPR { get; set; } = "";
        public string PatientRefNumber { get; set; } = "";
    }
}
