# Game Market Intelligence — Decisões de Dados, Reconciliação e PoC

## 1. Objetivo

Este documento consolida as decisões que orientam o MVP do **Game Market
Intelligence (GMI)** e é atualizado conforme a PoC da IGDB produz evidências.

O GMI não pretende armazenar todos os dados possíveis sobre jogos. Seu objetivo é selecionar, organizar e apresentar apenas o que realmente ajuda producers em pesquisa inicial de mercado, descoberta de comparáveis, exploração de nichos e compreensão do contexto competitivo.

> O valor do GMI está no pouco que agrega valor, não no excesso de dados que dificulta a análise.

## 2. Escopo do MVP

O MVP deverá oferecer:

- pesquisa por nome; aliases poderão ser adicionados apenas a partir de campos
  e fontes com política de proveniência aprovada;
- exploração por keywords;
- filtros por gênero, tema, plataforma, modo, multiplayer e período de lançamento;
- perspectivas como dado opcional de detalhe quando informadas pela fonte;
- contexto de empresas envolvidas;
- collections/séries;
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

A IGDB está aprovada com ressalvas como principal fonte de catálogo e taxonomia canônica do MVP para gêneros, temas, modos, perspectivas, keywords, tipos, relações, plataformas, datas, empresas, collections e identificadores externos. Ela é adequada para validar o primeiro MVP, mas não é uma fonte autoritativa: registros e relações individuais podem estar incompletos, inconsistentes ou representar conteúdo não oficial. `franchises` permanece disponível na fonte, mas fica adiado para depois do MVP.

O primeiro MVP preservará proveniência e aplicará proteções básicas de catálogo sem bloquear a entrega por um sistema de comprovação de oficialidade entre fontes. Detecção mais sofisticada de conflitos, confiança por campo, quarentena e validação cruzada serão refinadas no incremento seguinte, quando uma segunda fonte for integrada.

### Wikidata

Será usada para reconciliação, enriquecimento, validação auxiliar e identificadores cruzados. Não substituirá automaticamente a IGDB como taxonomia canônica.

### Steam

Será uma fonte especializada para fatos do ecossistema Steam, como data de lançamento e identidade do produto na plataforma. A arquitetura e a reconciliação não poderão depender da Steam.

## 4. Mapping da IGDB

### 4.1 Identidade e descoberta

**Incluir:** `id`, `name`, `game_type`, `version_parent`, `game_status`, `summary`.

**Adiar ou excluir:** excluir `alternative_names` do mapping do MVP; adiar
`slug`; não importar `storyline` inicialmente.

Regras:

- identidade externa = `Source + ExternalId`;
- nome nunca basta para reconciliação automática;
- `alternative_names` não deve ser usado para exibição, busca, identidade ou
  reconciliação no MVP, pois os valores observados misturam variações regionais
  e linguísticas com nomes de executáveis, títulos provisórios e aliases
  ambíguos sem proveniência suficiente;
- avaliar `game_localizations` separadamente como candidato mais estruturado
  para nomes regionais, sem presumir que a estrutura regional comprove uso
  oficial;
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

**Incluir papéis:** developer, publisher, porting e supporting.

**Incluir dados mínimos:** identificador externo, nome, status quando disponível e `updated_at`.

**Adiar:** `changed_company_id`, websites, parent company, histórico corporativo e descrição extensa.

Regras:

- empresas são relações multivaloradas;
- o papel é obrigatório;
- ausência numa fonte não é conflito;
- papéis diferentes podem ser complementares;
- porting e supporting não substituem developer ou publisher.

### 4.4 Tipos e relações entre produtos

**Incluir:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Adiar:** `expanded_games`, detalhes de `game_versions`, `forks` e `similar_games`.

> Produtos relacionados permanecem registros distintos.

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
- **Modos:** múltiplos usam AND; o filtro significa que a fonte associa todos os modos selecionados ao registro do jogo, e não que todos estejam disponíveis em cada plataforma ou edição.
- **Perspectivas:** ingerir como detalhe opcional em relação muitos-para-muitos;
  adiar o filtro público porque a cobertura atual produziria falsos negativos em
  excesso.
- **Keywords:** múltiplas usam AND; são pilar central do valor do GMI; usar IDs estruturados; não criar keywords próprias nem unificar termos automaticamente.
- **Plataformas:** múltiplas usam OR.
- **Multiplayer:** incluir multiplayer, co-op online e multiplayer local/offline; capacidades selecionadas usam AND, sem exclusividade.

Ficam fora do multiplayer do MVP: quantidade máxima de jogadores, LAN, drop-in/drop-out e configurações detalhadas por plataforma.

#### Decisão sobre modos de jogo

`game_modes` está aprovado para o MVP com ressalvas. Na amostra fixa de 100
jogos, 84 registros apresentaram ao menos um modo, 69 apresentaram exatamente
um, 15 apresentaram múltiplos modos e 16 não apresentaram nenhum. Não foram
encontrados IDs duplicados, IDs inválidos, nomes vazios nem nomes conflitantes
para o mesmo ID. Uma amostra direcionada separada de 26 jogos de nove séries
conhecidas teve cobertura de 100% e continua útil para inspeção semântica, mas
não representa a completude geral do catálogo. A relação é muitos-para-muitos e
anulável.

O campo pertence a cada registro de jogo da IGDB e não possui granularidade por plataforma. Ele não informa um modo principal, não distingue cooperação limitada ou assimétrica, não mede importância ou qualidade do modo e não comprova que um modo informado se aplique a todas as plataformas e edições. Modos ausentes devem ser tratados como dado desconhecido da fonte, e não como prova de que a capacidade não existe. Os modos não serão propagados entre originais, ports, remakes, remasters, edições, updates ou outros registros relacionados e não constituem evidência forte de reconciliação.

Uma inspeção semântica direcionada também encontrou uma suposta versão Android de `Super Mario Galaxy`. Como não existe versão oficial do jogo para Android, esse resultado não pode servir como evidência sobre o produto oficial. O caso demonstra que busca por nome, associação de plataforma e relações da IGDB não comprovam oficialidade isoladamente. Resultados de busca por nome são apenas candidatos de descoberta; registros relacionados não devem influenciar modos, plataformas ou lançamentos de outro jogo sem validação suficiente de identidade.

Para este MVP, o risco residual é aceito e documentado. O catálogo identificará a fonte e não apresentará os dados como completos ou infalíveis. A validação entre fontes e o tratamento mais forte de registros suspeitos ficam adiados até a integração de uma segunda base.

A cobertura de 84% na amostra fixa é suficiente para aprovar um filtro público
qualificado pela fonte. O filtro significa "jogos para os quais a IGDB informa o
modo selecionado"; ele não deve sugerir que jogos omitidos não possuem esse modo.
O campo também poderá ser exibido nos detalhes e usado em comparações quando
disponível.

#### Decisão sobre perspectivas do jogador

`player_perspectives` está aprovado para ingestão e exibição opcional nos
detalhes do MVP, mas não como filtro público. Na mesma amostra fixa de 100 jogos,
45 registros apresentaram ao menos uma perspectiva, 42 apresentaram exatamente
uma, três apresentaram múltiplas perspectivas e 55 não apresentaram nenhuma. Não
foram encontrados IDs duplicados, IDs inválidos, nomes vazios nem nomes
conflitantes para o mesmo ID. Os cinco valores da fonte apareceram. A relação é
muitos-para-muitos e anulável.

A amostra direcionada de 26 jogos conhecidos teve cobertura de 100%, contra 45%
na amostra fixa. A diferença demonstra forte viés de completude em favor de
registros famosos e bem mantidos. Por isso, a amostra fixa mede cobertura, e a
amostra de jogos conhecidos permanece apenas para interpretação semântica.

O campo não identifica perspectiva principal ou predominante e pode combinar
perspectivas usadas em sistemas, cenas ou modos diferentes. Ele não possui
granularidade por plataforma ou edição. Ausência significa dado desconhecido, e
não que o jogo não possua perspectiva. Os valores não serão propagados entre
produtos relacionados e não constituem evidência forte de reconciliação. Com
55% da amostra fixa sem o campo, um filtro público produziria falsos negativos
demais; ele fica adiado até que a cobertura possa ser ampliada ou qualificada
com outra fonte.

### 4.7 Collections e franchises

**Incluir no MVP:** `collections`, com IDs, nomes, relações com jogos e metadados técnicos de sincronização.

**Adiar para depois do MVP:** `franchises`.

Uma amostra direcionada de 26 jogos pertencentes a nove séries conhecidas apresentou 26 registros com `collections` e 26 correspondências com a collection esperada. O resultado sustenta o uso de `collections` para representar séries e agrupamentos relacionados, mas não demonstra cobertura universal da IGDB.

A relação entre jogos e collections é muitos-para-muitos. Um jogo pode pertencer simultaneamente a agrupamentos amplos e específicos. A IGDB não forneceu hierarquia, prioridade ou indicação de collection principal; portanto, nenhuma dessas propriedades será inferida pela ordem ou pelo nome das associações.

Na mesma amostra, `franchises` apareceu em 24 de 26 registros. Em 21 desses 24 casos, ao menos um rótulo de franchise também aparecia entre as collections. Os cinco registros com algum rótulo adicional mostraram que `franchises` pode representar contexto mais amplo, mas também crossovers, participações e propriedades licenciadas. `Mario Kart 8` e `Kingdom Hearts III`, por exemplo, retornaram múltiplas franchises sem indicar qual seria a principal.

Decisões:

- usar `collections` no MVP como relação muitos-para-muitos;
- não presumir collection principal nem hierarquia entre collections;
- adiar `franchises`, sem descartá-lo definitivamente;
- não usar `franchises` na ingestão principal, nos filtros ou na reconciliação do MVP;
- tratar ausência de collection ou franchise como dado desconhecido ou não aplicável, e não como prova de que o jogo é isolado;
- não interpretar collection ou franchise como evidência de sucesso comercial, tamanho de público ou propriedade jurídica;
- não modelar separadamente IP, subfranquia, universo, linha editorial, marca, propriedade licenciada ou grupo corporativo.

`franchises` poderá ser reavaliado futuramente se o produto precisar analisar crossovers, presença de propriedades intelectuais licenciadas ou alcance de uma marca entre séries diferentes. Mesmo nesse cenário, deverá ser uma relação muitos-para-muitos, sem escolha automática da primeira associação.

## 5. Pesquisa e experiência do usuário

O GMI distinguirá duas intenções:

### Encontrar um jogo conhecido

- nome;
- aliases aprovados com proveniência suficiente;
- refinamento por plataforma, período e tipo.

### Explorar uma ideia ou nicho

- uma ou várias keywords;
- refinamento com gêneros, temas, modos, plataformas e período;
- perspectivas exibidas como detalhes complementares quando disponíveis.

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

Sem ID forte, usar sinais compostos: nome normalizado, aliases aprovados com
proveniência suficiente, tipo, empresas, plataformas, período, collections e
relações declaradas.

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

- AND para gêneros, temas, modos, keywords e multiplayer;
- sem filtro público de perspectivas no primeiro MVP;
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
