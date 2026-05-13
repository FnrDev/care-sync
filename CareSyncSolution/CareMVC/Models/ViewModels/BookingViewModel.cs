namespace CareMVC.Models.ViewModels
{
    public class BookingViewModel
    {
        public List<SpecializationItem> Specializations { get; set; } = new();
        public int? PatientProfileId { get; set; }
        public string? PatientName { get; set; }
    }

    public class SpecializationItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }

    public class DoctorItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int ConsultationDurationMin { get; set; }
        public string? Bio { get; set; }
        public List<string> Specializations { get; set; } = new();
    }

    public class TimeSlotItem
    {
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
    }

    public class BookingConfirmationViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
