# Changelog

Todas as mudancas relevantes do projeto sao registradas aqui.

## [2026-05-08] - Product Vision Expansion & Planning

### Changed

- Atualizado o `README.md` para refletir o momento atual do backend e a nova visao de produto do AlphaSquad.
- Atualizado o `ARCHITECTURE.md` para incluir os modulos estrategicos `Store`, `Social`, `Gamification`, `Profiles` e `Events`.
- Atualizado o `ROADMAP.md` com nova ordem recomendada de implementacao, fases por dominio e dependencias entre modulos.
- Registrada a direcao de V2 para multi-idioma com traducao dinamica de conteudo no backend.

### Noted

- Loja interna, rede social, gamificacao, profile do usuario e mural de eventos passam a ser tratados como pilares core do produto.
- A implementacao deve seguir ordem que reduza retrabalho entre profile, loja, feeds sociais e gamificacao.

## [2026-05-08] - Security Documentation Reinforcement

### Changed

- Atualizado o `README.md` com uma secao dedicada a seguranca em camadas.
- Atualizado o `ARCHITECTURE.md` para detalhar autenticacao, isolamento multi-tenant, autorizacao por role e protecao por contexto do recurso.
- Documentadas as policies base `AdminOnly` e `AdminOrTeacher` como parte da arquitetura atual.

### Noted

- A documentacao agora deixa explicito que o AlphaSquad combina varias camadas de seguranca para ambientes de academia com operacao white-label e multi-tenant.

## [2026-05-08] - Profile Module Base

### Added

- Entidade `UserProfile` para separar dados de experiencia do usuario dos dados centrais de autenticacao.
- Endpoint `GET /api/profile/me` para leitura do proprio profile.
- Endpoint `PUT /api/profile/me` para atualizacao de nome e username.
- Endpoint `PUT /api/profile/me/photo` para upload e substituicao da foto de profile.
- Endpoint `DELETE /api/profile/me/photo` para remocao da foto de profile.

### Changed

- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a entrega inicial do modulo de profile.

### Noted

- O campo de plano ativo foi preparado como snapshot informativo para integracao futura com o dominio de planos.

## [2026-05-08] - Profile & Auth Integration

### Added

- Endpoint `PUT /api/profile/me/email` para troca segura de e-mail com validacao de senha atual.

### Changed

- `login` e `/api/auth/me` agora devolvem dados complementares de profile, como `username`, foto e plano ativo.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a ampliacao do modulo de profile.

## [2026-05-08] - Profile Validation & Plans Domain

### Added

- Entidade `MembershipPlan` para representar o catalogo de planos da academia.
- Entidade `UserMembership` para representar o plano ativo e o historico de vinculo do usuario.
- Endpoints administrativos de planos para listagem, criacao, atualizacao e atribuicao de plano a usuario.

### Changed

- `Profile` agora aplica validacao mais forte de formato de e-mail e regras de `username`.
- `ActivePlan` deixou de depender de snapshot solto e passou a ser projetado a partir de um dominio real de planos.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a nova base de planos.

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
