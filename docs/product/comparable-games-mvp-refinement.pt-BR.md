# Refinamento do MVP de Comparable Games

> Status da revisão: atualizado após a conclusão técnica da GMI-28 em 24 de agosto de 2026.

## MVP completo

O MVP completo reúne três capacidades:

1. Comparable Games com filtros básicos e avançados sustentados por dados aprovados.
2. Data Sources com a origem dos campos do jogo selecionado e informações institucionais sobre confiabilidade, limitações, atribuição, atualização e URLs das integrações.
3. Modos Alta confiança, Equilibrado e Cobertura ampla.

## Comparable Games

Neste MVP com IGDB, o filtro de Comparable Games é a primeira entrega de valor do produto.

O conjunto de filtros é orientado por evidências: cada campo incluído, restrito aos detalhes, adiado ou excluído reflete cobertura, semântica, comportamento operacional e limitações legais avaliadas na PoC.

A experiência deve oferecer a maior confiança prática possível dentro da operação com custo zero, identificando a fonte e comunicando ao producer dados ausentes ou limitados.

Filtros básicos:

- nome;
- gênero;
- plataforma;
- período/ano.

No primeiro MVP com IGDB, a divulgação progressiva poderá acrescentar:

- temas;
- modos;
- empresas envolvidas;
- product type;
- navegação contextual por keyword.

Esses recursos devem permanecer qualificados pela fonte e só devem ser expostos quando responderem a uma pergunta validada de produto e houver cobertura suficiente.

As decisões atuais são:

- modos podem ser filtro público, tratando ausência na fonte como desconhecido;
- perspectivas são detalhes opcionais, não filtro público;
- o clique numa keyword exibida pode abrir jogos relacionados com um critério contextual removível;
- seleção manual, múltiplas keywords com `AND` e autocomplete foram adiados;
- capas são opcionais e não dominantes nos resultados e detalhes;
- screenshots ficam nos detalhes e em galerias abertas sob demanda;
- product type é um candidato forte a filtro público futuro;
- relações entre produtos devem aparecer prioritariamente nos detalhes, não como propagação automática de dados entre jogos;
- empresas podem ser apresentadas agrupadas por papel, como developer, publisher, porting e supporting;
- collections são contexto útil de detalhe, mas não precisam aparecer em todos os cards;
- dados de proveniência técnica não devem ser expostos diretamente ao usuário.

Iterações futuras poderão avaliar ou acrescentar:

- subgênero;
- temas e tags mais ricos;
- filtro de perspectiva se a cobertura se tornar suficiente ou puder ser qualificada por fontes complementares;
- developer e publisher como filtros;
- status de lançamento;
- características mais profundas de single-player, multiplayer e coop;
- filtro manual com múltiplas keywords;
- buscas salvas;
- filtros por product type;
- filtros por collection quando houver uma pergunta de produto que justifique isso;
- filtros por período de lançamento mais ricos.

Os resultados são candidatos comparáveis, não concorrentes diretos automáticos.

## Status da fundação de domínio e persistência

As necessidades de domínio antecipadas por este documento avançaram substancialmente por meio da GMI-25 até a GMI-28.

Já estão implementadas as seguintes fundações:

- identidade externa source-neutral via `DataSource + ExternalId`;
- `Game.FirstReleaseDate` separado de `GameRelease`;
- releases contextuais com proveniência;
- themes, game modes, player perspectives e keywords como classificações canônicas separadas;
- identidades externas para essas classificações;
- associações entre jogo e classificação com proveniência;
- `Game.ProductType`;
- `GameProductType`;
- `GameProductRelationType`;
- `GameProductRelation`;
- `Company`;
- `ExternalCompanyRecord`;
- `GameCompanyRole`;
- `GameCompany`;
- `Collection`;
- `ExternalCollectionRecord`;
- `GameCollection`;
- delete behavior restritivo nas relações com proveniência;
- mappings do EF Core;
- migrations PostgreSQL;
- integration tests de persistência e integridade.

Essas estruturas pertencem ao domínio e à persistência.

Elas **não significam que todos esses campos já estão expostos na API ou no frontend**.

A camada de persistência é intencionalmente mais rica do que qualquer tela ou endpoint individual.

## Data Sources

Data Sources deixa de ser apenas um catálogo institucional.

Para o jogo selecionado, mostra quais fontes contribuíram e relaciona cada campo ou contexto relevante à sua origem, sem repetir os valores já apresentados nos detalhes do jogo.

Um link nos detalhes poderá abrir essa auditoria diretamente.

Em uma seção complementar, para cada fonte, mostrar:

- organização e status oficial/independente;
- categorias fornecidas;
- confiabilidade por categoria;
- limitações;
- coleta/atualização;
- atribuição e restrições;
- URL original.

O GMI materializa um resultado canônico segundo regras por campo e contexto.

O producer não escolhe entre cópias alternativas do catálogo por provider porque essas cópias não serão mantidas.

A natureza oficial, curada ou comunitária continua visível como evidência e limitação.

### Proveniência técnica versus apresentação

A persistência pode manter identificadores como:

```text
ExternalGameRecordId
ExternalCompanyRecordId
ExternalCollectionRecordId
DataSourceId
SourceUpdatedAt
```

Esses identificadores não devem aparecer diretamente na interface.

O frontend deve apresentar proveniência de forma compreensível, por exemplo:

```text
Developer
Grezzo

Fonte
IGDB
```

A API deve fazer a mediação entre persistência e frontend por meio de contratos específicos de caso de uso.

## Modos de confiabilidade

- **Alta confiança:** oficial, primário, fortemente verificado ou concordância confiável; cobertura menor é esperada.
- **Equilibrado:** dados oficiais, curados e normalizados com proveniência aceitável; provável padrão.
- **Cobertura ampla:** pode incluir estimativas reconhecidas, comunidade e conflitos, sempre identificados.

Os modos de confiabilidade qualificam o resultado canônico; não selecionam uma cópia diferente do catálogo nem substituem a proveniência por campo.

## Por que confiabilidade vem antes do filtro por fonte

Para uma producer, normalmente é mais importante saber se um valor é confiável do que escolher manualmente qual provider deve fornecê-lo.

O filtro de confiabilidade permite que o GMI continue normalizando e selecionando os dados segundo regras internas, enquanto o usuário controla o nível de confiança aceitável.

Um filtro por fonte continua possível no futuro, principalmente para auditoria, investigação e reprodutibilidade.

## Dados normalizados e proveniência

A experiência padrão deve apresentar dados normalizados do Game Market Intelligence preservando o contexto de origem.

Exemplo:

```text
Data de lançamento
18 de outubro de 2024

Normalizado pelo Game Market Intelligence
Fontes:
- fonte oficial
- fonte curada com o mesmo valor
- fonte agregadora com valor conflitante
```

A composição acima é ilustrativa.

No Milestone 2, a IGDB permanece a fonte ativa.

Wikidata e Steam continuam como integrações futuras do Milestone 3.

## Direção de domínio

O domínio já evoluiu além do modelo inicial de `Game`, mas sem concentrar toda a informação diretamente nessa entidade.

A estrutura atual inclui conceitos como:

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
└── External source identities and provenance
```

Isso confirma uma decisão importante:

> Nem toda informação deve ser uma propriedade direta de `Game`.

Identidade específica de fonte e proveniência são modeladas separadamente.

Exemplo:

```text
Game
    ↓
ExternalGameRecord
    ↓
DataSource + ExternalId
```

As associações contextuais preservam também os registros externos que sustentam a informação.

### First release versus release contextual

O modelo distingue:

```text
Game.FirstReleaseDate
→ valor canônico resumido usado no filtro atual por ano

GameRelease
→ evidência contextual de release por plataforma, região e fonte
```

### Tipo de produto versus relação entre produtos

Também existe uma separação explícita entre:

```text
GameProductType
→ o que o produto é

GameProductRelationType
→ como um produto se relaciona com outro
```

Produtos relacionados permanecem `Game`s independentes.

Uma relação não propaga automaticamente:

- gêneros;
- plataformas;
- releases;
- empresas;
- collections;
- classifications;
- outros metadados.

## Empresas

Empresas são modeladas como entidades canônicas source-neutral.

O papel não pertence permanentemente à empresa, mas à relação entre empresa e jogo.

Papéis atuais:

- Developer;
- Publisher;
- Porting;
- Supporting.

Isso permite que uma mesma empresa:

- participe de vários jogos;
- tenha papéis diferentes em jogos diferentes;
- tenha mais de um papel no mesmo jogo quando a evidência sustentar isso.

No frontend, esses dados devem ser organizados por papel e não apresentados como estruturas técnicas de persistência.

## Collections

Collections são entidades canônicas próprias e podem agrupar ou contextualizar múltiplos jogos.

A relação entre `Game` e `Collection` é N:N e preserva proveniência.

Collections estão aprovadas para o Milestone 2.

Franchises permanecem adiadas.

## Persistência, API e frontend

A arquitetura atual separa claramente três responsabilidades.

### Persistência

Mantém:

- dados canônicos tratados;
- identidades externas;
- relações;
- proveniência mínima necessária;
- integridade referencial;
- informação suficiente para auditoria, remoção de fonte e reconciliação futura.

### Application/API

Expõe contratos orientados a caso de uso.

A API não precisa devolver todos os campos existentes no banco.

Conceitos futuros de leitura podem incluir:

- product type;
- produtos relacionados;
- empresas agrupadas por papel;
- collections;
- releases contextuais;
- classificações selecionadas;
- informação amigável de fonte e atribuição.

### Frontend

Organiza os contratos da API para o fluxo de decisão do producer.

O frontend não deve espelhar o schema do banco.

Nos cards, a prioridade continua sendo concisão.

Nos detalhes, pode haver maior riqueza de contexto.

## Regra de migration

A regra original permanece válida como princípio geral:

1. confirmar a necessidade do producer;
2. definir campos e filtros aprovados para o MVP;
3. avaliar fontes candidatas;
4. verificar disponibilidade e permissões;
5. revisar o domínio;
6. propor o novo modelo;
7. validar impacto arquitetural e de ingestão;
8. criar migration.

Para os campos do Milestone 2 aprovados pela PoC e implementados na GMI-25 até a GMI-28, essa sequência já foi cumprida.

Mudanças futuras de schema devem continuar seguindo essa regra orientada por evidência.

Migrations geradas também devem ser revisadas para garantir que não incluam alterações não relacionadas ao incremento atual.

## Checkpoint atual

Após a GMI-28:

```text
Domain e persistência
→ fundações GMI-25 a GMI-28 implementadas

API pública
→ continua expondo os contratos atuais selecionados

Frontend
→ continua com a experiência atual de Comparable Games

Collector de produção
→ ainda pendente

Reconciliação multi-fonte
→ adiada para o Milestone 3
```

Quality gate completo da solução:

```text
Total de testes: 424
Passaram: 424
Falharam: 0
Ignorados: 0
```

## Próximos passos

Dentro da persistência do Milestone 2:

1. GMI-29 — metadados de cover e screenshots;
2. GMI-30 — validação do orçamento de armazenamento.

Depois da conclusão do modelo de persistência aprovado:

1. implementar/refatorar o Worker/Collector de produção;
2. ingerir o subconjunto aprovado da IGDB;
3. popular dados reais representativos;
4. validar storage;
5. expor os novos conceitos selecionados na Application/API;
6. definir o que pertence aos cards, detalhes e filtros;
7. apresentar proveniência e atribuição de forma compreensível;
8. validar o MVP ponta a ponta com dados reais.

## Fora do escopo deste refinamento do MVP

Capacidades futuras incluem:

- análise de vendas e receita;
- tendências de engajamento;
- histórico de preço;
- sentimento de reviews;
- análise de audiência;
- saturação de mercado;
- pesquisa salva;
- comparação lado a lado;
- modelos preditivos.

Também permanecem adiados para incrementos separados:

- reconciliação multi-fonte;
- franchises;
- filtragem pública arbitrária por múltiplas keywords;
- exposição automática de toda classificação persistida como filtro;
- propagação automática entre produtos relacionados.
