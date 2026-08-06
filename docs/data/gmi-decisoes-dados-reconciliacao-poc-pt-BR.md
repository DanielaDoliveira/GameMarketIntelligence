# Game Market Intelligence — Decisões de Dados, Reconciliação e PoC

## 1. Objetivo

Este documento consolida as decisões que orientarão o MVP do **Game Market Intelligence (GMI)** antes da PoC com a IGDB.

O GMI não pretende armazenar todos os dados possíveis sobre jogos. Seu objetivo é selecionar, organizar e apresentar apenas o que realmente ajuda producers em pesquisa inicial de mercado, descoberta de comparáveis, exploração de nichos e compreensão do contexto competitivo.

> O valor do GMI está no pouco que agrega valor, não no excesso de dados que dificulta a análise.

## 2. Escopo do MVP

O MVP deverá oferecer:

- pesquisa por nome e aliases;
- exploração por keywords;
- filtros por gênero, tema, plataforma, modo, perspectiva, multiplayer e período de lançamento;
- contexto empresarial somente após a cobertura poder ser complementada e
  reconciliada entre fontes adequadas;
- franquias e collections/séries;
- relações entre produtos, como remake, remaster, port, edição, DLC e expansão;
- proveniência e nível de confiança;
- transparência sobre limitações, conflitos e cobertura.

Ficam fora do MVP:

- métricas financeiras, vendas e receita;
- previsão de sucesso e pontuação automática de oportunidade;
- taxonomia própria de subgêneros;
- modelagem separada de IP, subfranquia, universo, linha editorial, marca, propriedade licenciada ou grupo corporativo;
- multiplayer aprofundado;
- histórico corporativo complexo;
- comparação detalhada entre edições;
- arquivamento ilimitado de observações antigas.

Métricas financeiras poderão ser consideradas após a conclusão do MVP.

## 3. Fontes selecionadas

### IGDB

Será a principal fonte de catálogo e a taxonomia canônica do MVP para gêneros, temas, modos, perspectivas, keywords, tipos, relações, plataformas, datas, empresas, franquias, collections e identificadores externos.

### Wikidata

Será usada para reconciliação, enriquecimento, validação auxiliar e identificadores cruzados. Não substituirá automaticamente a IGDB como taxonomia canônica.

### Steam

Será uma fonte especializada para fatos do ecossistema Steam, como data de lançamento e identidade do produto na plataforma. A arquitetura e a reconciliação não poderão depender da Steam.

## 4. Mapping da IGDB

### 4.1 Identidade e descoberta

**Incluir:** `id`, `name`, `alternative_names`, `game_type`, `version_parent`, `game_status`, `summary`.

**Adiar ou excluir:** `slug` adiado; `storyline` não importado inicialmente.

Regras:

- identidade externa = `Source + ExternalId`;
- nome nunca basta para reconciliação automática;
- `game_status` é contexto;
- `summary` serve à página de detalhes, não à identidade.

### 4.2 Plataformas e lançamentos

**Incluir:** `platforms`, `release_dates.platform`, `date`, `date_format`, `release_region`, `status`, `updated_at`.

**Derivar:** `first_release_date`, apenas como conveniência.

**Adiar:** `platform_version_release_dates`.

Regras:

- preservar datas por plataforma e região;
- preservar a precisão original;
- não transformar ano ou mês em datas fictícias;
- permitir consultas por intervalo;
- datas diferentes em plataformas diferentes não são conflito.

### 4.3 Empresas envolvidas

**Adiar na primeira iteração de Comparable Games:** developer, publisher,
porting, supporting e o mapping correspondente de empresas.

Na amostra de 100 jogos da PoC da IGDB, 51% dos registros possuíam alguma
empresa, 47% possuíam developer e 44% possuíam publisher. Os 79 vínculos
presentes demonstraram boa integridade estrutural, mas a proporção de dados
ausentes é alta demais para sustentar uma dimensão principal e equilibrada de
comparação no MVP.

Esta decisão é um adiamento, não uma rejeição. A cobertura de developers e
publishers deverá ser comparada e reconciliada com Wikidata e outras fontes
adequadas antes de reconsiderarmos a dimensão. Os nomes das empresas poderão
ajudar o producer a investigar o contexto organizacional, mas o campo isolado
não comprova porte, orçamento, oficialidade, força de distribuição ou
responsabilidade pelo resultado comercial de um jogo.

Regras:

- empresas são relações multivaloradas;
- o papel é obrigatório;
- ausência numa fonte não é conflito;
- papéis diferentes podem ser complementares;
- porting e supporting não substituem developer ou publisher.
- ausência de empresa significa "não informado pela fonte", nunca "não existiu
  empresa";
- a presença de empresa não comprova que o registro seja um produto oficial de
  mercado;
- empresas não sustentarão filtros, rankings, pontuações de confiança ou
  elegibilidade na primeira iteração.

### 4.4 Tipos e relações entre produtos

**Incluir:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Adiar:** `expanded_games`, detalhes de `game_versions`, `forks` e `similar_games`.

> Produtos relacionados permanecem registros distintos.

No primeiro catálogo analítico, registros classificados pela IGDB como
`game_type = Mod` serão excluídos da pesquisa geral de Comparable Games. Essa
regra simples remove muitos mods, ROM hacks e jogos de fã enquanto a IGDB for a
única fonte ativa, mas não garante detecção completa: a IGDB não possui um tipo
específico para ROM hacks e pode classificar registros de forma imprecisa. A
regra é provisória e independente dos dados de empresas.

### 4.5 Identificadores externos e websites

**Incluir de `external_games`:** `external_game_source`, `uid`, `url`, `platform`, `name`, `year`, `updated_at`.

**Incluir de `websites`:** `type`, `url`.

**Adiar:** países, formato de lançamento e checksums específicos de websites.

Regras:

- `Source + UID` identifica o registro externo;
- URLs precisam de validação;
- `trusted` é apenas sinal auxiliar.

### 4.6 Filtros e taxonomia

- **Gêneros:** múltiplos IDs usam AND; gêneros adicionais são permitidos.
- **Temas:** múltiplos usam AND; temas adicionais são permitidos.
- **Modos:** múltiplos usam AND.
- **Perspectivas:** múltiplas usam AND.
- **Keywords:** múltiplas usam AND; são pilar central do valor do GMI; usar IDs estruturados; não criar keywords próprias nem unificar termos automaticamente.
- **Plataformas:** múltiplas usam OR.
- **Multiplayer:** incluir multiplayer, co-op online e multiplayer local/offline; capacidades selecionadas usam AND, sem exclusividade.

Ficam fora do multiplayer do MVP: quantidade máxima de jogadores, LAN, drop-in/drop-out e configurações detalhadas por plataforma.

### 4.7 Franquias e collections

**Incluir:** franchises, collections/séries, IDs, nomes e metadados técnicos.

**Não modelar separadamente:** IP, subfranquia, universo, linha editorial, marca, propriedade licenciada e grupo corporativo.

Estrutura simplificada:

```text
Franchise
└── Collection / Series
    └── Game
```

Franquias e collections poderão ser filtros avançados.

## 5. Pesquisa e experiência do usuário

O GMI distinguirá duas intenções:

### Encontrar um jogo conhecido

- nome;
- aliases;
- refinamento por plataforma, período e tipo.

### Explorar uma ideia ou nicho

- uma ou várias keywords;
- refinamento com gêneros, temas, modos, perspectivas, plataformas e período.

Busca apenas por nome poderá mostrar versões relacionadas agrupadas e expansíveis.

```text
Mario Kart 8
└── 1 versão relacionada
    └── Mario Kart 8 Deluxe
```

## 6. Reconciliação entre fontes

### 6.1 Identidade

- cada jogo terá um `Game.Id` no GMI;
- cada identidade externa será `Source + ExternalId`.

### 6.2 Correspondência automática

Somente quando houver:

- ID externo cruzado exato;
- tipo de produto compatível;
- ausência de conflito grave.

### 6.3 Correspondência provável

Sem ID forte, usar sinais compostos: nome normalizado, aliases, tipo, empresas, plataformas, período, franquia, collection e relações declaradas.

Correspondência composta gera candidato, não fusão automática.

### 6.4 Produtos distintos relacionados

Remakes, remasters, ports, edições, DLCs, expansões, bundles, demos, soundtracks e ferramentas permanecem separados.

Quando o vínculo for claro e o tipo específico divergir, usar:

```text
RelatedVersionOf
```

Conflitos graves, como jogo-base versus DLC, demo, soundtrack ou ferramenta, bloqueiam reconciliação automática.

### 6.5 Filtro simples de nome-base + complemento

O worker poderá detectar:

```text
Mario Kart 8
Mario Kart 8 Deluxe
```

Se um título normalizado completo for seguido por complemento após fronteira válida:

- os produtos permanecem distintos;
- podem virar candidatos a relação;
- o worker não precisa interpretar o complemento;
- a regra não confirma sozinha remake, remaster, port ou edição.

A PoC avaliará viabilidade e falsos positivos.

## 7. Validação progressiva

Fluxo:

```text
Fonte externa
→ validações baratas
→ camada intermediária
→ validações de identidade e negócio
→ modelo canônico
```

### 7.1 Na borda

Aplicar JSON válido, campos mínimos, tipos básicos, nome não vazio, identificador externo, normalização simples, `updated_at`, checksum e descarte de dados obviamente inválidos.

### 7.2 Camada intermediária

Persistir apenas:

- candidatos de reconciliação;
- conflitos reais;
- relações candidatas;
- registros inválidos recuperáveis;
- observações ausentes, em revisão ou inativas;
- decisões manuais;
- evidência mínima necessária.

Não persistir permanentemente payloads brutos completos, snapshots repetidos, respostas completas já processadas ou logs detalhados de casos normais.

### 7.3 Modelo canônico

Aceitar apenas dados que satisfaçam identidade, compatibilidade de tipo, consistência de relações, proveniência, precedência contextual e ausência de conflitos graves não resolvidos.

## 8. Tratamento de conflitos

### 8.1 Datas

Comparar como conflito apenas no mesmo produto, plataforma, região, tipo/status e precisão.

Em conflito real:

- priorizar a fonte mais adequada e confiável para a plataforma;
- preservar a observação divergente;
- mostrar plataforma junto da data;
- mostrar ecossistema quando útil, por exemplo `PC — 07/08/2020 (Steam)`.

### 8.2 Empresas

- tratar como relações multivaloradas;
- combinar informações complementares;
- considerar produto, versão, plataforma e papel;
- não fundir empresas apenas pelo nome;
- preservar conflitos relevantes.

### 8.3 Classificações

A IGDB será canônica para gêneros, temas, modos, perspectivas e keywords.

Classificações externas serão preservadas com proveniência, mas não serão misturadas nem alterarão os filtros canônicos automaticamente.

### 8.4 Tipos e relações

- manter produtos separados;
- preservar classificações originais;
- usar relação genérica quando o vínculo for claro e o tipo específico divergir;
- bloquear reconciliação automática em conflitos de natureza básica.

## 9. Política de confiança

A confiança será contextual por dado, não pelo jogo inteiro.

### High confidence

Fonte altamente adequada, contexto completo, sem conflito relevante, evidência forte ou confirmação adicional, dado recente e preciso.

### Balanced

Fonte confiável, dado útil porém parcial, precisão limitada, sem confirmação adicional ou com pequena limitação contextual.

### Broad coverage

Maior abrangência, fonte menos adequada, conflito ou ambiguidade relevante, contexto incompleto ou correspondência ainda provável.

Não haverá fórmula numérica complexa no MVP.

## 10. Ausência, inativação e retenção

### 10.1 Ausências consecutivas

```text
1ª ausência
→ manter e marcar

2ª ausência
→ reavaliar

3ª ausência
→ desativar

remoção explícita ou snapshot completo confiável
→ desativar imediatamente
```

Ausência não significa remoção automática.

### 10.2 Reativação

Se o registro reaparecer:

- reativar;
- zerar o contador de ausências;
- reavaliar o valor canônico.

### 10.3 Exclusão física

Não haverá exclusão automática brusca.

Registros inativos ficarão dentro de um orçamento máximo de armazenamento. Quando ele for ultrapassado:

- remover os inativos mais antigos;
- excluir apenas registros não protegidos;
- processar em pequenos lotes;
- deixar ativos e itens em revisão fora da limpeza.

O limite será baseado em bytes ocupados, não numa quantidade fixa de registros.

A PoC medirá tamanho médio de ativos, intermediários e inativos, custo dos índices, crescimento por ciclo e espaço seguro no Neon.

## 11. Atualização incremental e custo

A PoC comparará:

```text
updated_at
versus
updated_at + checksum
```

Objetivos:

- evitar reprocessamento;
- reduzir gravações;
- reduzir tempo do worker;
- controlar infraestrutura;
- garantir idempotência.

Checksum será técnico e não fará parte da experiência do usuário.

## 12. Critérios de aprovação da PoC

A PoC deverá demonstrar:

### Cobertura

- campos essenciais com cobertura suficiente;
- limitações identificáveis e comunicáveis;
- ausência de dependência de campos pouco confiáveis.

### Keywords

- volume útil;
- resultados relevantes;
- combinações por AND;
- autocomplete viável;
- duplicidades e aliases compreensíveis;
- valor real para exploração de nichos.

### Filtros

- AND para gêneros, temas, modos, perspectivas, keywords e multiplayer;
- OR para plataformas;
- AND entre categorias;
- busca por nome;
- busca por período.

### Datas

- preservação de plataforma, região e precisão;
- múltiplos lançamentos;
- consultas por intervalo;
- diferenças por plataforma tratadas corretamente.

### Relações

- cobertura suficiente para DLC, expansão, port, remake, remaster, bundle e versões;
- prevenção de fusões indevidas;
- teste da regra nome-base + complemento.

### Reconciliação

- IDs cruzados exatos;
- validação de tipo;
- candidatos compostos;
- candidatos rejeitados não recriados repetidamente;
- produtos relacionados mantidos separados.

### Worker

- autenticação;
- paginação;
- rate limit;
- retomada após falha;
- idempotência;
- atualização incremental;
- comparação de checksum;
- redução de reprocessamento.

### Validação em camadas

- casos normais seguem sem persistência intermediária completa;
- casos ambíguos preservam apenas evidência mínima;
- modelo canônico recebe apenas dados aprovados.

### Armazenamento

- crescimento compatível com o Neon gratuito;
- orçamento de inativos mensurável;
- retenção controlável;
- índices sustentáveis;
- nenhum payload bruto desnecessário.

### Critério final

> A IGDB e o pipeline serão considerados suficientes para o MVP quando produzirem pesquisas úteis, preservarem contexto e proveniência, evitarem fusões perigosas, operarem de forma incremental e idempotente e permanecerem compatíveis com a infraestrutura gratuita.

## 13. Próximo passo

O próximo ciclo será dedicado à implementação da PoC da IGDB, validando as decisões deste documento sem ampliar o escopo do MVP.
