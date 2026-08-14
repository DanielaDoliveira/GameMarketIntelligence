# Game Market Intelligence — Avaliações das Fontes Selecionadas

> Idioma: Português (Brasil)  
> Status: documento vivo  
> Revisado em: 14 de agosto de 2026

## Objetivo

Este documento registra a avaliação detalhada das fontes selecionadas para o MVP do **Game Market Intelligence (GMI)**.

Ele complementa o documento resumido de seleção de fontes e concentra as decisões de produto, dados, legalidade, arquitetura e engenharia relacionadas às três fontes aprovadas:

| Fonte | Classificação | Status |
|---|---|---|
| **IGDB** | Fonte geral principal | Selecionada; PoC concluída e aprovada para a primeira iteração de fonte |
| **Wikidata** | Fonte de reconciliação e enriquecimento | Selecionada; PoC detalhada adiada para iteração futura |
| **Steam** | Fonte oficial especializada | Selecionada; PoC detalhada adiada para iteração futura |

As fontes não selecionadas permanecem documentadas no benchmark comparativo.

---

# 1. IGDB — Fonte geral principal

## Papel proposto

- catálogo geral principal;
- base para os filtros de Comparable Games;
- fonte de identificadores externos;
- entrada para normalização e reconciliação;
- fonte estruturada para uso humano e futuro uso por agentes através dos contratos do GMI.

## Valor para producers

A IGDB oferece suporte tanto à descoberta inicial quanto ao refinamento da pesquisa.

Perguntas que ela pode ajudar a responder:

- Quais jogos compartilham gênero, plataforma e período de lançamento?
- Quais jogos possuem temas, modos ou perspectivas semelhantes?
- Quais empresas desenvolveram ou publicaram títulos parecidos?
- Quais lançamentos pertencem a determinada plataforma ou região?
- O resultado é jogo principal, port, remake, remaster, expansão ou DLC?
- Quais referências externas podem apoiar uma validação posterior?

| Dimensão | Avaliação |
|---|---|
| Descoberta inicial | Muito alta |
| Refinamento avançado | Muito alta |
| Relações com empresas | Alta |
| Modelagem de lançamentos | Alta |
| IDs externos e reconciliação | Alta |
| Métricas comerciais | Baixa para o MVP atual |
| Utilidade futura para agentes | Muito alta |

## Categorias de dados relevantes

O subconjunto potencialmente útil inclui:

- jogos;
- gêneros;
- temas;
- modos de jogo;
- perspectiva do jogador;
- plataformas;
- datas de lançamento;
- regiões;
- empresas envolvidas;
- developers e publishers;
- palavras-chave;
- modos multiplayer;
- sites e referências externas;
- tipos e relações entre jogos;
- timestamps de criação e atualização;
- checksums.

O GMI não deve importar todo o esquema da IGDB. O recorte final deve ser determinado pelos filtros aprovados e pelas necessidades do domínio.

## Confiabilidade por categoria

| Categoria | Confiabilidade provisória | Observação |
|---|---|---|
| Identidade e nome do jogo | Alta | Ainda exige tratamento de edições e duplicidades |
| Plataformas | Média-alta | Fontes oficiais podem confirmar fatos específicos |
| Datas de lançamento | Média-alta | Boa estrutura por plataforma, região e precisão |
| Developer e publisher | Média-alta | Relações estruturadas, sem pressupor oficialidade absoluta |
| Gêneros | Média | Classificação curada |
| Temas | Média | Úteis, porém interpretativos |
| Modos de jogo | Média-alta | Mais objetivos, mas ainda curados |
| Perspectiva | Média | Classificação interpretativa |
| Palavras-chave | Média-baixa | Podem conter ruído |
| Ratings e popularidade | Contextual | Sinais, não comprovação comercial |
| Imagens | Pendente | Direitos e armazenamento exigem revisão separada |

## Natureza dos dados

A IGDB deve ser representada como uma base estruturada e curada.

Ela não será tratada como fonte oficial para todos os atributos de um jogo. Validações oficiais poderão vir de publishers, developers, plataformas ou páginas oficiais.

## Custo e uso

A IGDB foi aprovada para o cenário atual de uso não comercial e custo zero, respeitando os termos aplicáveis e os requisitos de atribuição.

Qualquer mudança para monetização ou uso comercial exigirá nova avaliação.

## Modelo de armazenamento e exposição

```text
IGDB
  ↓
Worker agendado
  ↓
Validação e normalização
  ↓
PostgreSQL
  ↓
API do GMI
  ↓
Blazor e futuras integrações de leitura
```

O GMI deve armazenar dados normalizados e expor contratos próprios, sem funcionar como espelho da IGDB.

## Atribuição

A implementação deverá preservar:

- identificação visível da IGDB na página Data Sources;
- link para a fonte;
- proveniência associada aos dados relevantes;
- atribuição estática quando exigida;
- data de coleta e verificação.

A redação e a posição finais da atribuição deverão seguir os termos e as orientações de marca aplicáveis.

## Viabilidade técnica

### Autenticação

A autenticação utiliza credenciais de aplicação e tokens OAuth vinculados ao ecossistema Twitch.

Segredos devem permanecer no backend ou no worker e nunca ser expostos ao Blazor WebAssembly.

### Limites e coleta

O worker deve operar de forma conservadora, com:

- frequência controlada;
- concorrência limitada;
- tratamento de `429`;
- retries com espera;
- checkpoints;
- idempotência;
- atualização incremental.

### Atualizações incrementais

Timestamps e checksums podem apoiar a identificação de registros alterados.

No primeiro incremento, a preferência é por coleta agendada, sem adicionar webhooks ao escopo.

### Isolamento

A sintaxe e os contratos específicos da IGDB devem ficar em um adapter de Infrastructure.

Application, Domain e contratos públicos devem permanecer independentes da fonte.

## Implicações arquiteturais

Evitar:

- IDs de temas da IGDB diretamente em `Game`;
- enums externos nos contratos públicos;
- objetos de requisição da IGDB em Application;
- credenciais externas no frontend.

Preferir:

- conceitos normalizados;
- referências externas explícitas;
- proveniência;
- adapters;
- regras de mapping e reconciliação;
- contratos estáveis do GMI.

## Uso por agentes

O GMI deve acrescentar a semântica necessária para consumo seguro por agentes:

- fonte;
- natureza do dado;
- confiabilidade;
- URL original;
- data de verificação;
- alertas de conflito;
- limitações de cobertura;
- indicação de que resultados são candidatos comparáveis.

## Riscos e questões abertas

1. limites exatos de exposição pela API pública do GMI;
2. atribuição final;
3. direitos de imagens e descrições;
4. tratamento de duplicidades e edições;
5. conflitos de lançamentos regionais;
6. mudanças futuras nas condições de acesso gratuito.

## Decisão

**Selecionada como fonte geral principal do MVP.**

Nenhuma migration deve ser criada apenas para reproduzir o esquema da IGDB.

---

# 2. Wikidata — Reconciliação e enriquecimento

## Papel proposto

- reconciliar registros de fontes diferentes;
- fornecer IDs externos;
- recuperar aliases e nomes multilíngues;
- enriquecer relações entre jogos, franquias, empresas e entidades;
- apoiar deduplicação e referências;
- oferecer uma camada aberta para contratos humanos e machine-readable.

## Valor para producers

A Wikidata não substitui a IGDB como catálogo principal.

Seu valor está em conectar e explicar entidades:

- identificar que dois registros representam o mesmo jogo;
- relacionar nomes alternativos;
- conectar jogos a franquias;
- relacionar developers, publishers e empresas;
- preservar referências externas;
- apoiar buscas multilíngues;
- facilitar navegação entre entidades.

| Dimensão | Avaliação |
|---|---|
| Catálogo geral | Média e irregular |
| Reconciliação | Muito alta |
| IDs externos | Muito alta |
| Nomes alternativos | Alta |
| Relações entre entidades | Alta |
| Cobertura de jogos menores | Variável |
| Compatibilidade com agentes | Muito alta |

## Categorias de dados relevantes

- identificadores externos;
- nomes e aliases;
- rótulos multilíngues;
- franquias;
- developers;
- publishers;
- empresas;
- plataformas;
- relações entre obras e versões;
- URLs e referências;
- qualificadores e ranks;
- fontes associadas às afirmações.

## Confiabilidade por categoria

| Categoria | Confiabilidade provisória | Observação |
|---|---|---|
| IDs externos | Alta | Muito úteis para reconciliação |
| Nome principal | Alta quando consistente | Pode variar por idioma ou item |
| Aliases | Média-alta | Úteis para busca e matching |
| Relações de franquia | Média-alta | Dependem da qualidade do item |
| Empresas relacionadas | Média | Requer leitura da afirmação e qualificadores |
| Plataformas e datas | Variável | Não devem substituir fontes principais sem validação |
| Afirmações referenciadas | Mais alta | Priorizar declarações com fontes |
| Afirmações sem referência | Mais baixa | Devem carregar menor confiança |

## Natureza dos dados

Cada afirmação pode possuir:

- valor;
- referência;
- qualificador;
- rank;
- histórico de edição.

Por isso, a confiança deve acompanhar a afirmação, não apenas o item inteiro.

## Licença e custo

Os dados estruturados do Wikidata são disponibilizados sob **CC0**.

Isso oferece forte compatibilidade com:

- armazenamento;
- transformação;
- normalização;
- redistribuição estruturada;
- contratos próprios;
- uso por ferramentas e agentes.

O uso continua sujeito a práticas responsáveis e aos limites operacionais dos serviços públicos.

## Modelo de ingestão

A estratégia recomendada é enriquecimento direcionado:

```text
Jogo já identificado
  ↓
Consulta por nome ou ID externo
  ↓
Correspondência e validação
  ↓
Importação seletiva de IDs, aliases e relações
  ↓
Registro de proveniência
```

Não é recomendado:

- depender da SPARQL pública em tempo de requisição;
- importar o dump completo no MVP;
- usar a Wikidata como única fonte de identidade;
- aceitar correspondências automáticas sem confiança.

## Reconciliação

O processo deve considerar:

- nomes;
- aliases;
- plataformas;
- datas;
- developers;
- publishers;
- IDs externos;
- tipo de produto;
- relação entre original, remake, remaster, DLC e port.

Uma correspondência deve possuir confiança e evidências suficientes antes de unir registros.

## Viabilidade técnica

- consultas SPARQL direcionadas;
- endpoints e dumps abertos;
- IDs estáveis;
- RDF e JSON estruturados;
- integração adequada a workers;
- boa compatibilidade com processamento incremental.

O worker deve limitar consultas, usar cache e evitar pressão desnecessária sobre o serviço público.

## Implicações arquiteturais

A Wikidata reforça a necessidade de conceitos como:

```text
ExternalReference
├── Source
├── ExternalId
├── SourceUrl
├── ImportedAt
├── LastVerifiedAt
└── MatchConfidence
```

Também apoia a separação entre:

- fonte que forneceu o valor;
- evidência que confirmou o valor;
- observação capturada em determinado momento.

## Uso por agentes

A Wikidata agrega alto valor para agentes porque fornece:

- IDs persistentes;
- relações semânticas;
- aliases;
- referências;
- estrutura machine-readable;
- licença aberta.

Ainda assim, o GMI deve expor apenas dados reconciliados e contextualizados, não respostas brutas de SPARQL.

## Riscos e questões abertas

1. cobertura irregular;
2. afirmações sem referência;
3. correspondências incorretas entre edições;
4. disponibilidade e limites do endpoint público;
5. qualidade desigual entre idiomas e itens;
6. necessidade de revisão manual para casos ambíguos.

## Decisão

**Selecionada como fonte de reconciliação e enriquecimento do MVP.**

Ela complementa a IGDB e não disputa o papel de catálogo geral principal.

---

# 3. Steam — Fonte oficial especializada

## Papel proposto

- confirmar presença oficial no ecossistema Steam;
- preservar Steam App ID;
- fornecer a página oficial do produto;
- validar informações específicas da plataforma;
- futuramente registrar observações temporais autorizadas;
- oferecer evidência oficial complementar ao catálogo geral.

## Valor para producers

A Steam pode apoiar questões como:

- O jogo está oficialmente disponível na Steam?
- Qual é seu App ID?
- Qual é a página oficial do produto?
- Quais sistemas operacionais ou recursos são indicados na loja?
- Qual é o estado observado de disponibilidade?
- Quais avaliações agregadas existem no ecossistema Steam?
- Qual era o preço em uma região e momento específicos, quando permitido?

| Dimensão | Avaliação |
|---|---|
| Autoridade sobre a Steam | Muito alta |
| Identidade dentro da plataforma | Muito alta |
| Página oficial | Muito alta |
| Cobertura do mercado geral | Baixa |
| Métricas comerciais absolutas | Não disponível |
| Dados temporais | Potencialmente altos, com contexto |
| Valor para agentes | Alto, com limites claros |

## Categorias de dados relevantes

- Steam App ID;
- nome e tipo de produto;
- página oficial;
- presença na plataforma;
- informações da loja;
- developer e publisher exibidos;
- release na Steam;
- sistemas operacionais;
- categorias e recursos;
- avaliações agregadas;
- preço e disponibilidade, quando autorizados;
- relações com DLCs e pacotes, quando aplicável.

## Confiabilidade por categoria

| Categoria | Confiabilidade provisória | Observação |
|---|---|---|
| Steam App ID | Muito alta | Identificador oficial |
| Presença na Steam | Muito alta | Fato oficial |
| Página do produto | Muito alta | Referência oficial |
| Nome e tipo | Alta | Exige distinguir jogo, DLC, demo, bundle e ferramenta |
| Release na Steam | Alta | Não representa necessariamente o primeiro lançamento mundial |
| Developer e publisher exibidos | Alta | Pode refletir nome comercial da listagem |
| Plataformas suportadas | Alta no contexto Steam | Limitada ao ecossistema |
| Reviews agregadas | Alta como métrica Steam | Não representam vendas |
| Preço | Muito alta no momento e região | Dado temporal |
| Vendas e receita | Indisponíveis | Não devem ser inferidas |

## Natureza dos dados

A Steam é oficial para fatos do próprio ecossistema.

Isso não torna os dados universais. Exemplos:

- data na Steam não é necessariamente data original;
- reviews na Steam não são satisfação global;
- concorrência simultânea não representa vendas;
- ranking não revela unidades ou receita.

## Escopo de acesso

A avaliação deve permanecer endpoint por endpoint.

É necessário distinguir:

- APIs públicas;
- métodos com chave de usuário;
- métodos de publisher;
- funcionalidades Steamworks;
- dados de loja;
- endpoints não documentados.

O GMI utilizará apenas mecanismos documentados e compatíveis com o uso pretendido.

## Dados temporais

Preço, disponibilidade, reviews e rankings devem ser tratados como observações:

```text
SourceObservation
├── GameId
├── Source
├── Metric
├── Value
├── Region
├── Currency
├── ObservedAt
└── Method
```

Eles não devem virar propriedades estáticas e universais de `Game`.

## Modelo de ingestão

```text
Steam
  ↓
Worker especializado
  ↓
Validação do App ID e tipo
  ↓
Normalização
  ↓
Referência oficial e observações permitidas
  ↓
PostgreSQL
```

O worker deve respeitar limites, cache, frequência e permissões específicas.

## Relação com SteamDB

SteamDB é uma ferramenta independente e não será ingerida.

Quando um dado puder ser obtido oficialmente da Steam, a origem correta é a Steam.

O GMI não deve:

- raspar a SteamDB;
- copiar históricos;
- usar endpoints internos;
- reproduzir seu banco;
- tratar a SteamDB como fonte oficial.

## Atribuição e marca

A apresentação deve respeitar:

- identidade da Steam;
- regras de branding;
- disclaimers;
- links oficiais;
- eventuais exigências de atribuição;
- limites de uso de imagens e assets.

## Implicações arquiteturais

A Steam deve permanecer em um adapter próprio.

Evitar:

- tornar App ID a identidade interna do jogo;
- misturar disponibilidade Steam com disponibilidade global;
- armazenar preço sem região e data;
- tratar reviews como vendas;
- expor métodos de publisher ou credenciais.

## Uso por agentes

Contratos voltados a agentes devem comunicar claramente:

- que o fato se aplica à Steam;
- quando foi verificado;
- região e momento de observações;
- que disponibilidade pode mudar;
- que reviews e concorrência não são vendas;
- que a página fornecida é oficial da plataforma.

## Riscos e questões abertas

1. revisão definitiva dos endpoints escolhidos;
2. permissões para armazenamento e exposição;
3. regras de branding e atribuição;
4. direitos de imagens e descrições;
5. diferenças regionais;
6. estabilidade de dados temporais;
7. prevenção de inferências comerciais indevidas.

## Decisão

**Selecionada como fonte oficial especializada do ecossistema Steam.**

Ela complementa, mas não substitui, a IGDB e a Wikidata.

---

# Responsabilidades combinadas

| Necessidade | IGDB | Wikidata | Steam |
|---|---|---|---|
| Catálogo geral | Principal | Complementar e irregular | Apenas Steam |
| Filtros de Comparable Games | Principal | Limitado | Limitado |
| IDs externos | Forte | Muito forte | Steam App ID |
| Reconciliação | Boa | Principal | Apoio por App ID |
| Relações entre entidades | Forte | Forte | Limitadas ao ecossistema |
| Confirmação oficial | Não universal | Não universal | Oficial para Steam |
| Links oficiais | Referências externas | Referências abertas | Página oficial Steam |
| Dados temporais | Limitados | Não prioritários | Possíveis, com contexto |
| Licença aberta | Não | Sim, CC0 | Não |
| Papel no MVP | Catálogo | Reconciliação | Validação oficial especializada |

---

# Regras compartilhadas de implementação

1. As fontes serão consumidas por workers controlados.
2. O frontend não acessará fontes externas diretamente.
3. Credenciais permanecerão no backend ou nos workers.
4. O domínio utilizará conceitos próprios do GMI.
5. Proveniência será preservada por valor ou afirmação.
6. Conflitos não serão ocultados.
7. Dados temporais serão observações.
8. Imagens e descrições exigem revisão própria por fonte; a revisão de imagens
   da IGDB está concluída para o primeiro MVP, e fontes futuras mantêm esse gate.
9. A API do GMI não funcionará como espelho das fontes.
10. Acesso futuro por agentes será somente leitura.
11. Nenhuma migration relevante será criada antes da definição dos campos da
    fonte e da iteração que a justificam.
12. Mudanças de termos, custo ou escopo exigirão reavaliação.

---

# Status de progressão

A primeira iteração de fonte com IGDB já definiu perguntas de produto, escopo
de campos, nulabilidade, restrições de proveniência, comportamento operacional
e aprovação da PoC. A próxima etapa imediata é o spike leve de compatibilidade
multifonte, seguido pelo Collector definitivo e limpo e pela revisão de
persistência da IGDB.

Perguntas, mappings, permissões e PoCs detalhadas de Wikidata e Steam continuam
como trabalho de iterações futuras, próximo às respectivas integrações. Assim a
visão multifonte completa é preservada sem bloquear o MVP atual com IGDB nem
forçar prematuramente campos futuros no modelo.
