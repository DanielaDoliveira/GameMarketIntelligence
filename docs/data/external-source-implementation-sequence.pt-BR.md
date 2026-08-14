# Sequência de Implementação das Fontes Externas

> Data da decisão: 29 de julho de 2026
>
> Revisão de status: 14 de agosto de 2026 — a PoC da IGDB está concluída e
> aprovada. A sequência continua com o spike leve de compatibilidade e o
> Collector definitivo e limpo; os milestones de fontes futuras permanecem
> inalterados.

## Objetivo

Esclarecer a ordem de ativação das fontes já selecionadas sem alterar o escopo multifonte geral do produto.

## Decisão

IGDB, Wikidata e Steam continuam planejadas.

Serão ativadas por incrementos verticais separados:

### Milestone 2

- IGDB será a primeira fonte ativa;
- PoC da IGDB concluída e aprovada;
- executar spike leve de compatibilidade multifonte;
- implementar coleta, mapping, persistência, deploy, API, frontend, proveniência e confiabilidade de ponta a ponta.

### Milestone 3

- executar PoC e integração Wikidata;
- executar PoC e integração Steam;
- comparar observações;
- implementar reconciliação e confiança;
- apresentar convergências e divergências.

## Condição arquitetural

A implementação IGDB deve preservar:

- IDs internos canônicos;
- múltiplas identidades externas;
- contratos e mappers por fonte;
- fronteira de importação independente da fonte;
- proveniência;
- releases contextuais;
- metadados específicos;
- pontos de extensão para reconciliação conservadora.

## Fora do escopo do spike do Milestone 2

O spike não irá:

- autenticar em Wikidata ou Steam;
- implementar clients produtivos;
- definir mapping final;
- definir algoritmo completo de reconciliação;
- antecipar todos os campos específicos.

Seu objetivo é apenas identificar incompatibilidades estruturais antes da
aprovação do mapping canônico definitivo e de mudanças significativas de
persistência. O spike não reabre a investigação já concluída da IGDB.
