# 💊 SmartPharmacy API

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![Status](https://img.shields.io/badge/status-in%20development-yellow)
![License](https://img.shields.io/badge/license-TBD-lightgrey)

A backend API for an online pharmacy platform that connects **patients** 🧑‍⚕️ with **pharmacies** 🏥, built with ASP.NET Core. It combines e-commerce (browse, cart, checkout) with pharmacy-specific workflows such as prescription review and smart inventory alerts.

## ✨ Key Features

- 🔐 **Authentication & Authorization** — JWT-based auth with ASP.NET Core Identity and role-based access control (`Admin`, `Pharmacist`, `Patient`).
- 💊 **Medicine Management** — CRUD for medicines/products with multi-language support (name, description) and category management.
- 📦 **Smart Inventory Management** — tracks stock quantities, minimum stock thresholds, and expiry dates.
- 📄 **Prescription Management** — patients upload prescriptions for medicines that require one; pharmacists review and approve/reject before the order can be paid.
- 🛒 **Online Ordering** — cart, checkout, order tracking, and order history.
- 💳 **Payment Integration** — Stripe Checkout, with the payment confirmed against Stripe itself (redirect + signed webhook), never on the caller's word.
- 🔔 **Notifications** — in-app notifications for patients (order/prescription status) and pharmacists (low stock, expiring medicine).
- ⏱️ **Background Jobs** — Hangfire runs a daily inventory check (in-app alerts plus a single digest email per pharmacist) and an hourly sweep that cancels abandoned orders and releases their reserved stock.

## 🏗️ Architecture

The solution follows a layered (N-tier) architecture:

```
SmartPharmacy.sln
├── SmartPharmacy          (SmartPharmacy.PL)   → API layer: Controllers, Program.cs, appsettings
├── SmartPharmacy.PLL                            → Business logic: Services, DTOs mapping (Mapster)
└── SmartPharmacy.DAL                            → Data access: EF Core DbContext, Models, Repositories, Migrations
```

- **DAL** — `ApplicationDbContext` (EF Core + Identity), entity models (`Product`, `Order`, `OrderItem`, `CartItem`, `Category`, `Prescription`, `Notification`, ...), generic repository pattern, and migrations.
- **PLL** — application services (e.g. `AuthenticationService`, `EmailSender`) and DTOs for requests/responses.
- **PL** — the Web API project: controllers, dependency injection setup, and configuration.

## 🧰 Tech Stack

- ⚙️ **.NET 9 / ASP.NET Core Web API**
- 🗄️ **Entity Framework Core 9** (SQL Server)
- 👤 **ASP.NET Core Identity** for user management
- 🔑 **JWT Bearer Authentication**
- 🔄 **Mapster** for object-to-object mapping
- ⏱️ **Hangfire** *(planned)* for scheduled/background jobs

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or a full instance)

### ⚙️ Setup

1. Clone the repository:
   ```bash
   git clone <repo-url>
   cd SmartPharmacy
   ```

2. Configure secrets (connection string, JWT settings, email credentials) using .NET user-secrets — these are **not** committed to source control:
   ```bash
   cd SmartPharmacy
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=SmartPharmacyDb;Trusted_Connection=True;TrustServerCertificate=True"
   dotnet user-secrets set "Jwt:Key" "<your-secret-key>"
   dotnet user-secrets set "Jwt:Issuer" "SmartPharmacy"
   dotnet user-secrets set "Jwt:Audience" "SmartPharmacyUsers"
   dotnet user-secrets set "EmailSettings:Username" "<your-email>"
   dotnet user-secrets set "EmailSettings:Password" "<your-app-password>"
   dotnet user-secrets set "StripeSettings:SecretKey" "<sk_test_...>"
   dotnet user-secrets set "StripeSettings:WebhookSecret" "<whsec_...>"
   ```

   Every account registered through the API starts as a `Patient`, so the first `Admin` is
   seeded from configuration on startup (skipped once an admin exists):
   ```bash
   dotnet user-secrets set "AdminUser:Email" "<admin-email>"
   dotnet user-secrets set "AdminUser:Password" "<admin-password>"
   ```

3. Apply EF Core migrations:
   ```bash
   dotnet ef database update --project SmartPharmacy.DAL --startup-project SmartPharmacy
   ```

4. Run the API:
   ```bash
   dotnet run --project SmartPharmacy
   ```

## 👥 Roles

| Role | Capabilities |
|---|---|
| 🛡️ **Admin** | Manage users, roles, and view system-wide reports |
| 💊 **Pharmacist** | Manage medicines/inventory, review prescriptions, manage orders |
| 🧑 **Patient** | Browse medicines, upload prescriptions, place orders, track status |

## 🗺️ Roadmap

- [x] ⏱️ Hangfire integration for recurring stock/expiry checks and background notification delivery
- [x] 💳 Stripe payment integration
- [x] 🔒 Stock reserved on order creation inside a transaction, released when an order is cancelled or expires
- [ ] 📊 Reports & dashboard endpoints
- [ ] ✅ Request validation (FluentValidation) and a global exception handler

## 📄 License

TBD

---

<p align="center">Made with ❤️ for SmartPharmacy</p>
