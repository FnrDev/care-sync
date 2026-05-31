namespace CareMVC.Models.ViewModels
{
    public class DoctorDashboardViewModel
    {
            public string DoctorFullName { get; set; } = "";
            public string LicenseNumber { get; set; } = "";
            public int DoctorProfileId { get; set; }

            public List<DashboardAppointmentRow> TodaysAppointments { get; set; } = new();
            public List<DashboardPatientRow> MyPatients { get; set; } = new();

            // Quick stats
            public int TotalToday => TodaysAppointments.Count;
            public int Completed => TodaysAppointments.Count(a => a.StatusName == "Completed");
            public int Pending => TodaysAppointments.Count(a => a.StatusName is "Requested" or "Confirmed" or "CheckedIn");
        }

        public class DashboardAppointmentRow
        {
            public int Id { get; set; }
            public string PatientName { get; set; } = "";
            public string PatientCPR { get; set; } = "";
            public int PatientProfileId { get; set; }
            public string Specialization { get; set; } = "";
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string StatusName { get; set; } = "";
            public bool HasVisitRecord { get; set; }
        }

        public class DashboardPatientRow
        {
            public int PatientProfileId { get; set; }
            public string FullName { get; set; } = "";
            public string CPR { get; set; } = "";
            public string PatientRefNumber { get; set; } = "";
            public DateTime? LastVisit { get; set; }
            public int TotalAppointments { get; set; }
        }
}
