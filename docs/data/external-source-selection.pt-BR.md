# Game Market Intelligence — Seleção de Fontes Externas

> Idioma: Português (Brasil)  
> Revisado em: 27 de julho de 2026

## Objetivo

Este documento registra a pesquisa, a comparação e a decisão sobre as fontes externas do MVP do **Game Market Intelligence (GMI)**.

A seleção foi orientada pelo valor para producers e pelos seguintes critérios eliminatórios:

- operação obrigatória com custo de **R$ 0**;
- uso autorizado e compatível com os termos da fonte;
- ausência de scraping não autorizado;
- possibilidade de preservar proveniência, atribuição e limitações;
- confiabilidade avaliada por categoria de dado;
- valor complementar real, evitando integrações redundantes;
- compatibilidade com armazenamento, normalização e futura exposição pelos contratos do GMI.

> O GMI só apresenta dados que consegue justificar, rastrear e usar de forma autorizada. A ausência de dado confiável é preferível à presença de dado duvidoso.

## Comparação das fontes avaliadas

| Fonte | Papel avaliado | Valor principal | Limitação decisiva | Decisão para o MVP |
|---|---|---|---|---|
| **IGDB** | Catálogo geral principal | Ampla estrutura para jogos, gêneros, temas, modos, perspectivas, plataformas, empresas, lançamentos e identificadores externos | Uso não comercial, atribuição e limites contratuais devem continuar sendo respeitados | **Selecionada** |
| **Wikidata** | Reconciliação e enriquecimento | IDs externos, nomes alternativos, relações entre entidades, franquias, empresas e referências | Cobertura e qualidade variam por item e afirmação | **Selecionada** |
| **Steam** | Fonte oficial especializada | App ID, presença oficial na Steam, página do produto e dados específicos do ecossistema Steam | Escopo e permissão devem ser avaliados por endpoint; não representa o mercado inteiro | **Selecionada** |
| **RAWG** | Catálogo geral complementar | Cobertura ampla e estrutura simples de API | Forte sobreposição com IGDB; termos do plano gratuito e exposição downstream não oferecem segurança suficiente para o desenho atual | **Não selecionada; reconsideração condicional** |
| **MobyGames** | Catálogo histórico e curado | Histórico, versões, plataformas, créditos e curadoria detalhada | Acesso adequado ao uso pretendido depende de plano pago ou autorização excepcional; restrições adicionais para IA | **Não selecionada** |
| **SteamDB** | Referência independente para pesquisa Steam | Histórico de preços, jogadores, alterações e sinais do ecossistema Steam | Não oferece API pública e proíbe scraping/crawling | **Somente referência manual** |
| **Nintendo** | Validação oficial de fatos Nintendo | Alta autoridade sobre seus próprios produtos e plataformas | O portal encontrado é voltado a desenvolvimento e publicação; não foi identificada API autorizada de catálogo de terceiros | **Sem ingestão no MVP** |
| **Microsoft/Xbox** | Validação oficial do ecossistema Microsoft | IDs, páginas e fatos oficiais da Microsoft Store/Xbox | APIs encontradas são orientadas a produtos, publishers, usuários e Partner Center, não a um catálogo público geral | **Sem ingestão no MVP** |
| **PlayStation** | Validação oficial do ecossistema PlayStation | Potencial confirmação de páginas e disponibilidade oficiais | Não foi identificado mecanismo público e autorizado adequado ao GMI | **Sem ingestão no MVP** |
| **Amazon e outros marketplaces gerais** | Evidência comercial secundária | Confirmação de listagens e edições de varejo | Finalidade comercial, baixa adequação como identidade canônica e restrições de reutilização | **Não selecionados** |

## Fontes selecionadas

### 1. IGDB — catálogo geral principal

**Papel no GMI**

- descoberta de jogos comparáveis;
- filtros básicos e avançados;
- estrutura inicial de plataformas, lançamentos, empresas e classificações;
- identificadores para reconciliação com outras fontes.

**Justificativa**

A IGDB oferece a melhor combinação entre cobertura, estrutura de dados e utilidade para o fluxo de Comparable Games. Sua API é gratuita para uso não comercial nos termos publicados, e a integração será feita por worker, com armazenamento local, normalização e atribuição.

**Cuidados**

- não tratar toda classificação como oficial;
- preservar a natureza curada de gêneros, temas e palavras-chave;
- revisar separadamente direitos sobre imagens e descrições;
- não expor o esquema da IGDB diretamente no domínio ou na API pública do GMI.

### 2. Wikidata — reconciliação e enriquecimento aberto

**Papel no GMI**

- relacionar registros de fontes diferentes;
- armazenar IDs externos;
- recuperar nomes alternativos e multilíngues;
- enriquecer relações entre jogos, franquias, empresas e entidades;
- apoiar referências e deduplicação.

**Justificativa**

Os dados estruturados do Wikidata são disponibilizados sob **CC0**, o que oferece forte compatibilidade com armazenamento, transformação e exposição estruturada. Seu maior valor não é substituir o catálogo principal, mas complementar a IGDB com uma camada aberta de reconciliação.

**Cuidados**

- avaliar confiabilidade por afirmação;
- priorizar declarações referenciadas;
- não assumir cobertura uniforme;
- usar consultas direcionadas aos jogos já conhecidos, evitando dependência excessiva do serviço público.

### 3. Steam — fonte oficial especializada

**Papel no GMI**

- confirmar presença oficial na Steam;
- preservar Steam App ID;
- fornecer página oficial do produto;
- representar fatos específicos do ecossistema Steam;
- futuramente registrar observações temporais autorizadas, como preço ou disponibilidade.

**Justificativa**

A Steam é a fonte primária para fatos sobre seu próprio ecossistema e complementa a IGDB com autoridade oficial. Ela não ocupa o papel de catálogo geral nem deve ser usada para inferir vendas ou desempenho comercial.

**Cuidados**

- revisar cada endpoint e seu escopo de uso;
- tratar preço, disponibilidade e avaliações como dados temporais ou contextuais;
- não confundir reviews, jogadores simultâneos ou rankings com vendas;
- não copiar dados da SteamDB quando a origem adequada for a própria Steam.

## Fontes não selecionadas

### RAWG

Não foi incluída porque sua cobertura se sobrepõe fortemente à IGDB e as permissões do plano gratuito não oferecem clareza suficiente para armazenamento normalizado, redistribuição pela API do GMI e uso futuro por agentes. Pode ser reconsiderada apenas se demonstrar cobertura complementar relevante e autorização compatível.

### MobyGames

Embora possua grande valor histórico e curatorial, o acesso sustentável para o escopo desejado depende de assinatura ou aprovação excepcional. O MVP não será construído sobre uma exceção incerta nem sobre uma fonte com custo obrigatório.

### SteamDB

É uma ferramenta independente muito útil para pesquisa manual, mas não oferece API pública e declara que scraping e crawling não são permitidos. O GMI não criará worker, não copiará sua base e não redistribuirá seus dados. Ela poderá ser citada apenas como referência externa para investigação humana.

### Nintendo, Microsoft/Xbox e PlayStation

São autoridades oficiais sobre seus próprios ecossistemas, mas não foram encontrados mecanismos públicos e autorizados de catálogo geral adequados ao GMI. Permanecem disponíveis para validação futura de fatos e links oficiais, sem ingestão automática no MVP.

## Conjunto final do MVP

| Fonte | Classificação | Responsabilidade no GMI |
|---|---|---|
| **IGDB** | Fonte geral principal | Catálogo e filtros de Comparable Games |
| **Wikidata** | Reconciliação e enriquecimento | IDs, aliases, relações e referências abertas |
| **Steam** | Fonte oficial especializada | Fatos e referências oficiais do ecossistema Steam |
| **SteamDB** | Referência externa, sem ingestão | Pesquisa manual opcional fora do fluxo automatizado |

## Consequências para a implementação

- As fontes serão consumidas por **workers controlados**, nunca diretamente pelo frontend.
- O domínio utilizará conceitos próprios do GMI, sem copiar o formato de uma API externa.
- A origem, a data de coleta, a natureza do dado, a confiabilidade e eventuais conflitos deverão ser preservados.
- Dados temporais serão modelados como observações, não como propriedades permanentes de `Game`.
- A seleção não autoriza automaticamente imagens, descrições ou redistribuição irrestrita; esses itens exigem revisão específica.
- Nenhuma migration relevante será criada antes da definição dos campos efetivamente necessários e permitidos.

## Referências oficiais consultadas

- [IGDB API Documentation](https://api-docs.igdb.com/)
- [Wikidata Licensing](https://www.wikidata.org/wiki/Wikidata:Licensing)
- [Steamworks Web API Documentation](https://partner.steamgames.com/doc/webapi_overview)
- [RAWG API Documentation](https://rawg.io/apidocs)
- [RAWG API Terms of Service](https://rawg.io/tos_api)
- [MobyGames API](https://www.mobygames.com/info/api/)
- [MobyGames API Subscription](https://www.mobygames.com/api/subscribe/)
- [MobyGames Terms of Use](https://www.mobygames.com/info/terms/)
- [SteamDB FAQ](https://steamdb.info/faq/)
- [Nintendo Developer Portal — The Process](https://developer.nintendo.com/the-process)
- [Microsoft Store Service APIs](https://learn.microsoft.com/en-us/gaming/gdk/docs/store/commerce/service-to-service/microsoft-store-apis/xstore-nav)
