# Changelog

Todas as mudancas relevantes do projeto sao registradas aqui.

## [2026-06-18] - Mobile Card Contract For Events, Social, Store And Notifications

### Added

- `src/AlphaSquad.Shared/DTOs/Common/MobileContractDtos.cs` com o contrato reutilizavel `MobileCardItemResponse`.
- Propriedades computadas de contrato mobile em:
- `AcademyEventResponse`
- `SocialPostResponse`
- `ProductListItemResponse`
- `NotificationListItemResponse`
- `NotificationDetailsResponse`

### Changed

- `NotificationListItemResponse` passou a carregar tambem o destino relacionado (`RelatedEntityType` e `RelatedEntityId`) para navegacao orientada por payload no app.
- `ROADMAP.md` agora registra a padronizacao de metadados de cards mobile como concluida.
- As notificacoes seguem oficialmente no modelo paginado (`PagedResponse<T>`) para o app, enquanto `Events`, `Social` e `Store` mantem contratos cursor-based para feeds maiores.

## [2026-06-14] - Local Container Stack For Api, Postgres And Redis

### Added

- `src/AlphaSquad.Api/Dockerfile` com build multi-stage para publicar a API em container.
- `.dockerignore` na raiz para reduzir contexto de build Docker.
- `.env` local para espelhar os segredos antes mantidos apenas em `user-secrets` no modo container.
- `.env.example` com placeholders para onboarding de outros ambientes.
- `docker-compose.yml` oficial na raiz com:
- `api`
- `db`
- `redis`
- volumes nomeados para Postgres e Redis
- healthchecks dos servicos de infraestrutura

### Changed

- `AlphaSquad.Api` agora usa a chave `App:EnableHttpsRedirection` para alternar entre:
- execucao tradicional com HTTPS local
- execucao containerizada em `http://localhost:8080`
- `AlphaSquad.Web` e `AlphaSquad.Backoffice` passam a documentar em codigo que a URL versionada default da API continua `https://localhost:7054`.
- O modo container passa a sobrescrever a `BaseUrl` dos frontends apenas em runtime com `Apis__AlphaSquad__BaseUrl=http://localhost:8080`.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para documentar:
- stack local containerizada
- regras de `user-secrets` x `.env`
- exposicao de portas para banco e Redis
- fluxo de migrations com Postgres em container

### Noted

- `AlphaSquad.Web` e `AlphaSquad.Backoffice` continuam fora do Docker nesta fase.
- O consumo da API continua passando por `AlphaSquad.Lmt.Application.Http`, `AlphaSquad.Lmt.Application.Contracts` e pelo cliente HTTP do backoffice.
- O projeto permanece reversivel entre modo tradicional e modo container sem troca estrutural de codigo nos frontends.

## [2026-05-19] - V1 Billing Decision And Documentation Alignment

### Changed

- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para separar formalmente:
- billing da academia para seus alunos
- billing SaaS da AlphaSquad para novas academias clientes
- Registrada a decisao oficial da V1:
- mensalidade do aluno fica fora do checkout online do app e do dashboard
- a academia continua registrando pagamento manualmente quando necessario
- a venda do SaaS AlphaSquad entra com Stripe via landing page
- o onboarding da academia permanece semiautomatico, com intervencao do sponsor

### Noted

- `MembershipPayment`, `BillingDueDay` e a situacao financeira derivada do aluno passam a ser tratados como parte consolidada da V1 operacional da academia.
- `PaymentTransaction` fica reservado para uma etapa futura de billing transacional dos alunos.

## [2026-05-18] - Tenant Context Middleware

### Added

- `TenantRequestContext` para representar o contexto multi-tenant validado da requisicao.
- `TenantContextMiddleware` no pipeline da API para validar tenant e usuario das academias antes dos endpoints.

### Changed

- `GetTenantId()` e `GetTenantSlug()` passam a usar o contexto resolvido quando ele existe no `HttpContext`.
- O isolamento do tenant deixa de depender apenas de resolucao manual espalhada pelos endpoints.

## [2026-05-18] - Platform Audit Trail And Tenant Extra Admins

### Added

- Entidade `PlatformAuditLog` para registrar a trilha administrativa do sponsor no contexto master.
- Endpoints master para:
- listar admins da academia
- criar admin adicional do tenant
- regenerar senha provisoria de admin especifico
- listar auditoria da academia
- Pagina de `Admins` no `AlphaSquad.Backoffice` para suporte operacional do sponsor aos tenants.

### Changed

- `PlatformTenantDetailsResponse` passa a devolver todos os admins da academia e os registros recentes de auditoria.
- A tela de detalhes da academia no backoffice passa a exibir admins adicionais e historico resumido de acoes do sponsor.

### Noted

- O contexto master agora consegue apoiar a operacao do tenant mesmo quando o admin principal nao consegue criar outros administradores.

## [2026-05-17] - Documentation Alignment & Current Backend State

### Changed

- `README.md` atualizado para refletir a solution atual com `AlphaSquad.Web`, `AlphaSquad.Backoffice` e a camada `AlphaSquad.Lmt.Application.*`.
- `README.md` atualizado com os endpoints master de `platform-auth`, `platform-profile` e `platform-tenants`.
- `README.md` atualizado com o fluxo atual da senha provisoria gerada automaticamente para o admin inicial da academia.
- `ARCHITECTURE.md` atualizado para registrar `PlatformAuth`, `PlatformProfile`, `PlatformTenants`, a camada LMT e o estado real do backoffice.
- `ARCHITECTURE.md` atualizado com o fluxo final do modulo de `Events`, incluindo senha de check-in, presenca, conclusao administrativa e pontuacao em lote.
- `ROADMAP.md` atualizado para marcar a fundacao master ja concluida e refletir as pendencias reais de backend.

### Noted

- A documentacao da raiz passa a tratar o contexto master da AlphaSquad como parte consolidada do produto.
- As proximas pendencias estruturais do backend se concentram em middleware central de tenant, auditoria master e billing.

## [2026-05-17] - Notifications And Institutional Events Backend

### Added

- Central de notificacoes do app com publicacao por tenant, leitura por usuario, filtros `all/read/unread`, contador de nao lidas e marcacao como lida.
- Endpoints administrativos para CRUD de notificacoes institucionais do tenant.
- Suporte no dominio de `Events` para `BirthdayHighlight` e `GamificationWinnersHighlight`.
- Janelas temporarias de destaque com `HighlightStartsAt` e `HighlightEndsAt`.
- Geradores administrativos de eventos institucionais para:
- aniversariantes do dia
- vencedores fechados da gamificacao mensal
- Publicacao automatica de notificacoes do app quando esses destaques institucionais sao gerados.

## [2026-05-17] - Backoffice Web Integration With Master API

### Added

- Integracao do `AlphaSquad.Backoffice` com os endpoints reais de `platform-auth`, `platform-profile` e `platform-tenants`.
- Perfil do owner do backoffice com nome, foto e troca de senha.
- Upload e preview de logo da academia no backoffice.

### Changed

- O backoffice deixa de depender do fluxo bootstrap em memoria para listar, detalhar e editar academias.
- O projeto passa a abrir na tela de login do backoffice, com autenticacao e sessao proprias.

### Noted

- O login do backoffice continua separado do dashboard das academias e nao reutiliza `tenant_id`.

## [2026-05-16] - Backoffice Backend Foundation

### Added

- Entidade `PlatformUser` para representar o sponsor e futuros operadores globais da AlphaSquad.
- Entidade `PlatformRefreshToken` para separar a sessao do backoffice do fluxo multi-tenant das academias.
- Endpoints `/api/platform-auth/login`, `/me`, `/refresh`, `/change-password` e `/logout`.
- Endpoints `/api/platform-tenants` para listagem, detalhe, criacao, atualizacao e reset de senha provisoria do admin principal.
- Seed bootstrap do sponsor da plataforma a partir da configuracao `PlatformBootstrap`.

### Changed

- `AppUser` passou a ter `MustChangePassword`, habilitando provisao segura do admin inicial da academia.
- O onboarding master da academia agora cria:
- tenant
- features contratadas
- admin inicial
- senha provisoria
- obrigatoriedade de troca de senha no primeiro acesso
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para registrar a separacao entre contexto tenant e contexto master da AlphaSquad.

### Noted

- A camada master foi implementada sem alterar o modelo de autenticacao das academias.
- O dashboard das academias continua isolado; o backoffice master usa claims e refresh tokens proprios.

## [2026-05-15] - Profile Contact Fields & Notification Planning

### Added

- Campos de `celular com DDD` e `data de nascimento` no dominio de `Profile`.
- Atualizacao do dashboard de perfil para exibir e editar esses dados.

### Changed

- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` agora registram formalmente a preparacao do produto para:
- destaques internos de aniversariantes no mural
- divulgacao de vencedores da gamificacao
- futura central de notificacoes do app com leitura por usuario, filtros por status e modal rolavel

### Noted

- A notificacao prevista nesta fase e geral para todos os alunos do app, sem segmentacao individual obrigatoria no primeiro recorte.

## [2026-05-15] - Dashboard Toast Standard

### Changed

- Documentado em `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` que o dashboard administrativo deve usar `toast` como padrao oficial de feedback visual.
- Mensagens inline deixam de ser o padrao recomendado para sucesso, erro, alerta e informacao nas telas do frontend web.

### Noted

- O posicionamento padrao dos `toasts` fica no topo direito da area de conteudo, alinhado com a linha do breadcrumb e logo abaixo do header principal.

## [2026-05-09] - Tenant Legal Content

### Added

- Entidade `TenantLegalContent` para centralizar `Termos de Uso` e `Politica de Privacidade` por tenant.
- Endpoint `GET /api/legal/current` para o app baixar os textos legais do tenant autenticado.
- Endpoint `PUT /api/legal/current` para manutencao administrativa dos textos legais.

### Changed

- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir o novo modulo de textos legais.

### Noted

- A manutencao dos textos legais ficou restrita a `Admin`, enquanto a leitura e liberada para qualquer usuario autenticado.

## [2026-05-09] - Admin-Only Hardening For Administrative Endpoints

### Changed

- Endpoints administrativos de `Users`, `Plans`, `Media`, `Store`, `Classes`, `Events`, `Exercises`, `Workouts` e consultas administrativas de `Checkins` passaram por endurecimento de autorizacao.
- Operacoes administrativas de leitura e escrita nesses modulos agora exigem `AdminOnly` quando representam cadastro, manutencao ou visao de dashboard administrativo.
- `README.md` e `ARCHITECTURE.md` atualizados para refletir a nova regra de acesso.

### Noted

- `Teacher` deixa de acessar endpoints estritamente administrativos e permanece apenas nos fluxos que nao sao de administracao quando a regra de negocio permitir.

## [2026-05-09] - V1 Closure: Transversal Hardening

### Added

- Contrato compartilhado `PagedResponse<T>` para padronizar respostas page-based nos principais endpoints de listagem.
- `UserSecretsId` no projeto da API para facilitar configuracao local segura.

### Changed

- `Checkins`, `Classes`, `Media`, `Events`, `Social` e `Store` agora expõem respostas paginadas tipadas e consistentes no Swagger.
- `appsettings.json` passou a guardar apenas placeholders seguros em vez de credenciais reais.
- `appsettings.Development.json` passou a concentrar defaults locais de desenvolvimento.
- `Program.cs` foi limpo para remover duplicidade de `AddEndpointsApiExplorer` e documentar melhor a estrategia de configuracao por ambiente.
- `README.md`, `ARCHITECTURE.md` e `ROADMAP.md` atualizados para refletir o padrao dual de listagem e a estrategia de configuracao segura.

### Noted

- A base fica mais pronta para publicacao inicial porque o app mobile e os dashboards administrativos agora consomem contratos mais previsiveis.
- Os segredos reais devem ser mantidos em `user-secrets` ou variaveis de ambiente.

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
