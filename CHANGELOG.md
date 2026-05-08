# Changelog

Todas as mudancas relevantes do projeto sao registradas aqui.

## [2026-05-07] - Documentation Alignment & Codebase Overview

### Changed

- Atualizada a documentacao da raiz para refletir o estado real do workspace.
- Alinhado o `README.md` com os modulos atualmente presentes no backend.
- Revisado o `ARCHITECTURE.md` para incluir `Exercises`, feature flags por tenant e o estado do dominio de `Workouts`.
- Revisado o `ROADMAP.md` para marcar `Exercise CRUD` como concluido e `Workout` como frente em consolidacao.

### Noted

- O backend ja possui base consolidada para `Auth`, `Tenants`, `Media`, `Users` e `Exercises`.
- O dominio de `Workout` ja aparece no modelo de dados, migrations e implementacao local, mas ainda deve ser tratado como modulo em evolucao.

## [2026-05-07] - Check-in Module

### Added

- Entidade `CheckIn` com suporte a multi-tenancy.
- Endpoint `POST /api/checkins` para registrar entrada do usuario autenticado.
- Endpoint `GET /api/checkins/me` para consultar o historico do proprio usuario.
- Endpoint `GET /api/checkins/tenant` para consulta paginada do tenant com filtro por usuario e periodo.
- Regra para impedir check-in duplicado no mesmo dia.

### Changed

- `README.md` e `ROADMAP.md` atualizados para refletir a entrega inicial do modulo de check-in.

## [2026-05-08] - Classes & Scheduling Base

### Added

- Entidade `GymClass` para representar aulas e agendas do tenant.
- Endpoints `GET /api/classes`, `GET /api/classes/{id}`, `POST /api/classes`, `PUT /api/classes/{id}` e `DELETE /api/classes/{id}`.
- Filtros por periodo e status na listagem de aulas.
- Validacao de instrutor por tenant e por role (`Teacher` ou `Admin`).
- Migration inicial do modulo de classes/agendas.

### Changed

- `README.md` e `ROADMAP.md` atualizados para refletir a entrega do CRUD basico de aulas.

## [2026-05-08] - Class Booking Flow

### Added

- Entidade `ClassBooking` para representar reservas de aula.
- Endpoint `POST /api/classes/{id}/book` para reservar uma aula.
- Endpoint `DELETE /api/classes/{id}/book` para cancelar a própria reserva.
- Validação de capacidade máxima da aula.
- Bloqueio de reserva duplicada para o mesmo usuário na mesma aula.
- Bloqueio de reserva para aulas já iniciadas.

### Changed

- `README.md` e `ROADMAP.md` atualizados para refletir a entrega do fluxo de booking.

## [2026-05-06] - Authentication & Security Base

### Added

- Refresh token rotation com persistencia em banco.
- Change password com validacao da senha atual.
- Logout com revogacao de tokens ativos.
- Entidade `RefreshToken` e configuracao no `AppDbContext`.
- Revogacao de sessoes durante troca de senha.
- Documentacao inicial de arquitetura, roadmap e changelog.

### Fixed

- Relacionamento entre `AppUser` e `RefreshToken`.
- Null safety no parse de `UserId` vindo das claims.
- Comentarios tecnicos restaurados e expandidos no codigo.

## [2026-05-06] - Tenant, Media & Features Implementation

### Added

- `Feature` e `TenantFeature` para controle modular por tenant.
- Seed automatico de features base para o tenant `alpha-demo`.
- Endpoints `/api/tenants/current` e `/api/tenants/current/features`.
- Endpoint `PUT /api/tenants/current/logo` com remocao automatica do arquivo anterior.
- Endpoint `GET /api/media/{id}` com isolamento por tenant.
- Relacao entre `Tenant` e `TenantMedia` via `LogoMediaId`.
