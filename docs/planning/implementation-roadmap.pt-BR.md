# Roadmap de Implementação

> Atualizado em: 29 de julho de 2026

## Objetivo

Este documento apresenta a evolução da implementação do Game Market Intelligence por milestones.

Ele registra incrementos concluídos, foco atual de entrega e evolução esperada sem substituir documentos detalhados de domínio, arquitetura, avaliação de fontes, design, prova de conceito ou implementação.

O roadmap pode mudar conforme fontes reais, limitações de infraestrutura, validação do produto e aprendizado de deploy produzam novas evidências.

## Direção do produto

Game Market Intelligence é uma plataforma de apoio à decisão para Game Producers e pequenos estúdios.

O produto organiza jogos comparáveis, referências confiáveis, evidências conscientes da fonte e futuro contexto comercial para reduzir incerteza durante o planejamento inicial.

```text
Ideia de jogo
    ↓
Descoberta de jogos comparáveis
    ↓
Referências de pesquisa
    ↓
Evidência comercial
    ↓
Análise de mercado
    ↓
Apoio à decisão com stakeholders
```

A plataforma deve preservar proveniência, diferenciar observações externas de conclusões internas e permanecer extensível a múltiplas fontes.

## Princípios de entrega

O projeto é desenvolvido por incrementos verticais pequenos e completos.

Cada entrega deve:

- responder ou viabilizar uma pergunta real de produto;
- entregar uma experiência de ponta a ponta quando apropriado;
- preservar as fronteiras entre Domain, Application, Infrastructure, API, Collector, Shared e Web;
- preservar proveniência e identidades externas;
- incluir testes automatizados adequados;
- atualizar documentação técnica e de produto;
- passar pela validação de Pull Request antes de entrar em `main`;
- continuar compatível com a infraestrutura gratuita aprovada;
- evitar decisões irreversíveis de domínio ou persistência antes de evidência suficiente;
- integrar fontes gradualmente sem impedir suporte multifonte futuro.

## Milestone 0 — Fundação do projeto

Status: **Concluído**

Entregue:

- estrutura da solução .NET;
- projetos Domain, Application, Infrastructure, API, Shared, Collector e Web;
- PostgreSQL local;
- EF Core e Npgsql;
- modelagem inicial de `DataSource` e `SourceReliability`;
- bases de testes automatizados;
- Continuous Integration com GitHub Actions;
- fundamentos de Terraform e deploy;
- documentação inicial de arquitetura e produto.

## Milestone 1 — Fundação de Comparable Games e primeira experiência de leitura

Status: **Concluído**

Entregue:

- fundamentos de `Genre`, `Platform` e `Game`;
- normalização de nomes e prevenção de duplicidade;
- relações muitos-para-muitos;
- persistência PostgreSQL e testes de integração;
- endpoints de busca e detalhes;
- endpoints de gêneros e plataformas;
- filtros por nome, gênero, plataforma, ano e paginação;
- tratamento global de exceções com `ProblemDetails`;
- integração Blazor WebAssembly com a API;
- shell responsivo, navegação, filtros, estados, paginação e componentes reutilizáveis;
- integração de deploy por ambiente;
- validação no navegador com banco de produção ainda vazio;
- ação visível de envio da pesquisa;
- documentação e revisão de aprendizado.

A validação dependente de dados permanece pendente até a persistência de dados reais representativos.

## Milestone 2 — MVP vertical com IGDB

Status: **Em andamento**

### Objetivo

Entregar o primeiro MVP funcional com dados reais usando a IGDB como primeira fonte ativa, da coleta autorizada até o deploy e a apresentação no produto.

Este milestone **não abandona** a estratégia multifonte. Ele entrega uma fonte verticalmente para validar Collector, persistência, deploy, API, frontend, proveniência e operação antes da entrada das demais fontes.

### 2.1 Prova de conceito da IGDB

Em andamento:

- autenticação OAuth da Twitch;
- integração com `/v4/games`;
- contratos específicos da fonte;
- amostra recente por `updated_at`;
- amostra controlada por IDs;
- amostra reproduzível de 100 registros por IDs fixados;
- inspeção de cobertura de `alternative_names`, `version_title` e
  `game_localizations`, com exclusão de `alternative_names` do mapping do MVP e
  adiamento de `game_localizations` para depois do MVP por não melhorar, neste
  momento, uma capacidade prioritária de comparação de viabilidade;
- amostra com `parent_game`;
- inspeção de tipo, status, relações, plataformas, gêneros, temas e keywords;
- diagnóstico explícito de erros HTTP;
- documentação de nulabilidade, completude e relações.

Ainda necessário:

- exemplos controlados de DLC e remake;
- aprofundar edições, bundles, ports, remasters, mods e expansões;
- avaliar releases por plataforma;
- avaliar capas e permissões de imagem;
- avaliar empresas, franquias, collections, modos e perspectivas;
- medir cobertura e nulabilidade em amostra maior;
- validar paginação, rate limit, atualização incremental, token e retomada;
- fechar campos candidatos e critérios de aprovação.

### 2.2 Spike leve de compatibilidade multifonte

Antes de aprovar o mapping IGDB para o modelo interno, realizar uma revisão arquitetural curta para Wikidata e Steam.

O spike será documental, sem autenticação, clients produtivos ou PoCs completas.

Deve confirmar suporte a:

- ID canônico interno;
- múltiplas identidades externas por `Source + ExternalId`;
- contratos específicos por fonte;
- mappers específicos por fonte;
- fronteira de importação ou observação independente da fonte;
- preservação de proveniência;
- metadados exclusivos sem forçá-los no modelo canônico;
- futura comparação e reconciliação;
- diferenças futuras em nomes, datas, plataformas, empresas e relações.

Critério de saída:

> A implementação IGDB pode avançar sem exigir remodelação estrutural quando Wikidata e Steam entrarem no Milestone 3.

### 2.3 Implementação do Collector

Após a PoC e o spike:

- refatorar a estrutura exploratória;
- manter o Worker como coordenador pequeno;
- separar Jobs;
- introduzir serviço/caso de uso de importação;
- introduzir mapper IGDB;
- isolar contratos da IGDB do modelo canônico;
- implementar paginação e rate limit;
- implementar retries e falhas seguras;
- implementar coleta incremental por `updated_at`;
- avaliar checksum quando útil;
- garantir idempotência;
- nunca registrar secrets ou tokens.

Fluxo esperado:

```text
Agendador
→ Worker
→ Job de importação IGDB
→ client IGDB
→ contratos IGDB
→ mapper/fronteira de importação
→ modelo canônico + identidade externa + proveniência
→ repositório
→ checkpoint
→ encerramento
```

### 2.4 Revisão de domínio e persistência

Nenhuma migration importante deve ser criada antes da revisão da PoC e do spike multifonte.

O modelo não deve:

- usar ID da IGDB como chave primária de `Game`;
- assumir uma única identidade externa;
- tornar tipos da IGDB conceitos universais;
- apagar proveniência;
- forçar todo campo externo em `Game`;
- fundir produtos relacionados apenas pelo nome.

O modelo deve preservar:

- identidade canônica do GMI;
- identidade externa;
- associação com fonte;
- timestamps relevantes;
- produtos relacionados como registros distintos;
- releases opcionais e contextuais;
- fronteira conservadora de reconciliação.

### 2.5 Persistência e qualidade IGDB

Implementar:

- criação e atualização idempotentes;
- prevenção de duplicidade por identidade externa;
- regras conservadoras de inclusão;
- tratamento de campos nulos e incompletos;
- evidência mínima para registros problemáticos;
- sem obrigação de guardar permanentemente todo payload processado;
- validações de qualidade;
- reexecução segura;
- testes de integração.

### 2.6 Integração com API e frontend

Validar com dados representativos:

- controles preenchidos;
- cards reais;
- paginação;
- detalhes;
- fonte e proveniência;
- confiabilidade e limitações;
- versões relacionadas e tipos de produto;
- utilidade de temas e keywords;
- exploração por ideia e nicho;
- links e atribuição quando exigidos e permitidos.

### 2.7 Deploy e operação do Worker

Definir e validar:

- processo one-shot agendado;
- GitHub Actions ou agendador gratuito aprovado;
- secrets de produção;
- conexão com Neon;
- empacotamento/build;
- logs e visibilidade de falhas;
- procedimento de retry e rerun;
- checkpoints;
- duração e rate limit;
- confirmação de custo operacional zero.

### Definition of Done do Milestone 2

Concluído quando:

- PoC IGDB aprovada;
- spike multifonte confirmar extensibilidade;
- Collector estiver refatorado com responsabilidades claras;
- coleta for paginada, incremental e idempotente;
- mapping não transformar IGDB no modelo interno;
- registros canônicos preservarem identidade externa e proveniência;
- dados representativos estiverem no Neon;
- API e frontend funcionarem com dados reais;
- fonte e confiabilidade estiverem visíveis;
- Worker estiver implantado e executado em produção;
- falhas e reexecução estiverem documentadas;
- testes passarem;
- documentação estiver atualizada;
- MVP real funcionar de ponta a ponta.

## Milestone 3 — Enriquecimento e reconciliação multifonte

Status: **Planejado**

### Objetivo

Adicionar Wikidata e Steam como fontes complementares sem substituir o MVP vertical da IGDB nem reescrever suas fundações estruturais.

### 3.1 PoC Wikidata

Avaliar:

- acesso estruturado autorizado;
- QID e identificadores externos;
- aliases e links canônicos;
- empresas, franquias e relações;
- variabilidade por statement;
- licença e atribuição;
- limites e estabilidade;
- valor para reconciliação.

### 3.2 PoC Steam

Avaliar somente acessos oficiais e permitidos:

- AppId;
- nomes e releases específicos da plataforma;
- developers e publishers;
- categorias e features;
- sistemas e idiomas;
- sinais Steam legalmente utilizáveis;
- limitações de armazenamento, atribuição, região e endpoint.

Ausência de identidade Steam não invalida um jogo.

### 3.3 Modelo comum de observação e reconciliação

Definir fronteira independente da fonte para:

- fonte e identidade;
- nome e aliases observados;
- releases contextualizados;
- plataformas;
- developers e publishers;
- franquias/collections;
- tipo de produto;
- relações;
- metadados exclusivos.

A reconciliação deve:

- priorizar identificadores cruzados fortes;
- gerar candidatos de forma conservadora;
- preservar produtos relacionados distintos;
- manter classificações e proveniência;
- evitar votação por maioria sem contexto semântico;
- bloquear conflitos graves de natureza do produto;
- preservar decisões manuais quando necessário.

### 3.4 Experiência multifonte

Adicionar:

- convergências e divergências;
- perfis de confiança;
- apresentação consciente da fonte;
- status de reconciliação quando útil;
- links e atribuição;
- distinção entre observação, valor canônico e inferência do GMI.

### Definition of Done do Milestone 3

Concluído quando:

- PoCs Wikidata e Steam forem aprovadas para papéis definidos;
- integrações respeitarem as mesmas fronteiras;
- observações puderem ser comparadas sem apagar origem;
- reconciliação for conservadora e testável;
- proveniência e confiança forem visíveis;
- fluxo multifonte funcionar de ponta a ponta.

## Milestones posteriores

### Exploração avançada de Comparable Games

Possíveis itens:

- múltiplos gêneros com regra de todos selecionados;
- múltiplas plataformas com OR;
- temas, modos, perspectivas e keywords;
- filtros de período e empresas;
- ordenação e performance;
- pesquisas salvas;
- detalhes e análises mais ricos.

Keywords permanecem estratégia central de valor porque permitem partir de uma ideia ou micro-nicho.

### Fundação de métricas de mercado

Prioridade:

- vendas;
- receita;
- owners estimados;
- downloads;
- usuários ativos;
- concorrência;
- reviews;
- wishlists.

Toda métrica deve preservar fonte, significado, período, método e confiança.

### Análise e apoio à decisão

Adiado até existirem dados estáveis e perguntas validadas:

- market signals;
- análise de gênero e plataforma;
- contexto de janela de lançamento;
- relatórios;
- recomendações;
- forecasting e machine learning.

## Foco atual

```text
Milestone 2 — MVP vertical com IGDB
```

Sequência imediata:

1. concluir PoC de relações e campos;
2. fechar documento de observações;
3. realizar spike multifonte;
4. aprovar fronteira de importação independente da fonte;
5. refatorar Collector;
6. revisar domínio e persistência;
7. implementar ingestão idempotente;
8. fazer deploy e operar o Worker;
9. validar API e frontend com dados reais;
10. fechar MVP IGDB.
