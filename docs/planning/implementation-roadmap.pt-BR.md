# Roadmap de Implementação

> Foco atual: mapping entre fontes e campos e desenho da ingestão.

## Princípios de entrega

Entregar incrementos verticais pequenos que respondam uma pergunta de produto, preservem limites arquiteturais, incluam testes e documentação, passem no PR, continuem publicáveis e evitem modelagem prematura.

## Bases concluídas

- solution, limites de Clean Architecture, PostgreSQL, EF Core, Docker, Terraform, CI/CD e deploy;
- domínio inicial de jogos, gêneros, plataformas, fontes e confiabilidade;
- API de leitura com filtros, paginação, validação, detalhes, gêneros e plataformas;
- tratamento global com `ProblemDetails`;
- shell responsivo e experiência de Comparable Games em Blazor;
- botão visível de busca, estado na URL, feedbacks, componentes, tipografia e navegação responsiva;
- pesquisa e seleção final de fontes externas.

## Marco atual — Preparação dos dados reais

### Objetivo

Transformar a pesquisa concluída em um desenho aprovado de dados e ingestão.

### Entregáveis

1. mapa fonte → pergunta de produto;
2. inventário de campos permitidos de IGDB, Wikidata e Steam;
3. checklist jurídico e de atribuição;
4. proposta de referências externas e reconciliação;
5. regras de confiabilidade e proveniência;
6. frequência e plano de rate limits;
7. pequena PoC de client;
8. proposta de domínio e decisão sobre migration.

### Definição de pronto

- cada campo importado tem finalidade para o producer;
- cada campo tem fonte, natureza, permissão e regra de confiança;
- IDs externos e reconciliação estão definidos;
- imagens e descrições foram incluídas ou adiadas explicitamente;
- workers, retries, checkpoints e idempotência estão definidos;
- impacto de armazenamento cabe na infraestrutura gratuita;
- mudanças de domínio são aprovadas antes de migrations.

## Próximo marco — Ingestão inicial

Escopo potencial:

- adapters e DTOs;
- workers controlados;
- mappings normalizados;
- referências externas;
- reconciliação;
- timestamps;
- importação idempotente;
- testes de integração e qualidade;
- agendamento em GitHub Actions;
- deploy e validação com dados representativos.

## Marco seguinte — MVP refinado

- filtros avançados suportados pelos dados aprovados;
- página Data Sources preenchida;
- modos Alta confiança, Equilibrado e Cobertura ampla;
- linguagem clara de candidatos comparáveis;
- validação completa no navegador com dados reais.

## Futuro

- comparação lado a lado;
- pesquisas salvas;
- observações de mercado e evidência comercial com vendas como prioridade;
- histórico de métricas;
- interfaces read-only para agentes;
- analytics e previsões somente após validar o fluxo central.
