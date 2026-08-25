# Game Market Intelligence — Decisões de Dados, Reconciliação e PoC

## 1. Objetivo

Este documento consolida as decisões que orientam o MVP do **Game Market Intelligence (GMI)** após a proof of concept da IGDB e os refinamentos posteriores do modelo de persistência.

O GMI não pretende armazenar toda informação possível sobre jogos. Seu objetivo é selecionar, organizar e apresentar apenas o que realmente ajuda producers em pesquisa inicial de mercado, descoberta de jogos comparáveis, exploração de nichos e análise de contexto competitivo.

> O GMI cria valor por meio de um conjunto focado de dados úteis, e não por excesso de informação que torne a análise mais difícil.

## 2. Escopo do MVP

O MVP fornecerá:

- busca por nome e aliases;
- exploração por keywords;
- filtros por gênero, theme, plataforma, game mode, perspective, multiplayer e período de release;
- contexto sobre involved companies;
- franchises e collections/series;
- relacionamentos de produto como remake, remaster, port, edition, DLC e expansion;
- provenance e níveis de confiança;
- transparência sobre limitações, conflitos e cobertura.

Fora do escopo:

- métricas financeiras, vendas e receita;
- predição de sucesso e scoring automático de oportunidade;
- taxonomia própria de subgêneros;
- modelagem separada de IP, subfranchise, universe, editorial line, brand, licensed property ou corporate group;
- análise profunda de multiplayer;
- histórico corporativo complexo;
- comparação detalhada de conteúdo entre editions;
- armazenamento arquivístico ilimitado de observações antigas.

Métricas financeiras podem ser consideradas depois que o MVP estiver completo.

## 3. Fontes selecionadas

### IGDB

IGDB será a principal fonte de catálogo e taxonomia canônica do MVP para gêneros, themes, modes, perspectives, keywords, product types, relacionamentos, plataformas, release dates, companies, franchises, collections e identificadores externos.

### Wikidata

Wikidata dará suporte à reconciliação, enriquecimento, validação auxiliar e identificadores cross-source. Ela não substituirá automaticamente a IGDB como taxonomia canônica.

### Steam

Steam será uma fonte especializada para fatos específicos da Steam, como release dates e identidade de produto na Steam. A arquitetura e o processo de reconciliação não devem depender da Steam.

## 4. Mapping da IGDB

### 4.1 Identidade e descoberta

**Incluir:** `id`, `name`, `alternative_names`, `game_type`, `version_parent`, `game_status`, `summary`.

**Adiar ou excluir:** adiar `slug`; não importar inicialmente `storyline`.

Regras:

- identidade externa = `Source + ExternalId`;
- nome sozinho nunca sustenta reconciliação automática;
- `game_status` é contextual;
- `summary` apoia detalhes, não identidade.

### 4.2 Plataformas e releases

**Incluir:** `platforms`, `release_dates.platform`, `date`, `date_format`, `release_region`, `status`, `updated_at`.

**Derivar:** `first_release_date`, apenas como conveniência.

**Adiar:** `platform_version_release_dates`.

Regras:

- preservar datas por plataforma e região;
- preservar precisão original;
- nunca converter dados apenas de ano ou mês em datas artificiais;
- suportar consultas por intervalo de datas;
- datas diferentes em plataformas diferentes não são conflitos.

### 4.3 Involved companies

**Incluir roles:** developer, publisher, porting e supporting.

**Incluir dados mínimos da company:** identificador externo, nome, status quando disponível e `updated_at`.

**Adiar:** `changed_company_id`, websites, parent company, histórico corporativo e descrições longas.

Regras:

- companies são relacionamentos multivalorados;
- role é obrigatório;
- ausência em uma fonte não é conflito;
- roles diferentes podem ser complementares;
- porting e supporting não substituem developer ou publisher.

### 4.4 Product types e relacionamentos

**Incluir:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Adiar:** `expanded_games`, `game_versions` detalhados, `forks` e `similar_games`.

> Produtos relacionados permanecem registros separados.

### 4.5 Identificadores externos e websites

**Incluir de `external_games`:** `external_game_source`, `uid`, `url`, `platform`, `name`, `year`, `updated_at`.

**Incluir de `websites`:** `type`, `url`.

**Adiar:** countries, release format e checksums específicos de website.

Regras:

- `Source + UID` identifica um registro externo;
- URLs devem ser validadas;
- `trusted` é apenas um sinal auxiliar.

### 4.6 Filtros e taxonomia

- **Genres:** múltiplos IDs usam AND; gêneros extras são permitidos.
- **Themes:** múltiplos valores usam AND; themes extras são permitidos.
- **Game modes:** múltiplos valores usam AND.
- **Perspectives:** múltiplos valores usam AND.
- **Keywords:** múltiplos valores usam AND; keywords são centrais para o valor do GMI; usar IDs estruturados; não criar keywords customizadas nem mesclar termos automaticamente.
- **Platforms:** múltiplos valores usam OR.
- **Multiplayer:** incluir multiplayer, online co-op e local/offline multiplayer; capacidades selecionadas usam AND sem exclusividade.

Fora do escopo para multiplayer: quantidade máxima de jogadores, LAN, drop-in/drop-out e configurações detalhadas específicas de plataforma.

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
- refinamento por plataforma, período e tipo.

### Explorar uma ideia ou nicho

- uma ou múltiplas keywords;
- refinamento por genres, themes, modes, perspectives, platforms e período de release.

Busca apenas por nome pode mostrar versões relacionadas em grupo expansível.

```text
Mario Kart 8
└── 1 versão relacionada
    └── Mario Kart 8 Deluxe
```

## 6. Reconciliação cross-source

### 6.1 Identidade

- cada jogo possui um `Game.Id` do GMI;
- cada identidade externa é `Source + ExternalId`.

### 6.2 Match automático

Permitido apenas com:

- um external ID cross-source exato;
- product type compatível;
- nenhum conflito grave não resolvido.

### 6.3 Match provável

Sem um ID forte, usar sinais compostos: nome normalizado, aliases, type, companies, platforms, período de release, franchise, collection e relacionamentos declarados.

Matches compostos criam candidatos, não merges automáticos.

### 6.4 Produtos distintos porém relacionados

Remakes, remasters, ports, editions, DLCs, expansions, bundles, demos, soundtracks e tools permanecem separados.

Quando o relacionamento é claro, mas o tipo exato entra em conflito, usar:

```text
RelatedVersionOf
```

Conflitos graves como base game versus DLC, demo, soundtrack ou tool bloqueiam reconciliação automática.

### 6.5 Filtro simples de nome-base-mais-sufixo

O worker pode detectar:

```text
Mario Kart 8
Mario Kart 8 Deluxe
```

Quando um título normalizado completo é seguido por conteúdo extra após uma fronteira válida:

- os produtos permanecem distintos;
- podem virar candidatos a relacionamento;
- o worker não precisa entender o sufixo;
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

### 7.1 Na fronteira

Aplicar JSON válido, campos mínimos, tipos básicos, nome não vazio, identificador externo, normalização simples, `updated_at`, checksum e rejeição de dados obviamente inválidos.

### 7.2 Camada intermediária

Persistir apenas:

- candidatos de reconciliação;
- conflitos reais;
- candidatos de relacionamento;
- registros inválidos recuperáveis;
- observações em revisão;
- decisões manuais;
- evidência mínima necessária.

Não persistir permanentemente payloads brutos completos, snapshots repetidos, respostas de API já totalmente processadas, logs detalhados para casos normais ou histórico indefinido de observações missing/inactive.

### 7.3 Modelo canônico

Aceitar apenas dados que satisfaçam identidade, compatibilidade de tipo, consistência de relacionamento, provenance, precedência contextual e ausência de conflito grave não resolvido.

## 8. Tratamento de conflitos

### 8.1 Release dates

Tratar como conflito apenas para o mesmo produto, plataforma, região, release type/status e precisão.

Para conflito real:

- preferir a fonte mais apropriada e confiável para aquela plataforma;
- preservar a observação divergente apenas quando ainda for útil para reconciliação, auditoria ou decisão ativa de produto;
- mostrar plataforma junto da data;
- mostrar contexto de ecossistema quando útil, como `PC — 2020-08-07 (Steam)`.

### 8.2 Companies

- tratar companies como relacionamentos multivalorados;
- combinar informação complementar;
- considerar produto, versão, plataforma e role;
- não mesclar companies apenas pelo nome;
- preservar conflitos relevantes quando ainda exigirem reconciliação ou explicação ao usuário.

### 8.3 Classificações

IGDB é canônica para genres, themes, modes, perspectives e keywords.

Classificações externas permanecem preservadas com provenance quando materializadas, mas não são mescladas automaticamente e não alteram filtros canônicos.

### 8.4 Product types e relacionamentos

- manter produtos separados;
- preservar classificações originais quando exigidas por provenance ativa;
- usar relacionamento genérico quando a conexão for clara mas o tipo específico entrar em conflito;
- bloquear reconciliação automática em conflitos de natureza básica.

## 9. Política de confiança

Confiança é contextual por dado, não atribuída ao jogo inteiro.

### High confidence

Fonte altamente apropriada, contexto completo, nenhum conflito relevante, evidência forte ou confirmação e dado recente e preciso.

### Balanced

Fonte confiável, dado útil porém parcial, precisão limitada, ausência de confirmação adicional ou pequena limitação de contexto.

### Broad coverage

Cobertura mais ampla, fonte menos apropriada, conflito ou ambiguidade relevante, contexto incompleto ou correspondência ainda provável.

O MVP não usará fórmula numérica complexa.

## 10. Dados ausentes, sincronização e retenção

### 10.1 Ausência não prova remoção

Um valor ou associação que não aparece em uma coleta não é automaticamente considerado removido.

Possíveis causas incluem:

- respostas parciais da fonte;
- campo não solicitado naquela execução;
- paginação ou falhas transitórias;
- comportamento da fonte/API que não garante snapshot atual completo.

Portanto:

```text
não observado na execução atual
≠
confirmado removido pela fonte
```

O Collector deve preservar o estado atual aceito quando a observação for incompleta ou sua completude for desconhecida.

### 10.2 Sincronizando uma mudança confirmada do estado atual

Uma contribuição existente de uma fonte só pode ser sincronizada para fora do estado atual quando o Collector souber que:

- o campo ou conjunto de relacionamentos relevante foi explicitamente solicitado;
- a resposta da fonte para esse escopo foi concluída com sucesso;
- a semântica da fonte indica que o conjunto retornado representa o estado atual completo, ou a fonte informa remoção explicitamente;
- nenhuma evidência independente de outra fonte está sendo apagada por essa mudança específica.

A remoção confirmada de uma associação não exclui o `External*Record` correspondente.

Por exemplo:

```text
GameTheme removido da contribuição IGDB
→ remover/sincronizar essa associação sustentada pela IGDB

ExternalThemeRecord
→ continua sendo identidade válida do conceito na fonte
```

Mudanças específicas de uma fonte não devem ser propagadas cegamente para evidências de outras fontes.

### 10.3 Política de armazenamento do estado atual

O banco operacional foi desenhado para armazenar:

```text
estado canônico tratado atual
+
identidades externas ativas
+
provenance mínima necessária para explicar e recomputar esse estado
```

Ele não foi desenhado como warehouse de histórico indefinido.

Histórico detalhado de observações, snapshots repetidos ou cópias inactive por ciclo não devem ser introduzidos sem uma necessidade concreta de produto ou operação.

Se uma funcionalidade futura exigir histórico, sua janela de retenção deve ser definida quando a funcionalidade for introduzida.

### 10.4 Retenção motivada por capacidade

Retenção de armazenamento e reconciliação de fonte são preocupações separadas.

Excluir dados porque o banco está se aproximando do limite de capacidade nunca deve ser interpretado como evidência de que uma fonte removeu ou alterou um fato.

O limite atual de planejamento do Neon Free é aproximadamente 0,5 GB por projeto.

Limiares operacionais:

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

Prioridade de retenção:

1. estado canônico atual;
2. identidades externas ativas;
3. provenance ainda sustentando o estado atual;
4. histórico ou dados auxiliares recentes e úteis, quando existirem.

Quando histórico ou dados auxiliares forem explicitamente elegíveis para pruning, remover primeiro os dados elegíveis mais antigos preservando a janela útil mais recente.

Decisões de capacidade devem se basear em bytes ocupados e crescimento medido de tabelas/índices, e não em quantidade fixa de registros.

### 10.5 Cobertura de catálogo versus profundidade de metadados

A GMI-30 confirmou que cobertura de catálogo e profundidade de metadados devem ser gerenciadas como decisões separadas de capacidade.

Um orçamento de banco não deve ser convertido em uma regra arbitrária como:

```text
a fonte tem 20.000 jogos relevantes
→ armazenar só 10.000 porque a amostra de validação usou 10.000
```

O tamanho da amostra de validação não é um limite de catálogo.

Quando os jogos forem relevantes ao escopo aprovado do MVP, a ordem preferida é:

```text
preservar cobertura relevante do catálogo
→ persistir metadados exigidos por perguntas aprovadas de produto
→ controlar ou adiar associações não essenciais de alta cardinalidade
→ monitorar crescimento real de tabelas e índices
```

Isso é especialmente importante porque a GMI-30 mostrou que relacionamentos multiplicativos, e não a tabela canônica `Games` sozinha, dominam o crescimento de armazenamento.

Redução de profundidade de metadados motivada por capacidade nunca deve ser apresentada aos usuários ou à lógica de reconciliação como prova de que a fonte não possui o dado omitido.

### 10.6 Evidência representativa de armazenamento da GMI-30

A primeira fase de validação sintética preservou um catálogo de 10.000 jogos e incluiu:

- 20.000 associações game-genre;
- 20.000 associações game-platform;
- 10.000 external game records;
- 16.666 registros de metadados de imagem.

O armazenamento PostgreSQL medido após `ANALYZE` foi aproximadamente:

```text
dados de tabela  ~7,4 MB
índices          ~11 MB
total            ~19 MB
```

Uma segunda fase preservou os mesmos 10.000 jogos e adicionou:

- 30.000 releases contextuais;
- 30.000 associações de theme;
- 20.000 associações de game mode;
- 20.000 associações de player perspective;
- 80.000 associações de keyword;
- 20.000 associações de company;
- 5.000 associações de collection;
- 2.500 product relations;
- registros canônicos e externos de provenance necessários.

O armazenamento medido passou para aproximadamente:

```text
dados de tabela  ~29 MB
índices          ~33 MB
total            ~63 MB
```

A fase de pressão acrescentou aproximadamente 44 MB sem aumentar a quantidade de jogos canônicos.

Maiores relações observadas:

| Relação | Linhas | Tamanho total aproximado | Aproximadamente bytes totais por linha |
|---|---:|---:|---:|
| `game_keywords` | 80.000 | 14 MB | 181 B |
| `game_releases` | 30.000 | 10,1 MB | 344 B |
| `game_images` | 16.666 | 5,3 MB | 328 B |
| `game_themes` | 30.000 | 5,3 MB | 182 B |
| `game_companies` | 20.000 | 5,3 MB | 271 B |
| `GameGenres` | 20.000 | 3,5 MB | 181 B |
| `GamePlatforms` | 20.000 | 3,5 MB | 181 B |
| `Games` | 10.000 | 3,2 MB | 327 B |

Os valores de bytes por linha usam `pg_total_relation_size`, portanto incluem índices associados e são evidência para orçamento, não garantias fixas de armazenamento.

O cenário de pressão apresentou média aproximada de 6,3 KB de objetos medidos de banco por jogo quando todas as associações sintéticas estavam incluídas.

Essa proporção não deve ser tratada como garantia linear de capacidade em produção porque cardinalidades reais da fonte, alocação de páginas PostgreSQL, comportamento de índices, VACUUM, bloat, migrations futuras e distribuições reais serão diferentes.

Os maiores pontos sintéticos de pressão de armazenamento observados até agora são keywords e releases contextuais.

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

## 12. Critérios de aprovação da PoC e persistência

A PoC e o trabalho posterior de persistência devem demonstrar:

### Cobertura

- cobertura suficiente para campos essenciais;
- limitações identificáveis e explicáveis;
- nenhuma dependência de campos não confiáveis.

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
- busca por período de release.

### Release dates

- preservação de plataforma, região e precisão;
- múltiplos releases;
- consultas por intervalo de data;
- tratamento correto de diferenças específicas de plataforma.

### Relacionamentos

- cobertura suficiente para DLC, expansion, port, remake, remaster, bundle e versions;
- prevenção de merges incorretos;
- avaliação da regra nome-base-mais-sufixo.

### Reconciliação

- IDs cross-source exatos;
- validação de tipo;
- candidatos compostos;
- candidatos rejeitados não recriados repetidamente;
- produtos relacionados mantidos separados.

### Worker

- autenticação;
- paginação;
- conformidade com rate limits;
- resume após falha;
- idempotência;
- atualizações incrementais;
- avaliação de checksum;
- redução de reprocessamento;
- distinção explícita entre observações completas e parciais antes de sincronizar associações existentes para fora do estado atual.

### Camadas de validação

- casos normais prosseguem sem persistência intermediária completa;
- casos ambíguos retêm apenas evidência mínima;
- o modelo canônico recebe apenas dados aprovados.

### Armazenamento

A GMI-30 agora forneceu a primeira evidência local representativa de persistência:

- cadeia de migrations aplica do zero;
- PostgreSQL local existente pode ser atualizado para o schema atual;
- modelo EF e snapshot de migrations estão alinhados;
- crescimento de armazenamento é mensurável por tabela e índice;
- armazenamento sintético baseline com 10.000 jogos foi aproximadamente 19 MB;
- armazenamento sob pressão de alta cardinalidade foi aproximadamente 63 MB para o mesmo tamanho de catálogo;
- índices representaram aproximadamente 33 MB dos 63 MB do cenário de pressão;
- keywords e releases contextuais foram os maiores pontos multiplicativos medidos;
- a política atual de armazenamento pode priorizar estado canônico e provenance ativa enquanto evita histórico/raw indefinido;
- cobertura relevante de catálogo deve ser preservada antes de metadados não essenciais;
- nenhum limite fixo de quantidade de jogos é autorizado pela amostra de validação.

A evidência atual de planos também confirma:

- filtro por gênero usa `IX_GameGenres_GenreId`;
- filtro por plataforma usa `IX_GamePlatforms_PlatformId`;
- lookup batch de cover usa `IX_game_images_GameId`;
- busca atual por nome com `ILIKE '%term%'` usa sequential scan, mas completou em aproximadamente 3,7 ms com 10.000 jogos;
- filtro atual por ano de release usa sequential scan, mas completou em aproximadamente 1,8 ms com 10.000 jogos;
- nenhum novo índice é justificado apenas para eliminar esses sequential scans atualmente baratos.

### Critério final

> IGDB e o pipeline são suficientes para o MVP quando produzem buscas úteis, preservam contexto e provenance, evitam merges perigosos, operam incrementalmente e de forma idempotente, preservam cobertura relevante de catálogo enquanto controlam profundidade de metadados e permanecem compatíveis com infraestrutura gratuita.

## 13. Checkpoint atual de persistência e próximo passo

O modelo de persistência até a GMI-29 está implementado.

A GMI-30 agora validou:

- migrations do zero;
- migration sobre o PostgreSQL local existente;
- ausência de mudanças pendentes no modelo EF;
- compatibilidade completa de build e testes automatizados;
- compatibilidade dos contratos públicos atuais;
- planos representativos de consulta;
- comportamento representativo de armazenamento baseline e alta cardinalidade;
- distinção operacional entre cobertura de catálogo e profundidade de metadados.

A validação atual da solução completa passou:

```text
Testes: 460
Aprovados: 460
Falharam: 0
Ignorados: 0
```

O trabalho restante da GMI-30 é consolidar documentação, executar validação final de qualidade do repositório, atualizar `AGENTS.md`, integrar em `develop` e fechar o Jira.

Depois que a GMI-30 for integrada, o próximo passo de implementação é a ingestão do Collector orientada a produção, onde essas regras de persistência, sincronização, provenance, capacidade e profundidade de metadados devem ser aplicadas em vez de redescobertas.
