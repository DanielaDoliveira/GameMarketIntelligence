# Refinamento do MVP de Comparable Games

> Status da revisão: atualizado após a aprovação da PoC da IGDB em 14 de agosto
> de 2026.

## MVP completo

O MVP completo reúne três capacidades:

1. Comparable Games com filtros básicos e avançados sustentados por dados aprovados.
2. Data Sources com a origem dos campos do jogo selecionado e informações
   institucionais sobre confiabilidade, limitações, atribuição, atualização e
   URLs das integrações.
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

Data Sources deixa de ser apenas um catálogo institucional. Para o jogo
selecionado, mostra quais fontes contribuíram e relaciona cada campo ou contexto
relevante à sua origem, sem repetir os valores já apresentados nos detalhes do
jogo. Um link nos detalhes poderá abrir essa auditoria diretamente.

Em uma seção complementar, para cada fonte, mostrar:

- organização e status oficial/independente;
- categorias fornecidas;
- confiabilidade por categoria;
- limitações;
- coleta/atualização;
- atribuição e restrições;
- URL original.

O GMI materializa um resultado canônico segundo regras por campo e contexto. O
producer não escolhe entre cópias alternativas do catálogo por provider porque
essas cópias não serão mantidas. A natureza oficial, curada ou comunitária
continua visível como evidência e limitação.

## Modos de confiabilidade

- **Alta confiança:** oficial, primário, fortemente verificado ou concordância confiável; cobertura menor é esperada.
- **Equilibrado:** dados oficiais, curados e normalizados com proveniência aceitável; provável padrão.
- **Cobertura ampla:** pode incluir estimativas reconhecidas, comunidade e conflitos, sempre identificados.

Os modos de confiabilidade qualificam o resultado canônico; não selecionam uma
cópia diferente do catálogo nem substituem a proveniência por campo.

## Direção de domínio

O domínio pode exigir temas, modos, empresas, lançamentos, referências externas, proveniência, conflitos e observações. A primeira fonte não deve ditar o modelo.

## Regra de migration

Nenhuma migration da iteração atual com IGDB antes da revisão de necessidades,
campos aprovados, permissões, fronteiras de compatibilidade, domínio e impacto
de ingestão. Decisões detalhadas de Wikidata e Steam permanecem gates das
integrações futuras dessas fontes, não de toda persistência da IGDB.
