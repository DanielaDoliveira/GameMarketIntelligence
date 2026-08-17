# Fluxo de Controle de Versão

## Branches

- `main`: branch remota protegida que representa o código em produção.
- `develop`: branch remota de integração dos incrementos concluídos.
- branches curtas de feature/fix/docs: branches locais para trabalho focado; não são publicadas no remoto.
- branches de PoC explicitamente aprovadas: exceções remotas preservadas como evidência da condução de um estudo; não integram automaticamente o produto.

O remoto deve permanecer enxuto. Fora das branches de PoC preservadas como evidência, somente `main` e `develop` são mantidas no GitHub. Branches locais devem seguir um padrão rastreável, como `feature/GMI-12-descricao-curta`, e ser removidas localmente depois que seu incremento tiver sido integrado e validado.

## Fluxo

1. atualizar a `develop` local a partir de `origin/develop`;
2. criar uma branch local curta a partir de `develop`;
3. implementar um incremento coerente;
4. atualizar testes e documentação;
5. executar build e testes locais;
6. criar commits focados na branch local;
7. integrar a branch local na `develop` local somente após a validação;
8. executar novamente a validação na `develop` integrada;
9. enviar apenas a `develop` ao remoto, acionando sua CI;
10. remover a branch local depois que a integração estiver confirmada;
11. quando a entrega vertical estiver pronta, abrir PR de `develop` para `main`;
12. exigir a validação completa do PR antes do merge;
13. publicar em produção pelo pipeline da `main`.

## Regras

- nenhum push direto em `main`;
- não publicar branches de feature/fix/docs no remoto;
- não abrir PR de branch local para `develop`, pois branches exclusivamente locais não existem no GitHub;
- enviar incrementos concluídos ao remoto por meio da `develop`;
- promover `develop` para `main` somente quando o conjunto integrado estiver pronto para produção;
- não mesclar branches de PoC integralmente no produto; transportar somente implementações ou decisões deliberadamente aprovadas;
- commits devem ser revisáveis;
- separar documentação quando isso melhora o histórico;
- não misturar refatoração não relacionada com entrega funcional;
- registrar decisões de produto, domínio, arquitetura, UX e fontes durante o desenvolvimento;
- não marcar trabalho como concluído antes da validação;
- manter migrations e infraestrutura em commits revisáveis.

## Validação

Cada merge deve preservar:

- build aprovado;
- testes automatizados;
- compilação e formatação corretas;
- configuração de deploy válida;
- OpenAPI e documentação relevante atualizados;
- nenhum segredo exposto.

A CI da `develop` fornece validação rápida de integração por build e testes. O PR de `develop` para `main` executa a validação completa exigida antes da produção. Após o merge em `main`, o pipeline de CD publica a versão aprovada e a validação continua no ambiente público.
