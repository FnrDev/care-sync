# CareSync
A system for managing clinic appointments, doctor schedules, and resources, helping streamline operations and improve patient booking efficiency.

## API Endpoints

| Route | Method | Auth | Purpose |
|---|---|---|---|
| `/api/auth/login` | POST | None | Login, returns JWT token |
| `/api/appointments/lookup` | GET | None | Public patient lookup by CPR (+ optional ref number) |
| `/api/appointments` | GET | JWT | Get all appointments |
| `/api/appointments/my` | GET | JWT + Patient | Get current patient's appointments |
| `/api/appointments/today` | GET | JWT + Receptionist/Admin | Today's appointments queue |
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
| `/api/reports/appointment-stats` | GET | JWT + Admin | Appointment counts by status |
| `/api/reports/doctor-utilization` | GET | JWT + Admin | Appointments per doctor |

## Test Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@caresync.local | Admin@123 |
| Receptionist | reception@caresync.local | Reception@123 |
| Doctor | dr.smith@caresync.local | Doctor@123 |
| Doctor | dr.jones@caresync.local | Doctor@123 |
| Doctor | dr.ahmed@caresync.local | Doctor@123 |
| Doctor | dr.lee@caresync.local | Doctor@123 |
| Doctor | dr.khan@caresync.local | Doctor@123 |
| Patient | patient1@caresync.local | Patient@123 |
| Patient | patient2@caresync.local | Patient@123 |
| Patient | patient3@caresync.local | Patient@123 |

## Seeded Patient CPRs (for tracking page)

| Patient | CPR | Ref |
|---|---|---|
| Mohammed Ali | 880101234 | PAT-0001 |
| Fatima Al-Sayed | 920315567 | PAT-0002 |
| James Wilson | 750722891 | PAT-0003 |
