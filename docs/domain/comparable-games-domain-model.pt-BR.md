# Modelo de Domínio de Comparable Games

## Objetivo

Descrever a base implementada e a direção aprovada após a seleção de fontes. O domínio deve responder perguntas validadas de producers sem copiar os esquemas das APIs externas.

## Implementação atual

- `Game`, `Genre`, `Platform`, `DataSource` e `SourceReliability`;
- relações muitos-para-muitos entre jogo/gênero e jogo/plataforma;
- invariantes e unicidade por nome normalizado;
- persistência PostgreSQL e configuração EF Core;
- pesquisa por nome parcial, gênero, plataforma e ano;
- semântica AND entre categorias;
- ordenação alfabética e paginação;
- endpoints de jogo, detalhes, gêneros e plataformas;
- validação e erros padronizados;
- experiência responsiva em Blazor.

## `Game` atual

| Propriedade | Obrigatória | Significado |
|---|---:|---|
| `Id` | Sim | Identidade interna |
| `Name` | Sim | Nome de exibição |
| `Description` | Não | Contexto curto |
| `ReleaseDate` | Não | Data conhecida simplificada |
| `ImageUrl` | Não | Referência externa opcional |

A data atual não representa todos os eventos por plataforma, região, Early Access, port, remake ou remaster.

## Papéis aprovados

- IGDB: catálogo principal.
- Wikidata: reconciliação e enriquecimento.
- Steam: evidência oficial específica da Steam.

IDs de providers não devem virar propriedades permanentes específicas em `Game`.

## Conceitos propostos para revisão

```text
Game
├── Genres
├── Themes
├── GameModes
├── Companies
├── Releases
│   ├── Platform
│   ├── Region
│   ├── ReleaseDate
│   └── ReleaseStatus
└── ExternalReferences
    ├── Source
    ├── ExternalId
    ├── SourceUrl
    ├── ImportedAt
    └── LastVerifiedAt
```

Conceitos de apoio:

- `MatchConfidence`;
- separação entre fonte e evidência;
- proveniência por campo ou afirmação;
- confiabilidade e conflito;
- `SourceObservation` temporal.

## Regras de modelagem

- necessidades do producer vêm antes dos esquemas;
- origem e proveniência precisam ser inspecionáveis;
- conflitos não são apagados silenciosamente;
- jogo base, DLC, bundle, remake, remaster e port não são unidos cegamente;
- preço, reviews, rankings e jogadores são observações;
- imagens e descrições exigem revisão própria;
- payloads brutos, HTML e binários não são armazenados por padrão.

## Gate de migration

Nenhuma migration importante antes de:

1. fixar perguntas e filtros;
2. mapear campos permitidos de IGDB, Wikidata e Steam;
3. propor reconciliação;
4. revisar domínio e ingestão;
5. validar hipóteses em uma PoC pequena.
