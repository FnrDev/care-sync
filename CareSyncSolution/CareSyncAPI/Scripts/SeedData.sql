-- ============================================================
-- CareSync Sample Data Seed (SQL Server)
-- ============================================================
-- PREREQUISITE: ASP.NET Identity users must already exist.
-- The C# DbSeeder creates them via UserManager (hashes passwords
-- correctly). Run the API once, then this script becomes a no-op
-- for re-runs (idempotent via NOT EXISTS / WHERE lookups).
--
-- If you need to seed users through SQL, generate password hashes
-- separately with Microsoft.AspNetCore.Identity.PasswordHasher
-- and INSERT into AspNetUsers with those hashes + AspNetUserRoles
-- rows. That is intentionally not covered here.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ------------------------------------------------------------
-- 1. Specializations
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Specialization)
BEGIN
    SET IDENTITY_INSERT Specialization ON;
    INSERT INTO Specialization (Id, [Name], [Description]) VALUES
        (1, 'Cardiology',       'Heart and cardiovascular system'),
        (2, 'Pediatrics',       'Medical care for infants, children, and adolescents'),
        (3, 'General Practice', 'Primary care for all ages'),
        (4, 'Dermatology',      'Skin, hair, and nail conditions'),
        (5, 'Orthopedics',      'Musculoskeletal system');
    SET IDENTITY_INSERT Specialization OFF;
END

-- ------------------------------------------------------------
-- 2. Resolve user IDs (expects users created via DbSeeder)
-- ------------------------------------------------------------
DECLARE @AdminId        NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@caresync.local');
DECLARE @ReceptionId    NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'reception@caresync.local');
DECLARE @DrSmithId      NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'dr.smith@caresync.local');
DECLARE @DrJonesId      NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'dr.jones@caresync.local');
DECLARE @DrAhmedId      NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'dr.ahmed@caresync.local');
DECLARE @Patient1UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'patient1@caresync.local');
DECLARE @Patient2UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'patient2@caresync.local');
DECLARE @Patient3UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'patient3@caresync.local');

IF @DrSmithId IS NULL OR @Patient1UserId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Expected users not found in AspNetUsers. Start the API once to run DbSeeder, then re-run this script.', 16, 1);
    RETURN;
END

-- ------------------------------------------------------------
-- 3. Doctor profiles
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM DoctorProfile WHERE UserId = @DrSmithId)
    INSERT INTO DoctorProfile (UserId, LicenseNumber, ConsultationDurationMin, Bio, IsActive)
    VALUES (@DrSmithId, 'DOC-1001', 30, 'Consultant Cardiologist with 15 years of experience.', 1);

IF NOT EXISTS (SELECT 1 FROM DoctorProfile WHERE UserId = @DrJonesId)
    INSERT INTO DoctorProfile (UserId, LicenseNumber, ConsultationDurationMin, Bio, IsActive)
    VALUES (@DrJonesId, 'DOC-1002', 20, 'Pediatrician focused on early childhood development.', 1);

IF NOT EXISTS (SELECT 1 FROM DoctorProfile WHERE UserId = @DrAhmedId)
    INSERT INTO DoctorProfile (UserId, LicenseNumber, ConsultationDurationMin, Bio, IsActive)
    VALUES (@DrAhmedId, 'DOC-1003', 30, 'General practitioner with cardiology sub-specialization.', 1);

DECLARE @DrSmithPid INT = (SELECT Id FROM DoctorProfile WHERE UserId = @DrSmithId);
DECLARE @DrJonesPid INT = (SELECT Id FROM DoctorProfile WHERE UserId = @DrJonesId);
DECLARE @DrAhmedPid INT = (SELECT Id FROM DoctorProfile WHERE UserId = @DrAhmedId);

-- ------------------------------------------------------------
-- 4. Doctor specializations
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM DoctorSpecialization WHERE DoctorProfileId = @DrSmithPid AND SpecializationId = 1)
    INSERT INTO DoctorSpecialization (DoctorProfileId, SpecializationId) VALUES (@DrSmithPid, 1);

IF NOT EXISTS (SELECT 1 FROM DoctorSpecialization WHERE DoctorProfileId = @DrJonesPid AND SpecializationId = 2)
    INSERT INTO DoctorSpecialization (DoctorProfileId, SpecializationId) VALUES (@DrJonesPid, 2);

IF NOT EXISTS (SELECT 1 FROM DoctorSpecialization WHERE DoctorProfileId = @DrAhmedPid AND SpecializationId = 3)
    INSERT INTO DoctorSpecialization (DoctorProfileId, SpecializationId) VALUES (@DrAhmedPid, 3);

IF NOT EXISTS (SELECT 1 FROM DoctorSpecialization WHERE DoctorProfileId = @DrAhmedPid AND SpecializationId = 1)
    INSERT INTO DoctorSpecialization (DoctorProfileId, SpecializationId) VALUES (@DrAhmedPid, 1);

-- ------------------------------------------------------------
-- 5. Doctor availability (Sun-Thu)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM DoctorAvailability WHERE DoctorProfileId = @DrSmithPid)
BEGIN
    INSERT INTO DoctorAvailability (DoctorProfileId, [DayOfWeek], StartTime, EndTime, IsActive) VALUES
        (@DrSmithPid, 0, '09:00', '17:00', 1),
        (@DrSmithPid, 1, '09:00', '17:00', 1),
        (@DrSmithPid, 2, '09:00', '17:00', 1),
        (@DrSmithPid, 3, '09:00', '17:00', 1),
        (@DrSmithPid, 4, '09:00', '17:00', 1);
END

IF NOT EXISTS (SELECT 1 FROM DoctorAvailability WHERE DoctorProfileId = @DrJonesPid)
BEGIN
    INSERT INTO DoctorAvailability (DoctorProfileId, [DayOfWeek], StartTime, EndTime, IsActive) VALUES
        (@DrJonesPid, 0, '10:00', '16:00', 1),
        (@DrJonesPid, 1, '10:00', '16:00', 1),
        (@DrJonesPid, 2, '10:00', '16:00', 1),
        (@DrJonesPid, 3, '10:00', '16:00', 1),
        (@DrJonesPid, 4, '10:00', '16:00', 1);
END

IF NOT EXISTS (SELECT 1 FROM DoctorAvailability WHERE DoctorProfileId = @DrAhmedPid)
BEGIN
    INSERT INTO DoctorAvailability (DoctorProfileId, [DayOfWeek], StartTime, EndTime, IsActive) VALUES
        (@DrAhmedPid, 0, '08:00', '18:00', 1),
        (@DrAhmedPid, 1, '08:00', '18:00', 1),
        (@DrAhmedPid, 2, '08:00', '18:00', 1),
        (@DrAhmedPid, 3, '08:00', '18:00', 1),
        (@DrAhmedPid, 4, '08:00', '18:00', 1);
END

-- ------------------------------------------------------------
-- 6. Patient profiles
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM PatientProfile WHERE UserId = @Patient1UserId)
    INSERT INTO PatientProfile (UserId, CPR, PatientRefNumber, DateOfBirth, Gender, [Address], BloodType, EmergencyContact, EmergencyPhone)
    VALUES (@Patient1UserId, '880101001001', 'PAT-0001', '1988-01-01', 'Male', 'Manama, Bahrain', 'O+', 'Aisha Ali', '+97333111222');

IF NOT EXISTS (SELECT 1 FROM PatientProfile WHERE UserId = @Patient2UserId)
    INSERT INTO PatientProfile (UserId, CPR, PatientRefNumber, DateOfBirth, Gender, [Address], BloodType, EmergencyContact, EmergencyPhone)
    VALUES (@Patient2UserId, '920315456002', 'PAT-0002', '1992-03-15', 'Female', 'Riffa, Bahrain', 'A-', 'Omar Al-Sayed', '+97333333444');

IF NOT EXISTS (SELECT 1 FROM PatientProfile WHERE UserId = @Patient3UserId)
    INSERT INTO PatientProfile (UserId, CPR, PatientRefNumber, DateOfBirth, Gender, [Address], BloodType, EmergencyContact, EmergencyPhone)
    VALUES (@Patient3UserId, '750722789003', 'PAT-0003', '1975-07-22', 'Male', 'Muharraq, Bahrain', 'B+', 'Emma Wilson', '+97333555666');

DECLARE @P1 INT = (SELECT Id FROM PatientProfile WHERE UserId = @Patient1UserId);
DECLARE @P2 INT = (SELECT Id FROM PatientProfile WHERE UserId = @Patient2UserId);
DECLARE @P3 INT = (SELECT Id FROM PatientProfile WHERE UserId = @Patient3UserId);

-- ------------------------------------------------------------
-- 7. Appointments (various statuses)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Appointment)
BEGIN
    DECLARE @Today DATETIME2 = CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2);

    -- Completed (5 days ago)
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CreatedAt, UpdatedAt)
    VALUES (@P1, @DrSmithPid, 1, @ReceptionId, 'Receptionist',
            DATEADD(DAY, -5, @Today), '10:00', '10:30', 5,
            DATEADD(DAY, -10, @Today), DATEADD(DAY, -5, @Today));
    DECLARE @ApptA INT = SCOPE_IDENTITY();

    -- Completed (3 days ago)
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CreatedAt, UpdatedAt)
    VALUES (@P2, @DrJonesPid, 2, @Patient2UserId, 'Patient',
            DATEADD(DAY, -3, @Today), '11:00', '11:20', 5,
            DATEADD(DAY, -7, @Today), DATEADD(DAY, -3, @Today));
    DECLARE @ApptB INT = SCOPE_IDENTITY();

    -- Confirmed (today)
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CreatedAt, UpdatedAt)
    VALUES (@P3, @DrAhmedPid, 3, @ReceptionId, 'Receptionist',
            @Today, '14:00', '14:30', 2,
            DATEADD(DAY, -2, @Today), DATEADD(DAY, -2, @Today));
    DECLARE @ApptConfirmed INT = SCOPE_IDENTITY();

    -- Requested (future)
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CreatedAt, UpdatedAt)
    VALUES (@P1, @DrAhmedPid, 3, @Patient1UserId, 'Patient',
            DATEADD(DAY, 2, @Today), '09:00', '09:30', 1,
            @Today, @Today);
    DECLARE @ApptRequested INT = SCOPE_IDENTITY();

    -- Cancelled
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CancellationReason, CreatedAt, UpdatedAt)
    VALUES (@P2, @DrSmithPid, 1, @Patient2UserId, 'Patient',
            DATEADD(DAY, -7, @Today), '09:00', '09:30', 6, 'Patient unavailable',
            DATEADD(DAY, -14, @Today), DATEADD(DAY, -8, @Today));
    DECLARE @ApptCancelled INT = SCOPE_IDENTITY();

    -- Checked in (today)
    INSERT INTO Appointment (PatientProfileId, DoctorProfileId, SpecializationId, BookedById, BookedByRole,
                             AppointmentDate, StartTime, EndTime, StatusId, CreatedAt, UpdatedAt)
    VALUES (@P3, @DrSmithPid, 1, @ReceptionId, 'Receptionist',
            @Today, '15:00', '15:30', 3,
            DATEADD(DAY, -1, @Today), @Today);
    DECLARE @ApptCheckedIn INT = SCOPE_IDENTITY();

    -- Status history
    INSERT INTO AppointmentStatusHistory (AppointmentId, PreviousStatusId, NewStatusId, ChangedAt, ChangedById, Notes) VALUES
        (@ApptA,         NULL, 1, DATEADD(DAY, -10, @Today),                   @ReceptionId,    'Booked by reception'),
        (@ApptA,         1,    2, DATEADD(HOUR, 1, DATEADD(DAY, -10, @Today)), @ReceptionId,    'Confirmed'),
        (@ApptA,         2,    5, DATEADD(DAY, -5, @Today),                    @DrSmithId,      'Visit complete'),
        (@ApptB,         NULL, 1, DATEADD(DAY, -7, @Today),                    @Patient2UserId, 'Requested by patient'),
        (@ApptB,         1,    5, DATEADD(DAY, -3, @Today),                    @DrJonesId,      'Visit complete'),
        (@ApptCancelled, 1,    6, DATEADD(DAY, -8, @Today),                    @Patient2UserId, 'Cancelled by patient'),
        (@ApptCheckedIn, 2,    3, @Today,                                      @ReceptionId,    'Checked in at front desk');

    -- Visit records (for completed appointments)
    INSERT INTO VisitRecord (AppointmentId, DoctorProfileId, PatientProfileId, Diagnosis, DoctorNotes, Treatment, CreatedAt)
    VALUES (@ApptA, @DrSmithPid, @P1,
            'Hypertension, stage 1',
            'Patient reports occasional headaches. BP 140/92. Advised lifestyle changes and started on medication.',
            'Low-sodium diet, daily 30-minute walk, follow-up in 4 weeks.',
            DATEADD(DAY, -5, @Today));
    DECLARE @Visit1 INT = SCOPE_IDENTITY();

    INSERT INTO VisitRecord (AppointmentId, DoctorProfileId, PatientProfileId, Diagnosis, DoctorNotes, Treatment, CreatedAt)
    VALUES (@ApptB, @DrJonesPid, @P2,
            'Acute upper respiratory infection',
            'Mild fever, sore throat, runny nose. No signs of bacterial infection.',
            'Rest, fluids, over-the-counter symptom relief. Return if symptoms worsen.',
            DATEADD(DAY, -3, @Today));

    -- Prescription for visit 1
    INSERT INTO Prescription (VisitRecordId, DoctorProfileId, PatientProfileId, DateIssued, Notes)
    VALUES (@Visit1, @DrSmithPid, @P1, DATEADD(DAY, -5, @Today),
            'Monitor blood pressure daily. Stop if dizziness occurs.');
    DECLARE @Rx1 INT = SCOPE_IDENTITY();

    INSERT INTO PrescriptionItem (PrescriptionId, MedicationName, Dosage, Frequency, DurationDays, Instructions) VALUES
        (@Rx1, 'Amlodipine', '5 mg',  'Once daily', 30, 'Take in the morning with water.'),
        (@Rx1, 'Aspirin',    '81 mg', 'Once daily', 30, 'Take with food.');

    -- Notifications
    INSERT INTO Notification (UserId, Title, [Message], [Type], IsRead, RelatedEntityId, RelatedEntityType, CreatedAt) VALUES
        (@Patient1UserId, 'Prescription ready',      'Your prescription from Dr. John Smith is ready.',               'PrescriptionReady',   0, @Rx1,            'Prescription', DATEADD(DAY, -5, @Today)),
        (@Patient3UserId, 'Appointment confirmed',   'Your appointment today at 14:00 with Dr. Ahmed Khalil is confirmed.', 'AppointmentConfirmed', 0, @ApptConfirmed,  'Appointment',  DATEADD(DAY, -2, @Today)),
        (@Patient1UserId, 'Appointment requested',   'Your appointment request is awaiting confirmation.',             'AppointmentRequested',1, @ApptRequested,  'Appointment',  @Today),
        (@DrSmithId,      'New patient checked in',  'James Wilson has checked in for the 15:00 slot.',               'PatientCheckedIn',    0, @ApptCheckedIn,  'Appointment',  GETUTCDATE());
END

COMMIT TRANSACTION;
PRINT 'Seed data applied.';
