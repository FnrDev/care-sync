using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class AppointmentStatusHistory
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int? PreviousStatusId { get; set; }

    public int NewStatusId { get; set; }

    public DateTime ChangedAt { get; set; }

    public string ChangedById { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual AppointmentStatus NewStatus { get; set; } = null!;

    public virtual AppointmentStatus? PreviousStatus { get; set; }
}
