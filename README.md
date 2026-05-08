# AlphaSquad Platform

AlphaSquad e uma plataforma SaaS white-label para academias, com foco em engajamento, retencao de alunos e operacao mobile-first.

A proposta do produto e permitir que cada academia tenha seu proprio aplicativo com identidade visual, modulos habilitaveis por tenant e uma base tecnica preparada para crescimento.

## Visao do produto

- White-label para academias
- Multi-tenant desde o backend
- Foco em retencao, frequencia e experiencia do aluno
- Base preparada para app mobile no futuro

## Estado atual do projeto

O backend atual ja possui uma base funcional consistente para autenticacao, gestao de tenant, usuarios e midia, alem de modulos iniciais de dominio para exercicios e treinos.

### Modulos consolidados

- Autenticacao com JWT
- Refresh token rotation
- Endpoint de sessao atual (`/api/auth/me`)
- Change password com revogacao de sessoes
- Logout com revogacao de refresh tokens
- CRUD de usuarios com isolamento por tenant
- Tenant config por slug com cache Redis
- Tenant atual e features habilitadas
- Upload e gestao de logo do tenant
- CRUD de midias com Cloudflare R2
- CRUD de exercicios
- Check-in com consultas por usuario e por tenant
- CRUD basico de aulas/agendas
- Booking e unbooking de aulas

### Modulos em evolucao

- Workouts/treinos
- Associacao treino-exercicio
- Permissoes por role
- Reservas de aulas
- Fluxos de engajamento do aluno

## Arquitetura em resumo

- .NET 10
- ASP.NET Core Minimal APIs
- Vertical Slice Architecture
- EF Core para escrita
- Dapper para leitura
- PostgreSQL como fonte principal de dados
- Redis para cache distribuido
- Cloudflare R2 para storage de arquivos

## Estrutura da solution

```text
AlphaSquad/
|
+-- src/
|   +-- AlphaSquad.Api
|   +-- AlphaSquad.Infrastructure
|   +-- AlphaSquad.Shared
|
+-- AlphaSquad.slnx
```

## Multi-tenancy

O isolamento e baseado em `TenantId` e aparece em tres camadas principais:

- Banco de dados: filtros por tenant nas queries
- JWT: claims com `tenant_id` e `tenant_slug`
- Storage: paths segregados por tenant

## Endpoints principais

### Auth

- `POST /api/auth/login`
- `GET /api/auth/me`
- `POST /api/auth/refresh`
- `POST /api/auth/change-password`
- `POST /api/auth/logout`

### Tenants

- `GET /api/tenants/by-slug/{slug}`
- `PUT /api/tenants/{id}`
- `GET /api/tenants/current`
- `PUT /api/tenants/current/logo`
- `GET /api/tenants/current/features`

### Media

- `POST /api/media/upload`
- `GET /api/media`
- `GET /api/media/{id}`
- `PUT /api/media/{id}/file`
- `DELETE /api/media/{id}`

### Users

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

### Exercises

- `GET /api/exercises`
- `GET /api/exercises/{id}`
- `POST /api/exercises`
- `PUT /api/exercises/{id}`
- `DELETE /api/exercises/{id}`

### Checkins

- `POST /api/checkins`
- `GET /api/checkins/me`
- `GET /api/checkins/tenant`

### Classes

- `GET /api/classes`
- `GET /api/classes/{id}`
- `POST /api/classes`
- `PUT /api/classes/{id}`
- `DELETE /api/classes/{id}`
- `POST /api/classes/{id}/book`
- `DELETE /api/classes/{id}/book`

## Banco de dados

Entidades ja presentes no projeto:

- `Tenant`
- `AppUser`
- `RefreshToken`
- `TenantMedia`
- `Feature`
- `TenantFeature`
- `Exercise`
- `Workout`
- `WorkoutExercise`
- `CheckIn`
- `GymClass`
- `ClassBooking`

## Seed inicial

Ao subir a aplicacao, o projeto aplica migrations e garante a existencia de:

- tenant demo `alpha-demo`
- usuario admin `admin@alphasquad.app`
- features base vinculadas ao tenant demo

## Infra local

O repositório possui `docker-compose.yml` para subir o Redis localmente:

```bash
docker compose up -d
```

O PostgreSQL atualmente deve estar disponivel separadamente, conforme a connection string configurada em `src/AlphaSquad.Api/appsettings.json`.

## Como rodar

### 1. Subir Redis

```bash
docker compose up -d
```

### 2. Garantir PostgreSQL local

Exemplo atual de configuracao:

```text
Host=localhost;Port=5432;Database=AlphaSquad;Username=postgres;Password=123
```

### 3. Executar a API

```bash
dotnet run --project src/AlphaSquad.Api
```

### 4. Abrir o Swagger

```text
https://localhost:7054/swagger
```
