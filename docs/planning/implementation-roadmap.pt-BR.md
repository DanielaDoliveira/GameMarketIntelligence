# Roadmap de Implementação

> Atualizado em: 25 de agosto de 2026

## Objetivo

Este documento fornece uma visão por marcos da implementação do Game Market Intelligence.

Ele registra os incrementos concluídos, o foco atual de entrega e a evolução esperada do produto sem substituir a documentação detalhada de domínio, arquitetura, avaliação de fontes, design, prova de conceito ou incrementos específicos.

O roadmap pode mudar conforme fontes de dados reais, restrições de infraestrutura, validação de produto, aprendizado de deploy e medições de armazenamento tragam novas evidências.

## Direção do produto

Game Market Intelligence é uma plataforma de apoio à decisão para Game Producers e pequenos estúdios.

O produto organiza jogos comparáveis, referências confiáveis de pesquisa, evidências com origem preservada e futuro contexto comercial em um fluxo que ajuda a reduzir incerteza durante o planejamento inicial de produto.

```text
Ideia de jogo
    ↓
Descoberta de jogos comparáveis
    ↓
Referências de pesquisa
    ↓
Evidências comerciais
    ↓
Análise de mercado
    ↓
Apoio à decisão de stakeholders
```

A plataforma deve preservar provenance, distinguir observações das fontes de conclusões internas e permanecer extensível a múltiplas fontes.

## Princípios de entrega

Game Market Intelligence é desenvolvido por meio de incrementos verticais pequenos e completos.

Cada entrega deve:

- responder ou habilitar uma pergunta real de produto;
- fornecer uma experiência utilizável de ponta a ponta quando apropriado;
- preservar as fronteiras entre Domain, Application, Infrastructure, API, Collector, Shared e Web;
- preservar provenance e identidades externas;
- incluir testes automatizados apropriados;
- atualizar a documentação técnica e de produto;
- passar pela validação de Pull Request antes de entrar em `main`;
- permanecer implantável na infraestrutura zero-cost aprovada;
- evitar decisões irreversíveis de domínio ou persistência antes de haver evidência suficiente de fontes reais;
- integrar fontes incrementalmente sem impedir suporte futuro a múltiplas fontes;
- evitar expor diretamente o modelo de persistência pelos contratos de API ou frontend;
- tratar limites de armazenamento como restrição de produto;
- preservar cobertura relevante de catálogo antes de reduzir profundidade de metadados por razões de capacidade.

## Milestone 0 — Fundação do projeto

Status: **Concluído**

Entregue:

- estrutura da solução .NET;
- projetos Domain, Application, Infrastructure, API, Shared, Collector e Web;
- ambiente local PostgreSQL;
- configuração de EF Core e Npgsql;
- modelagem inicial de `DataSource` e `SourceReliability`;
- fundações de testes automatizados;
- Continuous Integration com GitHub Actions;
- fundações de Terraform e deploy;
- documentação inicial de arquitetura e produto.

## Milestone 1 — Fundação de Comparable Games e primeira experiência de leitura

Status: **Concluído**

Entregue:

- fundações de domínio de `Genre`, `Platform` e `Game`;
- regras de nome normalizado e prevenção de duplicidade;
- relacionamentos muitos-para-muitos entre jogos, gêneros e plataformas;
- persistência PostgreSQL e testes de integração;
- endpoints de busca e detalhes de Comparable Games;
- endpoints de listagem de gêneros e plataformas;
- suporte a nome parcial, gênero, plataforma, ano de lançamento e paginação;
- tratamento centralizado de exceções com `ProblemDetails`;
- integração da API com Blazor WebAssembly;
- shell responsivo, navegação, filtros, estados de resultado, paginação e componentes reutilizáveis;
- integração de deploy baseada em ambiente;
- validação no navegador com o dataset de produção ainda vazio;
- envio de busca visível e explícito no formulário de filtros;
- documentação e revisão de aprendizado do primeiro milestone de frontend.

A validação dependente de dados continua pendente até que dados reais representativos sejam persistidos.

## Milestone 2 — MVP vertical com IGDB

Status: **Em andamento**

### Objetivo

Entregar o primeiro MVP funcional com dados reais usando IGDB como primeira fonte ativa, desde a coleta autorizada até persistência, deploy, uso pela API e apresentação no produto.

Este milestone **não** abandona a estratégia multi-source.

Ele entrega uma fonte verticalmente para que o projeto possa validar:

- comportamento do Collector;
- contratos específicos da fonte;
- persistência canônica;
- provenance;
- impacto de armazenamento;
- deploy;
- uso pela API;
- apresentação no frontend;
- atribuição e comportamento operacional.

## 2.1 Proof of concept da IGDB

Status: **Concluída para o escopo de persistência aprovado do Milestone 2**

Áreas validadas incluem:

- autenticação Twitch OAuth;
- recuperação de jogos da IGDB;
- contratos específicos da fonte;
- amostras controladas;
- amostras reproduzíveis;
- tipos de produto;
- comportamento de parent e produtos relacionados;
- bundles;
- releases contextuais por plataforma;
- precisão e nulabilidade de release;
- regiões e status;
- gêneros;
- temas;
- modos de jogo;
- perspectivas do jogador;
- keywords;
- involved companies;
- collections;
- covers e metadados de imagem;
- observações de títulos alternativos/localizados;
- nulabilidade e registros incompletos;
- utilidade dos campos para Comparable Games;
- limitações específicas da fonte.

Decisões-chave de implementação produzidas pela PoC incluem:

- `first_release_date` permanece como valor canônico de resumo/filtro;
- releases contextuais exigem um modelo de release separado;
- jogos originais, ports, remakes, remasters, bundles, DLCs, expansões e versões permanecem produtos distintos;
- relacionamentos não devem propagar automaticamente classificações ou metadados;
- companies exigem roles e provenance;
- collections estão aprovadas;
- franchises permanecem adiadas;
- themes, modes, perspectives e keywords são persistidos como classificações separadas e source-neutral;
- binários de imagem não são armazenados;
- nulabilidade da fonte permanece significativa e não é convertida em valores falsos;
- nomes normalizados são úteis para lookup, mas não são evidência suficiente de identidade.

Os documentos históricos da PoC permanecem como fonte de evidência para decisões detalhadas de campos e não devem ser reescritos como documentos de implementação.

## 2.2 Spike leve de compatibilidade multi-source

Status: **Concluído**

O spike de compatibilidade confirmou que a arquitetura do Milestone 2 pode suportar IGDB agora sem depender estruturalmente dela.

Regras arquiteturais validadas incluem:

- identificadores canônicos internos;
- múltiplas identidades externas por meio de `DataSource + ExternalId`;
- contratos específicos por fonte;
- mappers específicos por fonte;
- entidades canônicas independentes da fonte;
- entidades contextuais com provenance;
- metadados específicos da fonte fora de conceitos universais do domínio;
- reconciliação futura conservadora;
- nenhuma dependência da Steam para identidade canônica;
- nenhuma propriedade de ID específica de provider em `Game`.

Critério de saída atingido:

> O design de persistência orientado inicialmente à IGDB pode prosseguir sem exigir redesenho estrutural quando Wikidata e Steam forem introduzidas posteriormente.

A reconciliação multi-source em si permanece adiada para o Milestone 3.

## 2.3 Implementação de domínio e persistência

Status: **Em andamento — GMI-25 até GMI-29 concluídas e integradas; validação da GMI-30 em andamento**

A implementação de persistência é entregue por meio de issues filhas no Jira sob GMI-14.

### GMI-25 — Identidade externa da fonte

Status: **Concluída**

Entregue:

- separação entre identidade canônica e externa;
- `ExternalGameRecord`;
- identidade da fonte por `DataSourceId + ExternalId`;
- vínculo opcional do registro externo a `Game` canônico;
- timestamps de observação;
- timestamp de atualização na fonte;
- prevenção de duplicidade;
- mappings EF Core;
- migration;
- testes de persistência.

### GMI-26 — Releases contextuais com provenance

Status: **Concluída**

Entregue:

- `GameRelease` contextual;
- `Game.FirstReleaseDate` canônico;
- contexto de release específico de plataforma;
- identidade externa do release;
- `ExternalGameRecord` de origem;
- representação de data parcial;
- região, status e metadados de observação;
- comportamento de delete restritivo;
- mappings EF Core;
- migration;
- testes de persistência.

### GMI-27 — Classificações consultáveis aprovadas

Status: **Concluída**

Classificações canônicas entregues:

- `Theme`;
- `GameMode`;
- `PlayerPerspective`;
- `Keyword`.

Identidades externas entregues:

- `ExternalThemeRecord`;
- `ExternalGameModeRecord`;
- `ExternalPlayerPerspectiveRecord`;
- `ExternalKeywordRecord`.

Associações com provenance entregues:

- `GameTheme`;
- `GameGameMode`;
- `GamePlayerPerspective`;
- `GameKeyword`.

A implementação preserva a identidade da fonte e não trata nomes normalizados como prova de equivalência entre fontes.

### GMI-28 — Produtos, companies, collections e relacionamentos

Status: **Concluída e integrada**

Modelagem de produto entregue:

- `Game.ProductType`;
- `GameProductType`;
- `GameProductRelationType`;
- `GameProductRelation`;
- relacionamentos direcionados entre produtos canônicos distintos;
- provenance por meio de `ExternalGameRecord` de origem e destino;
- nenhuma propagação automática entre produtos relacionados.

Modelagem de company entregue:

- `Company` canônica;
- `ExternalCompanyRecord`;
- `GameCompanyRole`;
- `GameCompany` com provenance;
- cardinalidade N:N game/company;
- múltiplos roles quando suportados por evidência.

Modelagem de collection entregue:

- `Collection` canônica;
- `ExternalCollectionRecord`;
- `GameCollection` com provenance;
- cardinalidade N:N game/collection.

Integridade de persistência entregue:

- conceitos canônicos source-neutral;
- identidades externas únicas por fonte;
- chaves compostas em associações;
- comportamento de delete restritivo;
- cobertura de migrations;
- testes de delete/integridade;
- reset do banco de testes de integração atualizado para as novas tabelas do schema.

Gate final de qualidade da GMI-28:

```text
Build: aprovado
Testes: 424 aprovados
Falhas: 0
Ignorados: 0
```

A GMI-28 foi mergeada em `develop`, enviada ao remoto e encerrada com working tree limpo.

### GMI-29 — Metadados de covers e screenshots

Status: **Concluída e integrada**

Modelagem de metadados de imagem entregue:

- `GameImage` source-neutral;
- `GameImageType` com `Cover` e `Screenshot`;
- identidade do registro de imagem da fonte por `ExternalId`;
- endereçamento do asset de origem por `SourceImageId`;
- width e height opcionais;
- `SortOrder` opcional;
- `GameId` canônico;
- provenance por `ExternalGameRecordId`;
- nenhum `DataSourceId` duplicado;
- nenhum binário de imagem persistido;
- nenhuma URL pronta de imagem de jogo persistida.

Regras de integridade entregues:

- `GameImage` só pode ser criado a partir de `ExternalGameRecord` já vinculado a um `Game` canônico;
- identificadores externo e de asset obrigatórios e trimados;
- dimensões opcionais positivas;
- sort order opcional não negativo;
- evidência única por `ExternalGameRecordId + ExternalId + Type`;
- delete restritivo tanto para o jogo canônico quanto para o registro externo que sustenta a evidência;
- reset de testes PostgreSQL atualizado para a nova tabela.

Transição de persistência entregue:

- remoção de `Game.ImageUrl` persistido;
- migration de `game_images` adicionada e validada;
- migration removendo `Games.ImageUrl` adicionada e validada;
- `Platform.ImageUrl` preservado fora do escopo da GMI-29.

Resolução de URL source-aware entregue:

```text
Metadados de GameImage
→ seleção do cover primário
→ ExternalGameRecord
→ DataSource.Code
→ IGameImageUrlResolver
→ ImageUrl pública
→ contrato Shared
→ frontend
```

A resolução atual para IGDB é derivada de `SourceImageId`.

O frontend continua recebendo apenas um `ImageUrl?`; identificadores específicos do provider, regras de CDN e metadados de persistência permanecem responsabilidade do backend.

Seleção de cover primário entregue:

- apenas registros `Cover` são elegíveis;
- `SortOrder` preenchido é preferido a `null`;
- menor `SortOrder` é preferido;
- `GameImage.Id` fornece desempate determinístico;
- detalhes usam lookup de um único cover primário;
- busca paginada usa lookup batch de covers para evitar N+1.

Integração Application/API-read entregue:

- `GameDetails.ImageUrl` é preenchido a partir de metadados resolvidos quando disponível;
- `GameSearchItem.ImageUrl` é preenchido após lookup batch de covers;
- comportamento de fallback existente no frontend continua válido quando não existe cover ou URL suportada;
- screenshots permanecem persistidos para uso futuro em detalhes/galeria e ainda não são expostos pelo contrato público atual.

Revisão de consistência concluída durante a GMI-29:

- vínculos canônicos em `External*Record` permanecem protegidos contra relink para outra entidade canônica;
- regras de timestamp de `ExternalCompanyRecord` e `ExternalCollectionRecord` foram alinhadas às demais entidades External*Record;
- timestamps de observação são normalizados para UTC;
- `LastSeenAt` não retrocede;
- `SourceUpdatedAt` avança apenas quando um timestamp mais novo da fonte é observado.

Política de sincronização definida para trabalho futuro do Collector:

- ausência em uma única coleta não significa automaticamente remoção na fonte;
- apenas observação conhecida como completa e confiável pode justificar sincronizar uma associação para fora do estado atual;
- remover uma associação não remove o `External*Record` correspondente;
- mudanças específicas de uma fonte não devem se propagar cegamente para evidências de outras fontes;
- o banco operacional armazena o estado tratado atual mais a provenance mínima necessária, e não histórico detalhado indefinido.

Gate final de qualidade da GMI-29:

```text
Build: aprovado
Testes: 460 aprovados
Falhas: 0
Ignorados: 0
```

A GMI-29 foi mergeada em `develop`, enviada ao remoto e encerrada com working tree limpo.

### GMI-30 — Validação de persistência e orçamento de armazenamento

Status: **Em andamento — migrations, planos de consulta, contratos e volume representativo já validados**

Validações de migration e compatibilidade concluídas:

- sequência de migrations revisada até `RemoveGameImageUrl`;
- `dotnet ef migrations has-pending-model-changes` confirmou ausência de drift do modelo;
- o banco atual apontado pelo Neon não tinha migrations pendentes;
- o PostgreSQL local aceitou corretamente todas as migrations pendentes quando o destino foi explicitado por `--connection`;
- um PostgreSQL vazio e isolado de validação aplicou com sucesso toda a cadeia de migrations desde zero;
- o build completo da solução passou;
- a suíte automatizada completa passou com 460 de 460 testes;
- os contratos públicos atuais de imagem permaneceram compatíveis.

Achado de ambiente de desenvolvimento:

- `DefaultConnection` pode ser resolvido por .NET User Secrets e, portanto, apontar para Neon;
- validações locais de EF devem usar destino de conexão explícito quando o banco pretendido for o PostgreSQL do Docker.

Dataset local representativo:

```text
Games                  10.000
GameGenres             20.000
GamePlatforms          20.000
ExternalGameRecords    10.000
GameImages             16.666
```

Baseline medido após `ANALYZE`:

```text
Dados das tabelas      ~7,4 MB
Índices                ~11 MB
Total de objetos       ~19 MB
```

Achados representativos dos planos:

- busca substring por nome com `ILIKE '%term%'` usou sequential scan e concluiu em aproximadamente 3,7 ms com 10.000 jogos;
- filtro por ano de release usando `EXTRACT(YEAR FROM FirstReleaseDate)` usou sequential scan e concluiu em aproximadamente 1,8 ms;
- filtro por gênero usou `IX_GameGenres_GenreId`;
- filtro por plataforma usou `IX_GamePlatforms_PlatformId`;
- lookup batch de covers usou `IX_game_images_GameId`;
- nenhum novo índice é justificado apenas para eliminar os sequential scans atualmente baratos.

Cenário de pressão de alta cardinalidade adicionado, preservando o mesmo catálogo de 10.000 jogos:

```text
Releases contextuais             30.000
Associações de theme             30.000
Associações de game mode         20.000
Associações de player perspective 20.000
Associações de keyword           80.000
Associações de company           20.000
Associações de collection         5.000
Product relations                 2.500
```

Medição após a extensão de alta cardinalidade:

```text
Dados das tabelas      ~29 MB
Índices                ~33 MB
Total de objetos       ~63 MB
```

O cenário de pressão acrescentou aproximadamente 44 MB sem aumentar a quantidade de jogos.

Maiores consumidores de armazenamento observados:

- `game_keywords`: aproximadamente 14 MB;
- `game_releases`: aproximadamente 10,1 MB;
- `game_images`: aproximadamente 5,3 MB;
- `game_themes`: aproximadamente 5,3 MB;
- `game_companies`: aproximadamente 5,3 MB.

Evidência de orçamento medida por `pg_total_relation_size`:

| Relação | Linhas | Aproximadamente bytes totais por linha |
|---|---:|---:|
| `game_releases` | 30.000 | 344 B |
| `game_images` | 16.666 | 328 B |
| `Games` | 10.000 | 327 B |
| `game_companies` | 20.000 | 271 B |
| `game_themes` | 30.000 | 182 B |
| `GameGenres` | 20.000 | 181 B |
| `GamePlatforms` | 20.000 | 181 B |
| `game_keywords` | 80.000 | 181 B |

Interpretação de capacidade:

- o cenário medido de ~63 MB / 10.000 jogos é referência de orçamento, não previsão fixa de produção;
- cobertura de catálogo e profundidade de metadados são decisões separadas de capacidade;
- jogos relevantes não devem ser descartados arbitrariamente apenas para satisfazer um número fixo de registros;
- profundidade de metadados e associações multiplicativas devem ser controladas de acordo com perguntas aprovadas de produto e capacidade observada;
- índices representam parte material do custo de armazenamento e devem ser justificados por access paths reais;
- keywords e releases contextuais foram os maiores pontos sintéticos de pressão de alta cardinalidade medidos até agora.

Política atual de capacidade:

```text
< 70%
→ operação normal

70%+
→ investigar crescimento por tabela e índice

antes de 80%
→ executar retenção controlada ou ação de capacidade

aproximando 90%
→ proteger writes essenciais e reduzir ingestão não essencial
```

Prioridade de retenção permanece:

1. estado canônico atual do produto;
2. identidades externas ativas;
3. provenance ainda sustentando o estado atual;
4. histórico/auxiliares recentes e úteis quando esse histórico existir.

Exclusão motivada por capacidade nunca deve ser interpretada como fato de domínio da fonte.

Retenção de armazenamento e reconciliação da fonte permanecem preocupações separadas.

Trabalho restante para fechamento da GMI-30:

- consolidar a evidência medida na documentação bilíngue do projeto;
- confirmar a redação final do orçamento operacional e risk register;
- remover ou reter explicitamente os artefatos locais de validação sintética conforme a política do repositório;
- gerar a revisão final de `AGENTS.md` da GMI-30;
- repetir build/test final e gates de qualidade do Git;
- commitar documentação e artefatos de validação;
- mergear a feature branch em `develop`;
- fazer push de `develop`;
- atualizar o Jira com evidência de migrations, planos, armazenamento, build e testes;
- fechar a subtarefa Jira após a integração completa.

## 2.4 Implementação do Collector

Status: **Pendente após conclusão do modelo de persistência aprovado**

O Collector deve ser refatorado da estrutura de PoC para responsabilidades orientadas a produção.

Estrutura esperada:

```text
Scheduler
→ Worker
→ IGDB import Job
→ IGDB client
→ IGDB contracts
→ mapper/import boundary
→ canonical model + external identity + provenance
→ repository
→ checkpoint
→ shutdown
```

Comportamento requerido:

- manter o Worker pequeno;
- separar Jobs;
- isolar contratos IGDB;
- mapear DTOs da fonte para input source-neutral de Application/Domain;
- implementar paginação;
- respeitar rate limits;
- implementar retries e falha segura;
- suportar coleta incremental por `updated_at`;
- garantir execução idempotente;
- evitar logar secrets ou tokens;
- evitar persistir payloads brutos completos permanentemente;
- preservar identidade da fonte e provenance;
- preservar cobertura relevante do catálogo enquanto controla profundidade de metadados não essenciais;
- tratar explicitamente observações completas versus parciais antes de remover associações atuais.

O Collector não deve mapear respostas do provider diretamente para entidades EF Core.

## 2.5 Persistência IGDB e qualidade dos dados

Status: **Modelo de persistência implementado até GMI-29; validação operacional em andamento pela GMI-30**

Já implementado no nível de modelo de persistência:

- identidade externa;
- prevenção de duplicidade por identidade de fonte;
- releases contextuais;
- classificações aprovadas;
- tipos de produto;
- relacionamentos de produto;
- companies e roles;
- collections;
- metadados de cover/screenshot;
- construção source-aware de URL de imagem para casos atuais de leitura;
- delete restritivo;
- associações com provenance.

Ainda necessário no caminho de ingestão:

- orquestração idempotente de create/update;
- regras de seleção canônica;
- regras conservadoras de inclusão;
- tratamento de registros rejeitados/problemáticos da fonte;
- comportamento de atualização da fonte;
- reexecução segura;
- tratamento explícito de observações completas versus parciais antes de remover associações atuais;
- validação com dados reais representativos;
- medição do impacto real de armazenamento em produção;
- decisões orientadas a produto sobre profundidade de campos de alta cardinalidade.

## 2.6 Integração API e frontend

Status: **Pendente de dados reais representativos e trabalho pós-persistência na API**

A API deve expor contratos source-neutral por caso de uso.

Ela **não** deve expor todo campo persistido ou identificador interno de provenance apenas porque existe no banco.

Os contratos atuais já preservam uma fronteira simples de imagem para o frontend:

- `GameDetails.ImageUrl?`;
- `GameSearchItem.ImageUrl?`.

Essas URLs são resolvidas a partir de metadados de imagem persistidos no backend, em vez de armazenadas diretamente em `Game`.

Conceitos adicionais possíveis para API/detalhes após conclusão da persistência incluem:

- tipo de produto;
- produtos relacionados;
- companies e roles;
- collections;
- releases contextuais;
- contexto de classificações selecionadas;
- screenshots/metadados de galeria se uma experiência de detalhe validada exigir;
- informações de fonte/atribuição.

O frontend deve organizar esses contratos para o workflow de decisão do usuário, em vez de espelhar o schema do banco.

Apresentação provável do produto:

### Cards de Comparable Games

Manter concisos.

Possíveis adições:

- tipo de produto;
- apenas contexto selecionado de alto valor.

Evitar exibir provenance completa ou toda associação persistida nos cards.

### Detalhes do jogo

Possíveis adições:

- tipo de produto;
- produtos relacionados;
- companies agrupadas por role;
- collections;
- informações de release contextual;
- classificações selecionadas;
- contexto de fonte e atribuição.

### Filtros

Possíveis filtros futuros incluem:

- tipo de produto;
- themes;
- game modes;
- companies;
- períodos de release.

Um filtro só deve ser introduzido quando responder a uma pergunta validada de produto e a cobertura dos dados for suficiente.

Validação com dados reais deve incluir:

- controles de gênero/plataforma populados;
- cards de resultado populados;
- resolução de cover e comportamento de fallback;
- paginação sem N+1 de imagem;
- detalhes do jogo;
- apresentação de fonte e provenance;
- reliability e limitations;
- produtos relacionados;
- tipos de produto;
- company roles;
- collections;
- utilidade de keywords;
- atribuição e links originais quando exigidos e permitidos.

## 2.7 Deploy e operação do Worker

Status: **Planejado**

Definir e validar:

- execução one-shot agendada;
- scheduler zero-cost aprovado;
- secrets de produção;
- conectividade com Neon;
- empacotamento/build;
- logs e visibilidade de falha;
- procedimento de retry/rerun;
- checkpoints;
- duração de execução;
- conformidade com rate limits;
- confirmação de custo operacional.

## Definition of Done do Milestone 2

O Milestone 2 está concluído quando:

- decisões da PoC IGDB estão documentadas e aprovadas;
- o spike multi-source confirma extensibilidade;
- o modelo de persistência aprovado está completo;
- GMI-25 até GMI-30 estão integradas e validadas;
- o Collector possui responsabilidades claras;
- coleta é paginada, incremental e idempotente;
- dados IGDB são mapeados sem tornar IGDB o modelo interno;
- registros canônicos preservam identidade externa e provenance;
- dados representativos são persistidos no Neon;
- impacto de armazenamento é medido e aceitável;
- contratos da API expõem os dados selecionados úteis ao produto;
- frontend opera com dados reais representativos;
- contexto de fonte e reliability é visível quando útil;
- requisitos de atribuição são atendidos;
- Worker é implantado e executado com sucesso;
- comportamento de falha e rerun é documentado;
- testes automatizados passam;
- documentação está atualizada;
- o MVP com dados reais funciona de ponta a ponta.

## Milestone 3 — Enriquecimento multi-source e reconciliação

Status: **Planejado**

### Objetivo

Adicionar Wikidata e Steam como fontes complementares sem substituir o MVP vertical com IGDB ou reescrever as fundações source-neutral.

### 3.1 Proof of concept da Wikidata

Avaliar:

- acesso estruturado autorizado;
- QID e identificadores externos;
- aliases e links canônicos;
- statements de company e relacionamentos;
- variabilidade em nível de statement e ausência de dados;
- licenciamento e atribuição;
- limites de query e estabilidade operacional;
- valor para reconciliação.

Franchises permanecem adiadas a menos que um requisito futuro de produto as ative explicitamente.

### 3.2 Proof of concept da Steam

Avaliar apenas caminhos oficiais e permitidos:

- identidade AppId;
- nomes e informações de release específicas da Steam;
- developers e publishers;
- categories e features;
- sistemas e idiomas suportados;
- sinais específicos da Steam que sejam legalmente utilizáveis;
- limitações de armazenamento, atribuição, região e endpoint.

Um jogo sem identidade Steam continua válido no catálogo canônico.

Steam não deve se tornar autoridade obrigatória de identidade.

### 3.3 Modelo comum de observação e reconciliação

Definir uma fronteira source-independent de comparação para conceitos como:

- fonte e identidade externa;
- nome observado e aliases;
- observações de release com contexto;
- plataformas;
- developers e publishers;
- collections;
- tipo de produto;
- relacionamentos de produto;
- metadados específicos da fonte.

A reconciliação deve:

- priorizar identificadores cruzados fortes;
- usar geração conservadora de candidatos;
- preservar produtos relacionados distintos;
- manter classificações e provenance;
- evitar majority voting sem contexto semântico;
- bloquear reconciliação automática em conflitos graves de tipo de produto;
- preservar aprovações e rejeições manuais quando necessário.

Nomes normalizados podem contribuir para descoberta de candidatos, mas não provam identidade.

### 3.4 Experiência multi-source no produto

Possíveis adições:

- contexto de convergência e divergência entre fontes;
- perfis de confiança;
- apresentação source-aware de campos;
- status de reconciliação quando útil;
- links de fonte e atribuição;
- distinção clara entre observações, valores canônicos e inferência do GMI.

## Definition of Done do Milestone 3

O Milestone 3 está concluído quando:

- PoCs de Wikidata e Steam estão documentadas e aprovadas para papéis definidos;
- ambas integrações seguem as mesmas fronteiras arquiteturais da IGDB;
- observações podem ser comparadas sem apagar contexto de fonte;
- reconciliação é conservadora e testável;
- provenance e confidence são visíveis quando úteis;
- o fluxo multi-source funciona de ponta a ponta.

## Milestones posteriores

### Exploração avançada de Comparable Games

Escopo potencial:

- múltiplos gêneros;
- múltiplas plataformas;
- themes;
- modes;
- perspectives;
- keywords;
- filtros de tipo de produto;
- filtros de company;
- filtros por período de release;
- detalhes mais ricos;
- ordenação;
- otimização de performance;
- buscas salvas;
- resumos analíticos.

Keywords continuam como uma estratégia central de valor do produto porque permitem que producers comecem por uma ideia ou micro-nicho, e não apenas por um título conhecido.

### Fundação de métricas de mercado

Métricas prioritárias:

- vendas;
- receita;
- estimated owners;
- downloads;
- active players;
- concurrent players;
- reviews;
- wishlists;
- outras observações justificadas de engajamento.

Todas as métricas devem preservar:

- fonte;
- significado;
- período;
- método;
- confiança.

### Análise de mercado e apoio à decisão

Adiado até haver dados estáveis e perguntas validadas:

- sinais de mercado;
- análise de gênero;
- análise de plataforma;
- contexto de janela de lançamento;
- relatórios source-aware;
- recomendações;
- forecasting;
- avaliação de machine learning.

## Foco atual de entrega

```text
Milestone 2 — MVP vertical com IGDB
→ validação de persistência e orçamento da GMI-30
→ checkpoint do modelo de persistência
→ trabalho do Collector orientado a produção
```

Sequência imediata:

1. finalizar documentação e validação do orçamento operacional da GMI-30;
2. gerar a revisão final de `AGENTS.md` da GMI-30;
3. rodar build/test final e gates de qualidade do Git;
4. integrar GMI-30 em `develop`;
5. confirmar o modelo de persistência aprovado do Milestone 2;
6. refatorar o Collector para responsabilidades orientadas a produção;
7. implementar ingestão IGDB idempotente;
8. popular dados reais representativos;
9. validar comportamento real de armazenamento em produção contra o orçamento local medido;
10. expor conceitos selecionados por meio de contratos de API;
11. organizar esses contratos no frontend;
12. fazer deploy e operar o Worker;
13. validar o MVP com dados reais de ponta a ponta.

## Checkpoint arquitetural atual

A direção atual de persistência é:

```text
Fonte externa
→ observação específica da fonte
→ External*Record / provenance
→ filtragem e seleção no Worker
→ modelo canônico do GMI
→ contrato por caso de uso em Application/API
→ apresentação no frontend
```

A camada de persistência é intencionalmente mais rica que qualquer tela individual do frontend.

A API seleciona o que cada caso de uso precisa.

O frontend organiza essa seleção para o producer e não deve se tornar um espelho direto do banco.

A lição atual de capacidade também está explícita:

```text
Preservar cobertura relevante do catálogo
→ controlar profundidade de metadados pelo valor de produto
→ monitorar associações multiplicativas e índices
→ agir antes que pressão de armazenamento vire incidente
```
