using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class PatientProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string CPR { get; set; } = null!;

    public string PatientRefNumber { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<VisitRecord> VisitRecords { get; set; } = new List<VisitRecord>();
}
