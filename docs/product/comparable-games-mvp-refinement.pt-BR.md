# Refinamento do MVP de Comparable Games

> Status da revisão: atualizado após a aprovação da PoC da IGDB em 14 de agosto
> de 2026.

## MVP completo

O MVP completo reúne três capacidades:

1. Comparable Games com filtros básicos e avançados sustentados por dados aprovados.
2. Data Sources com proveniência, confiabilidade, limitações, atribuição, atualização e URLs.
3. Modos Alta confiança, Equilibrado e Cobertura ampla.

## Comparable Games

Neste MVP com IGDB, o filtro de Comparable Games é a primeira entrega de valor
do produto. O conjunto de filtros é orientado por evidências: cada campo
incluído, restrito aos detalhes, adiado ou excluído reflete cobertura,
semântica, comportamento operacional e limitações legais avaliadas na PoC. A
experiência deve oferecer a maior confiança prática possível dentro da operação
com custo zero, identificando a fonte e comunicando ao producer dados ausentes
ou limitados.

Filtros básicos:

- nome;
- gênero;
- plataforma;
- período/ano.

No primeiro MVP com IGDB, a divulgação progressiva poderá acrescentar temas,
modos, empresas envolvidas e navegação contextual por keyword, sempre
qualificados pela fonte. As decisões atuais são:

- modos podem ser filtro público, tratando ausência na fonte como desconhecido;
- perspectivas são detalhes opcionais, não filtro público;
- o clique numa keyword exibida pode abrir jogos relacionados com um critério
  contextual removível;
- seleção manual, múltiplas keywords com `AND` e autocomplete foram adiados;
- capas são opcionais e não dominantes nos resultados e detalhes;
- screenshots ficam nos detalhes e em galerias abertas sob demanda.

Iterações futuras poderão avaliar ou acrescentar:

- subgênero;
- temas e tags mais ricos;
- filtro de perspectiva se a cobertura se tornar suficiente ou puder ser
  qualificada por fontes complementares;
- developer e publisher;
- status de lançamento;
- características mais profundas de single-player, multiplayer e coop;
- filtro manual com múltiplas keywords e buscas salvas.

Os resultados são candidatos comparáveis, não concorrentes diretos automáticos.

## Data Sources

Para cada fonte, mostrar:

- organização e status oficial/independente;
- categorias fornecidas;
- confiabilidade por categoria;
- limitações;
- coleta/atualização;
- atribuição e restrições;
- URL original.

## Modos de confiabilidade

- **Alta confiança:** oficial, primário, fortemente verificado ou concordância confiável; cobertura menor é esperada.
- **Equilibrado:** dados oficiais, curados e normalizados com proveniência aceitável; provável padrão.
- **Cobertura ampla:** pode incluir estimativas reconhecidas, comunidade e conflitos, sempre identificados.

O filtro de confiabilidade precede o filtro por fonte porque o producer normalmente precisa definir qualidade antes de auditar providers.

## Direção de domínio

O domínio pode exigir temas, modos, empresas, lançamentos, referências externas, proveniência, conflitos e observações. A primeira fonte não deve ditar o modelo.

## Regra de migration

Nenhuma migration da iteração atual com IGDB antes da revisão de necessidades,
campos aprovados, permissões, fronteiras de compatibilidade, domínio e impacto
de ingestão. Decisões detalhadas de Wikidata e Steam permanecem gates das
integrações futuras dessas fontes, não de toda persistência da IGDB.
