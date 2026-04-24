using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class DoctorLeave
{
    public int Id { get; set; }

    public int DoctorProfileId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public bool IsApproved { get; set; }

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;
}
