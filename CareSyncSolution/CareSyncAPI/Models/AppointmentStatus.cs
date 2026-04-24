using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class AppointmentStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AppointmentStatusHistory> AppointmentStatusHistoryNewStatuses { get; set; } = new List<AppointmentStatusHistory>();

    public virtual ICollection<AppointmentStatusHistory> AppointmentStatusHistoryPreviousStatuses { get; set; } = new List<AppointmentStatusHistory>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
