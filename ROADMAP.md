# Project Roadmap

Este roadmap foi alinhado com o estado atual do codigo e com a visao estrategica definida em 2026-05-08.

## Principios de priorizacao

- entregar primeiro o que gera valor direto para a experiencia do aluno
- preservar isolamento multi-tenant em toda nova feature
- preferir modulos com reuso de componentes entre si
- manter o foco didatico do projeto em toda implementacao

## Ordem recomendada de implementacao

1. Consolidar `Workouts` e autorizacao base
2. Implementar `Profile`
3. Implementar `Store`
4. Implementar `Events`
5. Implementar `Social`
6. Implementar `Gamification`
7. Evoluir para `Multi-language` em V2

Essa ordem foi escolhida para reduzir retrabalho:

- `Profile` ajuda `Social`, `Store` e `Gamification`
- `Store` cria a base de beneficios e recompensas futuras
- `Events` e `Social` compartilham padroes de feed e paginação
- `Gamification` depende de eventos gerados pelos modulos anteriores

## Fase 1: Authentication, Security & Tenant Foundation

- [x] Implement refresh token rotation
- [x] Implement change password
- [x] Implement logout com revogacao de sessoes
- [x] Corrigir relacionamento de refresh tokens
- [x] Tratar parse seguro de claims de usuario
- [ ] Adicionar policies e permissoes por role
- [ ] Centralizar tenant resolution em middleware

## Fase 2: Tenant, Media & Feature Access

- [x] Modelar `Feature` e `TenantFeature`
- [x] Seed de features base para o tenant demo
- [x] Implementar `GET /api/tenants/current`
- [x] Implementar `PUT /api/tenants/current/logo`
- [x] Implementar `GET /api/tenants/current/features`
- [x] Implementar `GET /api/media/{id}`
- [x] Implementar CRUD de midias com storage externo
- [x] Implementar cache de tenant por slug com Redis

## Fase 3: Exercises, Workouts & Training Experience

- [x] Implementar Exercise CRUD
- [ ] Consolidar Workout CRUD
- [ ] Consolidar associacao treino-exercicio
- [ ] Validar fluxos completos de treinos no workspace
- [ ] Revisar regras de exclusao e integridade entre treinos, exercicios e midias

## Fase 4: Check-in & Scheduling

- [x] Implementar check-in (`POST /api/checkins`)
- [x] Implementar consultas de check-in por aluno e por tenant
- [x] Implementar classes/agendas CRUD
- [x] Implementar booking e unbooking de aulas
- [ ] Expor consultas operacionais de reservas por aula e por aluno

## Fase 5: User Profile

- [ ] Criar endpoint `GET /api/profile/me`
- [ ] Criar endpoint `PUT /api/profile/me`
- [ ] Permitir upload e troca de foto de perfil
- [ ] Permitir definicao de `username`
- [ ] Expor plano ativo do usuario
- [ ] Integrar area de profile com troca de senha ja existente

## Fase 6: Store & Stripe Commerce

- [ ] Modelar `Product`
- [ ] Modelar `ProductVariant`
- [ ] Modelar `Order` e `OrderItem`
- [ ] Modelar `PaymentTransaction`
- [ ] Implementar CRUD administrativo de produtos
- [ ] Implementar listagem publica por tenant com paginação infinita
- [ ] Implementar detalhe de produto
- [ ] Implementar fluxo de compra
- [ ] Integrar checkout com Stripe
- [ ] Implementar webhook de confirmacao de pagamento
- [ ] Registrar status do pedido e auditoria minima

## Fase 7: Events Wall

- [ ] Modelar `EventPost`
- [ ] Implementar CRUD administrativo do mural
- [ ] Implementar feed publico do tenant com paginação infinita
- [ ] Permitir upload de imagens para eventos
- [ ] Restringir escrita para `Admin` e roles equivalentes
- [ ] Preparar visual e ordenacao pensados para o app mobile

## Fase 8: Social Network

- [ ] Modelar `SocialPost`
- [ ] Modelar `PostLike`
- [ ] Modelar `PostComment`
- [ ] Implementar criacao de post com imagem e descricao curta
- [ ] Implementar feed global do tenant com scroll infinito
- [ ] Implementar like e unlike
- [ ] Implementar comentarios simples sem thread
- [ ] Expor contadores agregados de interacao
- [ ] Avaliar moderacao basica e denuncias em etapa posterior

## Fase 9: Gamification

- [ ] Modelar eventos de pontuacao
- [ ] Criar ledger de pontos por usuario
- [ ] Definir tabela de regras para pontuacao inicial
- [ ] Pontuar acoes como check-in, booking, posts e interacoes
- [ ] Gerar ranking mensal por tenant
- [ ] Expor top 3 e ranking geral
- [ ] Preparar integracao futura com premios, descontos e recompensas da loja

## Fase 10: Product Experience & Mobile Readiness

- [ ] Padronizar contratos de feed infinito para `Store`, `Events` e `Social`
- [ ] Padronizar metadados de cards para app mobile
- [ ] Estruturar notificacoes futuras
- [ ] Definir padrao de imagens, thumbnails e tamanhos

## Fase 11: Multi-language V2

- [ ] Permitir escolha de idioma no profile
- [ ] Propagar idioma preferido em auth e requests
- [ ] Definir contrato de idioma nos endpoints
- [ ] Mapear quais conteudos do backend exigem traducao
- [ ] Integrar camada de IA para traducao dinamica de conteudo
- [ ] Implementar cache de traducoes por tenant e idioma
- [ ] Avaliar custos, privacidade e latencia da traducao de conteudo social

## Dependencias importantes entre modulos

- `Profile` antes de `Social`, para foto e username terem origem clara
- `Store` antes de `Gamification`, para permitir recompensas conectadas ao ecommerce
- `Events` antes ou junto de `Social`, para reaproveitar feed, media e paginação
- `Gamification` depois dos modulos que vao gerar eventos de pontuacao

## Metas continuas

- [ ] Garantir isolamento multi-tenant em todos os modulos
- [ ] Padronizar paginacao e filtros
- [ ] Aumentar cobertura de testes
- [ ] Preparar configuracao por ambiente para deploy real
- [ ] Manter comentarios didaticos nas novas features
