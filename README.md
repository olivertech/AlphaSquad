# AlphaSquad Platform

AlphaSquad e uma plataforma SaaS white-label para academias, com foco em engajamento, retencao de alunos, operacao mobile-first e construcao de comunidade.

A proposta do produto e permitir que cada academia tenha seu proprio aplicativo com identidade visual, modulos habilitaveis por tenant e uma base tecnica preparada para crescimento incremental.

## Visao do produto

- White-label para academias
- Multi-tenant desde o backend
- Foco em retencao, frequencia e experiencia do aluno
- Base preparada para app mobile no futuro
- Comunidade digital interna entre alunos, professores e gestao
- Gamificacao como diferencial central do ecossistema

## Momento atual do projeto

O backend atual ja possui uma base funcional consistente para autenticacao, gestao de tenant, usuarios, midia e modulos operacionais da academia.

### Modulos consolidados

- Autenticacao com JWT
- Refresh token rotation
- Endpoint de sessao atual (`/api/auth/me`)
- Change password com revogacao de sessoes
- Logout com revogacao de refresh tokens
- Policies base de acesso por role (`AdminOnly` e `AdminOrTeacher`)
- CRUD de usuarios com isolamento por tenant
- Tenant config por slug com cache Redis
- Tenant atual e features habilitadas
- Upload e gestao de logo do tenant
- CRUD de midias com Cloudflare R2
- CRUD de exercicios
- Check-in com consultas por usuario e por tenant
- CRUD basico de aulas/agendas
- Booking e unbooking de aulas
- Profile do usuario autenticado com dados basicos e foto
- Dominio inicial de planos com atribuicao de plano ativo por usuario
- Historico de planos por usuario com motivo de status e auditoria de troca
- Consultas de retencao para usuarios sem plano ativo e usuarios sem check-in recente
- Base inicial da loja com catalogo de produtos e variantes
- Swagger com descricoes curtas nos endpoints principais

### Modulos em consolidacao

- Workouts/treinos
- Associacao treino-exercicio
- Padronizacao de paginacao e filtros

### Proximas frentes core do produto

- Loja de produtos personalizados da academia com Stripe
- Rede social interna da academia
- Gamificacao com pontuacao, ranking mensal e recompensas
- Mural de eventos e acoes outdoor promovidas pela academia
- Multi-idioma com traducao dinamica de conteudo no backend em uma V2

## Diferenciais estrategicos planejados

### 1. Loja interna da academia

Modulo para anuncio e venda de produtos personalizados do tenant, com foco em experiencia mobile e navegacao por feed infinito.

Capacidades previstas:

- listagem paginada com scroll infinito
- cards ou lista linear de produtos
- fotos, descricao, variacoes de cor e tamanho
- controle de estoque e disponibilidade
- fluxo de compra com Stripe

### 2. Rede social do tenant

Feed social interno onde todos os usuarios da mesma academia visualizam as publicacoes de alunos e professores.

Capacidades previstas:

- posts com imagem e descricao curta
- feed global por tenant
- likes
- comentarios simples em nivel unico
- sem threads ou respostas encadeadas nesta fase

### 3. Gamificacao

Camada transversal de engajamento que transforma a participacao no ecossistema em pontos e reconhecimento.

Capacidades previstas:

- pontuacao por acoes relevantes
- ranking mensal por tenant
- premiacao para top 3 do mes
- integracao futura com beneficios da loja e mensalidade

### 4. Profile do usuario

Area pessoal para concentrar dados basicos da conta e relacao do usuario com a academia.

Capacidades previstas:

- nome, email e username
- foto de perfil
- troca de senha
- plano ativo
- acesso aos dados pessoais do usuario autenticado

Status atual:

- `GET /api/profile/me`
- `PUT /api/profile/me`
- `PUT /api/profile/me/email`
- `PUT /api/profile/me/photo`
- `DELETE /api/profile/me/photo`
- validacao mais forte de formato de e-mail e username
- `ActivePlan` agora ligado a um dominio real de planos
- integracao com `login` e `/api/auth/me` para devolver dados de profile junto da sessao

### 4.1. Planos, historico e retencao

O dominio de planos deixou de ser apenas um campo informativo e passou a sustentar regras reais de acesso, historico e reengajamento.

Capacidades atuais:

- apenas usuarios com plano ativo podem acessar a plataforma
- um unico plano ativo por usuario por vez
- troca de plano preserva o historico anterior
- historico com motivo do status e usuario responsavel pela alteracao
- consulta de usuarios sem plano ativo ha X dias
- consulta de usuarios com plano vigente que estao ha X dias sem check-in

### 5. Mural de eventos da academia

Canal institucional usado pela gestao da academia para divulgar eventos, acoes sociais, apoiadores, registros de atividades e iniciativas outdoor.

Capacidades previstas:

- feed visual leve e atrativo
- publicacoes exclusivas da gestao
- imagens e descricao
- navegacao vertical com carregamento incremental

## Arquitetura em resumo

- .NET 10
- ASP.NET Core Minimal APIs
- Vertical Slice Architecture
- EF Core para escrita
- Dapper para leitura
- PostgreSQL como fonte principal de dados
- Redis para cache distribuido
- Cloudflare R2 para storage de arquivos
- Stripe planejado para pagamentos

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

Toda nova feature deve preservar esse isolamento em leituras, gravacoes, integracoes e arquivos.

## Seguranca em camadas

O AlphaSquad foi estruturado para oferecer uma base segura para academias que desejam operar seu proprio aplicativo com segregacao forte de dados e controles de acesso coerentes com ambientes multi-tenant.

### 1. Isolamento por tenant

Cada academia opera em seu proprio contexto logico de dados.

- queries filtradas por `TenantId`
- claims de tenant no JWT
- arquivos segregados por tenant no storage
- validacoes adicionais em relacionamentos entre entidades

### 2. Autenticacao e sessao

O acesso a areas protegidas depende de autenticacao por JWT, com sessao reforcada por refresh token persistido e rotacionado.

- login com `tenant slug + email + password`
- access token com claims de usuario, role e tenant
- refresh token rotation
- logout com revogacao de tokens
- troca de senha com encerramento das sessoes ativas

### 3. Autorizacao por role

O backend ja possui uma camada base de autorizacao por perfil para separar operacoes de aluno, professor e gestao.

Perfis atuais:

- `Admin`
- `Teacher`
- `Student`

Policies atuais:

- `AdminOnly`: operacoes exclusivas de administracao do tenant
- `AdminOrTeacher`: operacoes de gestao academica compartilhadas entre administradores e professores

### 4. Protecao de endpoints por contexto

Os endpoints nao sao protegidos apenas por login. Eles tambem seguem regras de acesso por natureza da operacao.

- leitura administrativa de usuarios restrita a gestao
- gestao de tenant restrita a `Admin`
- upload e administracao de midias restritos a gestao
- criacao e manutencao de exercicios restritas a `Admin` e `Teacher`
- criacao e manutencao de treinos restritas a `Admin` e `Teacher`
- gestao de aulas restrita a `Admin` e `Teacher`
- visao consolidada de check-ins restrita a `Admin` e `Teacher`

### 5. Defesa em profundidade

Mesmo com autenticacao e roles, o sistema tambem aplica protecoes adicionais no proprio fluxo de dados.

- validacao de pertencimento ao tenant antes de gravar relacionamentos
- restricao de leitura de recursos por tenant
- remocao de metadados internos sensiveis dos contratos publicos
- validacoes de integridade para impedir cruzamento indevido entre tenants

Esse conjunto de camadas reforca que o AlphaSquad nao depende de um unico ponto de protecao. A seguranca foi pensada em autenticacao, autorizacao, isolamento de dados e integridade das relacoes.

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

### Workouts

- `GET /api/workouts`
- `GET /api/workouts/{id}`
- `POST /api/workouts`
- `PUT /api/workouts/{id}`
- `DELETE /api/workouts/{id}`
- `POST /api/workouts/{id}/assign-exercises`

### Checkins

- `POST /api/checkins`
- `GET /api/checkins/me`
- `GET /api/checkins/tenant`
- `GET /api/checkins/inactive-users`

### Classes

- `GET /api/classes`
- `GET /api/classes/{id}`
- `POST /api/classes`
- `PUT /api/classes/{id}`
- `DELETE /api/classes/{id}`
- `POST /api/classes/{id}/book`
- `DELETE /api/classes/{id}/book`

### Profile

- `GET /api/profile/me`
- `PUT /api/profile/me`
- `PUT /api/profile/me/email`
- `PUT /api/profile/me/photo`
- `DELETE /api/profile/me/photo`

### Plans

- `GET /api/plans`
- `POST /api/plans`
- `PUT /api/plans/{id}`
- `POST /api/plans/{id}/assign`
- `GET /api/plans/users/{userId}/history`
- `GET /api/plans/inactive-users`

### Store

- `GET /api/store/products`
- `GET /api/store/products/{id}`
- `POST /api/store/products`
- `PUT /api/store/products/{id}`
- `DELETE /api/store/products/{id}`
- `POST /api/store/products/{id}/variants`
- `PUT /api/store/products/{productId}/variants/{variantId}`
- `DELETE /api/store/products/{productId}/variants/{variantId}`

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
- `UserProfile`
- `MembershipPlan`
- `UserMembership`
- `Product`
- `ProductVariant`

Entidades estrategicas previstas para as proximas fases:

- `Product`
- `ProductVariant`
- `Order`
- `OrderItem`
- `PaymentTransaction`
- `SocialPost`
- `PostLike`
- `PostComment`
- `GamificationEvent`
- `PointsLedger`
- `MonthlyRanking`
- `UserProfile`
- `MembershipPlanSnapshot`
- `EventPost`

## Foco didatico

O projeto tambem tem objetivo didatico.

Por isso, todo novo codigo deve:

- trazer comentarios simples e objetivos em classes e metodos novos
- explicar regras de negocio importantes sem excesso de texto
- ajudar futuros profissionais a entender a feature pelo proprio codigo

## Seed inicial

Ao subir a aplicacao, o projeto aplica migrations e garante a existencia de:

- tenant demo `alpha-demo`
- usuario admin `admin@alphasquad.app`
- features base vinculadas ao tenant demo
- planos base `Basic`, `Advanced` e `Premium` para o tenant demo

## Infra local

O repositorio possui `docker-compose.yml` para subir o Redis localmente:

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

### 3. Gerar migration localmente quando houver mudancas de modelo

Exemplo:

```powershell
Add-Migration NomeDaMigration -Project AlphaSquad.Infrastructure -StartupProject AlphaSquad.Api
Update-Database -Project AlphaSquad.Infrastructure -StartupProject AlphaSquad.Api
```

Observacao:
As migrations devem ser geradas localmente pelo desenvolvedor responsavel pela rodada atual.

### 4. Executar a API

```bash
dotnet run --project src/AlphaSquad.Api
```

### 5. Abrir o Swagger

```text
https://localhost:7054/swagger
```
