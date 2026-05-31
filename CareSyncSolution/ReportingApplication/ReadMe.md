# CareSync Reporting Application

## Overview

CareSync Reporting Application is a separate ASP.NET Core MVC reporting system developed for the Healthcare Clinic Appointment & Resource System project.

The reporting application provides the Clinic Manager with operational insights and analytics related to appointments, doctor workload, and clinic performance. The application consumes data exclusively through the CareSync Web API using HttpClient and JWT authentication.

This project follows the requirements specified in the IT8118 Advanced Programming brief.

---

# Features

## Authentication

* Secure JWT-based login
* Session-based token storage
* Protected dashboard access
* Logout functionality

## Reporting Dashboard

The dashboard provides:

### Appointment Statistics

* Total appointments
* Today's appointments
* Confirmed appointments
* Checked-in appointments
* Completed appointments
* Cancelled appointments
* Requested appointments
* Missed appointments

### Doctor Utilization Report

* Total appointments per doctor
* Workload level indicators
* Doctor utilization analytics

### Manager Insights

* Completion rate
* Cancellation rate
* Missed appointment rate
* Busiest doctor identification

### Charts & Analytics

* Appointment status distribution chart
* Doctor utilization bar chart

### Additional Features

* Download report functionality
* Responsive dashboard UI
* Sidebar navigation
* Recent clinic activity section

---

# Technologies Used

* ASP.NET Core MVC
* Bootstrap 5
* Chart.js
* HttpClient
* JWT Authentication

---

# API Integration

The reporting application communicates with the CareSync Web API using HttpClient.

### Main API Endpoints Used

| Endpoint                          | Purpose                                    |
| --------------------------------- | ------------------------------------------ |
| `/api/auth/login`                 | Authenticate manager and receive JWT token |
| `/api/reports/appointment-stats`  | Retrieve appointment statistics            |
| `/api/reports/doctor-utilization` | Retrieve doctor workload data              |

---

# Architecture

* Separate MVC reporting application
* No direct database access
* No DbContext usage
* All data retrieved through API calls
* JWT-secured communication

---

# Login Credentials

## Manager Account

Email:

```text
manager@caresync.local
```

Password:

```text
Manager@123
```

---

# Running the Application

## 1. Start the API

Run the CareSync API project first:

```bash
dotnet run --project CareSyncSolution/CareSyncAPI
```

## 2. Run the Reporting Application

Run the ReportingApplication project.

## 3. Login

Use the manager credentials to access the reporting dashboard.

---

# Dashboard Modules

## Dashboard Overview

Provides high-level operational statistics and KPIs for the clinic manager.

## Appointment Analytics

Displays appointment distribution and lifecycle insights.

## Doctor Utilization

Displays appointment workload per doctor.

## Cancellation & Missed Analysis

Provides operational indicators regarding missed and cancelled appointments.

---

# UI Features

* Responsive design
* Modern dashboard layout
* Bootstrap cards and tables
* Interactive charts
* Sidebar navigation
* Downloadable reports

---

# Security

* JWT-secured API access
* Protected reporting routes
* Session token management
* Restricted manager-only access

---

# Project Purpose

This reporting application was developed as part of the IT8118 Advanced Programming group project to simulate a real-world operational reporting tool for healthcare clinic management.
