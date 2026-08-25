# Comparable Games — Refinamento do MVP

## Objetivo

Comparable Games é a principal experiência de pesquisa do MVP do Game Market Intelligence.

Ela deve ajudar producers a descobrir e refinar jogos comparáveis candidatos por meio de pesquisa estruturada e consciente da origem dos dados.

A funcionalidade não pretende declarar que todo jogo retornado é um concorrente direto.

## Capacidade atual

A implementação existente fornece:

- busca por nome;
- filtro por gênero;
- filtro por plataforma;
- filtro por ano de lançamento;
- botão Search visível;
- envio combinado do formulário;
- estado da busca na URL;
- paginação;
- estados de loading, erro, vazio e sem resultados;
- cards responsivos de jogos.

Esse é um fluxo básico de descoberta sólido, mas ainda representa uma etapa superficial de comparação.

## Nota de status de implementação — 25 de agosto de 2026

Os refinamentos de domínio e persistência previstos por este documento avançaram substancialmente por meio da GMI-25 até a GMI-29, enquanto a GMI-30 agora validou o modelo atual de persistência em cenários de migration, planos de consulta e armazenamento representativo.

As fundações implementadas agora incluem:

- identidade externa de jogo source-neutral por `DataSource + ExternalId`;
- `Game.FirstReleaseDate` separado de `GameRelease` contextual;
- releases contextuais com provenance;
- themes, game modes, player perspectives e keywords canônicos;
- identidades externas e associações com provenance para essas classificações;
- `Game.ProductType`;
- relacionamentos direcionados de produto com provenance;
- companies canônicas;
- identidades externas de company;
- roles game/company com provenance;
- collections canônicas;
- identidades externas de collection;
- associações game/collection com provenance;
- metadados de imagem de jogo derivados da fonte por meio de `GameImage`;
- `GameImageType` com `Cover` e `Screenshot`;
- identidade do registro de imagem da fonte por `ExternalId`;
- endereçamento do asset da fonte por `SourceImageId`;
- resolução source-aware de URL pública de imagem no backend;
- lookup batch de cover primário para resultados paginados;
- comportamento de delete restritivo para relacionamentos com provenance;
- mappings PostgreSQL, migrations e testes de integração.

A GMI-30 também validou que:

- a cadeia de migrations pode ser aplicada do zero no PostgreSQL;
- o banco local existente pode ser migrado para o schema atual;
- o modelo EF e o snapshot de migrations estão alinhados;
- os contratos públicos atuais permanecem compatíveis;
- filtros por gênero e plataforma usam seus índices esperados;
- o lookup batch de covers usa o índice de imagem por `GameId`;
- as buscas atuais por substring de nome e por ano de release ainda usam sequential scans no volume medido de 10.000 jogos, mas continuam baratas o suficiente para que nenhum índice adicional seja atualmente justificado;
- os índices já representam parte material do custo de armazenamento;
- associações de alta cardinalidade podem crescer muito mais rápido que a tabela canônica `Games`.

Essas adições são fundações de persistência, domínio e operação.

Elas **ainda não equivalem a funcionalidades públicas de API ou frontend**.

Filtros avançados, detalhes mais ricos, apresentação de fonte, controles de reliability e outras mudanças de UI ainda exigem decisões separadas em Application/API e frontend.

O modelo de persistência é intencionalmente mais rico que qualquer tela ou endpoint individual.

## Objetivos de refinamento do MVP

O MVP completo deve refinar duas áreas existentes e conectá-las por uma terceira capacidade:

1. filtros avançados em Comparable Games;
2. Data Sources populado e informativo;
3. filtragem por reliability.

## 1. Filtros básicos e avançados

Comparable Games deve permanecer uma única página com progressive disclosure.

### Filtros básicos

Visíveis por padrão:

- nome do jogo;
- gênero;
- plataforma;
- ano ou período de lançamento;
- ação Search.

### Filtros avançados

Um controle como `Advanced filters` deve revelar critérios adicionais sem sobrecarregar a interface inicial.

Filtros candidatos incluem:

- subgênero;
- tags ou características de gameplay;
- themes;
- game modes;
- player perspective;
- developer;
- publisher;
- release status;
- product type;
- suporte a single-player, multiplayer, cooperative ou competitive;
- business model;
- reliability da fonte.

O escopo final depende da necessidade do producer, disponibilidade dos dados, reliability da fonte, viabilidade legal e técnica, adequação ao modelo de domínio e custo medido de armazenamento/consulta.

A existência de um campo ou relacionamento persistido não justifica automaticamente expô-lo como filtro.

A existência de um campo no provider também não justifica automaticamente ingeri-lo em profundidade máxima.

### Por que progressive disclosure

Um único formulário grande poderia ser difícil de usar. Uma página separada de busca avançada dividiria uma única intenção de pesquisa entre rotas diferentes.

Progressive disclosure preserva uma página, um modelo de resultado, uma URL compartilhável, um objetivo claro para o usuário, uma experiência inicial simples e espaço para pesquisa mais profunda.

## 2. Data Sources como funcionalidade de produto

Para cada fonte, a página deve mostrar:

- nome da fonte;
- organização ou owner;
- tipo de fonte;
- status oficial ou independente;
- categorias de dados fornecidas;
- reliability por categoria de dado;
- limitações conhecidas;
- método de coleta;
- frequência de atualização ou verificação;
- requisitos de atribuição;
- restrições relevantes de uso;
- URL original da fonte.

Data Sources deve ajudar producers a compreender provenance, inspecionar o site original, distinguir fatos de estimativas e entender dados ausentes.

Fornecer links de origem reforça que a aplicação é uma ferramenta de pesquisa não comercial e não um substituto das plataformas originais.

A experiência de fonte deve apresentar provenance em formato compreensível para humanos.

Identificadores internos de persistência como `ExternalGameRecordId`, `ExternalCompanyRecordId` ou `ExternalCollectionRecordId` não devem ser expostos diretamente no frontend.

## 3. Filtro de reliability

Um filtro de reliability é a expansão preferida após filtros avançados e Data Sources.

Ele se alinha mais diretamente à proposta de valor da aplicação do que um filtro por fonte.

### Modos possíveis

- High confidence;
- Balanced;
- Broad coverage.

### High confidence

Prioriza dados oficiais, primários, fortemente verificados ou concordância entre fontes confiáveis.

A interface deve avisar que a cobertura pode ser reduzida.

### Balanced

Combina dados oficiais, fontes curadas, agregadores confiáveis e valores normalizados com provenance aceitável.

Esse é o provável padrão.

### Broad coverage

Pode incluir estimativas reconhecidas, dados comunitários estruturados, atributos de menor confiança ou registros conflitantes.

Toda essa informação deve permanecer claramente identificada.

## Por que reliability vem antes de filtro por fonte

Um producer normalmente se importa primeiro com a confiabilidade de um valor, e não com qual site o forneceu.

Filtragem por reliability permite que workers e regras de normalização continuem selecionando e reconciliando dados enquanto o producer controla o limiar de confiança aceitável.

## Filtro por fonte como possibilidade posterior

Um filtro por fonte permanece válido, mas com prioridade menor.

Se implementado, deve suportar múltiplas fontes selecionadas, preservar agregação, esclarecer comportamento any/all e funcionar principalmente como ferramenta avançada de auditoria ou reprodutibilidade.

## Dados normalizados e provenance

A experiência padrão deve apresentar dados normalizados pelo Game Market Intelligence enquanto preserva detalhes de origem.

Exemplo:

```text
Release date
October 18, 2024

Normalized by Game Market Intelligence
Sources:
- Steam — official
- IGDB — curated, matching value
- Aggregated source — conflicting date
```

A composição exata de fontes nesse exemplo é ilustrativa.

O Milestone 2 usa atualmente IGDB como fonte ativa. Wikidata e Steam permanecem integrações futuras do Milestone 3.

## Impacto no modelo de domínio

O refinamento original identificou conceitos que o modelo inicial de `Game` eventualmente precisaria suportar.

Grande parte dessa fundação já foi implementada, mas nem toda informação foi adicionada diretamente a `Game`.

A estrutura source-neutral atual inclui conceitos como:

```text
Game
├── Genres
├── Platforms
├── Themes
├── GameModes
├── PlayerPerspectives
├── Keywords
├── ProductType
├── Companies + Roles
├── Collections
├── Contextual Releases
├── Product Relationships
├── GameImage metadata
└── External source identities and provenance
```

Isso confirma uma decisão importante de modelagem do refinamento original:

> Nem toda informação deve ser adicionada diretamente a `Game`.

Identidade específica da fonte e provenance são modeladas separadamente.

Por exemplo:

```text
Game
    ↓
ExternalGameRecord
    ↓
DataSource + ExternalId
```

E associações contextuais preservam os registros externos que as sustentam.

O modelo atual também distingue:

```text
Game.FirstReleaseDate
→ valor canônico de resumo usado pelo filtro atual de ano

GameRelease
→ evidência contextual de release por plataforma/região/fonte
```

Relacionamentos de produto permanecem separados de tipo de produto:

```text
GameProductType
→ o que o produto é

GameProductRelationType
→ como um produto se relaciona com outro
```

Produtos relacionados não herdam nem propagam automaticamente gêneros, plataformas, releases, companies, collections, classificações ou outros metadados.

O tratamento de imagens agora segue o mesmo princípio de separação.

O `Game` canônico não persiste mais uma `ImageUrl` pronta.

Em vez disso:

```text
GameImage
→ ExternalGameRecord
→ DataSource
```

armazena metadados derivados da fonte e provenance, enquanto um resolver no backend converte `SourceImageId` em URL pública quando um caso de leitura precisa dela.

Os contratos públicos atuais permanecem propositalmente simples:

```text
GameDetails.ImageUrl?
GameSearchItem.ImageUrl?
```

O frontend recebe portanto uma URL pronta para uso ou `null` para fallback e não precisa conhecer identificadores específicos de imagem do provider ou regras de CDN.

O fluxo atual de busca resolve covers primários em batch para os jogos da página, evitando N+1 de imagens.

Screenshots são persistidos como metadados para trabalho futuro de detalhes/galeria, mas não são expostos pelo contrato de leitura atual do MVP.

Métricas temporais de mercado como preço, vendas, reviews e player counts ainda não devem ser campos estáticos em `Game`.

## Fronteira de ingestão consciente de armazenamento

A GMI-30 introduziu uma distinção importante de produto/persistência:

```text
cobertura de catálogo
≠
profundidade de metadados
```

O objetivo não é ingerir um máximo fixo e arbitrário de jogos simplesmente porque o banco possui limite de armazenamento.

Se uma fonte expõe um conjunto maior de jogos que são relevantes ao escopo aprovado do MVP, preservar essas entradas relevantes do catálogo é preferível a descartá-las apenas para satisfazer um limite arbitrário de quantidade de registros.

A capacidade deve ser gerenciada primeiro por profundidade de metadados orientada a produto.

Por exemplo:

```text
preservar jogos relevantes
↓
persistir classificações e relacionamentos exigidos por perguntas aprovadas de produto
↓
limitar, adiar ou evitar metadados não essenciais de alta cardinalidade
↓
monitorar crescimento real de tabelas e índices
```

Isso significa que o Collector não deve interpretar "o provider expõe" como motivo suficiente para persistir todo campo disponível ou toda associação na profundidade máxima.

O source-product question map continua sendo a fronteira de decisão.

### Evidência representativa de armazenamento da GMI-30

O cenário sintético baseline utilizou:

```text
10.000 Games
20.000 GameGenres
20.000 GamePlatforms
10.000 ExternalGameRecords
16.666 GameImages
```

O armazenamento PostgreSQL medido após `ANALYZE` foi aproximadamente:

```text
dados de tabela  ~7,4 MB
índices          ~11 MB
total            ~19 MB
```

Um segundo cenário de pressão manteve o mesmo catálogo de 10.000 jogos e adicionou:

```text
30.000 releases contextuais
30.000 associações de theme
20.000 associações de game mode
20.000 associações de player perspective
80.000 associações de keyword
20.000 associações de company
5.000 associações de collection
2.500 product relations
```

O armazenamento medido passou para aproximadamente:

```text
dados de tabela  ~29 MB
índices          ~33 MB
total            ~63 MB
```

O aumento foi de aproximadamente 44 MB sem adicionar mais jogos.

Os pontos sintéticos de maior pressão foram:

- `game_keywords`;
- `game_releases`;
- `game_images`;
- `game_themes`;
- `game_companies`.

Essa evidência reforça que o risco de armazenamento é impulsionado principalmente por associações multiplicativas e seus índices de apoio, não apenas pela quantidade de jogos canônicos.

O cenário medido de ~63 MB / 10.000 jogos é apenas uma referência de orçamento. Ele não deve ser tratado como previsão linear fixa de produção.

## Observações de planos de consulta relevantes para Comparable Games

A GMI-30 mediu os formatos atuais das consultas com um dataset local sintético de 10.000 jogos.

### Busca por nome

A busca substring atual:

```text
ILIKE '%term%'
```

usou sequential scan de `Games`.

O tempo local aproximado de execução para a consulta representativa foi 3,7 ms.

O índice B-tree atual de `NormalizedName` não sustenta esse padrão de substring.

Decisão:

- manter o comportamento atual no volume medido do MVP;
- não introduzir trigram ou outro índice especializado até que volume próximo de produção ou latência demonstre necessidade.

### Filtro por gênero

O caminho de gênero usou `IX_GameGenres_GenreId`.

Decisão:

- o índice atual é justificado;
- nenhum índice adicional de gênero é necessário.

### Filtro por plataforma

O caminho de plataforma usou `IX_GamePlatforms_PlatformId`.

Decisão:

- o índice atual é justificado;
- nenhum índice adicional de plataforma é necessário.

### Filtro por ano de release

A consulta atual de ano aplica `EXTRACT(YEAR FROM FirstReleaseDate)` e usou sequential scan.

O tempo local aproximado de execução para a consulta representativa foi 1,8 ms.

Decisão:

- manter o comportamento atual no volume medido do MVP;
- não criar índice funcional de ano apenas para eliminar o sequential scan.

### Lookup batch de covers

O lookup de cover primário usou `IX_game_images_GameId`.

Decisão:

- o índice atual de imagem é justificado;
- o lookup batch permanece o access path preferido da página de busca.

Essas medições locais são evidência para as decisões atuais de design, e não garantias de nível de serviço em produção.

Sequential scan não é considerado automaticamente um defeito.

## Responsabilidades de persistência, API e frontend

A arquitetura atual separa três preocupações.

### Persistência

Armazena o dataset canônico tratado mais a identidade externa e provenance mínima necessária para integridade, auditabilidade, remoção de fonte e reconciliação futura.

A persistência também deve respeitar o orçamento operacional de armazenamento.

Isso significa preservar dados porque eles sustentam:

- uma pergunta aprovada de produto;
- provenance exigida;
- reconciliação;
- atribuição;
- correção operacional.

Ela não deve se tornar um warehouse de todo campo do provider apenas porque esses campos estão disponíveis.

### Application e API

Expõem contratos específicos por caso de uso.

A API não precisa retornar toda propriedade persistida.

Os contratos atuais de leitura já incluem uma fronteira de imagem propositalmente simples:

- `GameDetails.ImageUrl?`;
- `GameSearchItem.ImageUrl?`.

Esses valores são derivados de metadados persistidos de imagem, em vez de armazenados diretamente em `Game`.

Conceitos futuros possíveis de leitura incluem:

- product type;
- produtos relacionados;
- companies agrupadas por role;
- collections;
- releases contextuais;
- classificações selecionadas;
- dados de screenshot/galeria se validados pela experiência de detalhe;
- informações de fonte compreensíveis para humanos.

### Frontend

Organiza os contratos da API para o workflow do producer.

O frontend não deve espelhar o schema do banco.

Essa regra agora se aplica explicitamente ao tratamento de imagem: `GameImage`, `ExternalGameRecord`, `SourceImageId` e regras específicas de CDN do provider permanecem preocupações do backend. O frontend consome apenas o `ImageUrl?` resolvido exigido pela experiência atual de card/detalhes.

Por exemplo, a persistência pode manter:

```text
ExternalGameRecordId
ExternalCompanyRecordId
DataSourceId
SourceUpdatedAt
```

enquanto a experiência visível ao usuário pode mostrar:

```text
Developer
Grezzo

Source
IGDB
```

Decisões de orçamento de armazenamento também são preocupações de backend/ingestão.

O frontend não deve expor omissões arbitrárias de metadados como se uma fonte definitivamente não tivesse esses dados.

## Regra de migration

A regra original de migration continua válida como princípio geral de design:

1. confirmar necessidades do producer;
2. definir campos e filtros do MVP;
3. avaliar fontes candidatas;
4. verificar disponibilidade e permissões;
5. revisar o modelo de domínio;
6. propor o novo modelo;
7. validar impacto de arquitetura e ingestão;
8. criar migration.

Para os campos de persistência do Milestone 2 aprovados pela PoC e cobertos pela GMI-25 até GMI-29, essa sequência foi concluída.

A GMI-30 validou que o schema resultante pode ser aplicado tanto do zero quanto sobre o banco local existente, sem drift pendente do modelo EF.

Mudanças futuras de schema devem continuar seguindo a mesma regra evidence-first.

Migrations geradas também devem ser revisadas para mudanças não relacionadas antes de serem aplicadas.

Novos índices seguem a mesma regra de evidência: devem responder a necessidade demonstrada de consulta e justificar seu custo de armazenamento/write.

## Fronteira atual de implementação

No checkpoint atual da GMI-30:

```text
Fundações de Domain e persistência
→ implementadas para GMI-25 até GMI-29

Operabilidade de persistência
→ migrations validadas do zero e sobre o banco local existente
→ planos de consulta representativos medidos
→ pressão representativa de armazenamento medida

Exposição pública da API
→ ainda seletiva/contratos atuais apenas
→ URLs de imagem de jogo são derivadas de metadados persistidos

Apresentação no frontend
→ experiência atual de Comparable Games apenas
→ fronteira existente de cover/fallback preservada

Ingestão do Collector em produção
→ ainda pendente

Reconciliação multi-source
→ adiada para o Milestone 3
```

O quality gate atual da solução completa durante a GMI-30 passou:

```text
Testes: 460
Aprovados: 460
Falharam: 0
Ignorados: 0
```

Princípio atual de capacidade:

```text
preservar cobertura relevante do catálogo
→ controlar profundidade de metadados pelo valor de produto
→ monitorar associações de alta cardinalidade e índices
→ otimizar apenas quando evidência medida justificar
```

## Fora do escopo deste refinamento de MVP

Capacidades futuras incluem análise de vendas e receita, tendências de engajamento, histórico de preços, review sentiment, análise de audiência, saturação de mercado, pesquisas salvas, comparação lado a lado e modelos preditivos.

Também adiados até incrementos separados e validados:

- reconciliação multi-source;
- franchises;
- filtragem pública arbitrária por múltiplas keywords;
- expor toda classificação persistida como filtro de frontend;
- propagação automática entre produtos relacionados;
- otimização especulativa de busca/índices sem necessidade medida.
