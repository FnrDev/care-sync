using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int VisitRecordId { get; set; }

    public int DoctorProfileId { get; set; }

    public int PatientProfileId { get; set; }

    public DateTime DateIssued { get; set; }

    public string? Notes { get; set; }

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;

    public virtual PatientProfile PatientProfile { get; set; } = null!;

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual VisitRecord VisitRecord { get; set; } = null!;
}
