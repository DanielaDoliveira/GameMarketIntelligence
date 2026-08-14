# Processo de Desenvolvimento de Software Orientado a Produto

> Versão inicial: 29 de julho de 2026

## Objetivo

Documentar o processo prático usado pelo Game Market Intelligence à medida que o projeto evolui de exercício de aprendizagem para produto orientado por evidências.

## Ciclo principal

```text
Problema de produto
→ hipótese
→ pesquisa
→ elegibilidade legal e da fonte
→ spike técnico
→ prova de conceito
→ registro de evidências
→ decisão
→ implementação vertical
→ validação automatizada e manual
→ deploy
→ revisão de produto
→ atualização documental
```

## Níveis de trabalho

### Pesquisa

Entende pergunta de produto, evidências, restrições legais e alternativas.

Saídas:

- notas;
- comparação de fontes;
- perguntas abertas;
- recomendação inicial.

### Spike

Investigação limitada no tempo para reduzir um risco técnico ou arquitetural, sem comportamento produtivo completo.

Saídas:

- pergunta;
- premissas;
- achados;
- risco;
- recomendação;
- critério de saída.

### Prova de conceito

Usa tecnologia ou dados reais para testar viabilidade e comportamento.

Saídas:

- experimento executável;
- observações;
- limitações;
- critérios de aprovação;
- insumos para decisão.

Uma PoC não define automaticamente a arquitetura final.

### Decisão

Transforma evidência em direção aprovada.

Registros possíveis:

- roadmap;
- decisão de domínio;
- decisão de fonte;
- ADR quando houver impacto arquitetural durável;
- decisão adiada com gatilho explícito.

### Implementação vertical

Entrega um caminho utilizável pelas camadas necessárias.

```text
fonte externa
→ Collector
→ persistência
→ API
→ frontend
→ deploy
```

### Validação

Inclui:

- testes unitários;
- testes de integração;
- testes de endpoint;
- testes no navegador;
- smoke tests em produção;
- revisão da qualidade dos dados;
- revisão da utilidade para o produto.

### Encerramento

Um incremento só fecha após:

- código;
- testes;
- documentação;
- evidência de deploy;
- limitações conhecidas;
- próximo passo registrado.

## Registros de decisão

Usar o menor documento adequado:

- roadmap para sequência e escopo;
- documento de produto para valor e regras;
- documento de domínio para modelagem;
- avaliação de fonte para legalidade e qualidade;
- observações de PoC para experimentos;
- ADR para decisões arquiteturais duráveis com alternativas relevantes;
- resumo de atualização para mudanças cruzadas.

## Definition of Ready para implementação

Um incremento de fonte está pronto quando:

- pergunta de produto está definida;
- uso legal está aceitável;
- papel da fonte está definido;
- campos necessários estão identificados;
- PoC oferece evidência suficiente;
- compatibilidade arquitetural foi revisada;
- impactos no domínio e migrations são compreendidos;
- Definition of Done está escrita.

## Definition of Done de um incremento

- resultado de produto funciona;
- fronteiras são preservadas;
- testes passam;
- fonte e confiabilidade são preservadas;
- deploy funciona;
- falha e reexecução são conhecidas;
- documentação está atual;
- riscos restantes estão explícitos.

## Estrutura sugerida

```text
docs/
├── architecture/
│   ├── decisions/
│   └── explorations/
├── data/
│   ├── assessments/
│   ├── benchmarks/
│   └── proofs-of-concept/
├── development/
│   ├── product-development-process.en-US.md
│   ├── product-development-process.pt-BR.md
│   └── workflows/
├── domain/
├── planning/
├── product/
└── updates/
```

## Template leve de acompanhamento

```text
Problema:
Resultado para usuário/produto:
Hipótese:
Escopo:
Fora do escopo:
Riscos:
Evidência necessária:
Camadas envolvidas:
Validação:
Deploy:
Documentação:
Critério de saída:
Próxima dependência:
```

## Melhoria contínua

Ao final de cada milestone, revisar:

- o que aprendemos;
- o que causou retrabalho;
- quais premissas estavam erradas;
- qual documentação faltou;
- qual etapa deve ser adicionada, removida ou simplificada.

O processo é adaptativo. Ele deve servir ao produto, não virar burocracia.
