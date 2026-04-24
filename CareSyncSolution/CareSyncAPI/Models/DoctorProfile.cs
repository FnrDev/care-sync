using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class DoctorProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public int ConsultationDurationMin { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();

    public virtual ICollection<DoctorLeave> DoctorLeaves { get; set; } = new List<DoctorLeave>();

    public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<VisitRecord> VisitRecords { get; set; } = new List<VisitRecord>();
}
