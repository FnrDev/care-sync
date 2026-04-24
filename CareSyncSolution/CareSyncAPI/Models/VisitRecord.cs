using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class VisitRecord
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int DoctorProfileId { get; set; }

    public int PatientProfileId { get; set; }

    public string? DoctorNotes { get; set; }

    public string Diagnosis { get; set; } = null!;

    public string? Treatment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;

    public virtual PatientProfile PatientProfile { get; set; } = null!;

    public virtual Prescription? Prescription { get; set; }
}
