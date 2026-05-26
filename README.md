# CareSync
A system for managing clinic appointments, doctor schedules, and resources, helping streamline operations and improve patient booking efficiency.

## Database Setup

The API targets SQL Server LocalDB (`(localdb)\mssqllocaldb`, database `CareSyncDb`).

1. **Apply migrations** — from the solution root:
   ```
   dotnet ef database update --project CareSyncSolution/CareSyncAPI
   ```
   (equivalent to `Update-Database` in the Visual Studio Package Manager Console).

2. **Seed data** — seeding runs automatically on API startup. `Program.cs` calls
   `DbSeeder.SeedAsync` (`CareSyncAPI/Data/DbSeeder.cs`), so just run the API once:
   ```
   dotnet run --project CareSyncSolution/CareSyncAPI
   ```
   The seeder is idempotent — every section checks for existing rows first, so
   restarts are safe. It creates roles, appointment statuses, specializations,
   users, doctor/patient profiles, doctor availability, and sample appointments.

> `DbSeeder` (run via `Program.cs`) is the source of truth for seed data.
> `Scripts/SeedData.sql` is a standalone SQL alternative covering the same data.

## API Endpoints

| Route | Method | Auth | Purpose |
|---|---|---|---|
| `/api/auth/login` | POST | None | Login, returns JWT token |
| `/api/appointments/lookup` | GET | None | Public patient lookup by CPR (+ optional ref number) |
| `/api/appointments` | GET | JWT | Get all appointments |
| `/api/appointments/my` | GET | JWT + Patient | Get current patient's appointments |
| `/api/appointments/today` | GET | JWT + Receptionist/Manager | Today's appointments queue |
| `/api/appointments/available-slots` | GET | JWT | Available time slots for a doctor on a date |
| `/api/appointments` | POST | JWT + Patient/Receptionist | Create a new appointment |
| `/api/appointments/{id}/status` | PUT | JWT | Update appointment status (confirm, check-in, cancel) |
| `/api/doctors` | GET | JWT | Get all active doctors |
| `/api/doctors/{id}` | GET | JWT | Get doctor by ID with availability |
| `/api/doctors/by-specialization/{specId}` | GET | JWT | Get doctors filtered by specialization |
| `/api/specializations` | GET | JWT | List all specializations |
| `/api/patients/me` | GET | JWT + Patient | Get current patient's profile |
| `/api/patients/me/medical-records` | GET | JWT + Patient | Get patient's visit records and prescriptions |
| `/api/patients/search` | GET | JWT + Receptionist | Search patient by CPR |
| `/api/reports/appointment-stats` | GET | JWT + Manager | Appointment counts by status |
| `/api/reports/doctor-utilization` | GET | JWT + Manager | Appointments per doctor |

## Test Credentials

| Role | Email | Password |
|---|---|---|
| Manager | manager@caresync.local | manager@123 |
| Receptionist | reception@caresync.local | Reception@123 |
| Doctor | dr.smith@caresync.local | Doctor@123 |
| Doctor | dr.jones@caresync.local | Doctor@123 |
| Doctor | dr.ahmed@caresync.local | Doctor@123 |
| Doctor | dr.lee@caresync.local | Doctor@123 |
| Doctor | dr.khan@caresync.local | Doctor@123 |
| Patient | patient1@caresync.local | Patient@123 |
| Patient | patient2@caresync.local | Patient@123 |
| Patient | patient3@caresync.local | Patient@123 |

## Seeded Patients & Appointments

Patient CPRs (for the public tracking / lookup page):

| Patient | CPR | Ref | Login |
|---|---|---|---|
| Mohammed Ali | 880101234 | PAT-0001 | patient1@caresync.local |
| Fatima Al-Sayed | 920315567 | PAT-0002 | patient2@caresync.local |
| James Wilson | 750722891 | PAT-0003 | patient3@caresync.local |

Sample appointments created by the seeder (only when the `Appointment` table is
empty; dates are relative to the day the seeder first runs):

| Patient | Doctor | Specialization | When | Time | Status |
|---|---|---|---|---|---|
| Mohammed Ali | Dr. John Smith | Cardiology | 5 days ago | 10:00 | Completed |
| Mohammed Ali | Dr. Ahmed Khalil | General Practice | in 2 days | 09:00 | Requested |
| Fatima Al-Sayed | Dr. Sarah Jones | Pediatrics | 3 days ago | 11:00 | Completed |
| Fatima Al-Sayed | Dr. John Smith | Cardiology | 7 days ago | 09:00 | Cancelled |
| James Wilson | Dr. Ahmed Khalil | General Practice | today | 14:00 | Confirmed |
| James Wilson | Dr. John Smith | Cardiology | today | 15:00 | CheckedIn |
