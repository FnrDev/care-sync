using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareSyncAPI.Models;

namespace CareSyncAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
        public virtual DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }
        public virtual DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public virtual DbSet<DoctorLeave> DoctorLeaves { get; set; }
        public virtual DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public virtual DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<PatientProfile> PatientProfiles { get; set; }
        public virtual DbSet<Prescription> Prescriptions { get; set; }
        public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public virtual DbSet<Specialization> Specializations { get; set; }
        public virtual DbSet<VisitRecord> VisitRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment", t =>
                {
                    t.HasCheckConstraint("CK_Appointment_TimeOrder", "[StartTime] < [EndTime]");
                });

                entity.HasIndex(e => e.AppointmentDate, "IX_Appointment_Date");
                entity.HasIndex(e => e.DoctorProfileId, "IX_Appointment_Doctor");
                entity.HasIndex(e => new { e.DoctorProfileId, e.AppointmentDate }, "IX_Appointment_DoctorDate");
                entity.HasIndex(e => e.PatientProfileId, "IX_Appointment_Patient");
                entity.HasIndex(e => e.StatusId, "IX_Appointment_StatusId");

                entity.Property(e => e.BookedById).HasMaxLength(450);
                entity.Property(e => e.BookedByRole).HasMaxLength(50);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.EndTime).HasPrecision(0);
                entity.Property(e => e.StartTime).HasPrecision(0);
                entity.Property(e => e.StatusId).HasDefaultValue(1);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Appointment_Doctor");

                entity.HasOne(d => d.PatientProfile).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.PatientProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Appointment_Patient");

                entity.HasOne(d => d.Specialization).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Appointment_Specialization");

                entity.HasOne(d => d.Status).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Appointment_Status");

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.BookedById)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Appointment_BookedBy");
            });

            modelBuilder.Entity<AppointmentStatus>(entity =>
            {
                entity.ToTable("AppointmentStatus");

                entity.HasIndex(e => e.Name, "UQ_AppointmentStatus_Name").IsUnique();

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<AppointmentStatusHistory>(entity =>
            {
                entity.ToTable("AppointmentStatusHistory");

                entity.HasIndex(e => e.AppointmentId, "IX_StatusHistory_Appointment");

                entity.Property(e => e.ChangedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.ChangedById).HasMaxLength(450);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentStatusHistories)
                    .HasForeignKey(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StatusHistory_Appointment");

                entity.HasOne(d => d.NewStatus).WithMany(p => p.AppointmentStatusHistoryNewStatuses)
                    .HasForeignKey(d => d.NewStatusId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_StatusHistory_NewStatus");

                entity.HasOne(d => d.PreviousStatus).WithMany(p => p.AppointmentStatusHistoryPreviousStatuses)
                    .HasForeignKey(d => d.PreviousStatusId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_StatusHistory_PrevStatus");

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.ChangedById)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_StatusHistory_ChangedBy");
            });

            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.ToTable("DoctorAvailability", t =>
                {
                    t.HasCheckConstraint("CK_DoctorAvail_DayOfWeek", "[DayOfWeek] BETWEEN 0 AND 6");
                    t.HasCheckConstraint("CK_DoctorAvail_TimeOrder", "[StartTime] < [EndTime]");
                });

                entity.Property(e => e.EndTime).HasPrecision(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.StartTime).HasPrecision(0);

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.DoctorAvailabilities)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DoctorAvail_Doctor");
            });

            modelBuilder.Entity<DoctorLeave>(entity =>
            {
                entity.ToTable("DoctorLeave", t =>
                {
                    t.HasCheckConstraint("CK_DoctorLeave_DateOrder", "[EndDate] >= [StartDate]");
                });

                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.Reason).HasMaxLength(500);

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.DoctorLeaves)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DoctorLeave_Doctor");
            });

            modelBuilder.Entity<DoctorProfile>(entity =>
            {
                entity.ToTable("DoctorProfile");

                entity.HasIndex(e => e.LicenseNumber, "UQ_DoctorProfile_License").IsUnique();
                entity.HasIndex(e => e.UserId, "UQ_DoctorProfile_UserId").IsUnique();

                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.ConsultationDurationMin).HasDefaultValue(30);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LicenseNumber).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_DoctorProfile_User");
            });

            modelBuilder.Entity<DoctorSpecialization>(entity =>
            {
                entity.ToTable("DoctorSpecialization");

                entity.HasIndex(e => new { e.DoctorProfileId, e.SpecializationId }, "UQ_DoctorSpec_Combo").IsUnique();

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.DoctorSpecializations)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DoctorSpec_Doctor");

                entity.HasOne(d => d.Specialization).WithMany(p => p.DoctorSpecializations)
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DoctorSpec_Specialization");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.HasIndex(e => e.UserId, "IX_Notification_User");
                entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_Notification_UserUnread").HasFilter("([IsRead]=(0))");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Notification_User");
            });

            modelBuilder.Entity<PatientProfile>(entity =>
            {
                entity.ToTable("PatientProfile");

                entity.HasIndex(e => e.CPR, "UQ_PatientProfile_CPR").IsUnique();
                entity.HasIndex(e => e.PatientRefNumber, "UQ_PatientProfile_RefNumber").IsUnique();
                entity.HasIndex(e => e.UserId, "UQ_PatientProfile_UserId").IsUnique();

                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.BloodType).HasMaxLength(5);
                entity.Property(e => e.CPR).HasMaxLength(20);
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);
                entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.PatientRefNumber).HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_PatientProfile_User");
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.ToTable("Prescription");

                entity.HasIndex(e => e.VisitRecordId, "UQ_Prescription_VisitRecord").IsUnique();

                entity.Property(e => e.DateIssued).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.Prescriptions)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Prescription_Doctor");

                entity.HasOne(d => d.PatientProfile).WithMany(p => p.Prescriptions)
                    .HasForeignKey(d => d.PatientProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Prescription_Patient");

                entity.HasOne(d => d.VisitRecord).WithOne(p => p.Prescription)
                    .HasForeignKey<Prescription>(d => d.VisitRecordId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Prescription_VisitRecord");
            });

            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.ToTable("PrescriptionItem");

                entity.Property(e => e.Dosage).HasMaxLength(100);
                entity.Property(e => e.Frequency).HasMaxLength(100);
                entity.Property(e => e.Instructions).HasMaxLength(500);
                entity.Property(e => e.MedicationName).HasMaxLength(200);

                entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems)
                    .HasForeignKey(d => d.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PrescriptionItem_Prescription");
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.ToTable("Specialization");

                entity.HasIndex(e => e.Name, "UQ_Specialization_Name").IsUnique();

                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<VisitRecord>(entity =>
            {
                entity.ToTable("VisitRecord");

                entity.HasIndex(e => e.AppointmentId, "UQ_VisitRecord_Appointment").IsUnique();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Diagnosis).HasMaxLength(500);
                entity.Property(e => e.DoctorNotes).HasMaxLength(2000);
                entity.Property(e => e.Treatment).HasMaxLength(1000);

                entity.HasOne(d => d.Appointment).WithOne(p => p.VisitRecord)
                    .HasForeignKey<VisitRecord>(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_VisitRecord_Appointment");

                entity.HasOne(d => d.DoctorProfile).WithMany(p => p.VisitRecords)
                    .HasForeignKey(d => d.DoctorProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_VisitRecord_Doctor");

                entity.HasOne(d => d.PatientProfile).WithMany(p => p.VisitRecords)
                    .HasForeignKey(d => d.PatientProfileId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_VisitRecord_Patient");
            });
        }
    }
}
