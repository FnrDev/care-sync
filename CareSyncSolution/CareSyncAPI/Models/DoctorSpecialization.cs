using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class DoctorSpecialization
{
    public int Id { get; set; }

    public int DoctorProfileId { get; set; }

    public int SpecializationId { get; set; }

    public virtual DoctorProfile DoctorProfile { get; set; } = null!;

    public virtual Specialization Specialization { get; set; } = null!;
}
