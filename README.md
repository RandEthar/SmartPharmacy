# 💊 SmartPharmacy API

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![Status](https://img.shields.io/badge/status-in%20development-yellow)
![License](https://img.shields.io/badge/license-TBD-lightgrey)

A backend API for an online pharmacy platform that connects **patients** 🧑‍⚕️ with **pharmacies** 🏥, built with ASP.NET Core. It combines e-commerce (browse, cart, checkout) with pharmacy-specific workflows such as prescription review and smart inventory alerts.

## 🌐 Live Demo

The API is deployed and running — no setup required to try it.

| | |
|---|---|
| 📖 **API reference (Scalar)** | **https://smartpharmacy-rand.runasp.net/scalar/v1** |
| 🔗 Base URL | `https://smartpharmacy-rand.runasp.net` |
| 📄 OpenAPI document | `https://smartpharmacy-rand.runasp.net/openapi/v1.json` |
| ❤️ Health check | `https://smartpharmacy-rand.runasp.net/api/Health` |

The reference page has an **Authorize** button: sign in through `POST /api/Authentications/Login`, paste the returned `accessToken`, and every secured endpoint becomes callable from the browser.

> 💡 To explore in Postman instead, import the OpenAPI document directly:
> **Import → Link →** `https://smartpharmacy-rand.runasp.net/openapi/v1.json`

### Demo accounts

| Role | Email | What it unlocks |
|---|---|---|
| 🛡️ Admin | `admin@smartpharmacy.com` | User management, roles, background jobs |
| 💊 Pharmacist | *available on request* | Inventory, prescription review, order management |
| 🧑 Patient | *register your own* | Browsing, cart, checkout, prescriptions |

Any account you register through `POST /api/Authentications/register` starts as a **Patient**, so you can create one and walk the full customer journey yourself.

> ⚠️ Stripe runs in **test mode**. Use card `4242 4242 4242 4242` with any future expiry and any CVC — no real charge is ever made.

> ℹ️ The API is on free shared hosting that stops the application after 30 minutes without traffic. An external keep-alive request every 10 minutes keeps it awake, but the very first request after a long idle period may take a few seconds.

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
- ⏱️ **Hangfire** for recurring background jobs (SQL Server storage)
- ✅ **FluentValidation** for request validation
- 💳 **Stripe.net** for payments
- 📖 **Scalar** for the interactive API reference

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
   dotnet user-secrets set "Jwt:SecretKey" "<your-secret-key>"
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

## 🚢 Deployment

The application is hosted on **MonsterASP.NET** (IIS 10 / Windows) with SQL Server on `databaseasp.net`, published from Visual Studio over **Web Deploy**.

No secret is ever committed: every sensitive value in `appsettings.json` is left empty and supplied at runtime through **environment variables** on the host. ASP.NET Core reads environment variables after `appsettings.json`, so they take precedence without any code change.

Nested configuration keys use a **double underscore** in place of the colon:

| Configuration key | Environment variable |
|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `AdminUser:Email` | `AdminUser__Email` |
| `AdminUser:Password` | `AdminUser__Password` |
| `Jwt:SecretKey` | `Jwt__SecretKey` |
| `StripeSettings:SecretKey` | `StripeSettings__SecretKey` |
| `EmailSettings:Password` | `EmailSettings__Password` |

> A single underscore is silently ignored by the configuration binder, so the value never arrives and the application fails at startup. `GetRequiredConnectionString` exists to turn that failure into a message that names the missing variable instead of an unrelated stack trace.

The first `Admin` account is seeded from `AdminUser__Email` / `AdminUser__Password` on first startup and skipped on every later start, so redeploying never creates a duplicate.

### Background jobs on shared hosting

Hangfire runs **inside the application process**, and IIS stops the process after 30 minutes without traffic — which would delay the daily inventory check. An external scheduler pings `/api/Health` every 10 minutes to keep the application awake. Hangfire persists its schedule in SQL Server, so a job whose time was missed still runs on the next startup rather than being skipped.

Because the Hangfire dashboard authenticates with a cookie while this API issues bearer tokens, a browser opening `/hangfire` is always anonymous and rejected. `GET /api/BackgroundJobs/recurring`, `GET /api/BackgroundJobs/statistics`, and `POST /api/BackgroundJobs/recurring/{jobId}/trigger` expose the same information over the API's own authentication.

## 🧪 Testing

| Document | Scope |
|---|---|
| [`docs/ALPHA-TEST-PLAN.md`](docs/ALPHA-TEST-PLAN.md) | Internal round on the developer machine — 148 cases |
| [`docs/BETA-TEST-PLAN.md`](docs/BETA-TEST-PLAN.md) | Round on the deployed host, including post-deployment and external-user cases — 186 cases |

The beta plan adds a `DEP` section for failures that cannot appear locally at all: configuration that never reaches the host, documentation hidden behind a development-only check, and background jobs stopped by an idle application pool.

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
- [x] ✅ Request validation (FluentValidation) and a global exception handler
- [x] 🚀 Deployed to a live host, with every secret supplied through environment variables
- [ ] 📊 Reports & dashboard endpoints
- [ ] 📱 Flutter client consuming this API

## 📄 License

TBD

---

<p align="center">Made with ❤️ for SmartPharmacy</p>
