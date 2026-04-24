using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class DoctorAvailability
{
    public int Id { get; set; }

    public int DoctorProfileId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; }

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;
}
