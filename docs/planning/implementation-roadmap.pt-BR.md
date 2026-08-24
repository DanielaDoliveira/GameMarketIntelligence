# Roadmap de Implementação

> Atualizado em: 24 de agosto de 2026

## Propósito

Este documento fornece uma visão em nível de milestones da implementação do Game Market Intelligence.

Ele registra incrementos concluídos, o foco atual de entrega e a evolução esperada do produto sem substituir a documentação detalhada de domínio, arquitetura, avaliação de fontes, design, provas de conceito ou incrementos específicos.

O roadmap pode mudar conforme fontes reais, restrições de infraestrutura, validação de produto, aprendizado de deployment e medições de armazenamento produzam novas evidências.

## Direção do produto

Game Market Intelligence é uma plataforma de apoio à decisão para Game Producers e pequenos estúdios.

O produto organiza jogos comparáveis, referências confiáveis de pesquisa, evidências com contexto de fonte e, futuramente, contexto comercial em um fluxo que ajuda a reduzir incerteza no planejamento inicial de produto.

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

A plataforma deve preservar proveniência, distinguir observações de fonte de conclusões internas e permanecer extensível para múltiplas fontes.

## Princípios de entrega

Game Market Intelligence é desenvolvido por meio de incrementos verticais pequenos e completos.

Cada entrega deve:

- responder ou viabilizar uma pergunta real de produto;
- fornecer uma experiência ponta a ponta utilizável quando apropriado;
- preservar as fronteiras entre Domain, Application, Infrastructure, API, Collector, Shared e Web;
- preservar proveniência e identidades externas;
- incluir testes automatizados adequados;
- atualizar a documentação técnica e de produto;
- passar pela validação de Pull Request antes de entrar em `main`;
- permanecer implantável na infraestrutura gratuita aprovada;
- evitar decisões irreversíveis de domínio ou persistência antes de existir evidência suficiente de fontes reais;
- integrar fontes incrementalmente sem impedir suporte futuro a múltiplas fontes;
- evitar expor diretamente o modelo de persistência em contratos de API ou frontend;
- tratar limites de armazenamento como restrição de produto.

## Milestone 0 — Fundação do projeto

Status: **Concluído**

Entregue:

- estrutura da solução .NET;
- projetos Domain, Application, Infrastructure, API, Shared, Collector e Web;
- ambiente local com PostgreSQL;
- configuração do EF Core e Npgsql;
- modelagem inicial de `DataSource` e `SourceReliability`;
- fundações de testes automatizados;
- Continuous Integration com GitHub Actions;
- fundações de Terraform e deployment;
- documentação inicial de arquitetura e produto.

## Milestone 1 — Fundação de Comparable Games e primeira experiência de leitura

Status: **Concluído**

Entregue:

- fundações de domínio para `Genre`, `Platform` e `Game`;
- regras de nome normalizado e prevenção de duplicidade;
- relacionamentos N:N entre jogos, gêneros e plataformas;
- persistência PostgreSQL e integration tests;
- endpoints de busca e detalhes de Comparable Games;
- endpoints de listagem de gêneros e plataformas;
- suporte a nome parcial, gênero, plataforma, ano de lançamento e paginação;
- tratamento centralizado de exceções com `ProblemDetails`;
- integração do Blazor WebAssembly com a API;
- shell responsivo, navegação, filtros, estados de resultado, paginação e componentes reutilizáveis;
- integração de deployment por ambiente;
- validação no navegador com o dataset de produção ainda vazio;
- ação visível e explícita de submissão da busca;
- documentação e revisão de aprendizado do primeiro milestone de frontend.

A validação dependente de dados permanece pendente até que dados reais representativos sejam persistidos.

## Milestone 2 — MVP vertical com IGDB

Status: **Em andamento**

### Objetivo

Entregar o primeiro MVP funcional com dados reais usando a IGDB como primeira fonte ativa, da coleta autorizada até persistência, deployment, uso pela API e apresentação no produto.

Este milestone **não** abandona a estratégia multi-fonte.

Ele entrega uma fonte de ponta a ponta para que o projeto possa validar:

- comportamento do Collector;
- contratos específicos de fonte;
- persistência canônica;
- proveniência;
- impacto de armazenamento;
- deployment;
- uso pela API;
- apresentação no frontend;
- atribuição e comportamento operacional.

## 2.1 Prova de conceito da IGDB

Status: **Concluída para o escopo de persistência aprovado no Milestone 2**

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
- precisão e nulabilidade de datas;
- regiões e status;
- gêneros;
- temas;
- modos de jogo;
- perspectivas do jogador;
- keywords;
- empresas envolvidas;
- collections;
- covers e metadados de imagem;
- observações sobre títulos alternativos/localizados;
- nulabilidade e registros incompletos;
- utilidade de campos para Comparable Games;
- limitações específicas da fonte.

Decisões importantes produzidas pela PoC incluem:

- `first_release_date` permanece como valor canônico resumido e usado no filtro atual;
- releases contextuais exigem um modelo separado;
- jogos originais, ports, remakes, remasters, bundles, DLCs, expansões e versões permanecem produtos distintos;
- relações não propagam automaticamente classificações ou metadados;
- empresas exigem papéis e proveniência;
- collections foram aprovadas;
- franchises permanecem adiadas;
- themes, modes, perspectives e keywords são persistidos como classificações source-neutral separadas;
- binários de imagem não são armazenados;
- nulabilidade da fonte permanece semanticamente relevante e não é convertida em valores falsos;
- nomes normalizados ajudam em lookup, mas não são evidência suficiente de identidade.

Os documentos históricos da PoC permanecem como evidência das decisões detalhadas por campo e não devem ser reescritos como documentos de implementação.

## 2.2 Spike leve de compatibilidade multi-fonte

Status: **Concluído**

O spike confirmou que a arquitetura do Milestone 2 consegue suportar IGDB agora sem depender estruturalmente dela.

Regras arquiteturais validadas incluem:

- identificadores canônicos internos;
- múltiplas identidades externas via `DataSource + ExternalId`;
- contratos específicos por fonte;
- mappers específicos por fonte;
- entidades canônicas independentes de fonte;
- entidades contextuais com proveniência;
- metadados específicos de fonte fora dos conceitos universais do domínio;
- reconciliação futura conservadora;
- nenhuma dependência da Steam para identidade canônica;
- nenhuma propriedade de ID específica de provider no `Game`.

Critério de saída atingido:

> O design de persistência orientado inicialmente à IGDB pode prosseguir sem exigir redesenho estrutural quando Wikidata e Steam forem introduzidas posteriormente.

A reconciliação multi-fonte em si permanece adiada para o Milestone 3.

## 2.3 Implementação de domínio e persistência

Status: **Em andamento — GMI-25 a GMI-28 concluídas localmente**

A implementação de persistência é entregue por issues filhas sob a GMI-14.

### GMI-25 — Identidade externa de fonte

Status: **Concluída**

Entregue:

- separação entre identidade canônica e identidade externa;
- `ExternalGameRecord`;
- identidade de fonte via `DataSourceId + ExternalId`;
- vínculo opcional do registro externo com `Game` canônico;
- timestamps de observação;
- timestamp de atualização na fonte;
- prevenção de duplicidade;
- mappings do EF Core;
- migration;
- persistence tests.

### GMI-26 — Releases contextuais com proveniência

Status: **Concluída**

Entregue:

- `GameRelease` contextual;
- `Game.FirstReleaseDate` canônico;
- contexto de release por plataforma;
- identidade externa do release;
- `ExternalGameRecord` de origem;
- representação de data parcial;
- região, status e metadados de observação;
- delete behavior restritivo;
- mappings do EF Core;
- migration;
- persistence tests.

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

Associações com proveniência entregues:

- `GameTheme`;
- `GameGameMode`;
- `GamePlayerPerspective`;
- `GameKeyword`.

A implementação preserva identidade de fonte e não trata nomes normalizados como prova de equivalência cross-source.

### GMI-28 — Produtos, empresas, collections e relações

Status: **Implementação concluída; integração final da branch pendente**

Modelagem de produto entregue:

- `Game.ProductType`;
- `GameProductType`;
- `GameProductRelationType`;
- `GameProductRelation`;
- relações direcionadas entre produtos canônicos distintos;
- proveniência pelos `ExternalGameRecord` de origem e destino;
- nenhuma propagação automática entre produtos relacionados.

Modelagem de empresas entregue:

- `Company` canônica;
- `ExternalCompanyRecord`;
- `GameCompanyRole`;
- `GameCompany` com proveniência;
- cardinalidade N:N entre jogos e empresas;
- múltiplos papéis quando sustentados por evidência.

Modelagem de collections entregue:

- `Collection` canônica;
- `ExternalCollectionRecord`;
- `GameCollection` com proveniência;
- cardinalidade N:N entre jogos e collections.

Integridade de persistência entregue:

- conceitos canônicos source-neutral;
- identidades externas únicas por fonte;
- chaves compostas de associação;
- delete behavior restritivo;
- cobertura de migration;
- testes de deleção/integridade;
- reset do banco de integration tests atualizado para as novas tabelas do schema.

Quality gate final da GMI-28:

```text
Build: passou
Testes: 424 passaram
Falhas: 0
Ignorados: 0
```

Passos restantes para fechamento da GMI-28:

- commitar alterações finais de documentação/testes;
- verificar working tree limpa;
- fazer merge da feature branch em `develop`;
- fazer push de `develop`;
- registrar evidências de migration/build/test no Jira;
- transicionar a subtask no Jira somente após a integração estar concluída.

### GMI-29 — Metadados de cover e screenshots

Status: **Próxima**

Escopo esperado:

- metadados aprovados de cover;
- metadados de screenshots;
- identidade/proveniência de fonte;
- nulabilidade;
- referências de imagem seguras para armazenamento;
- sem persistência de binários de imagem;
- mappings;
- constraints;
- testes;
- migration.

O schema exato deve permanecer alinhado às decisões de imagem aprovadas na PoC.

### GMI-30 — Validação de persistência e orçamento de armazenamento

Status: **Planejada**

Escopo esperado:

- medição de armazenamento com dados representativos;
- análise de tabelas de alta cardinalidade;
- crescimento de linhas de releases;
- crescimento de associações de classificações;
- crescimento de metadados de imagem;
- revisão de tamanho de índices;
- validação do orçamento do free tier da Neon;
- decisões de retenção quando necessário;
- documentação dos limites medidos.

Nenhuma conclusão de capacidade deve depender apenas de estimativas quando dados representativos puderem ser medidos.

## 2.4 Implementação do Collector

Status: **Pendente após a conclusão do modelo de persistência aprovado**

O Collector deve ser refatorado da estrutura de PoC para responsabilidades orientadas à produção.

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

Comportamento necessário:

- manter o Worker pequeno;
- separar Jobs;
- isolar contratos da IGDB;
- mapear DTOs da fonte para input source-neutral de Application/Domain;
- implementar paginação;
- respeitar rate limits;
- implementar retries e comportamento seguro em falhas;
- suportar coleta incremental via `updated_at`;
- garantir execução idempotente;
- evitar logar secrets ou tokens;
- evitar persistência permanente de payloads completos;
- preservar identidade de fonte e proveniência.

O Collector não deve mapear respostas de provider diretamente para entidades do EF Core.

## 2.5 Persistência IGDB e qualidade de dados

Status: **Parcialmente implementada por meio da GMI-25–28**

Já implementado no nível de modelo de persistência:

- identidade externa;
- prevenção de duplicidade por identidade de fonte;
- releases contextuais;
- classificações aprovadas;
- tipos de produto;
- relações entre produtos;
- empresas e papéis;
- collections;
- delete behavior restritivo;
- associações com proveniência.

Ainda necessário no caminho de ingestão:

- orquestração idempotente de create/update;
- regras de seleção canônica;
- regras conservadoras de inclusão;
- tratamento de registros rejeitados/problemáticos;
- comportamento de atualização da fonte;
- reexecução segura;
- validação com dados representativos;
- impacto de armazenamento medido.

## 2.6 Integração API e frontend

Status: **Pendente de dados reais representativos e trabalho de API pós-persistência**

A API deve expor contratos source-neutral orientados a casos de uso.

Ela **não** deve expor todo campo persistido ou identificador interno de proveniência apenas porque existe no banco.

Conceitos potenciais para API/details após a conclusão da persistência incluem:

- product type;
- produtos relacionados;
- empresas e papéis;
- collections;
- releases contextuais;
- contexto selecionado de classificações;
- informações de fonte/atribuição.

O frontend deve organizar esses contratos para o fluxo de decisão do usuário, e não espelhar o schema do banco.

Apresentação provável no produto:

### Cards de Comparable Games

Manter conciso.

Possíveis adições:

- product type;
- apenas contexto selecionado de alto valor.

Evitar exibir proveniência completa ou todas as associações persistidas nos cards.

### Detalhes do jogo

Possíveis adições:

- product type;
- produtos relacionados;
- empresas agrupadas por papel;
- collections;
- informação de releases contextuais;
- classificações selecionadas;
- contexto de fonte e atribuição.

### Filtros

Possíveis filtros futuros incluem:

- product type;
- themes;
- game modes;
- empresas;
- períodos de lançamento.

Um filtro deve ser introduzido apenas quando responder a uma pergunta validada de produto e houver cobertura de dados suficiente.

A validação com dados reais deve incluir:

- controles de gênero/plataforma populados;
- cards populados;
- paginação;
- detalhes do jogo;
- apresentação de fonte e proveniência;
- confiabilidade e limitações;
- produtos relacionados;
- tipos de produto;
- papéis de empresas;
- collections;
- utilidade de keywords;
- atribuição e links originais quando exigidos e permitidos.

## 2.7 Deployment e operação do Worker

Status: **Planejado**

Definir e validar:

- execução agendada one-shot;
- scheduler gratuito aprovado;
- secrets de produção;
- conectividade com Neon;
- packaging/build;
- logs e visibilidade de falhas;
- procedimento de retry/reexecução;
- checkpoints;
- duração de execução;
- conformidade com rate limits;
- confirmação de custo operacional.

## Definition of Done do Milestone 2

O Milestone 2 está concluído quando:

- decisões da PoC IGDB estão documentadas e aprovadas;
- o spike de compatibilidade multi-fonte confirma extensibilidade;
- o modelo de persistência aprovado está completo;
- GMI-25 a GMI-30 estão integradas e validadas;
- o Collector possui responsabilidades claras;
- a coleta é paginada, incremental e idempotente;
- os dados da IGDB são mapeados sem tornar a IGDB o modelo interno;
- registros canônicos mantêm identidade externa e proveniência;
- dados representativos estão persistidos na Neon;
- o impacto de armazenamento foi medido e é aceitável;
- contratos da API expõem os dados selecionados de valor para o produto;
- o frontend opera com dados reais representativos;
- contexto de fonte e confiabilidade é visível quando útil;
- requisitos de atribuição estão atendidos;
- o Worker está implantado e executado com sucesso;
- comportamento de falha e reexecução está documentado;
- testes automatizados passam;
- documentação está atualizada;
- o MVP de dados reais funciona ponta a ponta.

## Milestone 3 — Enriquecimento multi-fonte e reconciliação

Status: **Planejado**

### Objetivo

Adicionar Wikidata e Steam como fontes complementares sem substituir o MVP vertical IGDB nem reescrever suas fundações source-neutral.

### 3.1 Prova de conceito Wikidata

Avaliar:

- acesso estruturado autorizado;
- QID e identificadores externos;
- aliases e links canônicos;
- statements de empresa e relações;
- variabilidade e ausência em nível de statement;
- licenciamento e atribuição;
- limites de consulta e estabilidade operacional;
- valor para reconciliação.

Franchises permanecem adiadas, a menos que um requisito futuro de produto as ative explicitamente.

### 3.2 Prova de conceito Steam

Avaliar apenas caminhos oficiais e permitidos:

- identidade por AppId;
- nomes e informações específicas de release na Steam;
- developers e publishers;
- categorias e features;
- sistemas e idiomas suportados;
- sinais específicos da Steam que possam ser usados legalmente;
- limitações de armazenamento, atribuição, região e endpoint.

Um jogo sem identidade Steam continua válido no catálogo canônico.

Steam não deve se tornar autoridade obrigatória de identidade.

### 3.3 Modelo comum de observação e reconciliação

Definir uma fronteira independente de fonte para conceitos como:

- fonte e identidade externa;
- nome observado e aliases;
- observações de release com contexto;
- plataformas;
- developers e publishers;
- collections;
- product type;
- relações entre produtos;
- metadados específicos de fonte.

A reconciliação deve:

- priorizar identificadores cruzados fortes;
- usar geração conservadora de candidatos;
- preservar produtos relacionados distintos;
- manter classificações e proveniência;
- evitar votação por maioria sem contexto semântico;
- bloquear reconciliação automática em conflitos sérios de tipo de produto;
- preservar aprovações e rejeições manuais quando necessário.

Nomes normalizados podem contribuir para descoberta de candidatos, mas não provam identidade.

### 3.4 Experiência multi-fonte no produto

Possíveis adições:

- contexto de convergência/divergência entre fontes;
- perfis de confiança;
- apresentação de campos com contexto de fonte;
- status de reconciliação quando útil;
- links de fonte e atribuição;
- distinção clara entre observações, valores canônicos e inferência do GMI.

## Definition of Done do Milestone 3

O Milestone 3 está concluído quando:

- PoCs de Wikidata e Steam estão documentadas e aprovadas para papéis definidos;
- ambas as integrações seguem as mesmas fronteiras arquiteturais da IGDB;
- observações podem ser comparadas sem apagar contexto de fonte;
- reconciliação é conservadora e testável;
- proveniência e confiança são visíveis quando úteis;
- o fluxo multi-fonte funciona ponta a ponta.

## Milestones posteriores

### Exploração avançada de Comparable Games

Escopo potencial:

- múltiplos gêneros;
- múltiplas plataformas;
- themes;
- modes;
- perspectives;
- keywords;
- filtros por product type;
- filtros por empresa;
- filtros por período de lançamento;
- detalhes mais ricos;
- ordenação;
- otimização de performance;
- buscas salvas;
- resumos analíticos.

Keywords permanecem uma estratégia central de valor porque permitem que producers comecem por uma ideia ou micro-nicho, e não apenas por um título já conhecido.

### Fundação de métricas de mercado

Métricas prioritárias:

- vendas;
- receita;
- owners estimados;
- downloads;
- jogadores ativos;
- jogadores concorrentes;
- reviews;
- wishlists;
- outras observações de engajamento justificadas.

Todas as métricas devem preservar:

- fonte;
- significado;
- período;
- método;
- confiança.

### Análise de mercado e apoio à decisão

Adiado até que dados estáveis e perguntas validadas existam:

- market signals;
- análise de gêneros;
- análise de plataformas;
- contexto de janela de lançamento;
- relatórios com contexto de fonte;
- recomendações;
- forecasting;
- avaliação de machine learning.

## Foco atual de entrega

```text
Milestone 2 — MVP vertical IGDB
→ integração final da GMI-28
→ GMI-29 metadados de cover e screenshots
→ GMI-30 validação do orçamento de armazenamento
```

Sequência imediata:

1. concluir o fechamento da branch da GMI-28 e fazer merge em `develop`;
2. implementar GMI-29;
3. implementar GMI-30;
4. confirmar o modelo de persistência aprovado para o Milestone 2;
5. refatorar o Collector para responsabilidades orientadas à produção;
6. implementar ingestão IGDB idempotente;
7. popular dados representativos;
8. validar comportamento de armazenamento;
9. expor novos conceitos selecionados por meio de contratos da API;
10. organizar esses contratos no frontend;
11. implantar e operar o Worker;
12. validar o MVP de dados reais ponta a ponta.

## Checkpoint arquitetural atual

A direção atual da persistência é:

```text
Fonte externa
→ observação específica da fonte
→ External*Record / proveniência
→ filtragem e seleção pelo Worker
→ modelo canônico do GMI
→ contrato de caso de uso da Application/API
→ apresentação no frontend
```

A camada de persistência é intencionalmente mais rica do que qualquer tela individual do frontend.

A API seleciona o que cada caso de uso precisa.

O frontend organiza essa informação selecionada para o producer e não deve se tornar um espelho direto do banco de dados.
