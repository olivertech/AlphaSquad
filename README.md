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

O backend atual ja possui uma base funcional consistente para autenticacao, gestao de tenant, usuarios, midia, modulos operacionais da academia e contexto master da plataforma.

O produto agora tambem passa a contar com dois frontends web separados e com a base do contexto master da AlphaSquad, separando:

- operacao da academia (`tenant`)
- operacao global da plataforma (`backoffice`)

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
- Textos legais do tenant para Termos de Uso e Politica de Privacidade
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
- Endpoints master de autenticacao, perfil do owner e gestao global de academias
- Backoffice web separado para operacao da AlphaSquad
- Swagger com descricoes curtas nos endpoints principais
- Base de gestao global de academias com provisionamento de admin inicial, senha provisoria e upload de logo

### Modulos em consolidacao

- nenhuma frente estrutural critica pendente para a V1
- o isolamento multi-tenant agora conta com resolucao central de tenant no pipeline da API

### Proximas frentes core do produto

- Billing SaaS da AlphaSquad com landing page, Stripe e onboarding semiautomatico
- Loja de produtos personalizados da academia com evolucao comercial futura
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
- celular com DDD
- data de nascimento
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
- suporte a celular com DDD e data de nascimento
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
- dia de vencimento por usuario (`BillingDueDay`)
- registro administrativo/manual de pagamento de mensalidade
- consulta de situacao financeira do aluno no dashboard
- historico administrativo de pontuacao e eventos de gamificacao por usuario

Decisao de produto para a V1:

- o aluno nao paga mensalidade pelo app
- o tenant nao recebe checkout online de mensalidade no dashboard
- o pagamento real da mensalidade acontece fora do AlphaSquad
- a academia registra administrativamente o pagamento quando precisar refletir adimplencia, historico e gamificacao

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
- suporte a destaques internos de aniversariantes e vencedores da gamificacao no backend

Direcoes de produto ja consideradas e ja iniciadas no backend:

- eventos internos de mural para destacar aniversariantes do dia ou do periodo
- eventos internos de mural para divulgar vencedores da gamificacao
- possibilidade de um destaque ser apenas um banner temporario, sem obrigar data e hora de realizacao
- uso de imagem ilustrativa ou foto real enviada pela administracao
- destaque desses eventos no app dos alunos

### 5.1. Notificacoes do app

O produto tambem passa a considerar oficialmente uma central de notificacoes para o aplicativo do aluno.

Regras previstas:

- o app exibira um icone de sino com indicacao de novas notificacoes
- notificacoes poderao representar eventos, novas aulas, novos produtos, informes da academia e outros avisos institucionais
- o aluno acessara uma lista de notificacoes com filtros por `lidas`, `nao lidas` e `todas`
- ao abrir uma notificacao, o conteudo sera exibido em uma modal rolavel
- uma notificacao aberta deve ser marcada como lida e sair da lista de pendencias
- as notificacoes descritas nesta fase serao gerais, visiveis para todos os alunos com acesso ao app

## Aquisicao de clientes da AlphaSquad

Na V1, o billing do SaaS da AlphaSquad sera tratado separadamente do billing da academia para seus alunos.

Fluxo comercial decidido para a V1:

- a academia interessada acessa uma landing page publica da AlphaSquad
- escolhe um plano SaaS da plataforma
- realiza o pagamento via Stripe Checkout
- o pagamento confirmado gera um aviso claro ao sponsor/backoffice
- o onboarding da academia segue semiautomatico, com intervencao humana do dono da AlphaSquad

Decisoes da V1:

- Stripe entra apenas para a venda do SaaS AlphaSquad
- a criacao/liberacao do tenant nao sera totalmente automatica nesta fase
- o sponsor continua responsavel por revisar o cliente, confirmar branding e concluir o onboarding
- o auto-provisionamento total da academia fica como evolucao futura

## Arquitetura em resumo

- .NET 10
- ASP.NET Core Minimal APIs
- Vertical Slice Architecture
- EF Core para escrita
- Dapper para leitura
- PostgreSQL como fonte principal de dados
- Redis para cache distribuido
- Cloudflare R2 para storage de arquivos
- Stripe planejado para a frente comercial do SaaS e para billing transacional futuro

## Backoffice master da AlphaSquad

O ecossistema passa a conviver com dois contextos distintos:

- `tenant`: operacao diaria de cada academia
- `platform`: operacao global da AlphaSquad como dona do produto

No backend, isso significa:

- autenticacao separada em `/api/platform-auth/*`
- gestao global de academias em `/api/platform-tenants/*`
- usuario master proprio (`PlatformUser`)
- refresh token proprio do sponsor (`PlatformRefreshToken`)
- sem reuso de `tenant_id` no token do backoffice

Capacidades iniciais da camada master:

- login do sponsor
- sessao atual do sponsor
- refresh token do sponsor
- troca de senha do sponsor
- logout do sponsor
- perfil do sponsor com nome, foto e troca de senha
- listagem de academias
- detalhe de academia
- criacao de academia com:
- branding
- features contratadas
- admin inicial
- senha provisoria
- troca obrigatoria de senha no primeiro acesso do admin da academia
- atualizacao de academia
- upload e troca da logo da academia
- regeneracao de senha provisoria do admin principal
- criacao de admins adicionais do tenant pelo sponsor
- regeneracao de senha provisoria de qualquer admin do tenant
- trilha de auditoria das acoes master sobre cada academia

Comportamento atual da senha provisoria:

- a senha do admin inicial e gerada automaticamente pelo backend
- o sponsor nao informa essa senha na tela de criacao
- a senha provisoria fica disponivel na resposta da criacao e na tela de detalhes da academia
- o admin inicial precisa trocar a senha no primeiro acesso

## Padroes de listagem

O projeto agora convive oficialmente com dois contratos de listagem:

- `PagedResponse<T>` para dashboards administrativos, consultas tradicionais e navegacao por pagina
- `CursorFeedResponse<T>` para feeds grandes com scroll infinito no app

Uso atual:

- `page-based`: `Store`, `Events`, `Social`, `Classes`, `Checkins` e `Media`
- `cursor-based`: `Store`, `Events` e `Social`

## Padrao de feedback visual

No frontend administrativo do AlphaSquad, todo feedback de operacao deve seguir um unico padrao visual.

- avisos de sucesso, erro, alerta e informacao devem ser exibidos por `toast`
- mensagens inline dentro do conteudo da tela nao devem mais ser usadas como padrao de UX
- os toasts devem aparecer no topo direito da area de conteudo
- a posicao padrao fica alinhada horizontalmente com a linha do breadcrumb, logo abaixo do header principal
- esse comportamento vale para operacoes de `insert`, `update`, `delete`, carregamentos invalidos e bloqueios operacionais relevantes

Objetivo desse padrao:

- reduzir poluicao visual nas telas
- manter consistencia entre os modulos do dashboard
- dar retorno mais claro e rapido para operacoes administrativas

## Estrutura da solution

```text
AlphaSquad/
|
+-- src/
|   +-- AlphaSquad.Api
|   +-- AlphaSquad.Backoffice
|   +-- AlphaSquad.Web
|   +-- AlphaSquad.Infrastructure
|   +-- AlphaSquad.Lmt.Application.ApiClient
|   +-- AlphaSquad.Lmt.Application.Contracts
|   +-- AlphaSquad.Lmt.Application.Http
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

### Legal

- `GET /api/legal/current`
- `PUT /api/legal/current`

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
- `POST /api/events/{id}/checkin`
- `POST /api/events/{id}/complete`
- `GET /api/events/{id}/participants`
- `POST /api/events/{id}/participants/{userId}`
- `DELETE /api/events/{id}/participants/{userId}`

### Platform Auth

- `POST /api/platform-auth/login`
- `GET /api/platform-auth/me`
- `POST /api/platform-auth/refresh`
- `POST /api/platform-auth/change-password`
- `POST /api/platform-auth/logout`

### Platform Profile

- `GET /api/platform-profile/me`
- `PUT /api/platform-profile/me`
- `PUT /api/platform-profile/me/photo`
- `DELETE /api/platform-profile/me/photo`

### Platform Tenants

- `GET /api/platform-tenants/features/catalog`
- `GET /api/platform-tenants`
- `GET /api/platform-tenants/{id}`
- `POST /api/platform-tenants`
- `PUT /api/platform-tenants/{id}`
- `PUT /api/platform-tenants/{id}/logo`
- `GET /api/platform-tenants/{id}/admins`
- `POST /api/platform-tenants/{id}/admins`
- `POST /api/platform-tenants/{tenantId}/admins/{userId}/reset-password`
- `POST /api/platform-tenants/{id}/reset-admin-password`
- `GET /api/platform-tenants/{id}/audit`

Proxima etapa prevista da loja e do billing:

- `PaymentTransaction` para transacoes da academia com seus alunos
- checkout online da mensalidade do aluno em uma V2
- integracao futura de billing recorrente da academia para seus proprios alunos
- beneficios comerciais conectados a gamificacao

Frente comercial paralela da AlphaSquad:

- landing page publica da plataforma
- catalogo de planos SaaS
- Stripe Checkout
- webhook comercial dedicado
- lead pago aguardando onboarding no backoffice

## Banco de dados

Entidades ja presentes no projeto:

- `Tenant`
- `AppUser`
- `RefreshToken`
- `TenantMedia`
- `Feature`
- `TenantFeature`
- `TenantLegalContent`
- `Configuration`
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
- `PlatformUser`
- `PlatformRefreshToken`

Entidades estrategicas previstas para as proximas fases:

- `Product`
- `ProductVariant`
- `PaymentTransaction`
- `PlatformLead`
- `PlatformCheckoutSession`
- `PlatformSubscription`
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
- respeitar os padroes visuais oficiais do dashboard, incluindo feedback por `toast` em vez de mensagens inline

## Seed inicial

Ao subir a aplicacao, o projeto aplica migrations e garante a existencia de:

- tenant demo `alpha-demo`
- usuario admin `admin@alphasquad.app`
- sponsor bootstrap `owner@alphasquad.app`
- features base vinculadas ao tenant demo
- planos base `Basic`, `Advanced` e `Premium` para o tenant demo
- regras base de gamificacao para o tenant demo

## Infra local

O repositorio agora possui uma stack oficial de desenvolvimento local em container para:

- `AlphaSquad.Api`
- PostgreSQL
- Redis

Portas expostas da stack:

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

O compose oficial passa a ser o arquivo da raiz:

```bash
docker compose up --build
```

Observacao:

- o `docker-compose.yml` da raiz passa a ser a referencia oficial da stack local
- qualquer compose legado dentro de `src/AlphaSquad.Api` deve ser tratado apenas como historico e nao como ponto principal de execucao

## Configuracao por ambiente

O projeto agora segue duas trilhas oficiais de execucao.

### Modo tradicional

- `src/AlphaSquad.Api/appsettings.json` guarda apenas placeholders seguros
- `src/AlphaSquad.Api/appsettings.Development.json` traz defaults locais de desenvolvimento
- segredos reais podem ficar em `user-secrets` ou variaveis de ambiente
- a API continua podendo rodar localmente em `https://localhost:7054`
- `AlphaSquad.Web` e `AlphaSquad.Backoffice` mantem em configuracao versionada a URL `https://localhost:7054`

### Modo container

- o container da API nao le `user-secrets` do Windows
- os segredos passam a vir de `.env` + variaveis do `docker-compose.yml`
- a API sobe em `http://localhost:8080`
- a redirecao HTTPS da API e desligada por `App__EnableHttpsRedirection=false`
- `AlphaSquad.Web` e `AlphaSquad.Backoffice` continuam com a URL versionada original e so trocam em runtime com:

```text
Apis__AlphaSquad__BaseUrl=http://localhost:8080
```

Observacao importante:

- os frontends continuam consumindo a API pela camada `AlphaSquad.Lmt.Application.Http` e pelos contratos de `AlphaSquad.Lmt.Application.Contracts`
- a troca de URL no modo container nao exige alteracao estrutural do codigo das paginas

Como o projeto possui `UserSecretsId`, voce pode configurar localmente com:

```bash
dotnet user-secrets --project src/AlphaSquad.Api set "Jwt:SecretKey" "sua-chave-local"
dotnet user-secrets --project src/AlphaSquad.Api set "Storage:AccessKey" "seu-access-key"
dotnet user-secrets --project src/AlphaSquad.Api set "Storage:SecretKey" "seu-secret-key"
```

No modo container, a referencia equivalente passa a ser o arquivo `.env` da raiz.

Arquivos relevantes:

- `.env`: valores reais locais da stack Docker
- `.env.example`: placeholders para onboarding de outros ambientes

## Como rodar

### Opcao 1. Modo tradicional sem Docker

1. Garantir PostgreSQL local ou acessivel em `localhost:5432`
2. Garantir Redis local ou acessivel em `localhost:6379`
3. Configurar segredos via `user-secrets` ou variaveis de ambiente
4. Executar:

```bash
dotnet run --project src/AlphaSquad.Api
```

5. Abrir o Swagger:

```text
https://localhost:7054/swagger
```

### Opcao 2. Modo container para infraestrutura local

1. Subir a stack:

```bash
docker compose up --build
```

2. Acessar:

```text
http://localhost:8080/swagger
```

3. Para usar os frontends contra a API containerizada, sobrescrever em runtime:

```text
Apis__AlphaSquad__BaseUrl=http://localhost:8080
```

### Migrations com banco em container

Mesmo com Postgres em container, o fluxo de migrations continua sendo feito no host.

Gerar migration:

```powershell
Add-Migration NomeDaMigration -Project AlphaSquad.Infrastructure -StartupProject AlphaSquad.Api
```

ou

```bash
dotnet ef migrations add NomeDaMigration --project src/AlphaSquad.Infrastructure --startup-project src/AlphaSquad.Api
```

Aplicar migration:

```powershell
Update-Database -Project AlphaSquad.Infrastructure -StartupProject AlphaSquad.Api
```

Observacoes:

- como o Postgres do container expõe `localhost:5432`, o EF Core no host continua conseguindo atuar normalmente
- a API tambem continua aplicando `MigrateAsync()` no startup por meio do `DatabaseSeeder`
- isso permite administrar o banco em container sem mudar o fluxo de evolucao do modelo de entidades
