# Copilot Instructions for Galileo_API

## Project Overview
- **Type:** ASP.NET Core 9 Web API (C# 12)
- **Purpose:** Secure, private API with JWT authentication, modular business logic, and dynamic RDLC reporting.
- **Key Features:**
  - JWT-based authentication (see `BusinessLogic/JwtBL.cs`, `Controllers/LogonController.cs`)
  - Dynamic RDLC report rendering (see `DataBaseTier/mReportingServicesDB.cs`)
  - Clean separation: Controllers, BusinessLogic, DataBaseTier, Models
  - Swagger UI for API docs/testing

## Directory Structure & Patterns
- `Controllers/`: API endpoints, thin controllers, delegate to BusinessLogic
- `BusinessLogic/`: Core business rules, orchestrates data access and logic
- `DataBaseTier/`: Data access, Dapper-based SQL/SP execution, reporting utilities
- `Models/`: DTOs, data contracts, error/result wrappers
- `appsettings*.json`: Configuration (no secrets/keys in source)

## Development Workflow
- **Build:**
  ```bash
  dotnet build Galileo_API/Galileo_API.csproj
  ```
- **Run (dev):**
  ```bash
  dotnet run --project Galileo_API/Galileo_API.csproj
  ```
- **User Secrets (JWT key):**
  ```bash
  dotnet user-secrets set "Jwt:Secret" "<your_secret>"
  ```
- **Swagger UI:**
  - Available at `/swagger` when running locally

## Conventions & Integration
- **No credentials in code:** Use user-secrets or environment variables for sensitive config
- **Error handling:** Use `ErrorDto<T>` for API responses (see `Models/manejoErroresModels.cs`)
- **Reporting:**
  - Use `mReportingServicesDB.ReporteRDLC_v2` for dynamic RDLC rendering (supports subreports, code injection, JSON/VB code sections)
  - Data access via Dapper, prefer parameterized queries/SPs
- **Testing:** No explicit test project found; manual/Swagger-based testing is common

## External Dependencies
- Dapper (SQL access)
- Microsoft.Reporting.NETCore (RDLC rendering)
- Newtonsoft.Json (JSON handling)

## Examples
- Add a new endpoint: create a controller in `Controllers/`, add logic in `BusinessLogic/`, data access in `DataBaseTier/`, and DTOs in `Models/`
- To add a new report: place RDLC in the configured folder, update config if needed, and use the reporting API

---
For more details, see `README.md` and key files in each directory.
