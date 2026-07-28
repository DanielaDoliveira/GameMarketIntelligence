# Game Market Intelligence — Benchmark de Fontes Externas

> Revisado em: 27 de julho de 2026

## Objetivo

Comparar fontes externas para o MVP do GMI, priorizando valor para producers e, em seguida, confiabilidade, elegibilidade legal, sustentabilidade com custo zero, cobertura, complementaridade e viabilidade técnica.

## Regras eliminatórias

- Custo operacional obrigatório: R$ 0.
- Nenhum scraping não autorizado ou mecanismo não documentado.
- Armazenamento, normalização, exibição pública, atribuição e exposição downstream devem ser compatíveis com os termos.
- A confiabilidade é avaliada por categoria de dado.
- Uma nova fonte deve acrescentar valor distinto, não apenas duplicar o conjunto selecionado.

## Comparação

| Fonte | Melhor papel | Principal força | Principal limitação | Decisão |
|---|---|---|---|---|
| IGDB | Catálogo geral principal | Metadados estruturados amplos e filtros avançados | Atribuição e limites contratuais continuam aplicáveis | Selecionada |
| Wikidata | Reconciliação e enriquecimento | Dados CC0, IDs, aliases e relações semânticas | Qualidade e cobertura variam por afirmação | Selecionada |
| Steam | Fonte oficial especializada | IDs, páginas e fatos oficiais do ecossistema Steam | Específica da plataforma; permissões por endpoint | Selecionada |
| RAWG | Catálogo complementar | API ampla e acessível | Grande sobreposição com IGDB e limites downstream pouco claros | Não selecionada; reconsideração condicional |
| MobyGames | Catálogo histórico curado | Profundidade histórica, lançamentos e créditos | Acesso adequado é pago ou discricionário | Não selecionada |
| SteamDB | Pesquisa independente sobre Steam | Histórico de preços, jogadores e alterações | Sem API pública; scraping e crawling proibidos | Somente referência |
| Nintendo | Validação oficial | Autoridade sobre produtos Nintendo | Nenhuma API pública autorizada de catálogo de terceiros identificada | Sem ingestão no MVP |
| Microsoft/Xbox | Validação oficial | Autoridade sobre Microsoft Store/Xbox | APIs orientadas a publishers, usuários ou Partner Center | Sem ingestão no MVP |
| PlayStation | Validação oficial | Autoridade sobre fatos PlayStation | Nenhum mecanismo público autorizado adequado identificado | Sem ingestão no MVP |
| Marketplaces gerais | Evidência de varejo | Confirmação de listagens e edições | Baixa adequação canônica e termos comerciais restritivos | Não selecionados |

## Conjunto final

| Fonte | Responsabilidade |
|---|---|
| IGDB | Catálogo e filtros de Comparable Games |
| Wikidata | Reconciliação, aliases, IDs e relações |
| Steam | Fatos e referências oficiais específicos da Steam |

A SteamDB pode ser citada como ferramenta externa opcional de pesquisa manual, mas não alimentará workers, armazenamento, API ou agentes do GMI.

## Consequência

A etapa de benchmarking está concluída. O próximo trabalho é definir campos, mappings, regras de reconciliação, prova de conceito de ingestão, revisão do domínio e somente depois migrations.
