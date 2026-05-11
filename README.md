# CareSync
A system for managing clinic appointments, doctor schedules, and resources, helping streamline operations and improve patient booking efficiency.

## API Endpoints

| Route | Method | Auth | Purpose |
|---|---|---|---|
| `/api/auth/login` | POST | None | Login, returns JWT token |
| `/api/appointments/lookup` | GET | None | Public patient lookup by CPR + ref number |
| `/api/appointments` | GET | JWT | Get all appointments |
| `/api/doctors` | GET | JWT | Get all active doctors |
| `/api/doctors/{id}` | GET | JWT | Get doctor by ID with availability |
| `/api/reports/appointment-stats` | GET | JWT + Admin | Appointment counts by status |
| `/api/reports/doctor-utilization` | GET | JWT + Admin | Appointments per doctor |

## Test Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@caresync.local | Admin@123 |
| Doctor | dr.smith@caresync.local | Doctor@123 |
| Patient | patient1@caresync.local | Patient@123 |
| Receptionist | reception@caresync.local | Reception@123 |
