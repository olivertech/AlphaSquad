# 🚀 AlphaSquad Platform

**AlphaSquad** é uma plataforma SaaS **white-label para academias**, focada em **engajamento, retenção de alunos e experiência mobile**.

A proposta é permitir que academias tenham **seu próprio aplicativo personalizado**, sustentado por uma arquitetura **multi-tenant escalável e performática**.

---

## 🎯 Objetivo do Projeto

Permitir que academias:

* Tenham um app próprio com sua marca (white-label)
* Engajem alunos com treinos, desafios e comunicação
* Aumentem retenção e frequência
* Centralizem operações mobile

---

## 🧠 Visão do Produto

> **Plataforma de engajamento para academias com foco em retenção de alunos**

---

## 🧱 Arquitetura

Abordagem **clean minimalista**, com foco em:

* Simplicidade
* Escalabilidade
* Performance
* Manutenibilidade

### Estrutura da Solution

```
AlphaSquad/
│
├── src/
│   ├── AlphaSquad.Api              → API principal (ASP.NET Core)
│   ├── AlphaSquad.Infrastructure   → Dados, EF Core, Dapper, Redis, Storage
│   └── AlphaSquad.Shared           → Contratos e modelos compartilhados
│
└── AlphaSquad.sln
```

---

## 🧩 Abordagem Arquitetural

* **Vertical Slice (Feature-first)**
* **Minimal APIs**
* **EF Core** → comandos (INSERT, UPDATE, DELETE)
* **Dapper** → consultas (SELECT)
* **Redis** → cache distribuído
* **Cloudflare R2** → armazenamento de arquivos
* **Multi-tenant (TenantId)**

---

## 🏗️ Stack Tecnológica

### Backend

* .NET 10
* ASP.NET Core (Minimal APIs)
* Entity Framework Core
* Dapper
* PostgreSQL
* Redis (Docker)
* Cloudflare R2 (S3-compatible storage)

### Infraestrutura

* VPS (Hostinger)
* Docker
* Cloudflare (Storage)

### Mobile (futuro)

* .NET MAUI

---

# ⚡ Cache Distribuído com Redis

### Estratégia

```text
Cache-aside (lazy loading)
```

### Fluxo

1. API consulta Redis
2. Cache hit → retorna imediatamente
3. Cache miss → consulta banco
4. Persiste no Redis com TTL
5. Retorna resposta

---

### 🔁 Invalidação

```csharp
await cache.RemoveAsync(CacheKeys.TenantConfig(slug));
```

---

### 🔑 Padrão de chave

```text
AlphaSquad:tenant-config:{slug}
```

---

### 🧪 Monitoramento

Ferramenta utilizada:

👉 **RedisInsight**

Permite:

* Visualização de chaves
* Inspeção de payload JSON
* Análise de TTL
* Debug de cache

---

# ☁️ Armazenamento com Cloudflare R2

O projeto utiliza **Cloudflare R2** para armazenamento de arquivos (uploads de mídia).

---

## 📦 Estratégia

* Compatível com API S3
* Sem custo de egress (vantagem relevante)
* Organização por tenant

---

## 📁 Estrutura lógica no storage

```text
tenants/{tenantSlug}/media/{guid}_{fileName}
```

---

## 🔐 Segurança

* Upload autenticado via JWT
* Tenant isolado via claims
* Bucket privado
* URL pública configurável

---

## ⚙️ Configuração (appsettings.json)

```json
"Storage": {
  "Provider": "CloudflareR2",
  "Endpoint": "https://<accountid>.r2.cloudflarestorage.com",
  "AccessKey": "SEU_ACCESS_KEY",
  "SecretKey": "SEU_SECRET_KEY",
  "BucketName": "alphasquad-media",
  "PublicBaseUrl": "https://SEU_DOMINIO_PUBLICO"
}
```

---

## 🔄 Fluxo de Upload

1. Cliente envia `multipart/form-data`
2. API valida arquivo
3. Extrai TenantId e TenantSlug do JWT
4. Faz upload para R2
5. Persiste metadata no banco
6. Retorna URL pública

---

# 🧩 Módulo de Mídia (Media)

## Funcionalidades implementadas

* Upload de arquivos
* Atualização de arquivo
* Exclusão de mídia
* Listagem por tenant

---

## 📡 Endpoints

### Upload

```http
POST /api/media/upload
```

---

### Atualizar arquivo

```http
PUT /api/media/{id}/file
```

---

### Remover mídia

```http
DELETE /api/media/{id}
```

---

### Listar mídias do tenant

```http
GET /api/media
```

---

## 🧠 Persistência

Tabela:

```text
TenantMedias
```

Campos:

* Id
* TenantId
* FileName
* ContentType
* Size
* StorageKey
* Url
* CreatedAt

---

## 🔗 Integridade

Relacionamento:

```text
Tenant (1) → (N) TenantMedias
```

---

# 👤 Módulo de Usuários (Users)

## Funcionalidades implementadas

* CRUD completo de usuários
* Isolamento por tenant
* Validação de e-mail único por tenant
* Hash de senha

---

## 📡 Endpoints

### Listar usuários

```http
GET /api/users
```

---

### Buscar por ID

```http
GET /api/users/{id}
```

---

### Criar usuário

```http
POST /api/users
```

---

### Atualizar usuário

```http
PUT /api/users/{id}
```

---

### Remover usuário

```http
DELETE /api/users/{id}
```

---

## 🔐 Segurança

* Todos endpoints protegidos por JWT
* TenantId extraído do token
* Isolamento garantido por query

---

# 🧠 Multi-Tenancy

Isolamento via:

```text
TenantId (GUID)
```

Aplicado em:

* Queries (WHERE TenantId)
* Storage (path por tenant)
* JWT (claims)

---

# 🔑 Autenticação

* JWT
* Roles:

  * Admin
  * Teacher
  * Student

---

# 🗄️ Banco de Dados

### Entidades

* Tenants
* Users
* TenantMedias

---

# 🚀 Como rodar o projeto

### 1. Clonar

```bash
git clone https://github.com/seu-usuario/alphasquad.git
cd alphasquad
```

---

### 2. Subir Redis

```bash
docker run -d -p 6379:6379 redis
```

---

### 3. Configurar PostgreSQL

```text
Database: alphasquad_db
```

---

### 4. Migrations

```bash
dotnet ef database update \
--project src/AlphaSquad.Infrastructure \
--startup-project src/AlphaSquad.Api
```

---

### 5. Executar API

```bash
dotnet run --project src/AlphaSquad.Api
```

---

### 6. Swagger

```text
https://localhost:7054/swagger
```

---

# 📦 Padrões utilizados

* Clean Architecture (minimalista)
* Vertical Slice Architecture
* Minimal APIs
* Cache-aside pattern
* Multi-tenant isolation
* S3-compatible storage abstraction

---

# ⚠️ Status do Projeto

🚧 Em desenvolvimento

### Módulos concluídos

* [x] Estrutura base
* [x] EF Core + PostgreSQL
* [x] Redis (cache distribuído)
* [x] Cloudflare R2 (upload de mídia)
* [x] Módulo de mídia (CRUD completo)
* [x] Módulo de usuários (CRUD completo)

---

# 🔮 Próximos passos

### Backend

* [ ] Refresh Token
* [ ] Change Password
* [ ] Middleware de Tenant
* [ ] Permissões por Role
* [ ] Paginação e filtros

### Produto

* [ ] Workouts
* [ ] Check-in
* [ ] Progresso
* [ ] Ranking
* [ ] Notificações

---

# 💡 Diferenciais

* Arquitetura moderna com .NET 10
* Multi-tenant desde o core
* Cache distribuído com Redis
* Upload escalável com Cloudflare R2
* Estrutura pronta para SaaS real
* Forte apelo para portfólio técnico

---

# 👨‍💻 Autor

Marcelo Oliveira
Senior .NET Developer
🔗 [https://www.linkedin.com/in/marcelo-de-oliveira-60b26514/](https://www.linkedin.com/in/marcelo-de-oliveira-60b26514/)

