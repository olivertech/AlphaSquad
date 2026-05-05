Seu README já está sólido. Vou fazer um upgrade focado em:

* Mostrar **uso real de Redis (não só “tem Redis”)**
* Demonstrar **maturidade arquitetural**
* Incluir **RedisInsight como ferramenta profissional**
* Deixar mais “vendável” para recrutador/dev técnico

Segue versão atualizada 👇

---

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
│   ├── AlphaSquad.Infrastructure   → Dados, EF Core, Dapper, Redis
│   └── AlphaSquad.Shared           → Contratos e modelos compartilhados
│
└── AlphaSquad.sln
```

---

## 🧩 Abordagem Arquitetural

* **Vertical Slice (Feature-first)**
* **EF Core** → comandos (INSERT, UPDATE, DELETE)
* **Dapper** → consultas (SELECT)
* **Redis** → cache distribuído
* **Multi-tenant (TenantId)**

---

## 🏗️ Stack Tecnológica

### Backend

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* Dapper
* PostgreSQL
* Redis (Docker)

### Infraestrutura

* VPS (Hostinger)
* Docker
* Armazenamento local (fase inicial)

### Mobile (futuro)

* .NET MAUI

---

## ⚡ Cache Distribuído com Redis

O projeto utiliza Redis para otimizar performance em leituras frequentes.

### Estratégia adotada

```text
Cache-aside (lazy loading)
```

### Fluxo

1. Requisição consulta Redis
2. Se existir → retorna (cache hit)
3. Se não existir → busca no banco (cache miss)
4. Armazena no Redis com TTL
5. Retorna resposta

---

### 🔁 Invalidação de Cache

Implementada via eventos de escrita:

```text
PUT → invalida cache
GET → reidrata cache
```

Exemplo:

```csharp
await cache.RemoveAsync(CacheKeys.TenantConfig(slug));
```

---

### 🧠 Benefícios

* Redução de carga no banco
* Resposta mais rápida
* Escalabilidade horizontal
* Isolamento por tenant

---

### 🔑 Padrão de chave

```text
AlphaSquad:tenant-config:{slug}
```

---

### ⏱️ TTL (Time-To-Live)

* Controle de expiração configurável
* Evita dados obsoletos permanentes
* Mantém Redis como cache, não fonte de verdade

---

## 🧪 Monitoramento com RedisInsight

Para inspeção e validação do cache, o projeto utiliza:

👉 **RedisInsight**

### Permite:

* Visualizar chaves em tempo real
* Inspecionar valores JSON
* Monitorar TTL
* Validar cache hits/misses
* Debug de comportamento do sistema

Exemplo de chave visualizada:

```text
AlphaSquad:tenant-config:alpha-demo
```

---

## 🧠 Conceito White-Label

* Backend único
* Banco único (multi-tenant)
* App customizado por academia:

  * Logo
  * Cores
  * Nome

---

## 🔐 Multi-Tenancy

Isolamento via:

```text
TenantId
```

Todas as entidades são vinculadas a um tenant.

---

## 🔑 Autenticação (em desenvolvimento)

* JWT (JSON Web Token)
* Roles:

  * Admin
  * Teacher
  * Student
* Isolamento por tenant

---

## 🗄️ Banco de Dados

### Entidades iniciais

* Tenants
* Users

### Estratégia

* Model-first
* Migrations via EF Core
* Versionamento incremental

---

## 🚀 Como rodar o projeto

### 1. Clonar

```bash
git clone https://github.com/seu-usuario/alphasquad.git
cd alphasquad
```

---

### 2. Subir Redis (Docker)

```bash
docker run -d -p 6379:6379 redis
```

---

### 3. Configurar PostgreSQL

```text
Database: alphasquad_db
```

---

### 4. Configurar connection strings

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=alphasquad_db;Username=postgres;Password=SUA_SENHA"
},
"Redis": {
  "ConnectionString": "localhost:6379",
  "InstanceName": "AlphaSquad:"
}
```

---

### 5. Rodar migrations

```bash
dotnet ef database update \
--project src/AlphaSquad.Infrastructure \
--startup-project src/AlphaSquad.Api
```

---

### 6. Executar API

```bash
dotnet run --project src/AlphaSquad.Api
```

---

### 7. Swagger

```text
https://localhost:7054/swagger
```

---

## 📦 Padrões utilizados

* Clean Architecture (minimalista)
* Vertical Slice Architecture
* Cache-aside pattern
* Separation of concerns
* Dependency Injection

---

## ⚠️ Status do Projeto

🚧 Em desenvolvimento inicial

### Módulos

* [x] Estrutura base
* [x] EF Core + PostgreSQL
* [x] Redis (cache distribuído)
* [x] Cache-aside + invalidação
* [ ] Autenticação JWT
* [ ] Multi-tenant middleware
* [ ] App MAUI

---

## 🔮 Roadmap

### Fase 1

* Autenticação
* Tenant
* Users

### Fase 2

* Workouts
* Check-in
* Progresso

### Fase 3

* Desafios
* Ranking
* Notificações

### Fase 4

* Social
* Agenda

---

## 💡 Diferenciais

* Arquitetura simples e escalável
* Multi-tenant desde o início
* Cache distribuído com Redis
* Preparado para mobile offline-first
* Base para expansão com IA

---

## 👨‍💻 Autor

Marcelo Oliveira
Senior .NET Developer
🔗 [https://www.linkedin.com/in/marcelo-de-oliveira-60b26514/](https://www.linkedin.com/in/marcelo-de-oliveira-60b26514/)

