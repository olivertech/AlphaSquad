# 🚀 AlphaSquad Platform

**AlphaSquad** é uma plataforma SaaS white-label voltada para academias, focada em **engajamento, retenção de alunos e experiência mobile**.

A proposta do projeto é permitir que academias tenham **seu próprio aplicativo personalizado**, mantendo uma base única, escalável e multi-tenant no backend.

---

## 🎯 Objetivo do Projeto

Criar uma plataforma que permita academias:

* Ter um app próprio com sua marca (white-label)
* Engajar alunos com treinos, desafios e comunicação
* Aumentar retenção e frequência
* Centralizar operações mobile

---

## 🧠 Visão do Produto

O AlphaSquad não é apenas um app, mas sim uma:

> **Plataforma de engajamento para academias com foco em retenção de alunos**

---

## 🧱 Arquitetura

O projeto segue uma abordagem **clean minimalista**, com foco em:

* Simplicidade
* Escalabilidade
* Manutenibilidade
* Performance

### Estrutura da Solution

```
AlphaSquad/
│
├── src/
│   ├── AlphaSquad.Api              → API principal (ASP.NET Core)
│   ├── AlphaSquad.Infrastructure   → Acesso a dados, EF Core, Dapper
│   └── AlphaSquad.Shared           → Contratos, enums e modelos compartilhados
│
└── AlphaSquad.sln
```

---

## 🧩 Abordagem Arquitetural

* **Vertical Slice (Feature-first)**
* **EF Core** → comandos (INSERT, UPDATE, DELETE)
* **Dapper** → consultas (SELECT)
* **Multi-tenant com banco compartilhado (TenantId)**

---

## 🏗️ Stack Tecnológica

### Backend

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* Dapper
* PostgreSQL
* Redis (rodando em container)

### Infraestrutura

* VPS (Hostinger)
* Docker
* Armazenamento local (fase inicial)

### Mobile (futuro)

* .NET MAUI

---

## 🧠 Conceito White-Label

A plataforma foi projetada para atender múltiplas academias:

* Backend único
* Banco único (multi-tenant)
* Aplicativo customizado por academia:

  * Logo
  * Cores
  * Nome

---

## 🔐 Multi-Tenancy

O isolamento de dados é feito via:

```text
TenantId
```

Todas as entidades possuem vínculo com um tenant.

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

### Principais entidades iniciais

* Tenants
* Users

### Estratégia

* Model-first
* Migrations via EF Core
* Versionamento incremental

---

## 🚀 Como rodar o projeto

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/alphasquad.git
cd alphasquad
```

---

### 2. Configurar o banco PostgreSQL

Certifique-se de ter um banco criado:

```text
Database: alphasquad_db
```

---

### 3. Configurar connection string

Arquivo:

```text
src/AlphaSquad.Api/appsettings.json
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=alphasquad_db;Username=postgres;Password=SUA_SENHA"
}
```

---

### 4. Rodar migrations

```bash
dotnet ef database update \
--project src/AlphaSquad.Infrastructure \
--startup-project src/AlphaSquad.Api
```

---

### 5. Executar a API

```bash
dotnet run --project src/AlphaSquad.Api
```

---

### 6. Acessar Swagger

```text
https://localhost:7054/swagger
```

---

## 📦 Padrões utilizados

* Clean Architecture (minimalista)
* Feature-based organization
* Separation of concerns
* Dependency Injection

---

## ⚠️ Status do Projeto

🚧 Em desenvolvimento inicial

Módulos atuais:

* [x] Estrutura base da solution
* [x] Configuração EF Core + PostgreSQL
* [x] Migrations iniciais
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
* Agenda de aulas

---

## 💡 Diferenciais

* Arquitetura simples e escalável
* Multi-tenant desde o início
* Preparado para offline-first (mobile)
* Integração futura com IA

---

## 👨‍💻 Autor

Marcelo Oliveira
Senior .NET Developer
[LinkedIn](https://www.linkedin.com/in/marcelo-de-oliveira-60b26514/)

---

## 📄 Licença

Este projeto está sob a licença MIT.
