# Game Market Intelligence — Decisões de Dados, Reconciliação e PoC

## 1. Objetivo

Este documento consolida as decisões que orientam o MVP do **Game Market Intelligence (GMI)** após a prova de conceito com IGDB e os refinamentos posteriores do modelo de persistência.

O GMI não pretende armazenar todo dado possível sobre jogos. Seu objetivo é selecionar, organizar e apresentar apenas o que realmente ajuda producers em pesquisa inicial de mercado, descoberta de jogos comparáveis, exploração de nichos e análise de contexto competitivo.

> O GMI cria valor por meio de um conjunto focado de dados úteis, não pelo excesso de informação que dificulta a análise.

## 2. Escopo do MVP

O MVP fornecerá:

- busca por nome e aliases;
- exploração por keywords;
- filtros por genre, theme, platform, game mode, perspective, multiplayer e período de lançamento;
- contexto sobre empresas envolvidas;
- franchises e collections/series;
- relações de produto como remake, remaster, port, edition, DLC e expansion;
- proveniência e níveis de confiança;
- transparência sobre limitações, conflitos e cobertura.

Fora de escopo:

- métricas financeiras, vendas e receita;
- previsão de sucesso e scoring automático de oportunidade;
- taxonomia própria de subgêneros;
- modelagem separada de IP, subfranchise, universe, editorial line, brand, licensed property ou corporate group;
- análise profunda de multiplayer;
- histórico corporativo complexo;
- comparação detalhada de conteúdo entre editions;
- armazenamento arquivístico ilimitado de observações antigas.

Métricas financeiras podem ser consideradas depois da conclusão do MVP.

## 3. Fontes selecionadas

### IGDB

A IGDB será a principal fonte de catálogo e a taxonomia canônica do MVP para genres, themes, modes, perspectives, keywords, product types, relationships, platforms, release dates, companies, franchises, collections e identificadores externos.

### Wikidata

Wikidata dará suporte à reconciliação, enriquecimento, validação auxiliar e identificadores cross-source. Ela não substituirá automaticamente a IGDB como taxonomia canônica.

### Steam

Steam será uma fonte especializada para fatos específicos do ecossistema Steam, como datas de lançamento e identidade de produto na plataforma. A arquitetura e o processo de reconciliação não podem depender da Steam.

## 4. Mapping IGDB

### 4.1 Identidade e descoberta

**Incluir:** `id`, `name`, `alternative_names`, `game_type`, `version_parent`, `game_status`, `summary`.

**Adiar ou excluir:** adiar `slug`; não importar `storyline` inicialmente.

Regras:

- identidade externa = `Source + ExternalId`;
- nome isolado nunca sustenta reconciliação automática;
- `game_status` é contextual;
- `summary` apoia detalhes, não identidade.

### 4.2 Platforms e releases

**Incluir:** `platforms`, `release_dates.platform`, `date`, `date_format`, `release_region`, `status`, `updated_at`.

**Derivar:** `first_release_date`, apenas como conveniência.

**Adiar:** `platform_version_release_dates`.

Regras:

- preservar datas por platform e region;
- preservar a precisão original;
- nunca converter dado apenas de ano ou mês em datas artificiais;
- suportar consultas por intervalo de datas;
- datas diferentes em platforms diferentes não são conflitos.

### 4.3 Empresas envolvidas

**Incluir papéis:** developer, publisher, porting e supporting.

**Incluir dados mínimos de company:** identificador externo, nome, status quando disponível e `updated_at`.

**Adiar:** `changed_company_id`, websites, parent company, histórico corporativo e descrições longas.

Regras:

- companies são relações multivaloradas;
- role é obrigatório;
- ausência em uma fonte não é conflito;
- papéis diferentes podem ser complementares;
- porting e supporting não substituem developer ou publisher.

### 4.4 Tipos de produto e relações

**Incluir:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Adiar:** `expanded_games`, `game_versions` detalhados, `forks` e `similar_games`.

> Produtos relacionados permanecem registros separados.

### 4.5 Identificadores externos e websites

**Incluir de `external_games`:** `external_game_source`, `uid`, `url`, `platform`, `name`, `year`, `updated_at`.

**Incluir de `websites`:** `type`, `url`.

**Adiar:** countries, release format e checksums específicos de websites.

Regras:

- `Source + UID` identifica um registro externo;
- URLs devem ser validadas;
- `trusted` é apenas um sinal auxiliar.

### 4.6 Filtros e taxonomia

- **Genres:** múltiplos IDs usam AND; genres extras são permitidos.
- **Themes:** múltiplos valores usam AND; themes extras são permitidos.
- **Game modes:** múltiplos valores usam AND.
- **Perspectives:** múltiplos valores usam AND.
- **Keywords:** múltiplos valores usam AND; keywords são centrais para o valor do GMI; usar IDs estruturados; não criar keywords customizadas nem mesclar termos automaticamente.
- **Platforms:** múltiplos valores usam OR.
- **Multiplayer:** incluir multiplayer, online co-op e local/offline multiplayer; capacidades selecionadas usam AND sem exclusividade.

Fora de escopo para multiplayer: número máximo de jogadores, LAN, drop-in/drop-out e configurações detalhadas específicas por platform.

### 4.7 Franchises e collections

**Incluir:** franchises, collections/series, identificadores, nomes e metadados técnicos de sincronização.

**Não modelar separadamente:** IP, subfranchise, universe, editorial line, brand, licensed property ou corporate group.

Estrutura simplificada:

```text
Franchise
└── Collection / Series
    └── Game
```

Franchises e collections podem se tornar filtros avançados.

## 5. Busca e experiência do usuário

O GMI distinguirá duas intenções:

### Encontrar um jogo conhecido

- nome;
- aliases;
- refinamento por platform, período e tipo.

### Explorar uma ideia ou nicho

- uma ou múltiplas keywords;
- refinamento por genres, themes, modes, perspectives, platforms e período de lançamento.

Busca somente por nome pode exibir versões relacionadas em um agrupamento expansível.

```text
Mario Kart 8
└── 1 versão relacionada
    └── Mario Kart 8 Deluxe
```

## 6. Reconciliação entre fontes

### 6.1 Identidade

- cada jogo possui um `Game.Id` do GMI;
- cada identidade externa é `Source + ExternalId`.

### 6.2 Match automático

Permitido somente com:

- um external ID cross-source exato;
- product type compatível;
- nenhum conflito severo não resolvido.

### 6.3 Match provável

Sem ID forte, usar sinais compostos: nome normalizado, aliases, type, companies, platforms, release period, franchise, collection e relationships declaradas.

Matches compostos criam candidatos, não merges automáticos.

### 6.4 Produtos distintos, porém relacionados

Remakes, remasters, ports, editions, DLCs, expansions, bundles, demos, soundtracks e tools permanecem separados.

Quando a relação é clara, mas o tipo exato conflita, usar:

```text
RelatedVersionOf
```

Conflitos severos como base game versus DLC, demo, soundtrack ou tool bloqueiam reconciliação automática.

### 6.5 Filtro simples por base-name + suffix

O worker pode detectar:

```text
Mario Kart 8
Mario Kart 8 Deluxe
```

Quando um título normalizado completo é seguido por conteúdo adicional após um limite válido:

- os produtos permanecem distintos;
- podem virar candidatos a relacionamento;
- o worker não precisa interpretar semanticamente o suffix;
- a regra nunca confirma remake, remaster, port ou edition sozinha.

A PoC avaliará viabilidade e falsos positivos.

## 7. Validação progressiva

Fluxo:

```text
Fonte externa
→ validação barata
→ camada intermediária
→ validação de identidade e negócio
→ modelo canônico
```

### 7.1 Na borda

Aplicar JSON válido, campos mínimos, tipos básicos, nome não vazio, identificador externo, normalização simples, `updated_at`, checksum e rejeição de dados obviamente inválidos.

### 7.2 Camada intermediária

Persistir apenas:

- candidatos de reconciliação;
- conflitos reais;
- candidatos de relacionamento;
- registros inválidos recuperáveis;
- observações sob revisão;
- decisões manuais;
- evidência mínima necessária.

Não persistir permanentemente payloads brutos completos, snapshots repetidos, respostas completas de API já processadas, logs detalhados para casos normais ou histórico indefinido de observações missing/inactive.

### 7.3 Modelo canônico

Aceitar somente dados que satisfaçam identidade, compatibilidade de tipo, consistência de relacionamento, proveniência, precedência contextual e ausência de conflito severo não resolvido.

## 8. Tratamento de conflitos

### 8.1 Datas de lançamento

Tratar como conflito somente para o mesmo produto, platform, region, release type/status e precision.

Para conflito real:

- preferir a fonte mais apropriada e confiável para aquela platform;
- preservar a observação divergente somente quando ela ainda for útil para reconciliação, auditoria ou decisão ativa de produto;
- exibir platform junto da data;
- mostrar contexto do ecossistema quando útil, como `PC — 2020-08-07 (Steam)`.

### 8.2 Companies

- tratar companies como relações multivaloradas;
- combinar informações complementares;
- considerar product, version, platform e role;
- não mesclar companies por nome isolado;
- preservar conflitos relevantes enquanto ainda exigirem reconciliação ou explicação para o usuário.

### 8.3 Classificações

A IGDB é canônica para genres, themes, modes, perspectives e keywords.

Classificações externas permanecem preservadas com proveniência quando materializadas, mas não são mescladas automaticamente e não alteram filtros canônicos.

### 8.4 Product types e relationships

- manter produtos separados;
- preservar classificações originais quando exigido por proveniência ativa;
- usar relação genérica quando a conexão é clara, mas o tipo específico conflita;
- bloquear reconciliação automática em conflitos de natureza básica.

## 9. Política de confiança

Confiança é contextual por ponto de dado, não atribuída ao jogo inteiro.

### Alta confiança

Fonte altamente apropriada, contexto completo, nenhum conflito relevante, evidência forte ou confirmação, e dado recente e preciso.

### Equilibrada

Fonte confiável, dado útil porém parcial, precisão limitada, sem confirmação adicional ou pequena limitação contextual.

### Cobertura ampla

Maior cobertura, fonte menos apropriada, conflito ou ambiguidade relevante, contexto incompleto ou correspondência ainda provável.

O MVP não usará uma fórmula numérica complexa.

## 10. Dados ausentes, sincronização e retenção

### 10.1 Ausência não prova remoção

Um valor ou associação que não aparece em uma execução de coleta não é automaticamente considerado removido.

Possíveis causas incluem:

- respostas parciais da fonte;
- campo não solicitado naquela execução;
- paginação ou falhas transitórias;
- comportamento da fonte/API que não garante snapshot atual completo.

Portanto:

```text
não observado na execução atual
≠
confirmado como removido pela fonte
```

O Collector deve preservar o estado atual aceito quando a observação for incompleta ou quando sua completude for desconhecida.

### 10.2 Sincronizando uma mudança confirmada de estado atual

Uma contribuição existente de uma fonte pode ser sincronizada para fora do estado atual somente quando o Collector sabe que:

- o campo ou conjunto de relações relevante foi solicitado explicitamente;
- a resposta da fonte para aquele escopo terminou com sucesso;
- a semântica da fonte indica que o conjunto retornado representa o estado atual completo, ou a fonte informa remoção explicitamente;
- nenhuma evidência independente de outra fonte está sendo apagada por aquela mudança específica.

Uma remoção confirmada de associação não remove o `External*Record` correspondente.

Por exemplo:

```text
GameTheme removido da contribuição IGDB
→ remover/sincronizar aquela associação sustentada pela IGDB

ExternalThemeRecord
→ continua sendo uma identidade válida para o conceito na fonte
```

Mudanças específicas de uma fonte não devem ser propagadas cegamente para evidências de outras fontes.

### 10.3 Política de armazenamento de estado atual

O banco operacional é desenhado para armazenar:

```text
estado canônico tratado atual
+
identidades externas ativas
+
proveniência mínima necessária para explicar e recomputar esse estado
```

Ele não é destinado a ser um warehouse histórico indefinido.

Histórico detalhado de observações, snapshots repetidos ou cópias inativas por ciclo não devem ser introduzidos sem uma necessidade concreta de produto ou operação.

Se uma feature futura exigir histórico, sua janela de retenção deve ser definida quando a feature for introduzida.

### 10.4 Retenção orientada por capacidade

Retenção de storage e reconciliação da fonte são preocupações separadas.

Excluir dados porque o banco está se aproximando do limite de capacidade nunca deve ser interpretado como evidência de que uma fonte removeu ou alterou um fato.

O limite atual de planejamento do Neon Free é de aproximadamente 0,5 GB por projeto.

Os gatilhos operacionais são:

```text
< 70%
→ operação normal

70%+
→ investigar crescimento por tabela e índice

antes de 80%
→ executar retenção controlada ou ação de capacidade

aproximando de 90%
→ proteger escritas essenciais e reduzir ingestão não essencial
```

A prioridade de retenção é:

1. estado canônico atual;
2. identidades externas ativas;
3. proveniência que ainda sustenta o estado atual;
4. dados históricos ou auxiliares recentes e úteis, quando existirem.

Quando dados históricos ou auxiliares forem explicitamente elegíveis para limpeza, remover primeiro os dados elegíveis mais antigos, preservando a janela recente útil.

Decisões de capacidade devem se basear em bytes ocupados e crescimento medido por tabela/índice, e não em uma quantidade fixa de registros.

## 11. Atualizações incrementais e custo

A PoC comparará:

```text
updated_at
versus
updated_at + checksum
```

Objetivos:

- evitar reprocessamento;
- reduzir writes;
- reduzir tempo de execução do worker;
- controlar uso de infraestrutura;
- garantir idempotência.

Checksum é técnico e não fará parte da experiência do usuário.

## 12. Critérios de aprovação da PoC

A PoC deve demonstrar:

### Cobertura

- cobertura suficiente dos campos essenciais;
- limitações identificáveis e explicáveis;
- nenhuma dependência de campos pouco confiáveis.

### Keywords

- volume útil;
- resultados relevantes;
- combinações AND;
- autocomplete viável;
- duplicatas e aliases compreensíveis;
- valor real para exploração de nicho.

### Filtros

- AND para genres, themes, modes, perspectives, keywords e multiplayer;
- OR para platforms;
- AND entre categorias;
- busca por nome;
- busca por período de lançamento.

### Datas de lançamento

- preservação de platform, region e precision;
- múltiplos releases;
- consultas por intervalo de datas;
- tratamento correto de diferenças específicas por platform.

### Relationships

- cobertura suficiente para DLC, expansion, port, remake, remaster, bundle e versions;
- prevenção de merges incorretos;
- avaliação da regra base-name + suffix.

### Reconciliação

- external IDs cross-source exatos;
- validação de type;
- candidatos compostos;
- candidatos rejeitados não recriados repetidamente;
- produtos relacionados mantidos separados.

### Worker

- autenticação;
- paginação;
- respeito a rate limit;
- retomada após falha;
- idempotência;
- updates incrementais;
- avaliação de checksum;
- redução de reprocessamento;
- distinção explícita entre observações completas e parciais antes de sincronizar associações existentes para fora do estado atual.

### Camadas de validação

- casos normais seguem sem persistência intermediária completa;
- casos ambíguos retêm somente evidência mínima;
- modelo canônico recebe somente dados aprovados.

### Storage

- crescimento compatível com o Neon free tier;
- crescimento medido por tabela e índice;
- retenção controlável para qualquer dado explicitamente histórico ou auxiliar;
- índices sustentáveis;
- nenhum payload bruto desnecessário retido;
- preservação do estado canônico atual e da proveniência ativa sob pressão de storage.

### Critério final

> IGDB e o pipeline são suficientes para o MVP quando produzem buscas úteis, preservam contexto e proveniência, evitam merges perigosos, operam de forma incremental e idempotente, e permanecem compatíveis com infraestrutura gratuita.

## 13. Próximo passo

O modelo atual de persistência até a GMI-29 está implementado. O próximo passo de persistência é a GMI-30, que validará comportamento de storage e capacidade com dados representativos antes da finalização da ingestão orientada à produção no Collector.
