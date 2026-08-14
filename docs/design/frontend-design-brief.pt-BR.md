# Brief de Design do Frontend de Comparable Games

> Status: primeira entrega responsiva implementada; revisão após a aprovação da
> PoC da IGDB em 14 de agosto de 2026; validação com dados representativos
> pendente.

## Objetivo

Definir a direção atual de responsividade, interação, acessibilidade e estados de feedback de Comparable Games sem repetir a visão geral do produto.

No primeiro MVP com IGDB, a experiência de filtros é a principal superfície de
valor do produto. Controles e explicações devem refletir a semântica dos campos
aprovada na PoC, qualificar resultados dependentes da fonte e comunicar dados
ausentes sem sobrecarregar o producer.

## Experiência implementada

- shell responsivo em Blazor WebAssembly;
- sidebar desktop iniciando expandida e podendo ser retraída;
- navegação mobile iniciando fechada e abrindo como drawer;
- identidade do produto visível em desktop e mobile;
- formulário explícito de busca e filtros com botão `Search`;
- envio unificado de nome, gênero, plataforma e ano;
- controles desabilitados durante carregamento;
- estado aplicado preservado na URL e sincronizado com o histórico;
- remoção de filtros, clear-all, paginação e contagem;
- estados de loading, sem dados, sem resultados, validação, erro e rota não encontrada;
- componentes reutilizáveis, code-behind, CSS isolation e validação responsiva.

O banco de produção continua vazio; opções preenchidas, cards e paginação com várias páginas dependem da ingestão.

## Busca

A API atual aceita:

- um nome parcial opcional;
- um gênero opcional;
- uma plataforma opcional;
- um ano opcional;
- paginação.

Categorias diferentes usam AND. Múltiplos valores na mesma categoria permanecem futuros.

A busca pertence ao formulário de Comparable Games, não ao header global. Enter e o botão visível enviam o mesmo estado completo.

## Sistema visual

- Open Sans no corpo;
- Space Grotesk nos títulos;
- variáveis CSS semânticas;
- CSS padrão, Grid, Flexbox e CSS isolation;
- mobile-first com breakpoints definidos pelo conteúdo;
- movimentos sutis com suporte a `prefers-reduced-motion`;
- sem tema escuro neste marco.

## Cards

Os cards exibem apenas informações concisas:

- capa opcional quando disponível e adequada ao tamanho de exibição escolhido;
- nome;
- data/ano;
- gêneros;
- plataformas;
- ação de detalhes.

Descrições longas e métricas analíticas não pertencem aos cards.

O layout do card é mobile-first e deve permanecer visualmente completo sem
imagem. Imagem ausente ou inadequada não exige placeholder permanente: o
conteúdo textual pode ocupar naturalmente o espaço disponível. Se um indicador
neutro de imagem ausente for adotado depois por acessibilidade ou consistência,
ele não poderá dominar o card nem sugerir que o registro esteja incompleto.

Imagens são reduzidas para containers predefinidos sem ampliar além de uma
variação adequada da fonte. A proporção é preservada com regras de encaixe ou
corte definidas pela UI; a imagem nunca será esticada. Capas apoiam
reconhecimento e respiro visual, mas não são filtro nem evidência de identidade.
Screenshots ficam nos detalhes e em galerias sob demanda, com atribuição visível
e as ressalvas de direitos documentadas.

## Feedback e acessibilidade

- loading preserva o contexto;
- sem resultados mantém os critérios e oferece recuperação;
- sem dados explica que o dataset ainda não está disponível;
- validação mantém a entrada e explica o campo;
- erros não expõem detalhes internos;
- teclado, foco, labels semânticos, `aria-live` e redução de movimento são obrigatórios.

## Adiado

Os itens abaixo estão adiados na interface já implementada. Alguns pertencem ao
restante do primeiro MVP com IGDB; outros continuam como visão de iterações
posteriores e devem ser priorizados conforme as decisões de dados aprovadas:

- múltiplos gêneros/plataformas;
- filtros qualificados pela fonte já aprovados e ainda não implementados, como
  modos e temas;
- página completa de detalhes;
- filtro de confiabilidade;
- apresentação de fontes nos resultados;
- navegação contextual por keyword;
- métricas, gráficos, recomendações, autenticação e tema escuro;
- filtro de perspectiva e filtro manual com múltiplas keywords, que permanecem
  em iterações futuras salvo nova evidência de cobertura.
