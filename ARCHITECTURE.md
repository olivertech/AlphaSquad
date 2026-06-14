# AlphaSquad Architecture

Este documento descreve a arquitetura tecnica atual do AlphaSquad com base no codigo existente no workspace e no direcionamento de produto definido para as proximas fases.

## Objetivo arquitetural

O sistema foi desenhado para sustentar um SaaS white-label para academias com:

- isolamento forte por tenant
- simplicidade operacional
- baixo acoplamento entre modulos
- boa evolucao incremental por feature
- base pronta para experiencias mobile, sociais e transacionais

## Padroes adotados

### Vertical Slice Architecture

Os endpoints sao organizados por feature em `src/AlphaSquad.Api/Features`, reduzindo dependencia entre modulos e favorecendo evolucao isolada de cada dominio.

Features identificadas hoje:

- `Auth`
- `Configurations`
- `Tenants`
- `Media`
- `Users`
- `Legal`
- `Exercises`
- `Checkins`
- `Classes`
- `Workouts`
- `Profile`
- `Plans`
- `Store`
- `Social`
- `Events`
- `Gamification`
- `PlatformAuth`
- `PlatformProfile`
- `PlatformTenants`

Features estrategicas planejadas:

- `Store`
- `Social`
- `Gamification`
- `Profiles`
- `Events`

### Minimal APIs

A API usa ASP.NET Core Minimal APIs para:

- reduzir boilerplate
- manter o fluxo HTTP direto e legivel
- documentar endpoints via Swagger com menor sobrecarga

### CQRS leve

Sem framework formal de CQRS, mas com divisao pragmatica:

- `EF Core` para escrita e controle de entidades
- `Dapper` para consultas, respostas enxutas e listagens paginadas

Esse padrao deve continuar nas novas features, principalmente em fluxos de feed, loja, ranking e mural de eventos.

### Padroes de listagem

O projeto passa a conviver com dois contratos de listagem, cada um pensado para um tipo de consumo diferente.

- `page-based`: voltado a dashboards, visoes administrativas e consultas com navegacao tradicional por pagina
- `cursor-based`: voltado a feeds grandes com scroll infinito no app

O contrato `page-based` agora foi padronizado com `PagedResponse<T>`, enquanto os feeds infinitos usam `CursorFeedResponse<T>`.

O modulo de `Events` e o primeiro a explicitar esse padrao duplo:

- `GET /api/events` para visao paginada tradicional
- `GET /api/events/feed` para consumo cursor-based no app

Esse mesmo padrao ja foi replicado em `Store` e `Social`.

### Padrao de feedback no dashboard

O frontend administrativo adota um padrao unico de retorno visual ao usuario.

- feedbacks de sucesso, erro, alerta e informacao devem usar `toast`
- mensagens inline no corpo da pagina deixam de ser o padrao oficial
- os toasts devem ser renderizados no topo direito da area de conteudo
- a referencia visual e a mesma linha do breadcrumb, logo abaixo do header principal

Esse padrao deve ser aplicado em:

- criacao
- edicao
- exclusao
- conclusao de fluxos administrativos
- falhas de carregamento que exijam redirecionamento ou interrupcao da experiencia

Objetivo arquitetural:

- manter consistencia de UX entre modulos
- desacoplar feedback visual do layout principal da tela
- evitar que estados operacionais "quebrem" o espacamento e a leitura do conteudo

### Foco didatico e documentacao em codigo

Este projeto tambem tem um objetivo didatico e deve poder ser consultado futuramente por outros profissionais.

Por isso, todo novo codigo deve seguir estas diretrizes:

- classes e metodos novos devem trazer comentarios simples explicando o que fazem
- comentarios devem ajudar leitura e manutencao, sem virar texto excessivo
- regras de negocio, integracoes relevantes e pontos de isolamento multi-tenant devem ser comentados
- a documentacao em codigo deve evoluir junto com as features, e nao ser tratada como etapa opcional
- o dashboard web tambem deve respeitar os padroes de UX documentados, especialmente o uso de `toast` como canal principal de feedback

## Estrutura da solution

```text
src/
  AlphaSquad.Api
  AlphaSquad.Backoffice
  AlphaSquad.Web
  AlphaSquad.Lmt.Application.ApiClient
  AlphaSquad.Lmt.Application.Contracts
  AlphaSquad.Lmt.Application.Http
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
- endpoints do contexto tenant
- endpoints do contexto master da plataforma

### AlphaSquad.Backoffice

Responsavel por:

- dashboard master da AlphaSquad
- autenticacao visual separada do dashboard das academias
- onboarding de novas academias
- operacao global do sponsor
- gestao do perfil do owner
- consumo da API master da plataforma

### AlphaSquad.Web

Responsavel por:

- dashboard operacional da academia
- consumo da API tenant por meio da camada LMT
- modulos administrativos da academia
- area de profile do usuario autenticado

### AlphaSquad.Lmt.Application.*

Responsavel por:

- cliente HTTP gerado e adaptado para consumo da API
- contratos de DTOs e interfaces para os dashboards
- fachada de servicos usada pelo frontend web

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
- integracoes externas atuais e futuras, como Stripe e webhooks comerciais

### AlphaSquad.Shared

Responsavel por:

- DTOs
- enums
- helpers compartilhados
- contratos usados entre features

## Multi-tenancy

O isolamento multi-tenant e baseado em `TenantId` como identificador primario de segregacao.

### Onde o isolamento acontece

- claims do JWT: `tenant_id` e `tenant_slug`
- queries SQL filtradas por tenant
- entidades persistidas com `tenant_id`
- paths de storage separados por tenant

### Regra para novas features

Toda feature nova deve aplicar isolamento por tenant em:

- leitura
- escrita
- cache
- storage
- integracoes externas

### Resolucao central do tenant

O pipeline da API agora resolve o contexto do tenant antes dos endpoints do mundo da academia rodarem.

- `TenantContextMiddleware` valida `tenant_id` e `user_id` das claims
- confirma se o tenant ainda existe e esta ativo
- confirma se o usuario ainda pertence ao tenant e esta ativo
- injeta `TenantRequestContext` no `HttpContext`

As extensoes de `HttpContext` continuam disponiveis, mas agora passam a ler esse contexto resolvido quando ele existir.

## Contexto master da plataforma

O AlphaSquad agora possui um segundo contexto de autenticacao e autorizacao, separado do mundo tenant.

Esse contexto existe para o sponsor operar o negocio AlphaSquad sem se comportar como usuario de uma academia.

### Principios do contexto master

- nao reutilizar `AppUser` para o sponsor
- nao reutilizar `tenant_id` no token master
- nao misturar policies de tenant com policies de plataforma
- nao depender do dashboard da academia para criar ou configurar novas academias

### Modelagem atual

- `PlatformUser`: usuario global da AlphaSquad
- `PlatformRefreshToken`: refresh token proprio do backoffice
- `PlatformAuditLog`: trilha administrativa do sponsor sobre academias e admins
- `AppUser.MustChangePassword`: suporte a senha provisoria do admin inicial da academia

### Billing da plataforma x billing da academia

O produto agora separa formalmente dois dominios financeiros distintos:

- `billing da academia`: relacao da academia com seus alunos
- `billing da AlphaSquad`: relacao da AlphaSquad com as academias clientes

Estado atual:

- a academia ja controla plano ativo, vencimento, adimplencia e registro manual de pagamento
- a AlphaSquad ainda nao possui a frente comercial online implementada, mas a V1 foi definida para usar Stripe na landing page

Direcao da V1:

- o aluno nao paga mensalidade pelo app
- o dashboard do tenant nao processa checkout online de mensalidade
- o Stripe entra apenas na aquisicao comercial do SaaS AlphaSquad
- o onboarding da nova academia permanece semiautomatico, com intervencao do sponsor

## Seguranca e controle de acesso

O AlphaSquad foi desenhado para ser um produto white-label com varias camadas de seguranca, adequadas a um ambiente onde cada academia precisa confiar que seus dados, usuarios e operacoes ficam isolados de outras academias.

### Camada 1: Autenticacao

A autenticacao protege os recursos privados por meio de:

- JWT para acesso a endpoints protegidos
- claims de usuario, role e tenant no access token
- refresh token persistido no banco
- rotacao de refresh token
- revogacao de sessao em logout e troca de senha

No contexto master, a plataforma repete essa estrategia com objetos proprios:

- `PlatformUser`
- `PlatformRefreshToken`
- claims sem `tenant_id`

### Camada 2: Isolamento multi-tenant

O isolamento de tenant atua como barreira principal entre academias diferentes.

- `TenantId` nas entidades de dominio
- filtros por tenant nas queries EF Core e Dapper
- claims `tenant_id` e `tenant_slug`
- separacao de paths no storage
- validacoes adicionais em relacoes entre entidades

### Camada 3: Autorizacao por role

O sistema possui tres perfis base:

- `Admin`
- `Teacher`
- `Student`

Policies base atualmente registradas:

- `AdminOnly`
- `AdminOrTeacher`
- `PlatformOwnerOnly`

### Matriz atual de acesso

Visao resumida do estado atual:

- `Auth`: acesso anonimo apenas para login e refresh; endpoints de sessao exigem autenticacao
- `Tenants`: consulta publica por slug anonima; alteracoes de tenant e logo restritas a `Admin`
- `Users`: leitura e escrita administrativas restritas a `Admin`
- `Profile`: acesso ao proprio profile para usuarios autenticados, com escrita restrita ao contexto do proprio usuario
- `Plans`: leitura e gestao administrativa restritas a `Admin`
- `Media`: listagem, upload, troca e exclusao restritos a `Admin`
- `Exercises`: leitura para autenticados; escrita restrita a `Admin`
- `Workouts`: leitura para autenticados; escrita restrita a `Admin`
- `Classes`: leitura para autenticados; gestao e consultas operacionais administrativas restritas a `Admin`
- `Checkins`: check-in e historico proprio para autenticados; visao consolidada do tenant restrita a `Admin`
- `Classes`: aulas especiais podem gerar pontuacao de gamificacao no booking do aluno
- `Events`: leitura para autenticados quando a feature `EVENTS` estiver habilitada; escrita restrita a `Admin`
- `Gamification`: dashboard pessoal para alunos autenticados; regras e fechamento mensal restritos a perfis de gestao
- `Platform Auth`: autenticacao do sponsor restrita ao contexto master
- `Platform Tenants`: onboarding e gestao global de academias restritos ao sponsor
- `Platform Tenants`: suporte a admins adicionais dos tenants e auditoria do sponsor

### Regra adicional de acesso por plano

O acesso do aluno ao ecossistema tambem passou a depender de um vinculo comercial valido com a academia.

- apenas usuarios com plano ativo podem fazer login
- o refresh da sessao tambem revalida a existencia de plano ativo
- plano ativo valido significa:
- `user_memberships.is_active = true`
- plano do catalogo tambem ativo
- vigencia iniciada
- vigencia nao encerrada

### Camada 4: Protecao por contexto do recurso

Mesmo quando o usuario esta autenticado e possui role compativel, o backend ainda valida:

- se o recurso pertence ao tenant atual
- se entidades relacionadas pertencem ao mesmo tenant
- se o usuario alvo ou instrutor informado pertence ao tenant correto
- se o recurso ainda esta ativo quando isso afeta a regra de negocio

### Camada 5: Reducao de exposicao

A API tambem reduz a exposicao de informacoes desnecessarias nos contratos retornados.

- remocao de metadados internos sensiveis, como `StorageKey`, dos DTOs publicos
- restricao de listagens administrativas para evitar exposicao ampla a usuarios sem papel de gestao

### Valor comercial dessa arquitetura

Para academias contratantes, isso significa que o produto nao depende apenas de um login para se considerar seguro.

O modelo atual combina:

- autenticacao
- isolamento multi-tenant
- autorizacao por perfil
- validacoes de integridade
- reducao de exposicao de dados

Esse desenho fortalece confianca operacional e reduz risco de acesso indevido entre usuarios e entre academias.

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

### Fluxo master da plataforma

1. Sponsor autentica via `/api/platform-auth/login`
2. API gera access token com claims de plataforma
3. API gera refresh token do backoffice e persiste em `platform_refresh_tokens`
4. O token master nao leva `tenant_id`
5. O sponsor acessa `/api/platform-tenants/*` para operar a base de academias

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
- `PlatformUser`
- `PlatformRefreshToken`
- `TenantMedia`
- `Configuration`
- `Feature`
- `TenantFeature`
- `Exercise`
- `CheckIn`
- `GymClass`
- `ClassBooking`
- `Workout`
- `WorkoutExercise`
- `UserProfile`
- `MembershipPlan`
- `UserMembership`
- `MembershipPayment`
- `Product`
- `ProductVariant`
- `StoreOrder`
- `StoreOrderItem`
- `GamificationEventRule`
- `UserGamificationEvent`
- `PointsLedger`
- `MonthlyStudentRanking`
- `TenantLegalContent`

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

### Casos de uso futuros recomendados

- cache de feed social paginado por tenant
- cache de catalogo publico da loja
- cache de ranking mensal
- cache de mural institucional

## Configuracao por ambiente

O projeto passou a adotar uma separacao mais segura entre configuracao versionada e segredos reais.

- `appsettings.json`: apenas placeholders seguros e nomes de secoes
- `appsettings.Development.json`: defaults locais para desenvolvimento
- `user-secrets` e variaveis de ambiente: destino recomendado para chaves reais de JWT, storage e demais integracoes

Essa separacao ajuda a publicar o repositorio sem expor credenciais e aproxima a base do que sera esperado em deploy real.

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

### Uso futuro planejado

- imagens de produtos
- fotos de posts sociais
- fotos de profile
- banners e imagens de eventos

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
- `STORE`
- `SOCIAL`
- `GAMIFICATION`
- `EVENTS`

Essa base pode evoluir para habilitar tambem:

- `SOCIAL`
- `PROFILE`

O modulo de `Events` ja usa essa base em runtime. Se a academia nao tiver a feature `EVENTS`, os endpoints do mural retornam bloqueio de acesso e o feed nao fica disponivel.

## Dominios estrategicos planejados

### Store

Objetivo:
Permitir venda de produtos personalizados da academia dentro do app.

Capacidades atuais:

- catalogo de produtos por tenant
- leitura paginada do catalogo para usuarios autenticados
- leitura cursor-based do catalogo para scroll infinito no app
- detalhe do produto com variantes
- gestao administrativa de produtos
- gestao administrativa de variantes
- imagem principal por produto com validacao de tenant
- criacao de pedido pelo app
- fluxo administrativo de reserva, retirada e pagamento local

Capacidades previstas nas proximas rodadas:

- variacoes de produto como cor e tamanho
- estoque e disponibilidade
- pedido com retirada presencial na academia
- pagamento local na administracao da academia
- integracao com Stripe para pagamento em etapa posterior

Entidades atuais e provaveis:

- `Product`
- `ProductVariant`
- `StoreOrder`
- `StoreOrderItem`
- `PaymentTransaction`

Consideracoes arquiteturais:

- paginação cursor-based ou page-based para feed infinito
- webhook de pagamento para confirmacao de pedidos
- separacao entre preco exibido, pedido e transacao
- a V1 da loja deve suportar pedido sem gateway externo
- o pedido pode nascer com status como `PendingApproval`, `Reserved`, `ReadyForPickup`, `PaidLocally` e `Cancelled`

### Social

Objetivo:
Criar um feed interno da academia para fortalecer comunidade e interacao.

Capacidades atuais:

- post com imagem e descricao curta
- feed global por tenant
- feed cursor-based para scroll infinito no app
- likes
- comentarios simples em nivel unico

Entidades atuais:

- `SocialPost`
- `SocialPostLike`
- `SocialPostComment`

Consideracoes arquiteturais:

- listagem paginada por data
- feed cursor-based com ordenacao estavel por data e id
- contadores agregados de likes e comentarios
- moderacao simples no futuro

### Gamification

Objetivo:
Pontuar acoes do usuario no ecossistema e gerar ranking mensal.

Capacidades atuais:

- regras de pontuacao por tenant
- registro de eventos de gamificacao por aluno
- razao de pontos acumulados
- ranking mensal ao vivo por tenant
- fechamento mensal com snapshot de vencedores e premios
- integracao inicial com `CheckIn`, `StoreOrder` pago localmente, renovacao de plano e `GymClass` especial

Capacidades previstas:

- eventos de pontuacao
- ledger de pontos
- ranking mensal por tenant
- premios e beneficios associados
- dashboard de gamificacao do aluno
- historico de vencedores mensais

Entidades provaveis:

- `GamificationEventRule`
- `UserGamificationEvent`
- `PointsLedger`
- `MonthlyStudentRanking`
- `RewardPolicy`

Consideracoes arquiteturais:

- pontuacao desacoplada por eventos de dominio
- regras configuraveis por tenant no futuro
- possibilidade de reprocessamento de ranking
- apenas usuarios com role `Student` participam da gamificacao
- cada evento precisa guardar usuario, tipo do evento, origem, pontos aplicados e data da ocorrencia
- o ranking mensal deve ser fechado por tenant e por competencia

### Profiles

Objetivo:
Dar ao usuario uma area central de informacoes pessoais e de vinculacao com a academia.

Capacidades previstas:

- foto de perfil
- username
- dados basicos
- celular com DDD
- data de nascimento
- plano ativo
- troca de senha

Entidades provaveis:

- `UserProfile`
- `MembershipPlanSnapshot` ou entidade equivalente

Consideracoes arquiteturais:

- evitar duplicacao entre `Auth`, `Users` e `Profile`
- separar dados de administracao de dados da experiencia do aluno

Estado atual:

- leitura do proprio profile
- atualizacao de nome e username
- atualizacao de celular com DDD e data de nascimento
- troca de e-mail com validacao de senha atual e unicidade por tenant
- upload e remocao de foto de profile
- validacao mais forte de e-mail e username
- `ActivePlan` ligado ao dominio real de planos
- integracao de `Profile` com `login` e `/api/auth/me`

### Plans

Objetivo:
Representar o catalogo de planos da academia e o vinculo ativo de cada usuario a um plano real.

Capacidades atuais:

- cadastro e atualizacao de planos por gestao
- atribuicao de plano a usuario do tenant
- controle de um unico plano ativo por usuario
- integracao direta com `Profile` e `Auth`
- historico de planos por usuario
- registro de motivo de status
- registro do usuario que realizou a troca
- consultas administrativas de reativacao comercial
- dia de vencimento (`BillingDueDay`) por usuario
- registro administrativo/manual de pagamento (`MembershipPayment`)
- situacao financeira derivada para uso no dashboard
- reflexo de pagamento em dia na gamificacao

Entidades atuais:

- `MembershipPlan`
- `UserMembership`

Consideracoes arquiteturais:

- o vinculo de plano e multi-tenant desde a origem
- a atribuicao encerra o plano ativo anterior do usuario
- o nome do plano ativo e projetado no profile e na sessao autenticada
- o historico de planos e preservado em `UserMembership`
- o acesso ao sistema depende de um plano ativo valido
- o dominio de planos agora tambem serve a campanhas de retencao e reengajamento
- o dominio atual nao processa checkout online nem recorrencia automatica na V1
- o pagamento do aluno e tratado como evento administrativo registrado apos recebimento fora do sistema

Consultas operacionais atuais:

- historico de planos por usuario
- usuarios sem plano ativo ha X dias
- usuarios com plano vigente ha X dias sem check-in

### Events

Objetivo:
Criar um mural institucional da academia para eventos, acoes sociais e comunicacao visual.

Capacidades atuais:

- feed paginado por tenant
- feed cursor-based para scroll infinito no app
- CRUD administrativo restrito a perfis de gestao
- suporte a imagem via `TenantMedia`
- eventos outdoor com participacao do aluno
- integracao da participacao outdoor com gamificacao
- senha de check-in por evento
- confirmacao de presenca por senha no app
- conclusao administrativa do evento com pontuacao em lote para os presentes
- matricula e remocao administrativa de participantes no dashboard
- feature opcional `EVENTS` por tenant
- suporte no backend para publicacoes internas de mural, como aniversariantes e vencedores da gamificacao

Entidades atuais:

- `AcademyEvent`
- `AcademyEventParticipation`

Consideracoes arquiteturais:

- permissao de escrita restrita a gestao
- leitura ampla para usuarios do tenant
- reaproveitamento do padrao de feed e paginacao
- participacao outdoor dispara `OutdoorEventParticipation` apenas para usuarios `Student`
- a pontuacao do evento acontece na conclusao administrativa do evento, e nao na matricula
- `AcademyEvent` usa `CheckInPassword` e `IsCompleted`
- `AcademyEventParticipation` usa `IsPresent`
- a ausencia da feature `EVENTS` bloqueia o modulo para o tenant atual
- reaproveitamento do padrao de feed e paginação

### Notificacoes do app

O AlphaSquad passa a considerar oficialmente uma central de notificacoes para os alunos no aplicativo mobile.

Regras previstas:

- o app exibira um sino com indicacao de novas notificacoes
- notificacoes podem representar eventos, novas aulas, novos produtos, informes da academia e destaques institucionais
- o aluno podera listar notificacoes por `lidas`, `nao lidas` e `todas`
- ao abrir uma notificacao, o conteudo sera exibido em uma modal rolavel
- a leitura deve marcar a notificacao como lida e retirar o item da lista de pendencias
- na primeira fase, essas notificacoes serao gerais para todos os alunos habilitados no app

Direcao arquitetural recomendada:

- separar publicacao institucional do controle de leitura por usuario
- permitir que aniversariantes e vencedores da gamificacao gerem destaque no mural e notificacoes sem depender de um evento presencial
- manter a base pronta para segmentacao futura, sem exigir personalizacao por publico neste primeiro recorte

### Billing SaaS da AlphaSquad

O produto passa a considerar uma frente comercial propria para vender o AlphaSquad como SaaS para academias.

Escopo da V1:

- landing page publica da AlphaSquad
- catalogo de planos SaaS
- criacao de sessao Stripe Checkout
- webhook comercial dedicado
- registro de lead/checkout pago
- exibicao no backoffice de academias pagas aguardando onboarding

Fluxo da V1:

1. a academia escolhe o plano na landing page
2. realiza o pagamento no Stripe
3. o webhook confirma a compra
4. o sistema registra o lead pago
5. o sponsor e avisado no backoffice
6. o sponsor cria/libera tenant e admin inicial manualmente

Fora da V1:

- auto-provisionamento total do tenant
- renovacao recorrente automatica da assinatura SaaS
- cancelamento e ciclo financeiro completo da plataforma
- checkout online de mensalidade dos alunos

## Multi-idioma em V2

O projeto deve considerar desde ja uma futura camada multi-idioma com:

- portugues
- ingles
- espanhol

### Requisito adicional importante

A internacionalizacao nao deve se limitar ao app cliente. O backend tambem precisara devolver conteudo textual no idioma desejado pelo usuario.

### Direcao arquitetural recomendada

- receber idioma preferencial por header, claim ou configuracao de profile
- identificar campos que sao conteudo livre e podem exigir traducao dinamica
- preservar texto original armazenado
- produzir traducao sob demanda via integracao de IA em componentes especificos
- cachear traducoes por idioma e tenant quando fizer sentido

### Cuidado arquitetural

Nem todo conteudo deve ser traduzido em tempo real desde o inicio. O ideal e separar:

- textos de interface: responsabilidade do app cliente
- textos institucionais e de negocio vindos do backend: candidatos a traducao
- conteudo social gerado por usuarios: politica de traducao futura, com custo e privacidade avaliados

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
- policies base por role
- cache Redis
- storage R2
- tenant config e features
- usuarios
- profile do usuario
- planos e vinculos de plano
- historico de planos e consultas de retencao
- media
- exercicios
- check-in
- classes/agendas
- reservas de aulas
- rede social interna
- mural de eventos

### Em progresso

- nenhuma frente estrutural critica do backend tenant segue bloqueando a operacao base da V1
- o proximo salto relevante do backend esta nas capacidades futuras de notificacoes, billing e automacao institucional

### Planejado como core de produto

- loja interna
- rede social do tenant
- gamificacao
- profile do usuario
- mural institucional de eventos

## Pendencias tecnicas relevantes

- cobertura automatizada de testes
- ciclo financeiro mais rico de planos e cobranca estruturada para os alunos
- `PaymentTransaction` para transacoes da academia com seus proprios alunos
- landing page comercial da AlphaSquad com Stripe Checkout e webhook de aquisicao
- moderacao e governanca futura de social
- cobertura observavel e trilhas analiticas mais profundas do contexto master
