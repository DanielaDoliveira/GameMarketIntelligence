# Resumo da Atualização da Documentação

> Atualizado em: 27 de julho de 2026

## Objetivo

Registrar as alterações documentais concluídas após o benchmarking de fontes externas e o primeiro marco do frontend.

## Principais atualizações

- Adicionada documentação bilíngue usando a convenção `.en-US.md` e `.pt-BR.md`.
- Consolidado o benchmark de fontes e removida a premissa desatualizada de que o MVP precisava obrigatoriamente de três fontes gerais.
- Confirmado o conjunto selecionado:
  - IGDB como catálogo geral principal;
  - Wikidata para reconciliação e enriquecimento;
  - Steam como fonte oficial especializada.
- Classificada a SteamDB como referência externa para pesquisa manual, sem ingestão.
- Registrados os motivos para não selecionar RAWG, MobyGames, Nintendo, Microsoft/Xbox, PlayStation e marketplaces gerais para ingestão no MVP.
- Atualizada a direção de domínio de Comparable Games para preservar proveniência, referências externas, confiança da reconciliação, conflitos e observações temporais.
- Reforçado o gate de migration: nenhuma migration importante antes do mapping de campos, permissões, regras de reconciliação, revisão do domínio e pequena prova de conceito de ingestão.
- Atualizado o roadmap para refletir o foco atual em source-to-field mapping e desenho da ingestão.
- Atualizado o brief do frontend para refletir:
  - botão visível `Search`;
  - envio unificado do formulário;
  - estado aplicado preservado na URL;
  - navegação responsiva;
  - componentes reutilizáveis;
  - validação ainda pendente com dados representativos.
- Removidas explicações repetidas e mantida cada documentação focada em sua responsabilidade.

## Estrutura atual

| Área | Responsabilidade |
|---|---|
| `docs/data` | Regras de avaliação, benchmark, seleção e análises detalhadas |
| `docs/design` | Direção visual e de interação do frontend |
| `docs/development` | Fluxo de versionamento e entrega |
| `docs/domain` | Base de domínio e regras de modelagem |
| `docs/planning` | Marcos de implementação e próximos passos |
| `docs/product` | Visão, proposta de valor, necessidades dos producers e escopo |
| `docs/updates` | Resumos de alterações documentais |

## Direção atual do projeto

A etapa de pesquisa de fontes está concluída.

O próximo trabalho documental e técnico deve definir:

1. qual pergunta de producer cada fonte atende;
2. campos permitidos e necessários;
3. regras de proveniência e confiabilidade;
4. modelos de referências externas e reconciliação;
5. frequência de coleta, retries, checkpoints e idempotência;
6. mudanças de domínio;
7. migrations somente após aprovação do modelo.

## Regra de manutenção

Sempre que uma decisão documentada mudar, as duas versões de idioma devem ser atualizadas no mesmo pull request.
