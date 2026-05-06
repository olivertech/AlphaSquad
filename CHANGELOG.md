# ?? Changelog

All notable changes to the AlphaSquad project will be documented here.

## [2026-05-06] - Authentication & Security Base
### Added
- **Refresh Token System**: Implemented full rotation logic to secure sessions.
- **Change Password**: Added endpoint to allow users to update passwords with current password validation.
- **Logout**: Added session revocation for all active refresh tokens.
- **Infrastructure**: Created `RefreshToken` entity and configured it in `AppDbContext`.
- **Security**: Integrated session termination during password changes.
- **Documentation**: Created `ARCHITECTURE.md`, `ROADMAP.md`, and `CHANGELOG.md`.

### Fixed
- **Database Relationships**: Fixed the missing Foreign Key relationship between `AppUser` and `RefreshToken`.
- **Null Safety**: Resolved potential null reference warnings when parsing User IDs from JWT claims.
- **Comments**: Restored and expanded technical documentation within the code for didactic purposes.
