using System.ComponentModel.DataAnnotations;

namespace CareMVC.Models.ViewModels
{
    public class VisitRecordViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = "";
        public string PatientCPR { get; set; } = "";
        public string Specialization { get; set; } = "";

        [Required(ErrorMessage = "Diagnosis is required")]
        public string Diagnosis { get; set; } = "";

        [Required(ErrorMessage = "Notes are required")]
        public string DoctorNotes { get; set; } = "";

        [Required(ErrorMessage = "Treatment is required")]
        public string Treatment { get; set; } = "";

        public string? PrescriptionNotes { get; set; }

        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }
}