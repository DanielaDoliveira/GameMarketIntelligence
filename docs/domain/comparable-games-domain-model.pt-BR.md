# Modelo de Domínio de Comparable Games

> Status da revisão: atualizado pela GMI-13 após a aprovação da PoC da IGDB e
> do spike de compatibilidade multifonte em 18 de agosto de 2026.

## Objetivo

Descrever a base implementada e a direção aprovada após a seleção de fontes. O domínio deve responder perguntas validadas de producers sem copiar os esquemas das APIs externas.

## Implementação atual

- `Game`, `Genre`, `Platform`, `DataSource` e `SourceReliability`;
- relações muitos-para-muitos entre jogo/gênero e jogo/plataforma;
- invariantes e unicidade por nome normalizado;
- persistência PostgreSQL e configuração EF Core;
- pesquisa por nome parcial, gênero, plataforma e ano;
- semântica AND entre categorias;
- ordenação alfabética e paginação;
- endpoints de jogo, detalhes, gêneros e plataformas;
- validação e erros padronizados;
- experiência responsiva em Blazor.

## `Game` atual

| Propriedade | Obrigatória | Significado |
|---|---:|---|
| `Id` | Sim | Identidade interna |
| `Name` | Sim | Nome de exibição |
| `Description` | Não | Contexto curto |
| `ReleaseDate` | Não | Data conhecida simplificada |
| `ImageUrl` | Não | Referência externa opcional |

A data atual não representa todos os eventos por plataforma, região, Early Access, port, remake ou remaster.

## Base canônica e papéis aprovados

O banco representa uma base canônica própria do GMI, não uma cópia permanente
da IGDB. No primeiro MVP, a IGDB é a única fonte ativa e fornece a taxonomia e
os valores iniciais. Em incrementos futuros, o GMI poderá materializar valores
selecionados de várias fontes sem manter cópias completas ou versões
intercambiáveis de cada catálogo.

- IGDB: fonte-base do catálogo e fallback geral quando não houver uma fonte
  contextual mais apropriada;
- Wikidata: preenchimento de lacunas, enriquecimento, identificadores cruzados
  e detecção de inconsistências;
- Steam: fonte especializada e prioritária somente para fatos do próprio
  ecossistema Steam;
- fontes oficiais futuras: prioridade limitada ao campo e ao contexto em que
  possuam autoridade e acesso autorizado.

Não existe precedência absoluta entre providers. A seleção ocorre por campo e
contexto. Um mesmo jogo poderá, por exemplo, usar capa e gêneros da IGDB,
identificadores da Wikidata, lançamento de Switch da IGDB e lançamento na Steam
da própria Steam.

IDs de providers não devem virar propriedades permanentes específicas em `Game`.

## Direção arquitetural aprovada

O spike leve de compatibilidade multifonte confirmou a separação entre registro
externo e entidade canônica. A decisão completa está registrada em
[`ADR-0003`](../architecture/adr/ADR-0003-external-source-identity-and-provenance.md).
Ela descreve uma direção extensível, não a obrigação de entregar todas as
fontes e capacidades analíticas futuras no primeiro MVP com IGDB.

```text
DataSource
└── ExternalGameRecord
    ├── ExternalId
    ├── GameId opcional
    ├── SourceUpdatedAt
    ├── LastSeenAt
    └── ProcessingStatus

Game
├── zero ou vários ExternalGameRecords vinculados
├── Genres
├── Themes
├── GameModes
├── Companies
└── Releases contextuais com proveniência
```

Conceitos de apoio futuros, introduzidos apenas quando uma necessidade concreta
os justificar:

- `MatchConfidence`;
- separação entre fonte e evidência;
- proveniência por campo ou afirmação;
- confiabilidade e conflito;
- `SourceObservation` temporal.

O GMI persiste o valor canônico selecionado e a evidência mínima necessária
para explicar sua origem. Não é requisito manter todas as observações
concorrentes nem permitir que o producer alterne o catálogo exibido por fonte.

## Proveniência mínima

`DataSource` descreve a integração e suas obrigações operacionais, incluindo
código estável, nome público, URL oficial, atribuição, status e regras de
retenção. A origem do valor não pertence apenas a `DataSource`: ela deve ser
associada ao campo ou à entidade contextual que recebeu a contribuição.

- lançamentos preservam fonte, plataforma, ecossistema, região, precisão e
  status;
- imagens preservam fonte e identificadores necessários para construir a URL,
  sem armazenar o binário;
- nomes localizados, websites, identificadores externos e relações preservam a
  fonte quando forem materializados;
- campos simples recebem proveniência enxuta apenas quando isso for necessário
  para atribuição, auditoria, remoção ou recomposição;
- a implementação evita antecipar uma tabela polimórfica genérica e pesada para
  todos os campos.

A proveniência também é uma regra operacional. Se uma fonte precisar ser
desativada ou seus dados removidos, o sistema deve localizar suas contribuições,
removê-las e, quando possível, recompor o valor usando uma fonte-base ou fallback
permitido sem alterar a identidade canônica do jogo.

## Estratégia de armazenamento

O desenho considera o limite inferior a 1 GB do banco de produção:

- não armazenar payloads brutos completos, dumps, HTML ou snapshots normais;
- não espelhar catálogos auxiliares inteiros;
- não armazenar binários de imagens;
- consultar Wikidata e Steam seletivamente para jogos já conhecidos;
- persistir somente valores aceitos, identidade externa, sincronização e
  proveniência necessárias;
- criar inicialmente apenas constraints e índices sustentados por consultas
  reais;
- medir tabelas de alta cardinalidade, como lançamentos, imagens, keywords e
  associações muitos-para-muitos, com dados representativos antes de ampliar a
  coleta.

## Impacto no produto e na API

O GMI oferece dados canônicos mesclados e auditáveis, não catálogos alternativos
selecionáveis. A API deverá expor, além dos detalhes do jogo, um resumo de
proveniência que relacione campos ou contextos às fontes que contribuíram sem
duplicar os valores.

`Data Sources` passa a ter duas funções:

1. mostrar, para o jogo selecionado, a fonte dos campos e associações relevantes;
2. explicar o papel, as limitações, a atribuição e os links de cada integração.

A natureza oficial, curada ou comunitária continua sendo evidência visível, mas
não um seletor para trocar todo o conjunto de dados exibido.

## Gap analysis da GMI-13

| Estado atual | Necessidade aprovada | Direção de implementação |
|---|---|---|
| `DataSource` não está ligado ao jogo importado | identidade externa e idempotência | introduzir registro externo associado à fonte e opcionalmente ao `Game` |
| `Game` contém `ReleaseDate` simplificada | lançamentos por contexto | modelar lançamentos por plataforma, região, precisão, status e fonte |
| `Game.ImageUrl` é uma URL direta | imagens auditáveis e econômicas | persistir metadados e IDs da fonte e construir a URL de exibição |
| proveniência não é ligada aos valores | atribuição, remoção e recomposição | fonte nas entidades contextuais e solução enxuta para campos simples |
| Data Sources é institucional | auditoria do jogo selecionado | acrescentar mapa campo/contexto → fonte e manter a seção institucional |
| visão futura permitia escolher provider | retenção seletiva não sustenta alternância | remover a seleção de catálogo por fonte |
| modelo começa com IGDB | evolução para composição seletiva | delimitar IGDB ao primeiro MVP e ao papel de fonte-base/fallback |

Regras de identidade aprovadas:

- `Game.Id` identifica o jogo canônico no GMI;
- `DataSource + ExternalId` identifica com segurança um registro dentro de uma
  fonte;
- o vínculo de um registro externo com `Game` pode permanecer ausente enquanto
  não houver evidência suficiente;
- nome normalizado auxilia busca e geração de candidatos, mas não autoriza
  reconciliação automática;
- IDs de provider não serão propriedades específicas em `Game`.

## Regras de modelagem

- necessidades do producer vêm antes dos esquemas;
- origem e proveniência precisam ser inspecionáveis;
- conflitos não são apagados silenciosamente;
- jogo base, DLC, bundle, remake, remaster e port não são unidos cegamente;
- preço, reviews, rankings e jogadores são observações;
- imagens e descrições exigem revisão própria;
- payloads brutos, HTML e binários não são armazenados por padrão.
- integrações devem respeitar licença, endpoint autorizado, atribuição, retenção
  e remoção específicas de cada fonte;
- SteamDB permanece somente como referência manual e não alimenta Worker,
  persistência ou API.

## Gate de migration

Nenhuma migration importante antes de:

1. fixar perguntas e filtros necessários ao MVP atual com IGDB;
2. aprovar pela PoC concluída os campos permitidos da IGDB e sua nulabilidade;
3. aplicar a compatibilidade confirmada pelo spike e pelo `ADR-0003` para
   identidade canônica, registros externos, proveniência e contratos por fonte;
4. revisar domínio e arquitetura de ingestão;
5. limitar a migration aos conceitos justificados pela iteração atual.

A primeira migration não precisa implementar reconciliação produtiva,
observações concorrentes, escolha de fonte pelo usuário ou histórico genérico
por campo. Ela deve, porém, evitar uma estrutura que impeça proveniência,
remoção e recomposição quando a segunda fonte for integrada.

O mapping completo de Wikidata e Steam não é pré-requisito para o primeiro
incremento de persistência da IGDB. Ele permanece como gate de iterações
futuras antes da integração dessas fontes. Assim a visão multifonte é
preservada sem forçar campos futuros no schema atual.
