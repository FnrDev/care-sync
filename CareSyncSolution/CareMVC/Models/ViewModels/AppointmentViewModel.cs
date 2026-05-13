namespace CareMVC.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Doctor { get; set; } = "";
        public string Patient { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string Status { get; set; } = "";
        public int StatusId { get; set; }
        public string? CancellationReason { get; set; }
    }
}
