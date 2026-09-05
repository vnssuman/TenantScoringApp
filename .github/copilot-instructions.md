# GitHub Copilot Rules: Tenant Scoring Monorepo

## Tech Stack Guidelines
- **Backend:** .NET 10 Web API, Entity Framework Core 10, SQL Server.
- **Frontend:** React 18+, TypeScript, Vite, Tailwind CSS.
- **Architecture:** Monorepo (`/backend` for C#, `/frontend` for React).

## Backend Guardrails (.NET 10)
1. **Target Runtime:** Target `.NET 10` using C# 14 syntax.
2. **Primary Constructors:** Prefer primary constructor syntax for service dependencies.
3. **Immutability:** Use `record` types for Request and Response DTOs (`TenantScoring.Models`).
4. **Thin Controllers/APIs:** Put pure business scoring logic strictly inside `TenantScoring.Core` domain services. Do not execute scoring logic directly in API controllers or Minimal API handlers.
5. **Database Security:** Use Entity Framework Core parameterized LINQ queries exclusively inside `TenantScoring.Data`. Never construct raw SQL strings.

## Frontend Guardrails (React & TypeScript)
1. **Type Safety:** Maintain strict TypeScript interfaces matching the C# DTOs in `/frontend/src/types`.
2. **Component Isolation:** Store general UI controls in `src/components/` and domain views in `src/features/`.
3. **API Layer:** Use a dedicated Axios/Fetch service wrapper inside `src/services/` to talk to the .NET 10 API.

## Security & Enterprise Rules
1. **No Hardcoded Secrets:** Connection strings, JWT secrets, and keys must read from `.env` or `appsettings.Development.json`.
2. **PII Masking:** Mask SSNs, bank details, and sensitive credit metrics in UI state and backend logs.

Always execute file creation and file edit tools directly into the workspace. Never output raw JSON tool calls or file edit payloads in chat text. Automatically apply changes to the target files.