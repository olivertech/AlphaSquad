# AlphaSquad Architecture

Este documento descreve a arquitetura tecnica atual do AlphaSquad com base no codigo existente no workspace.

## Objetivo arquitetural

O sistema foi desenhado para sustentar um SaaS white-label para academias com:

- isolamento forte por tenant
- simplicidade operacional
- baixo acoplamento entre modulos
- boa evolucao incremental por feature

## Padroes adotados

### Vertical Slice Architecture

Os endpoints sao organizados por feature em `src/AlphaSquad.Api/Features`, reduzindo dependencia entre modulos e favorecendo evolucao isolada de cada dominio.

Features identificadas hoje:

- `Auth`
- `Tenants`
- `Media`
- `Users`
- `Exercises`
- `Checkins`
- `Classes`
- `Workouts` em desenvolvimento no workspace

### Minimal APIs

A API usa ASP.NET Core Minimal APIs para:

- reduzir boilerplate
- manter o fluxo HTTP direto e legivel
- documentar endpoints via Swagger com menor sobrecarga

### CQRS leve

Sem framework formal de CQRS, mas com divisao pragmatica:

- `EF Core` para escrita e controle de entidades
- `Dapper` para consultas e respostas enxutas

Esse padrao aparece claramente em modulos como `Users`, `Media`, `Tenants`, `Exercises` e `Workouts`.

### Foco didatico e documentacao em codigo

Este projeto tambem tem um objetivo didatico e deve poder ser consultado futuramente por outros profissionais.

Por isso, todo novo codigo deve seguir estas diretrizes:

- classes e metodos novos devem trazer comentarios simples explicando o que fazem
- comentarios devem ajudar leitura e manutencao, sem virar texto excessivo
- regras de negocio, integracoes relevantes e pontos de isolamento multi-tenant devem ser comentados
- a documentacao em codigo deve evoluir junto com as features, e nao ser tratada como etapa opcional

## Estrutura da solution

```text
src/
  AlphaSquad.Api
  AlphaSquad.Infrastructure
  AlphaSquad.Shared
```

### AlphaSquad.Api

Responsavel por:

- bootstrapping da aplicacao
- autenticacao e autorizacao
- definicao de endpoints
- Swagger
- composicao dos modulos

### AlphaSquad.Infrastructure

Responsavel por:

- `AppDbContext`
- entidades e mapeamentos EF Core
- migrations
- JWT service
- password hashing com BCrypt
- Redis
- Cloudflare R2
- seeding inicial

### AlphaSquad.Shared

Responsavel por:

- DTOs
- enums
- helpers compartilhados

## Multi-tenancy

O isolamento multi-tenant e baseado em `TenantId` como identificador primario de segregacao.

### Onde o isolamento acontece

- claims do JWT: `tenant_id` e `tenant_slug`
- queries SQL filtradas por tenant
- entidades persistidas com `tenant_id`
- paths de storage separados por tenant

### Risco arquitetural a observar

O isolamento hoje depende principalmente da disciplina nos endpoints e queries. Ainda nao existe um middleware central de tenant ou uma camada mais automatica de enforcement.

## Autenticacao e sessao

### Estrategia de token

O backend usa dois tokens:

- access token JWT
- refresh token opaco persistido no banco

### Fluxo atual

1. Usuario autentica via `tenant slug + email + password`
2. API gera access token com claims de usuario e tenant
3. API gera refresh token aleatorio e persiste no banco
4. No refresh, o token anterior e marcado como usado
5. Um novo refresh token e emitido

### Salvaguardas implementadas

- BCrypt para hash de senha
- rotacao de refresh token
- revogacao de sessao no logout
- revogacao de todas as sessoes ao trocar senha
- emails case-insensitive via `citext`

## Persistencia

### Banco principal

- PostgreSQL

### Entidades atuais

- `Tenant`
- `AppUser`
- `RefreshToken`
- `TenantMedia`
- `Feature`
- `TenantFeature`
- `Exercise`
- `CheckIn`
- `GymClass`
- `Workout`
- `WorkoutExercise`

### Convencoes observadas

- nomes de tabelas em snake_case
- chaves `Guid`
- relacionamentos explicitos no `AppDbContext`
- soft delete em usuarios via `IsActive`
- delete fisico em algumas entidades, como media, exercise e workout

## Cache distribuido

### Tecnologia

- Redis

### Estrategia

- cache-aside

### Caso de uso atual claro

- configuracao de tenant por slug

### Padrao de chave

```text
AlphaSquad:{category}:{identifier}
```

Exemplo no codigo:

```text
AlphaSquad:tenant-config:{slug}
```

## Storage de arquivos

### Tecnologia

- Cloudflare R2 via API S3-compatible

### Organizacao de paths

```text
tenants/{tenantSlug}/media
tenants/{tenantSlug}/logos
```

### Uso atual

- upload de midias
- troca de arquivo de midia
- upload/substituicao de logo do tenant
- exclusao do binario antigo ao trocar logo

## Feature flags por tenant

O projeto ja possui base para habilitacao modular por tenant com:

- `Feature`
- `TenantFeature`

Hoje o seed inicial cria e vincula features como:

- `WORKOUTS`
- `CHECKIN`
- `SCHEDULE`
- `MEDIA`
- `USER_MGMT`

## Seed e bootstrap

Na inicializacao da aplicacao:

1. migrations sao aplicadas
2. tenant demo `alpha-demo` e garantido
3. usuario admin inicial e garantido
4. features base sao criadas e associadas ao tenant demo

## Estado atual da arquitetura

### Consolidado

- base multi-tenant
- auth com refresh token
- cache Redis
- storage R2
- tenant config e features
- usuarios
- media
- exercicios
- check-in
- classes/agendas

### Em progresso

- workouts e composicao treino-exercicio
- autorizacao mais fina por role
- modulos operacionais de academia como check-in e agenda

## Pendencias tecnicas relevantes

- middleware central de tenant
- politica de autorizacao por role/permissao
- cobertura automatizada de testes
- padronizacao de paginacao para todos os modulos
- endurecimento de configuracoes sensiveis por ambiente
