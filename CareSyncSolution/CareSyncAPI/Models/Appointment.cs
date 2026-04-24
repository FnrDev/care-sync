using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int PatientProfileId { get; set; }

    public int DoctorProfileId { get; set; }

    public int SpecializationId { get; set; }

    public string BookedById { get; set; } = null!;

    public string BookedByRole { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int StatusId { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppointmentStatusHistory> AppointmentStatusHistories { get; set; } = new List<AppointmentStatusHistory>();

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;

    public virtual PatientProfile PatientProfile { get; set; } = null!;

    public virtual Specialization Specialization { get; set; } = null!;

    public virtual AppointmentStatus Status { get; set; } = null!;

    public virtual VisitRecord? VisitRecord { get; set; }
}
