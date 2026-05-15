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
3. Consolidar `Plans`, historico e retencao
4. Implementar `Store`
5. Implementar `Events`
6. Implementar `Social`
7. Implementar `Gamification`
8. Evoluir para `Multi-language` em V2

Essa ordem foi escolhida para reduzir retrabalho:

- `Profile` ajuda `Social`, `Store` e `Gamification`
- `Plans` fortalece acesso comercial, historico e campanhas de retorno antes da loja e da gamificacao
- `Store` cria a base de beneficios e recompensas futuras
- `Events` e `Social` compartilham padroes de feed e paginaÃ§Ã£o
- `Gamification` depende de eventos gerados pelos modulos anteriores

## Fase 1: Authentication, Security & Tenant Foundation

- [x] Implement refresh token rotation
- [x] Implement change password
- [x] Implement logout com revogacao de sessoes
- [x] Corrigir relacionamento de refresh tokens
- [x] Tratar parse seguro de claims de usuario
- [x] Adicionar policies e permissoes por role
- [ ] Centralizar tenant resolution em middleware

## Fase 2: Tenant, Media & Feature Access

- [x] Modelar `Feature` e `TenantFeature`
- [x] Implementar textos legais do tenant para `Termos de Uso` e `Politica de Privacidade`
- [x] Seed de features base para o tenant demo
- [x] Implementar `GET /api/tenants/current`
- [x] Implementar `PUT /api/tenants/current/logo`
- [x] Implementar `GET /api/tenants/current/features`
- [x] Implementar `GET /api/media/{id}`
- [x] Implementar CRUD de midias com storage externo
- [x] Implementar cache de tenant por slug com Redis

## Fase 3: Exercises, Workouts & Training Experience

- [x] Implementar Exercise CRUD
- [x] Consolidar Workout CRUD
- [x] Consolidar associacao treino-exercicio
- [x] Validar fluxos completos de treinos no workspace
- [x] Revisar regras de exclusao e integridade entre treinos, exercicios e midias

## Fase 4: Check-in & Scheduling

- [x] Implementar check-in (`POST /api/checkins`)
- [x] Implementar consultas de check-in por aluno e por tenant
- [x] Implementar classes/agendas CRUD
- [x] Implementar booking e unbooking de aulas
- [x] Expor consultas operacionais de reservas por aula e por aluno

## Fase 5: User Profile

- [x] Criar endpoint `GET /api/profile/me`
- [x] Criar endpoint `PUT /api/profile/me`
- [x] Integrar `Profile` com `login` e `/api/auth/me`
- [x] Permitir troca de e-mail com validacoes proprias
- [x] Permitir upload e troca de foto de perfil
- [x] Permitir definicao de `username`
- [x] Expor plano ativo do usuario a partir de um dominio real de planos
- [x] Integrar area de profile com troca de senha ja existente

## Fase 5.1: Plans & Memberships

- [x] Modelar `MembershipPlan`
- [x] Modelar `UserMembership`
- [x] Implementar CRUD administrativo inicial de planos
- [x] Implementar atribuicao de plano ativo para usuario
- [x] Restringir acesso ao sistema para usuarios com plano ativo valido
- [x] Preservar historico de planos por usuario
- [x] Registrar motivo de status e usuario responsavel pela troca
- [x] Expor historico de planos por usuario
- [x] Expor consulta de usuarios sem plano ativo ha X dias
- [ ] Evoluir planos para cobranca, ciclo financeiro, cancelamento estruturado e historico mais rico

## Fase 5.2: Retention & Reengagement

- [x] Expor consulta de usuarios com plano vigente sem check-in ha X dias
- [ ] Integrar consultas de reengajamento com notificacoes futuras
- [ ] Integrar consultas de reengajamento com campanhas comerciais futuras
- [ ] Definir eventos de gamificacao relacionados a retorno de alunos

## Fase 6: Store & Stripe Commerce

- [x] Modelar `Product`
- [x] Modelar `ProductVariant`
- [x] Modelar `Order` e `OrderItem`
- [ ] Modelar `PaymentTransaction`
- [x] Implementar CRUD administrativo de produtos
- [x] Implementar listagem publica por tenant com paginação infinita
- [x] Implementar detalhe de produto
- [x] Implementar CRUD administrativo de variantes
- [x] Implementar fluxo de pedido com retirada presencial
- [x] Implementar status administrativos do pedido para separacao, retirada e pagamento local
- [ ] Integrar checkout com Stripe em etapa posterior
- [ ] Implementar webhook de confirmacao de pagamento em etapa posterior
- [x] Registrar status do pedido e auditoria minima

## Fase 7: Events Wall

- [x] Modelar `AcademyEvent`
- [x] Modelar `AcademyEventParticipation`
- [x] Implementar CRUD administrativo do mural
- [x] Implementar feed publico do tenant com paginacao
- [x] Implementar feed cursor-based para scroll infinito no app
- [x] Permitir vinculo de imagens ja existentes via `Media`
- [x] Restringir escrita para `Admin` e `Teacher`
- [x] Tratar `Events` como modulo opcional por tenant
- [x] Integrar participacao outdoor com `OutdoorEventParticipation`
- [x] Evoluir o contrato para infinite scroll padronizado
- [ ] Preparar confirmacao administrativa de presenca se a operacao exigir validacao manual

## Fase 8: Social Network

- [x] Modelar `SocialPost`
- [x] Modelar `SocialPostLike`
- [x] Modelar `SocialPostComment`
- [x] Implementar criacao de post com imagem e descricao curta
- [x] Implementar feed global do tenant com scroll infinito
- [x] Implementar like e unlike
- [x] Implementar comentarios simples sem thread
- [x] Expor contadores agregados de interacao
- [ ] Avaliar moderacao basica e denuncias em etapa posterior

## Fase 9: Gamification

- [x] Modelar `GamificationEventRule`
- [x] Modelar `UserGamificationEvent`
- [x] Criar ledger de pontos por usuario
- [x] Definir tabela de regras para pontuacao inicial
- [x] Pontuar acoes como booking, posts e interacoes
- [x] Pontuar check-in
- [x] Pontuar compras pagas localmente na loja
- [x] Pontuar pagamento em dia
- [x] Pontuar renovacao de plano
- [x] Pontuar booking de aulas especiais
- [x] Gerar ranking mensal por tenant
- [x] Garantir que apenas `Student` participe do ranking
- [x] Expor top 3 e ranking geral
- [x] Expor dashboard de gamificacao do aluno
- [x] Expor historico de vencedores dos meses anteriores
- [ ] Preparar integracao futura com premios, descontos e recompensas da loja

## Fase 10: Product Experience & Mobile Readiness

- [x] Padronizar contratos de feed infinito para `Store`, `Events` e `Social`
- [x] Padronizar contratos `page-based` com `PagedResponse<T>` nos principais endpoints de listagem
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
- `Plans` e `Retention` antes de `Gamification`, para reaproveitar sinais de retorno, cancelamento e reativacao
- `Store` antes de `Gamification`, para permitir recompensas conectadas ao ecommerce
- `Events` antes ou junto de `Social`, para reaproveitar feed, media e paginaÃ§Ã£o
- `Gamification` depois dos modulos que vao gerar eventos de pontuacao

## Metas continuas

- [ ] Garantir isolamento multi-tenant em todos os modulos
- [x] Padronizar paginacao nos principais modulos com contratos compartilhados
- [x] Padronizar feedback visual do dashboard com `toast` em vez de mensagens inline
- [ ] Aumentar cobertura de testes
- [x] Preparar configuracao base por ambiente para deploy real
- [ ] Manter comentarios didaticos nas novas features

