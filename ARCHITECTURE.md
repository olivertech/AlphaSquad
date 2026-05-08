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
- `Tenants`
- `Media`
- `Users`
- `Exercises`
- `Checkins`
- `Classes`
- `Workouts`

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
- futuras integracoes externas, como Stripe

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

### Risco arquitetural a observar

O isolamento hoje depende principalmente da disciplina nos endpoints e queries. Ainda nao existe um middleware central de tenant ou uma camada mais automatica de enforcement.

## Seguranca e controle de acesso

O AlphaSquad foi desenhado para ser um produto white-label com varias camadas de seguranca, adequadas a um ambiente onde cada academia precisa confiar que seus dados, usuarios e operacoes ficam isolados de outras academias.

### Camada 1: Autenticacao

A autenticacao protege os recursos privados por meio de:

- JWT para acesso a endpoints protegidos
- claims de usuario, role e tenant no access token
- refresh token persistido no banco
- rotacao de refresh token
- revogacao de sessao em logout e troca de senha

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

### Matriz atual de acesso

Visao resumida do estado atual:

- `Auth`: acesso anonimo apenas para login e refresh; endpoints de sessao exigem autenticacao
- `Tenants`: consulta publica por slug anonima; alteracoes de tenant e logo restritas a `Admin`
- `Users`: leitura administrativa e escrita restritas a `Admin` ou `Teacher`, com operacoes sensiveis de escrita restritas a `Admin`
- `Media`: listagem, upload, troca e exclusao restritos a `Admin` ou `Teacher`
- `Exercises`: leitura para autenticados; escrita restrita a `Admin` ou `Teacher`
- `Workouts`: leitura para autenticados; escrita restrita a `Admin` ou `Teacher`
- `Classes`: leitura para autenticados; gestao restrita a `Admin` ou `Teacher`
- `Checkins`: check-in e historico proprio para autenticados; visao consolidada do tenant restrita a `Admin` ou `Teacher`

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
- `ClassBooking`
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

### Casos de uso futuros recomendados

- cache de feed social paginado por tenant
- cache de catalogo publico da loja
- cache de ranking mensal
- cache de mural institucional

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

Essa base pode evoluir para habilitar tambem:

- `STORE`
- `SOCIAL`
- `GAMIFICATION`
- `PROFILE`
- `EVENTS`

## Dominios estrategicos planejados

### Store

Objetivo:
Permitir venda de produtos personalizados da academia dentro do app.

Capacidades previstas:

- catalogo por tenant
- variacoes de produto como cor e tamanho
- imagens
- estoque e disponibilidade
- carrinho e pedido
- integracao com Stripe para pagamento

Entidades provaveis:

- `Product`
- `ProductVariant`
- `Order`
- `OrderItem`
- `PaymentTransaction`

Consideracoes arquiteturais:

- paginação cursor-based ou page-based para feed infinito
- webhook de pagamento para confirmacao de pedidos
- separacao entre preco exibido, pedido e transacao

### Social

Objetivo:
Criar um feed interno da academia para fortalecer comunidade e interacao.

Capacidades previstas:

- post com imagem e descricao curta
- feed global por tenant
- likes
- comentarios simples em nivel unico

Entidades provaveis:

- `SocialPost`
- `PostLike`
- `PostComment`

Consideracoes arquiteturais:

- listagem paginada por data
- contadores agregados de likes e comentarios
- moderacao simples no futuro

### Gamification

Objetivo:
Pontuar acoes do usuario no ecossistema e gerar ranking mensal.

Capacidades previstas:

- eventos de pontuacao
- ledger de pontos
- ranking mensal por tenant
- premios e beneficios associados

Entidades provaveis:

- `GamificationEvent`
- `PointsLedger`
- `MonthlyRanking`
- `RewardPolicy`

Consideracoes arquiteturais:

- pontuacao desacoplada por eventos de dominio
- regras configuraveis por tenant no futuro
- possibilidade de reprocessamento de ranking

### Profiles

Objetivo:
Dar ao usuario uma area central de informacoes pessoais e de vinculacao com a academia.

Capacidades previstas:

- foto de perfil
- username
- dados basicos
- plano ativo
- troca de senha

Entidades provaveis:

- extensao de `AppUser`
- `MembershipPlanSnapshot` ou entidade equivalente

Consideracoes arquiteturais:

- evitar duplicacao entre `Auth`, `Users` e `Profile`
- separar dados de administracao de dados da experiencia do aluno

### Events

Objetivo:
Criar um mural institucional da academia para eventos, acoes sociais e comunicacao visual.

Capacidades previstas:

- posts exclusivos da gestao
- feed visual vertical
- imagens e textos
- ordenacao cronologica

Entidades provaveis:

- `EventPost`
- `EventMedia` ou reuso controlado de `TenantMedia`

Consideracoes arquiteturais:

- permissao de escrita restrita a gestao
- leitura ampla para usuarios do tenant
- reaproveitamento do padrao de feed e paginação

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
- media
- exercicios
- check-in
- classes/agendas
- reservas de aulas

### Em progresso

- workouts e composicao treino-exercicio
- autorizacao mais fina por role e permissao
- endurecimento de isolamento multi-tenant

### Planejado como core de produto

- loja interna
- rede social do tenant
- gamificacao
- profile do usuario
- mural institucional de eventos

## Pendencias tecnicas relevantes

- middleware central de tenant
- politica de autorizacao por role/permissao
- cobertura automatizada de testes
- padronizacao de paginacao para todos os modulos
- endurecimento de configuracoes sensiveis por ambiente
- padrao comum para feeds infinitos
- padrao comum para integracoes externas e webhooks
