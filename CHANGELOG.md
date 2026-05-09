# Changelog

Todas as mudancas relevantes do projeto sao registradas aqui.

## [2026-05-08] - Store Feed & Social Module

### Added

- Endpoint `GET /api/store/products/feed` com contrato cursor-based para o catalogo da loja.
- Entidades `SocialPost`, `SocialPostLike` e `SocialPostComment` para a rede interna da academia.
- Endpoints de post, feed, like e comentario simples no modulo `Social`.
- Feature opcional `SOCIAL` no seed base do tenant demo.

### Changed

- O modulo de `Store` agora convive com dois formatos de listagem:
- `GET /api/store/products` para paginacao tradicional
- `GET /api/store/products/feed` para scroll infinito no app
- Criacao de post social agora gera `SocialPost` na gamificacao para alunos.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a base inicial da rede social e do feed da loja.

### Noted

- A rede social nasce com o mesmo padrao dual de listagem usado em `Events`, preparando reuso consistente no app.

## [2026-05-08] - Events Feed Standardization

### Added

- DTO compartilhado `CursorFeedResponse<T>` para padronizar feeds cursor-based do app.
- Endpoint `GET /api/events/feed` com contrato cursor-based para scroll infinito.

### Changed

- O modulo de `Events` agora convive com dois formatos de listagem:
- `GET /api/events` para paginacao tradicional
- `GET /api/events/feed` para feed infinito no app
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para registrar esse padrao duplo.

### Noted

- Esse contrato cursor-based foi pensado para ser reaproveitado futuramente em `Store` e `Social`.

## [2026-05-08] - V1 Closure: Workouts, Profile & Class Operations

### Added

- Endpoint `PUT /api/profile/me/password` para permitir troca de senha dentro da area de profile.
- Endpoint `GET /api/classes/{id}/bookings` para consulta operacional de reservas por aula.
- Endpoint `GET /api/classes/bookings/by-user/{userId}` para consulta operacional de reservas por aluno.

### Changed

- `Workouts` agora valida nome com mais rigor, evita duplicidade por tenant e aceita limpar a composicao do treino sem inconsistencias.
- `README.md` e `ROADMAP.md` atualizados para refletir o fechamento desses pontos da V1.

### Noted

- A troca de senha pelo profile revoga as sessoes ativas do usuario, mantendo a mesma regra de seguranca do modulo de autenticacao.

## [2026-05-08] - Events Module & Outdoor Participation

### Added

- Entidade `AcademyEvent` para representar o mural institucional e outdoor da academia.
- Entidade `AcademyEventParticipation` para registrar participacao de alunos em eventos outdoor.
- Endpoints de leitura do mural, CRUD administrativo e confirmacao de participacao.
- Feature opcional `EVENTS` no seed base do tenant demo.
- Servico `FeatureAccessService` para validar features opcionais por tenant.

### Changed

- Participacoes em eventos outdoor agora geram `OutdoorEventParticipation` na gamificacao.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir o novo modulo opcional de eventos.

### Noted

- O modulo de eventos pode ou nao fazer parte do pacote contratado pela academia.
- A escrita do mural fica restrita a `Admin` e `Teacher`, enquanto a leitura depende da feature `EVENTS` estar habilitada.

## [2026-05-08] - Gamification Base

### Added

- Entidade `GamificationEventRule` para regras de pontuacao por tenant.
- Entidade `UserGamificationEvent` para registrar eventos pontuados por aluno.
- Entidade `PointsLedger` para manter saldo acumulado auditavel.
- Entidade `MonthlyStudentRanking` para snapshots mensais de vencedores e premios.
- Endpoints de dashboard pessoal, ranking mensal, historico de vencedores e gestao de regras.
- Seed inicial das regras padrao de gamificacao para o tenant demo.

### Changed

- `CheckIn` agora gera pontuacao para alunos quando o check-in e concluido.
- `StoreOrder` agora gera pontuacao quando o pedido e marcado como `PaidLocally`.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a base inicial de gamificacao.

### Noted

- Apenas usuarios com role `Student` participam da gamificacao.
- O ranking mensal pode ser consultado ao vivo ou fechado em snapshot administrativo.

## [2026-05-08] - Gamification Integrations: Plans, Payments & Special Classes

### Added

- Entidade `MembershipPayment` para registrar pagamentos de mensalidade com auditoria administrativa.
- Endpoint `POST /api/plans/payments` para registrar pagamentos e identificar quando foram feitos em dia.
- Campo `IsSpecialClass` em `GymClass` para marcar auloes e aulas especiais.

### Changed

- A atribuicao de novo plano agora pode pontuar renovacao para alunos com historico anterior.
- Pagamentos registrados em dia agora geram pontuacao de gamificacao.
- Booking de aula especial agora gera pontuacao de participacao.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir essas novas integracoes.

## [2026-05-08] - Store Orders V1

### Added

- Entidade `StoreOrder` para representar pedidos com retirada presencial.
- Entidade `StoreOrderItem` com snapshot comercial dos itens comprados.
- Endpoints para criacao e consulta de pedidos pelo proprio usuario.
- Endpoints administrativos para listagem de pedidos e atualizacao de status operacional.

### Changed

- A loja agora suporta fluxo completo de pedido sem gateway externo na V1.
- O estoque passa a ser reservado quando o pedido entra em status de separacao e restaurado em cancelamentos operacionais.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a evolucao da loja V1.

### Noted

- O pagamento continua acontecendo presencialmente na academia.
- Stripe permanece como etapa posterior, sem bloquear a operacao inicial da loja.

## [2026-05-08] - Store Module Base

### Added

- Entidade `Product` para representar itens da loja interna do tenant.
- Entidade `ProductVariant` para representar variacoes comerciais como tamanho, cor, preco e estoque.
- Endpoints de catalogo da loja para listagem paginada e detalhe de produto.
- Endpoints administrativos para criar, atualizar e remover produtos e variantes.

### Changed

- `AppDbContext` atualizado com o mapeamento do dominio inicial de loja.
- `DatabaseSeeder` atualizado para incluir a feature `STORE` no tenant demo.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir a entrega inicial da loja.

### Noted

- Esta primeira rodada prepara a base do catalogo e da gestao de produtos, deixando fluxo de compra, pedido e Stripe para a proxima etapa.

## [2026-05-08] - Store V1 & Gamification Direction

### Changed

- Registrada a decisao de negocio para a V1 da loja: pedido pelo app com retirada e pagamento presencial na academia.
- Registrado que Stripe permanece como etapa posterior, apos validacao operacional da loja.
- Registrada a direcao da gamificacao baseada em eventos com pontuacao configuravel por tipo de acao.

### Noted

- A gamificacao deve considerar apenas usuarios com role `Student`.
- Compras na loja, renovacao de plano, check-in, posts e outros eventos do ecossistema devem alimentar o ranking mensal.

## [2026-05-08] - Plans History & Retention Queries

### Added

- Campos de historico em `UserMembership` para motivo de status e usuario responsavel pela alteracao.
- Endpoint `GET /api/plans/users/{userId}/history` para historico de planos por usuario.
- Endpoint `GET /api/plans/inactive-users` para usuarios sem plano ativo ha X dias.
- Endpoint `GET /api/checkins/inactive-users` para usuarios com plano vigente sem check-in ha X dias.

### Changed

- O acesso ao sistema agora exige plano ativo valido no `login` e no `refresh`.
- A atribuicao de plano passou a registrar motivo da troca e o usuario responsavel.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir o pacote de historico, retencao e reengajamento.

### Noted

- O dominio de planos agora tambem serve como base para futuras campanhas comerciais de retorno e acoes de reengajamento ligadas a check-in e gamificacao.

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
