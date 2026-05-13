namespace CareMVC.Models.ViewModels
{
    public class MedicalRecordViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; } = "";
        public string Diagnosis { get; set; } = "";
        public string? DoctorNotes { get; set; }
        public string? Treatment { get; set; }
        public DateTime CreatedAt { get; set; }
        public PrescriptionViewModel? Prescription { get; set; }
    }

    public class PrescriptionViewModel
    {
        public int Id { get; set; }
        public DateTime DateIssued { get; set; }
        public string? Notes { get; set; }
        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }

    public class PrescriptionItemViewModel
    {
        public string MedicationName { get; set; } = "";
        public string Dosage { get; set; } = "";
        public string Frequency { get; set; } = "";
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
    }
}
