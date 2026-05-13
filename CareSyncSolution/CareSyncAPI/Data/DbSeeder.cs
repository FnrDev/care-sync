using CareSyncAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareSyncAPI.Data
{
    public static class DbSeeder
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Doctor = "Doctor";
            public const string Patient = "Patient";
            public const string Receptionist = "Receptionist";
        }

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRolesAsync(roleManager);
            await SeedAppointmentStatusesAsync(db);
            await SeedSpecializationsAsync(db);

            var specMap = await db.Specializations.ToDictionaryAsync(s => s.Name, s => s.Id);
            var cardiologyId = specMap["Cardiology"];
            var pediatricsId = specMap["Pediatrics"];
            var generalPracticeId = specMap["General Practice"];
            var dermatologyId = specMap["Dermatology"];
            var orthopedicsId = specMap["Orthopedics"];

            var admin = await EnsureUserAsync(userManager, "admin@caresync.local", "Admin@123", "System Admin", Roles.Admin);
            var reception = await EnsureUserAsync(userManager, "reception@caresync.local", "Reception@123", "Layla Mahmoud", Roles.Receptionist);

            var drSmith = await EnsureUserAsync(userManager, "dr.smith@caresync.local", "Doctor@123", "Dr. John Smith", Roles.Doctor);
            var drJones = await EnsureUserAsync(userManager, "dr.jones@caresync.local", "Doctor@123", "Dr. Sarah Jones", Roles.Doctor);
            var drAhmed = await EnsureUserAsync(userManager, "dr.ahmed@caresync.local", "Doctor@123", "Dr. Ahmed Khalil", Roles.Doctor);
            var drLee = await EnsureUserAsync(userManager, "dr.lee@caresync.local", "Doctor@123", "Dr. Emily Lee", Roles.Doctor);
            var drKhan = await EnsureUserAsync(userManager, "dr.khan@caresync.local", "Doctor@123", "Dr. Omar Khan", Roles.Doctor);

            var p1User = await EnsureUserAsync(userManager, "patient1@caresync.local", "Patient@123", "Mohammed Ali", Roles.Patient);
            var p2User = await EnsureUserAsync(userManager, "patient2@caresync.local", "Patient@123", "Fatima Al-Sayed", Roles.Patient);
            var p3User = await EnsureUserAsync(userManager, "patient3@caresync.local", "Patient@123", "James Wilson", Roles.Patient);

            var drSmithProfile = await EnsureDoctorProfileAsync(db, drSmith.Id, "DOC-1001", 30, "Consultant Cardiologist with 15 years of experience.", specializationIds: new[] { cardiologyId });
            var drJonesProfile = await EnsureDoctorProfileAsync(db, drJones.Id, "DOC-1002", 20, "Pediatrician focused on early childhood development.", specializationIds: new[] { pediatricsId });
            var drAhmedProfile = await EnsureDoctorProfileAsync(db, drAhmed.Id, "DOC-1003", 30, "General practitioner with cardiology sub-specialization.", specializationIds: new[] { generalPracticeId, cardiologyId });
            var drLeeProfile = await EnsureDoctorProfileAsync(db, drLee.Id, "DOC-1004", 25, "Dermatologist specializing in skin conditions and cosmetic procedures.", specializationIds: new[] { dermatologyId });
            var drKhanProfile = await EnsureDoctorProfileAsync(db, drKhan.Id, "DOC-1005", 30, "Orthopedic surgeon with expertise in sports injuries.", specializationIds: new[] { orthopedicsId });

            await EnsureAvailabilityAsync(db, drSmithProfile.Id, new TimeOnly(9, 0), new TimeOnly(17, 0));
            await EnsureAvailabilityAsync(db, drJonesProfile.Id, new TimeOnly(10, 0), new TimeOnly(16, 0));
            await EnsureAvailabilityAsync(db, drAhmedProfile.Id, new TimeOnly(8, 0), new TimeOnly(18, 0));
            await EnsureAvailabilityAsync(db, drLeeProfile.Id, new TimeOnly(9, 0), new TimeOnly(15, 0));
            await EnsureAvailabilityAsync(db, drKhanProfile.Id, new TimeOnly(8, 0), new TimeOnly(16, 0));

            var patient1 = await EnsurePatientProfileAsync(db, p1User.Id, "880101234", "PAT-0001", new DateOnly(1988, 1, 1), "Male", "Manama, Bahrain", "O+", "Aisha Ali", "+97333111222");
            var patient2 = await EnsurePatientProfileAsync(db, p2User.Id, "920315567", "PAT-0002", new DateOnly(1992, 3, 15), "Female", "Riffa, Bahrain", "A-", "Omar Al-Sayed", "+97333333444");
            var patient3 = await EnsurePatientProfileAsync(db, p3User.Id, "750722891", "PAT-0003", new DateOnly(1975, 7, 22), "Male", "Muharraq, Bahrain", "B+", "Emma Wilson", "+97333555666");

            if (!await db.Appointments.AnyAsync())
            {
                var today = DateTime.UtcNow.Date;

                var completedA = new Appointment
                {
                    PatientProfileId = patient1.Id,
                    DoctorProfileId = drSmithProfile.Id,
                    SpecializationId = cardiologyId,
                    BookedById = reception.Id,
                    BookedByRole = Roles.Receptionist,
                    AppointmentDate = today.AddDays(-5),
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(10, 30),
                    StatusId = 5,
                    CreatedAt = today.AddDays(-10),
                    UpdatedAt = today.AddDays(-5)
                };

                var completedB = new Appointment
                {
                    PatientProfileId = patient2.Id,
                    DoctorProfileId = drJonesProfile.Id,
                    SpecializationId = pediatricsId,
                    BookedById = p2User.Id,
                    BookedByRole = Roles.Patient,
                    AppointmentDate = today.AddDays(-3),
                    StartTime = new TimeOnly(11, 0),
                    EndTime = new TimeOnly(11, 20),
                    StatusId = 5,
                    CreatedAt = today.AddDays(-7),
                    UpdatedAt = today.AddDays(-3)
                };

                var confirmedToday = new Appointment
                {
                    PatientProfileId = patient3.Id,
                    DoctorProfileId = drAhmedProfile.Id,
                    SpecializationId = generalPracticeId,
                    BookedById = reception.Id,
                    BookedByRole = Roles.Receptionist,
                    AppointmentDate = today,
                    StartTime = new TimeOnly(14, 0),
                    EndTime = new TimeOnly(14, 30),
                    StatusId = 2,
                    CreatedAt = today.AddDays(-2),
                    UpdatedAt = today.AddDays(-2)
                };

                var requestedFuture = new Appointment
                {
                    PatientProfileId = patient1.Id,
                    DoctorProfileId = drAhmedProfile.Id,
                    SpecializationId = generalPracticeId,
                    BookedById = p1User.Id,
                    BookedByRole = Roles.Patient,
                    AppointmentDate = today.AddDays(2),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(9, 30),
                    StatusId = 1,
                    CreatedAt = today,
                    UpdatedAt = today
                };

                var cancelled = new Appointment
                {
                    PatientProfileId = patient2.Id,
                    DoctorProfileId = drSmithProfile.Id,
                    SpecializationId = cardiologyId,
                    BookedById = p2User.Id,
                    BookedByRole = Roles.Patient,
                    AppointmentDate = today.AddDays(-7),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(9, 30),
                    StatusId = 6,
                    CancellationReason = "Patient unavailable",
                    CreatedAt = today.AddDays(-14),
                    UpdatedAt = today.AddDays(-8)
                };

                var checkedIn = new Appointment
                {
                    PatientProfileId = patient3.Id,
                    DoctorProfileId = drSmithProfile.Id,
                    SpecializationId = cardiologyId,
                    BookedById = reception.Id,
                    BookedByRole = Roles.Receptionist,
                    AppointmentDate = today,
                    StartTime = new TimeOnly(15, 0),
                    EndTime = new TimeOnly(15, 30),
                    StatusId = 3,
                    CreatedAt = today.AddDays(-1),
                    UpdatedAt = today
                };

                db.Appointments.AddRange(completedA, completedB, confirmedToday, requestedFuture, cancelled, checkedIn);
                await db.SaveChangesAsync();

                db.AppointmentStatusHistories.AddRange(
                    new AppointmentStatusHistory { AppointmentId = completedA.Id, PreviousStatusId = null, NewStatusId = 1, ChangedAt = completedA.CreatedAt, ChangedById = reception.Id, Notes = "Booked by reception" },
                    new AppointmentStatusHistory { AppointmentId = completedA.Id, PreviousStatusId = 1, NewStatusId = 2, ChangedAt = completedA.CreatedAt.AddHours(1), ChangedById = reception.Id, Notes = "Confirmed" },
                    new AppointmentStatusHistory { AppointmentId = completedA.Id, PreviousStatusId = 2, NewStatusId = 5, ChangedAt = completedA.UpdatedAt, ChangedById = drSmith.Id, Notes = "Visit complete" },
                    new AppointmentStatusHistory { AppointmentId = completedB.Id, PreviousStatusId = null, NewStatusId = 1, ChangedAt = completedB.CreatedAt, ChangedById = p2User.Id, Notes = "Requested by patient" },
                    new AppointmentStatusHistory { AppointmentId = completedB.Id, PreviousStatusId = 1, NewStatusId = 5, ChangedAt = completedB.UpdatedAt, ChangedById = drJones.Id, Notes = "Visit complete" },
                    new AppointmentStatusHistory { AppointmentId = cancelled.Id, PreviousStatusId = 1, NewStatusId = 6, ChangedAt = cancelled.UpdatedAt, ChangedById = p2User.Id, Notes = "Cancelled by patient" },
                    new AppointmentStatusHistory { AppointmentId = checkedIn.Id, PreviousStatusId = 2, NewStatusId = 3, ChangedAt = today, ChangedById = reception.Id, Notes = "Checked in at front desk" }
                );
                await db.SaveChangesAsync();

                var visit1 = new VisitRecord
                {
                    AppointmentId = completedA.Id,
                    DoctorProfileId = drSmithProfile.Id,
                    PatientProfileId = patient1.Id,
                    Diagnosis = "Hypertension, stage 1",
                    DoctorNotes = "Patient reports occasional headaches. BP 140/92. Advised lifestyle changes and started on medication.",
                    Treatment = "Low-sodium diet, daily 30-minute walk, follow-up in 4 weeks.",
                    CreatedAt = completedA.UpdatedAt
                };

                var visit2 = new VisitRecord
                {
                    AppointmentId = completedB.Id,
                    DoctorProfileId = drJonesProfile.Id,
                    PatientProfileId = patient2.Id,
                    Diagnosis = "Acute upper respiratory infection",
                    DoctorNotes = "Mild fever, sore throat, runny nose. No signs of bacterial infection.",
                    Treatment = "Rest, fluids, over-the-counter symptom relief. Return if symptoms worsen.",
                    CreatedAt = completedB.UpdatedAt
                };

                db.VisitRecords.AddRange(visit1, visit2);
                await db.SaveChangesAsync();

                var prescription1 = new Prescription
                {
                    VisitRecordId = visit1.Id,
                    DoctorProfileId = drSmithProfile.Id,
                    PatientProfileId = patient1.Id,
                    DateIssued = completedA.UpdatedAt,
                    Notes = "Monitor blood pressure daily. Stop if dizziness occurs."
                };
                db.Prescriptions.Add(prescription1);
                await db.SaveChangesAsync();

                db.PrescriptionItems.AddRange(
                    new PrescriptionItem { PrescriptionId = prescription1.Id, MedicationName = "Amlodipine", Dosage = "5 mg", Frequency = "Once daily", DurationDays = 30, Instructions = "Take in the morning with water." },
                    new PrescriptionItem { PrescriptionId = prescription1.Id, MedicationName = "Aspirin", Dosage = "81 mg", Frequency = "Once daily", DurationDays = 30, Instructions = "Take with food." }
                );
                await db.SaveChangesAsync();

                db.Notifications.AddRange(
                    new Notification { UserId = p1User.Id, Title = "Prescription ready", Message = "Your prescription from Dr. John Smith is ready.", Type = "PrescriptionReady", RelatedEntityId = prescription1.Id, RelatedEntityType = nameof(Prescription), CreatedAt = completedA.UpdatedAt },
                    new Notification { UserId = p3User.Id, Title = "Appointment confirmed", Message = "Your appointment today at 14:00 with Dr. Ahmed Khalil is confirmed.", Type = "AppointmentConfirmed", RelatedEntityId = confirmedToday.Id, RelatedEntityType = nameof(Appointment), CreatedAt = confirmedToday.UpdatedAt },
                    new Notification { UserId = p1User.Id, Title = "Appointment requested", Message = "Your appointment request is awaiting confirmation.", Type = "AppointmentRequested", RelatedEntityId = requestedFuture.Id, RelatedEntityType = nameof(Appointment), IsRead = true, CreatedAt = requestedFuture.CreatedAt },
                    new Notification { UserId = drSmith.Id, Title = "New patient checked in", Message = "James Wilson has checked in for the 15:00 slot.", Type = "PatientCheckedIn", RelatedEntityId = checkedIn.Id, RelatedEntityType = nameof(Appointment), CreatedAt = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { Roles.Admin, Roles.Doctor, Roles.Patient, Roles.Receptionist })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private static async Task SeedAppointmentStatusesAsync(ApplicationDbContext db)
        {
            if (await db.AppointmentStatuses.AnyAsync()) return;

            await db.Database.ExecuteSqlRawAsync(@"
                SET IDENTITY_INSERT [AppointmentStatus] ON;
                INSERT INTO [AppointmentStatus] (Id, Name) VALUES (1, 'Requested');
                INSERT INTO [AppointmentStatus] (Id, Name) VALUES (2, 'Confirmed');
                INSERT INTO [AppointmentStatus] (Id, Name) VALUES (3, 'CheckedIn');
                INSERT INTO [AppointmentStatus] (Id, Name) VALUES (5, 'Completed');
                INSERT INTO [AppointmentStatus] (Id, Name) VALUES (6, 'Cancelled');
                SET IDENTITY_INSERT [AppointmentStatus] OFF;
            ");
        }

        private static async Task SeedSpecializationsAsync(ApplicationDbContext db)
        {
            if (await db.Specializations.AnyAsync()) return;

            db.Specializations.AddRange(
                new Specialization { Name = "Cardiology", Description = "Heart and cardiovascular system" },
                new Specialization { Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents" },
                new Specialization { Name = "General Practice", Description = "Primary care for all ages" },
                new Specialization { Name = "Dermatology", Description = "Skin, hair, and nail conditions" },
                new Specialization { Name = "Orthopedics", Description = "Musculoskeletal system" }
            );
            await db.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                    await userManager.AddToRoleAsync(existing, role);
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        private static async Task<DoctorProfile> EnsureDoctorProfileAsync(
            ApplicationDbContext db,
            string userId,
            string licenseNumber,
            int consultationDurationMin,
            string bio,
            int[] specializationIds)
        {
            var profile = await db.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (profile is null)
            {
                profile = new DoctorProfile
                {
                    UserId = userId,
                    LicenseNumber = licenseNumber,
                    ConsultationDurationMin = consultationDurationMin,
                    Bio = bio,
                    IsActive = true
                };
                db.DoctorProfiles.Add(profile);
                await db.SaveChangesAsync();
            }

            foreach (var specId in specializationIds)
            {
                var exists = await db.DoctorSpecializations
                    .AnyAsync(ds => ds.DoctorProfileId == profile.Id && ds.SpecializationId == specId);
                if (!exists)
                    db.DoctorSpecializations.Add(new DoctorSpecialization { DoctorProfileId = profile.Id, SpecializationId = specId });
            }
            await db.SaveChangesAsync();

            return profile;
        }

        private static async Task EnsureAvailabilityAsync(ApplicationDbContext db, int doctorProfileId, TimeOnly start, TimeOnly end)
        {
            var hasAny = await db.DoctorAvailabilities.AnyAsync(a => a.DoctorProfileId == doctorProfileId);
            if (hasAny) return;

            for (int day = 0; day <= 4; day++)
            {
                db.DoctorAvailabilities.Add(new DoctorAvailability
                {
                    DoctorProfileId = doctorProfileId,
                    DayOfWeek = day,
                    StartTime = start,
                    EndTime = end,
                    IsActive = true
                });
            }
            await db.SaveChangesAsync();
        }

        private static async Task<PatientProfile> EnsurePatientProfileAsync(
            ApplicationDbContext db,
            string userId,
            string cpr,
            string refNumber,
            DateOnly dateOfBirth,
            string gender,
            string? address,
            string? bloodType,
            string? emergencyContact,
            string? emergencyPhone)
        {
            var profile = await db.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile is not null) return profile;

            profile = new PatientProfile
            {
                UserId = userId,
                CPR = cpr,
                PatientRefNumber = refNumber,
                DateOfBirth = dateOfBirth,
                Gender = gender,
                Address = address,
                BloodType = bloodType,
                EmergencyContact = emergencyContact,
                EmergencyPhone = emergencyPhone
            };
            db.PatientProfiles.Add(profile);
            await db.SaveChangesAsync();
            return profile;
        }
    }
}
