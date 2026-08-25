# Modelo de Domínio de Comparable Games

## Propósito

Definir o modelo de domínio atual e o comportamento de leitura que sustentam a funcionalidade Comparable Games no GameMarketIntel.

O modelo deve fornecer informação suficiente para responder perguntas iniciais de produção e análise de mercado, permanecendo source-neutral, consciente de proveniência e compatível com as restrições de armazenamento do MVP.

O modelo é intencionalmente pragmático. Ele evoluiu da fundação inicial de Comparable Games por meio dos incrementos de identidade de fonte, releases contextuais, classificações, relações de produto, empresas e collections validados durante o Milestone 2.

## Perguntas de produto

A vertical Comparable Games deve ajudar a responder:

* Quais jogos podem ser considerados comparáveis?
* Quais gêneros estão associados a cada jogo?
* Quais plataformas estão associadas a cada jogo?
* Quando cada jogo foi lançado pela primeira vez?
* Quais jogos correspondem a uma busca parcial por nome?
* Quais jogos correspondem a um gênero selecionado?
* Quais jogos correspondem a uma plataforma selecionada?
* Quais jogos foram lançados em um ano selecionado?
* Qual é o tipo do produto, quando conhecido?
* Quais produtos relacionados são conhecidos para um jogo?
* Quais empresas estão associadas a um jogo e em quais papéis?
* Quais collections contêm ou contextualizam um jogo?
* Quais registros de fonte sustentam fatos contextuais e associações?
* Quais limitações afetam a interpretação dos dados?

O modelo deve futuramente suportar:

* filtros avançados de Comparable Games;
* análise de saturação por gênero;
* análise de janela de lançamento;
* comparação por plataforma;
* detalhes com contexto de fonte;
* indicadores históricos de mercado;
* comparação de performance comercial.

## Escopo de domínio

O escopo atual de domínio e persistência inclui:

* `Game`;
* `Genre`;
* `Platform`;
* `DataSource`;
* `SourceReliability`;
* `ExternalGameRecord`;
* `GameRelease` contextual;
* `Theme`;
* `GameMode`;
* `PlayerPerspective`;
* `Keyword`;
* identidades externas para classificações aprovadas;
* associações jogo/classificação com proveniência;
* `GameProductType`;
* `GameProductRelation`;
* `Company`;
* `ExternalCompanyRecord`;
* `GameCompanyRole`;
* `GameCompany`;
* `Collection`;
* `ExternalCollectionRecord`;
* `GameCollection`;
* `GameImage`;
* `GameImageType`;
* resolução de URL de imagem consciente da fonte;
* relacionamentos jogo-gênero;
* relacionamentos jogo-plataforma;
* contratos de leitura da busca atual de Comparable Games.

Nem todo conceito persistido está exposto publicamente pela API ou frontend neste momento.

O modelo de persistência pode conter mais informação tratada do que um contrato específico de API ou uma tela específica precisam. Application e Shared devem expor apenas a informação necessária a cada caso de uso.

## Status atual

Implementado:

* `Game`, `Genre` e `Platform` canônicos;
* relação N:N entre jogo e gênero;
* relação N:N entre jogo e plataforma;
* normalização do nome do jogo com índice de lookup não único;
* unicidade por nome normalizado para classificações canônicas, empresas e collections quando aplicável;
* identidade externa de fonte via `DataSource + ExternalId`;
* modelagem de releases contextuais com proveniência;
* classificações consultáveis aprovadas:
  * themes;
  * game modes;
  * player perspectives;
  * keywords;
* associações de classificação com proveniência;
* modelagem de tipo de produto;
* relações direcionadas entre produtos com proveniência;
* modelagem canônica de empresas;
* identidades externas de empresas;
* papéis de empresas em jogos com proveniência;
* modelagem canônica de collections;
* identidades externas de collections;
* associações jogo/collection com proveniência;
* delete behavior restritivo para relações com proveniência;
* configurações do EF Core;
* migrations PostgreSQL;
* persistência PostgreSQL;
* testes de domínio;
* integration tests com PostgreSQL;
* contratos de busca de Comparable Games;
* serviço de busca na Application;
* abstração de repositório;
* repositório PostgreSQL de busca;
* filtro por nome parcial case-insensitive;
* filtro por gênero;
* filtro por plataforma;
* filtro por ano de primeiro lançamento;
* combinação AND entre categorias de filtro;
* ordenação alfabética;
* paginação;
* metadados de paginação;
* `GET /api/games`;
* `GET /api/games/{id:guid}`;
* `GET /api/genres`;
* `GET /api/platforms`;
* FluentValidation para parâmetros de busca;
* documentação OpenAPI e Scalar;
* tratamento centralizado de exceções;
* respostas padronizadas com `ProblemDetails`;
* experiência responsiva de Comparable Games no Blazor;
* ação visível de Search;
* estados de loading, error, empty e no-results;
* 460 testes automatizados passando durante o quality gate de implementação da GMI-29.

Ainda não implementado na experiência pública de leitura:

* contratos públicos de API para product type;
* contratos públicos de API para relações de produto;
* contratos públicos de API para empresas e seus papéis;
* contratos públicos de API para collections;
* contratos públicos de API para releases contextuais e detalhes de proveniência;
* filtros avançados usando as novas classificações e metadados de produto persistidos;
* ingestão pelo Worker do subconjunto completo aprovado do Milestone 2;
* reconciliação multi-fonte;
* observações de métricas comerciais.

## Game

### Propósito

Representa um produto de jogo canônico usado em comparação e análise de mercado.

Um `Game` canônico não fica permanentemente preso a um provider. Identidade específica de fonte e evidência vivem em registros externos e entidades contextuais com proveniência.

### Propriedades implementadas

| Propriedade | Obrigatória | Propósito |
| --- | ---: | --- |
| `Id` | Sim | Identidade canônica interna |
| `Name` | Sim | Nome para exibição |
| `NormalizedName` | Sim | Nome técnico normalizado usado em lookup e descoberta de candidatos |
| `Description` | Não | Contexto descritivo curto |
| `FirstReleaseDate` | Não | Primeira data canônica conhecida do produto, usada pelo filtro atual por ano |
| `ImageUrl` | Não | Referência de imagem externa selecionada |
| `ProductType` | Não | Tipo canônico do produto, quando conhecido |

### Normalização de nome

`Name` preserva o valor de exibição após `Trim()`.

`NormalizedName` é derivado do nome tratado usando uppercase invariant.

Exemplo:

```text
Name:
The Legend of Zelda: Ocarina of Time

NormalizedName:
THE LEGEND OF ZELDA: OCARINA OF TIME
```

`Game.NormalizedName` não é único de propósito.

Produtos, edições, ports ou registros distintos podem legitimamente ter o mesmo título ou títulos muito semelhantes. Um nome normalizado ajuda em lookup e comparação de candidatos, mas não prova identidade cross-source.

### Relacionamentos implementados

Um jogo pode:

* ter múltiplos gêneros;
* estar disponível em múltiplas plataformas;
* ter múltiplos registros contextuais de release;
* ter múltiplos registros externos de fonte;
* ter múltiplos themes;
* ter múltiplos game modes;
* ter múltiplas player perspectives;
* ter múltiplas keywords;
* ter múltiplas associações com empresas;
* pertencer a múltiplas collections;
* participar de relações direcionadas entre produtos.

Nenhuma relação de produto propaga automaticamente gêneros, plataformas, releases, empresas, collections, classificações ou outros campos de um `Game` para outro.

## Primeira data de lançamento e releases contextuais

### `Game.FirstReleaseDate`

`FirstReleaseDate` é um valor canônico resumido usado pelo filtro atual de Comparable Games por ano de lançamento.

Ele responde:

> Qual é a primeira data de lançamento conhecida para este produto canônico?

É nullable porque a cobertura da fonte pode ser incompleta.

Não substitui observações detalhadas de release.

### `GameRelease`

`GameRelease` representa evidência contextual de lançamento.

Ele preserva o contexto relevante, incluindo:

* `Game` canônico;
* `Platform`;
* `ExternalGameRecord` de origem;
* identidade externa do release;
* precisão/componentes da data;
* região quando disponível;
* status quando disponível;
* metadados de observação.

Essa separação permite distinguir:

```text
Game.FirstReleaseDate
→ valor canônico resumido / usado em filtro

GameRelease
→ ocorrência de release específica por plataforma/região/fonte
```

Ports, remakes, remasters, bundles, expansões e outros produtos distintos não são colapsados em um único jogo apenas porque estão relacionados.

## Product Type

`GameProductType` descreve o que o próprio produto é.

Vocabulário atual do domínio:

* `MainGame`;
* `Dlc`;
* `Expansion`;
* `Bundle`;
* `StandaloneExpansion`;
* `Mod`;
* `Episode`;
* `Season`;
* `Remake`;
* `Remaster`;
* `ExpandedGame`;
* `Port`;
* `Fork`;
* `PackAddon`;
* `Update`.

`Game.ProductType` é nullable.

Tipo de produto desconhecido é representado por `null`, e não por um valor sintético `Unknown`.

O enum é source-neutral. Adapters de provider devem mapear valores da fonte explicitamente e não podem depender de igualdade entre IDs numéricos do provider e valores numéricos do enum de domínio.

## Relações entre produtos

### Propósito

`GameProductRelation` representa uma relação direcionada entre dois produtos canônicos.

`GameProductType` responde:

> O que este produto é?

`GameProductRelationType` responde:

> Como este produto se relaciona com outro produto?

### Vocabulário atual de relações

* `DlcOf`;
* `ExpansionOf`;
* `StandaloneExpansionOf`;
* `RemakeOf`;
* `RemasterOf`;
* `PortOf`;
* `ExpandedGameOf`;
* `EpisodeOf`;
* `SeasonOf`;
* `ForkOf`;
* `PackAddonOf`;
* `UpdateOf`;
* `BundleContains`;
* `VersionOf`.

Exemplo:

```text
The Legend of Zelda: Ocarina of Time 3D
    └── RemakeOf → The Legend of Zelda: Ocarina of Time
```

### Proveniência

Uma `GameProductRelation` preserva:

* `SourceGameId`;
* `TargetGameId`;
* `ExternalSourceGameRecordId`;
* `ExternalTargetGameRecordId`;
* `RelationType`.

Os dois registros externos de jogo devem:

* já estar vinculados a jogos canônicos;
* pertencer à mesma `DataSource`.

Um jogo não pode se relacionar consigo mesmo em um único registro de relação.

O mesmo par de registros externos de origem/destino mais o tipo da relação é único.

Todas as FKs de persistência usam delete behavior restritivo.

## Genre

### Propósito

Representa um gênero canônico usado para filtro, comparação, agregação e análise de saturação.

### Propriedades implementadas

| Propriedade | Obrigatória | Propósito |
| --- | ---: | --- |
| `Id` | Sim | Identidade interna |
| `Name` | Sim | Nome para exibição |
| `NormalizedName` | Sim | Valor normalizado usado em unicidade e comparação |

### Relacionamentos

* Um gênero pode estar associado a múltiplos jogos.
* Um jogo pode estar associado a múltiplos gêneros.

A classificação de gênero pode diferir entre fontes.

A unicidade por nome normalizado evita conceitos canônicos duplicados causados apenas por casing ou formatação. Equivalência cross-source ainda exige mapping consciente de fonte e não pode ser inferida apenas pelo nome normalizado.

## Platform

### Propósito

Representa uma plataforma canônica usada para filtro, contexto de lançamento, comparação de mercado e análise por plataforma.

### Propriedades implementadas

| Propriedade | Obrigatória | Propósito |
| --- | ---: | --- |
| `Id` | Sim | Identidade interna |
| `Name` | Sim | Nome da plataforma |
| `Family` | Não | Família/ecossistema |
| `Manufacturer` | Não | Fabricante |
| `ImageUrl` | Não | Referência de imagem externa |

### Relacionamentos

* Uma plataforma pode estar associada a múltiplos jogos.
* Um jogo pode estar associado a múltiplas plataformas.
* Um `GameRelease` contextual pertence a uma plataforma.

Imagens de plataforma e jogo são referenciadas por URL, e não armazenadas como binário.

## Classificações consultáveis

As classificações consultáveis aprovadas para o Milestone 2 são modeladas como entidades canônicas separadas:

* `Theme`;
* `GameMode`;
* `PlayerPerspective`;
* `Keyword`.

Cada classificação canônica possui:

* `Id`;
* `Name`;
* `NormalizedName`.

Cada classificação também possui seu próprio registro de identidade externa:

* `ExternalThemeRecord`;
* `ExternalGameModeRecord`;
* `ExternalPlayerPerspectiveRecord`;
* `ExternalKeywordRecord`.

A identidade de fonte segue:

```text
DataSourceId + ExternalId
```

As associações entre jogo e classificação preservam ambos:

* os IDs canônicos usados pelo GMI;
* os IDs dos registros externos que sustentam a associação.

Isso impede que a relação canônica perca sua proveniência.

As associações atuais são:

* `GameTheme`;
* `GameGameMode`;
* `GamePlayerPerspective`;
* `GameKeyword`.

A exposição pública dessas classificações como filtros continua sendo decisão futura de API/frontend.

## Company

### Propósito

Representa uma empresa canônica source-neutral.

Uma empresa não é classificada globalmente como developer ou publisher. O papel pertence à relação entre uma empresa e um jogo específico.

### Propriedades implementadas

| Propriedade | Obrigatória | Propósito |
| --- | ---: | --- |
| `Id` | Sim | Identidade canônica interna |
| `Name` | Sim | Nome para exibição |
| `NormalizedName` | Sim | Valor normalizado usado em unicidade canônica |

`Company.NormalizedName` é único no modelo atual do Milestone 2.

### Identidade externa de empresa

`ExternalCompanyRecord` preserva:

* `DataSourceId`;
* `ExternalId`;
* `CompanyId` opcional;
* `FirstSeenAt`;
* `LastSeenAt`;
* `SourceUpdatedAt` opcional.

`DataSourceId + ExternalId` é único.

Um registro externo de empresa pode permanecer sem vínculo até existir evidência suficiente.

Depois de vinculado, ele não pode ser silenciosamente redirecionado para outra empresa canônica.

### Papel da empresa no jogo

`GameCompanyRole` atualmente inclui:

* `Developer`;
* `Publisher`;
* `Porting`;
* `Supporting`.

### `GameCompany`

`GameCompany` representa a relação N:N com proveniência entre jogos e empresas.

Ela preserva:

* `GameId`;
* `CompanyId`;
* `ExternalGameRecordId`;
* `ExternalCompanyRecordId`;
* `Role`.

A mesma empresa pode:

* participar de muitos jogos;
* ter papéis diferentes em jogos diferentes;
* ter múltiplos papéis sustentados para o mesmo jogo.

A identidade de persistência é:

```text
ExternalGameRecordId
+ ExternalCompanyRecordId
+ Role
```

Todas as FKs usam delete behavior restritivo.

Participação de empresa é contexto/evidência. Não é prova automática de oficialidade, autorização ou legitimidade comercial.

## Collection

### Propósito

Representa uma collection canônica source-neutral.

Collections estão aprovadas para o modelo de persistência atual do MVP.

Franchises permanecem adiadas.

### Propriedades implementadas

| Propriedade | Obrigatória | Propósito |
| --- | ---: | --- |
| `Id` | Sim | Identidade canônica interna |
| `Name` | Sim | Nome para exibição |
| `NormalizedName` | Sim | Valor normalizado usado em unicidade canônica |

`Collection.NormalizedName` é único no modelo atual do Milestone 2.

### Identidade externa de collection

`ExternalCollectionRecord` preserva:

* `DataSourceId`;
* `ExternalId`;
* `CollectionId` opcional;
* `FirstSeenAt`;
* `LastSeenAt`;
* `SourceUpdatedAt` opcional.

`DataSourceId + ExternalId` é único.

Um registro externo de collection pode permanecer sem vínculo e, uma vez vinculado, não pode ser silenciosamente relinkado para outra collection canônica.

### `GameCollection`

`GameCollection` representa a relação N:N com proveniência entre jogos e collections.

Ela preserva:

* `GameId`;
* `CollectionId`;
* `ExternalGameRecordId`;
* `ExternalCollectionRecordId`.

A identidade composta de persistência é:

```text
ExternalGameRecordId
+ ExternalCollectionRecordId
```

Uma collection pode conter múltiplos jogos, e um jogo pode participar de mais de uma collection quando houver evidência de fonte.

Todas as FKs usam delete behavior restritivo.

## Data Source e identidade externa

### Identidade canônica

`Game.Id` é a identidade canônica do GMI.

A identidade canônica não pode depender de Steam, IGDB ou qualquer outra fonte única.

### Identidade externa

Um registro externo é identificado com segurança por:

```text
DataSourceId + ExternalId
```

`ExternalGameRecord` preserva a identidade de fonte de uma observação de jogo e pode opcionalmente se vincular a um `Game` canônico.

O mesmo padrão geral é usado para classificações, empresas e collections.

### Regra source-neutral

O domínio deve evitar propriedades permanentes específicas de provider, como:

```text
IgdbId
SteamId
WikidataId
```

IDs específicos de provider pertencem aos registros de identidade externa.

Nomes normalizados podem ajudar a descobrir candidatos de reconciliação, mas nunca provam equivalência por si só.

Reconciliação multi-fonte permanece fora da implementação atual do Milestone 2.

## Modelo de proveniência

O GMI armazena dados canônicos tratados mais a evidência mínima de fonte necessária para entender, auditar, remover ou futuramente reconciliar uma contribuição.

A proveniência é preservada em entidades contextuais e associações onde a fonte importa materialmente.

Exemplos:

* releases;
* associações de classificações consultáveis;
* relações entre produtos;
* papéis de empresas em jogos;
* associação jogo/collection;
* metadados de imagem do jogo.

O design atual evita deliberadamente uma tabela genérica polimórfica de histórico por campo.

Proveniência mais granular em nível de campo só deve ser introduzida se requisitos reais de produto ou legais justificarem o custo de armazenamento e complexidade.

## Modelo de relacionamentos implementado

O modelo atual inclui:

```text
Game
  ├── N:N → Genre
  ├── N:N → Platform
  ├── identidades de fonte → ExternalGameRecord
  ├── releases contextuais → GameRelease → Platform
  ├── associações com proveniência → Theme
  ├── associações com proveniência → GameMode
  ├── associações com proveniência → PlayerPerspective
  ├── associações com proveniência → Keyword
  ├── associações com proveniência → Company + Role
  ├── associações com proveniência → Collection
  ├── metadados de imagem derivados da fonte → GameImage
  └── relações direcionadas com proveniência → Game
```

Esse modelo de persistência é mais rico do que o contrato público atual de busca.

A API deve expor contratos de leitura source-neutral, e não retornar entidades de domínio ou EF Core diretamente.

## Busca de Comparable Games

### Contrato de busca atual

O contrato atual permanece:

```text
Search
GenreId
PlatformId
ReleaseYear
Page
PageSize
```

O endpoint atual aceita:

* um texto opcional de busca;
* um identificador opcional de gênero;
* um identificador opcional de plataforma;
* um ano opcional de lançamento;
* parâmetros de paginação.

Os novos conceitos persistidos em GMI-27/GMI-28 ainda não são parâmetros públicos de busca.

### Semântica da busca

Categorias diferentes de filtro usam semântica AND.

```text
Condição de busca
AND
Condição de gênero
AND
Condição de plataforma
AND
Condição de ano de lançamento
```

A implementação atual não aceita múltiplos gêneros ou plataformas na mesma requisição.

Filtros avançados permanecem um incremento futuro de API/frontend.

### Busca parcial por nome

A busca por nome:

* faz trim do termo;
* usa PostgreSQL `ILike`;
* ignora casing;
* encontra o termo em qualquer parte do nome do jogo.

`Game.NormalizedName` existe para consistência técnica e descoberta de candidatos, mas não substitui a semântica atual da busca parcial.

### Paginação

O resultado da busca expõe:

```text
Items
Page
PageSize
TotalItems
TotalPages
```

Regras:

* `Page` deve ser maior ou igual a `1`;
* `PageSize` deve ficar entre `1` e `100`;
* `Page` padrão é `1`;
* `PageSize` padrão é `20`.

O repositório:

1. aplica filtros;
2. conta todos os registros correspondentes;
3. ordena os jogos alfabeticamente;
4. pula registros de páginas anteriores;
5. pega apenas o tamanho solicitado;
6. projeta o resultado para contratos de leitura.

### Busca por ano de lançamento

`ReleaseYear` usa `Game.FirstReleaseDate`.

Quando informado:

* jogos sem `FirstReleaseDate` são excluídos;
* o ano armazenado deve corresponder ao informado;
* o ano não pode ser maior que o ano atual.

As linhas detalhadas de `GameRelease` ainda não dirigem o filtro público atual por ano.

## Resposta atual de Comparable Games

A resposta pública de busca contém:

```text
Identificador do jogo
Nome do jogo
Descrição
Data de lançamento
Image URL
Gêneros
Plataformas
```

O contrato público continua usando um campo de data de lançamento amigável ao usuário, mesmo que a propriedade canônica de domínio agora se chame `FirstReleaseDate`.

Gêneros e plataformas são retornados como categorias leves de leitura.

O frontend atual não recebe automaticamente todos os conceitos persistidos em GMI-27/GMI-28.

Contratos futuros de API podem expor informação selecionada como:

* product type;
* produtos relacionados;
* empresas e papéis;
* collections;
* releases contextuais;
* informação de fonte.

Essa exposição deve ser dirigida por casos de uso do produto, e não pela forma do banco.

## Restrições de armazenamento

O limite de armazenamento da Neon Free continua sendo uma restrição de produto.

O modelo prioriza:

* dados estruturados normalizados;
* identidades de fonte;
* valores canônicos selecionados;
* registros contextuais necessários às perguntas de produto;
* proveniência necessária para auditoria, remoção, atribuição e reconciliação futura.

O banco operacional deve evitar:

* payloads brutos de API;
* respostas JSON completas;
* páginas HTML;
* binários de imagem;
* duplicações desnecessárias entre fontes;
* espelhamento completo de providers auxiliares.

O crescimento de armazenamento pode exigir:

* ingestão seletiva por fonte;
* menor granularidade histórica;
* agregação;
* políticas de retenção;
* adiamento de features intensivas em armazenamento.

## Delete behavior e integridade

Relações com proveniência introduzidas no Milestone 2 usam deletes restritivos.

Um registro canônico ou externo referenciado não pode ser removido enquanto ainda existirem associações dependentes.

Integration tests representativos validam restrições envolvendo:

* relações de produto;
* empresas;
* collections;
* registros externos de jogo.

O reset do banco de integration tests deve incluir toda tabela adicionada por migrations concluídas, para evitar vazamento de estado no fixture PostgreSQL compartilhado.

## Responsabilidades de validação

A validação continua dividida por responsabilidade.

### Validação de domínio

Entidades de domínio protegem invariantes independentes do chamador.

Exemplos:

* nomes obrigatórios;
* trimming e normalização;
* limites máximos;
* validade de identidade externa;
* consistência de vínculos;
* exigência de mesma fonte em associações com proveniência;
* prevenção de auto-relação entre produtos;
* prevenção de relink silencioso.

### Validação de Application

Validators da Application protegem inputs de caso de uso.

Regras atuais da busca incluem:

```text
Page >= 1
PageSize entre 1 e 100
ReleaseYear <= ano atual
```

### Validação de persistência

Constraints do EF Core/PostgreSQL aplicam integridade de persistência como:

* primary keys;
* composite keys;
* identidades externas únicas;
* unicidade de nomes normalizados canônicos quando aprovada;
* foreign keys;
* delete behavior restritivo;
* prevenção de proveniência duplicada.

## Progresso de implementação

Fundação concluída:

1. fundação de `DataSource` e confiabilidade;
2. `Genre`, `Platform` e `Game` canônicos;
3. relações jogo-gênero e jogo-plataforma;
4. API de busca e detalhes de Comparable Games;
5. APIs de listagem de gêneros e plataformas;
6. frontend responsivo de Comparable Games;
7. pesquisa inicial de fontes reais e PoC IGDB;
8. design source-neutral de identidade e proveniência.

Incrementos de persistência concluídos no Milestone 2:

1. **GMI-25** — identidade externa de fonte;
2. **GMI-26** — releases contextuais com proveniência;
3. **GMI-27** — classificações consultáveis aprovadas;
4. **GMI-28** — tipos de produto, relações entre produtos, empresas, papéis de empresa, collections, proveniência, constraints, mappings, testes e migrations.

Quality gate final da GMI-28:

```text
Build: passou
Testes: 424 passaram
Falhas: 0
Ignorados: 0
```

## Próximos passos de implementação

Dentro do milestone de persistência:

1. GMI-29 — metadados de cover e screenshots;
2. GMI-30 — validação de persistência e orçamento de armazenamento.

Depois de concluir o modelo de persistência aprovado:

1. implementar o fluxo de ingestão do Worker/Collector para o subconjunto aprovado da fonte;
2. popular o dataset canônico com dados reais tratados;
3. expor novos conceitos selecionados por contratos de Application/API;
4. decidir quais informações persistidas entram nos cards, detalhes e filtros avançados de Comparable Games;
5. preservar apresentação source-aware e atribuição;
6. validar o fluxo completo de produção com dados reais.

O frontend deve organizar os contratos específicos de caso de uso expostos pela API. Ele não deve espelhar o schema do banco nem expor todo identificador interno de proveniência.

## Conceitos adiados

Os conceitos abaixo continuam deliberadamente adiados, a menos que uma issue futura os ative explicitamente:

* reconciliação multi-fonte;
* franchises;
* tabelas genéricas de histórico em nível de campo;
* múltiplos gêneros em uma única requisição de busca atual;
* múltiplas plataformas em uma única requisição de busca atual;
* scoring avançado de similaridade;
* sistemas de recomendação;
* observações de vendas e receita;
* snapshots históricos de métricas comerciais;
* machine learning;
* filtros públicos para toda classificação persistida;
* propagação automática de dados entre produtos relacionados.

## Princípio de design

O modelo de domínio deve sustentar perguntas de produto validadas sem se tornar uma cópia do schema de uma API externa.

O princípio orientador continua:

> Modelar a complexidade exigida por decisões de produto validadas, não a complexidade que pode existir em todo cenário futuro.

Um segundo princípio agora também se aplica:

> Preservar identidade e proveniência suficientes para explicar de onde um fato ou relação veio sem obrigar a API pública ou o frontend a expor todo o modelo de persistência.
