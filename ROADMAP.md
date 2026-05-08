# Project Roadmap

Este roadmap foi alinhado com o estado atual do codigo em 2026-05-07.

## Fase 1: Authentication & Security

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

## Fase 3: Exercises & Workouts

- [x] Implementar Exercise CRUD
- [ ] Consolidar Workout CRUD
- [ ] Consolidar associacao treino-exercicio
- [ ] Validar fluxos completos de treinos no workspace
- [ ] Revisar regras de exclusao e integridade entre treinos, exercicios e midias

Nota:
O dominio de `Workout` e `WorkoutExercise` ja existe no modelo de dados e ha implementacao em andamento no workspace, mas ele ainda nao deve ser tratado como modulo fechado ate validacao final.

## Fase 4: Engagement & Scheduling

- [x] Implementar check-in (`POST /api/checkins`)
- [x] Implementar consultas de check-in por aluno e por tenant
- [ ] Implementar classes/agendas CRUD
- [ ] Implementar booking e unbooking de aulas

## Fase 5: Product Experience

- [ ] Progresso do aluno
- [ ] Ranking e desafios
- [ ] Notificacoes
- [ ] Estrutura para app mobile

## Metas continuas

- [ ] Garantir isolamento multi-tenant em todos os modulos
- [ ] Padronizar paginacao e filtros
- [ ] Aumentar cobertura de testes
- [ ] Preparar configuracao por ambiente para deploy real
