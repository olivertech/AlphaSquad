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
- Store V1 com catalogo, variantes, feed cursor-based e pedidos para retirada presencial
- Rede social interna com post, feed, likes e comentarios simples
- Mural de eventos com feed do tenant e participacao outdoor
- Base inicial da gamificacao com regras, eventos, saldo e ranking mensal
- Swagger com descricoes curtas nos endpoints principais

### Modulos em consolidacao

- nenhuma frente estrutural critica pendente para a V1

### Proximas frentes core do produto

- Loja de produtos personalizados da academia com Stripe
- Gamificacao com pontuacao, ranking mensal e recompensas
- Rede social interna da academia
- Multi-idioma com traducao dinamica de conteudo no backend em uma V2

## Diferenciais estrategicos planejados

### 1. Loja interna da academia

Modulo para anuncio e venda de produtos personalizados do tenant, com foco em experiencia mobile e navegacao por feed infinito.

Capacidades atuais na V1:

- listagem paginada para dashboards
- feed cursor-based para scroll infinito no app
- cards ou lista linear de produtos
- fotos, descricao, variacoes de cor e tamanho
- controle de estoque e disponibilidade
- fluxo de pedido pelo app com retirada e pagamento presencial na academia na V1
- integracao com Stripe em etapa posterior, quando a operacao comercial estiver pronta

Regra de negocio atual para a V1:

- o aluno escolhe o produto e confirma o pedido no app
- a academia separa o item para retirada
- o pagamento acontece localmente na administracao da academia
- a retirada presencial reforca o retorno do aluno ao ambiente fisico da academia

### 2. Rede social do tenant

Feed social interno onde todos os usuarios da mesma academia visualizam as publicacoes de alunos e professores.

Capacidades atuais:

- posts com imagem e descricao curta
- feed global por tenant
- feed cursor-based para scroll infinito no app
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

Regra de negocio central:

- apenas alunos participam da gamificacao
- professores e administradores nao entram no ranking
- cada evento relevante gera pontos de acordo com uma tabela configuravel de regras

Exemplos de eventos previstos:

- check-in
- postagem na rede da academia
- participacao em auloes
- participacao em eventos externos
- compra de produtos na loja
- pagamento em dia da mensalidade
- renovacao do plano

Status atual:

- regras de pontuacao por tenant
- eventos pontuados por aluno
- razao de pontos acumulados
- ranking mensal por tenant
- fechamento mensal com historico de vencedores
- integracao inicial com `check-in`, compra paga localmente na loja, renovacao de plano e auloes

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
- `PUT /api/profile/me/password`
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

Capacidades atuais:

- feed paginado por tenant
- feed cursor-based para scroll infinito no app
- publicacoes exclusivas da gestao
- suporte a imagem, descricao, local e periodo do evento
- eventos outdoor com confirmacao de participacao pelo aluno
- integracao com gamificacao via `OutdoorEventParticipation`
- bloqueio do modulo por feature opcional `EVENTS`

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

## Padroes de listagem

O projeto agora convive oficialmente com dois contratos de listagem:

- `PagedResponse<T>` para dashboards administrativos, consultas tradicionais e navegacao por pagina
- `CursorFeedResponse<T>` para feeds grandes com scroll infinito no app

Uso atual:

- `page-based`: `Store`, `Events`, `Social`, `Classes`, `Checkins` e `Media`
- `cursor-based`: `Store`, `Events` e `Social`

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

## Modulos opcionais por tenant

O AlphaSquad foi desenhado para que partes do produto possam ou nao fazer parte do pacote contratado pela academia.

- o tenant recebe apenas as features contratadas
- o backend consulta `Feature` + `TenantFeature` para validar modulos opcionais
- o modulo de `Events` ja nasce seguindo esse modelo com a feature `EVENTS`
- a mesma base pode ser reutilizada por modulos futuros como `Social`, `Store` e novas camadas premium

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
- `AdminOrTeacher`: policy base ainda disponivel para operacoes gerenciais compartilhadas, quando a regra de negocio permitir

### 4. Protecao de endpoints por contexto

Os endpoints nao sao protegidos apenas por login. Eles tambem seguem regras de acesso por natureza da operacao.

- leitura administrativa de usuarios restrita a gestao
- gestao de tenant restrita a `Admin`
- upload e administracao de midias restritos a gestao
- criacao e manutencao de exercicios restritas a `Admin`
- criacao e manutencao de treinos restritas a `Admin`
- gestao de aulas restrita a `Admin`
- gestao de eventos restrita a `Admin`
- visao consolidada de check-ins restrita a `Admin`

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
- `GET /api/classes/{id}/bookings`
- `GET /api/classes/bookings/by-user/{userId}`

Observacao:
- aulas podem ser marcadas como `aulão` para gerar pontuacao especial no booking do aluno

### Profile

- `GET /api/profile/me`
- `PUT /api/profile/me`
- `PUT /api/profile/me/email`
- `PUT /api/profile/me/password`
- `PUT /api/profile/me/photo`
- `DELETE /api/profile/me/photo`

### Plans

- `GET /api/plans`
- `POST /api/plans`
- `PUT /api/plans/{id}`
- `POST /api/plans/{id}/assign`
- `POST /api/plans/payments`
- `GET /api/plans/users/{userId}/history`
- `GET /api/plans/inactive-users`

### Store

- `GET /api/store/products`
- `GET /api/store/products/feed`
- `GET /api/store/products/{id}`
- `POST /api/store/products`
- `PUT /api/store/products/{id}`
- `DELETE /api/store/products/{id}`
- `POST /api/store/products/{id}/variants`
- `PUT /api/store/products/{productId}/variants/{variantId}`
- `DELETE /api/store/products/{productId}/variants/{variantId}`
- `POST /api/store/orders`
- `GET /api/store/orders/me`
- `GET /api/store/orders/me/{id}`
- `GET /api/store/orders`
- `PUT /api/store/orders/{id}/status`

### Social

- `GET /api/social/posts`
- `GET /api/social/posts/feed`
- `GET /api/social/posts/{id}`
- `POST /api/social/posts`
- `POST /api/social/posts/{id}/like`
- `DELETE /api/social/posts/{id}/like`
- `GET /api/social/posts/{id}/comments`
- `POST /api/social/posts/{id}/comments`

### Gamification

- `GET /api/gamification/me`
- `GET /api/gamification/ranking/monthly`
- `GET /api/gamification/winners/history`
- `GET /api/gamification/rules`
- `POST /api/gamification/rules`
- `PUT /api/gamification/rules/{id}`
- `POST /api/gamification/ranking/monthly/close`

### Events

- `GET /api/events`
- `GET /api/events/feed`
- `GET /api/events/{id}`
- `POST /api/events`
- `PUT /api/events/{id}`
- `DELETE /api/events/{id}`
- `POST /api/events/{id}/participate`

Proxima etapa prevista da loja:

- `PaymentTransaction`
- integracao com Stripe
- webhook de confirmacao de pagamento
- beneficios comerciais conectados a gamificacao

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
- `MembershipPayment`
- `Product`
- `ProductVariant`
- `StoreOrder`
- `StoreOrderItem`
- `SocialPost`
- `SocialPostLike`
- `SocialPostComment`
- `AcademyEvent`
- `AcademyEventParticipation`
- `GamificationEventRule`
- `UserGamificationEvent`
- `PointsLedger`
- `MonthlyStudentRanking`

Entidades estrategicas previstas para as proximas fases:

- `Product`
- `ProductVariant`
- `PaymentTransaction`
- `SocialPost`
- `PostLike`
- `PostComment`
- `MembershipPlanSnapshot`

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
- regras base de gamificacao para o tenant demo

## Infra local

O repositorio possui `docker-compose.yml` para subir o Redis localmente:

```bash
docker compose up -d
```

O PostgreSQL atualmente deve estar disponivel separadamente, conforme a configuracao do ambiente local.

## Configuracao por ambiente

O projeto agora segue esta diretriz:

- `src/AlphaSquad.Api/appsettings.json` guarda apenas placeholders seguros
- `src/AlphaSquad.Api/appsettings.Development.json` traz defaults locais de desenvolvimento
- segredos reais devem ficar em `user-secrets` ou variaveis de ambiente

Como o projeto possui `UserSecretsId`, voce pode configurar localmente com:

```bash
dotnet user-secrets --project src/AlphaSquad.Api set "Jwt:SecretKey" "sua-chave-local"
dotnet user-secrets --project src/AlphaSquad.Api set "Storage:AccessKey" "seu-access-key"
dotnet user-secrets --project src/AlphaSquad.Api set "Storage:SecretKey" "seu-secret-key"
```

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
