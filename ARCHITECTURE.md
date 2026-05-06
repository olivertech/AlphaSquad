# ??? AlphaSquad Architecture

This document provides a technical overview of the AlphaSquad platform architecture, designed as a white-label SaaS for gym management.

## ?? Architectural Patterns

The project follows a **Clean Minimalist** approach with the following patterns:

- **Vertical Slice Architecture (Feature-first)**: Code is organized by features rather than technical layers. This reduces coupling and makes the system easier to maintain and scale.
- **Minimal APIs**: Leveraging ASP.NET Core 10 Minimal APIs for high performance and reduced boilerplate.
- **Multi-tenancy (Silo Isolation)**: Data isolation is achieved via a `TenantId` (GUID) present in almost every entity. All queries are filtered by this ID to ensure that one gym cannot access another gym\'s data.
- **Cache-Aside Pattern**: Uses Redis for distributed caching to reduce database load and improve response times for configuration and static data.

## ?? Solution Structure

- **`AlphaSquad.Api`**: 
    - Entry point of the application.
    - Contains the endpoints organized by features (e.g., `Features/Auth`, `Features/Users`).
    - Handles HTTP requests, validation, and responses.
- **`AlphaSquad.Infrastructure`**: 
    - Implements the "heavy lifting".
    - **Persistence**: EF Core with PostgreSQL.
    - **Auth**: JWT generation and password hashing (BCrypt).
    - **Storage**: Cloudflare R2 integration via S3-compatible API.
    - **Caching**: Redis implementation.
- **`AlphaSquad.Shared`**: 
    - Common contracts, DTOs, Enums, and helpers.
    - Ensures a consistent data contract between the API and Infrastructure.

## ?? Authentication & Security Flow

### Token Strategy
The system uses a dual-token strategy to balance security and user experience:
1. **Access Token (JWT)**: Short-lived token containing user identity and tenant info.
2. **Refresh Token**: Long-lived, opaque, cryptographically strong string stored in the database.

### Refresh Token Rotation
To prevent replay attacks, the system implements **Refresh Token Rotation**:
- When a user requests a new Access Token using a Refresh Token, the current Refresh Token is marked as `IsUsed`.
- A brand new Refresh Token is generated and returned to the client.
- If a used or revoked token is presented, the system can flag the session as compromised.

### Security Safeguards
- **Password Hashing**: Uses BCrypt for secure storage.
- **Session Termination**: Changing a password or logging out explicitly revokes all active Refresh Tokens for that user.
- **Case-Insensitive Emails**: Uses PostgreSQL `citext` to prevent duplicate accounts with different casing.

## ?? Infrastructure Integration

- **Storage**: Files are organized in Cloudflare R2 using the path: `tenants/{tenantSlug}/media/{guid}_{fileName}`.
- **Logo Management**: Tenant logos are linked to the `TenantMedia` entity, allowing precise tracking and deletion of the binary file in storage when updated.
- **Cache**: Redis keys follow the pattern `AlphaSquad:{category}:{identifier}`.
- **Database**: PostgreSQL serves as the primary source of truth.


