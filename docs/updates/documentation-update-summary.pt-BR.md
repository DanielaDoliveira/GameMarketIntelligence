# Resumo da Atualização da Documentação

> Atualizado em: 29 de julho de 2026

## Objetivo

Registrar as alterações de sequência dos milestones e de documentação aprovadas durante a PoC da IGDB.

## Principais decisões

- A estratégia multifonte do produto permanece inalterada.
- IGDB, Wikidata e Steam continuam planejadas com papéis distintos.
- A entrega passa a ser explicitamente incremental:
  - Milestone 2 entrega o MVP vertical com IGDB;
  - Milestone 3 entrega Wikidata, Steam e reconciliação multifonte.
- O Milestone 2 inclui um spike documental leve de compatibilidade multifonte antes da aprovação de domínio e persistência.
- As PoCs completas de Wikidata e Steam ficam para o Milestone 3, mantendo pesquisa, decisões e implementação próximas no tempo.
- A PoC da IGDB deve ser concluída antes de remodelagem significativa ou migrations.
- O Worker permanecerá um coordenador pequeno; Jobs, clients, mappers, serviços de importação e repositórios terão responsabilidades especializadas.
- O modelo canônico de `Game` não pode virar um modelo de resposta da IGDB.
- Identidade canônica, identidades externas, proveniência e contratos específicos por fonte devem ser preservados.
- O primeiro deploy do Worker faz parte da entrega do Milestone 2.
- O processo de desenvolvimento será documentado como workflow de produto orientado por evidências.

## Direção atualizada

### Milestone 2

- concluir PoC IGDB;
- executar spike multifonte;
- aprovar fronteira independente da fonte;
- refatorar e implementar Collector;
- persistir dados representativos;
- fazer deploy do Worker;
- validar API e frontend;
- entregar primeiro MVP funcional com dados reais.

### Milestone 3

- executar PoC Wikidata;
- executar PoC Steam;
- definir observações comuns;
- implementar reconciliação e confiança;
- adicionar as duas fontes;
- apresentar convergências e divergências.

## Regra de manutenção

Atualizar as duas versões de idioma no mesmo pull request sempre que uma decisão documentada mudar.
