# Fluxo de Controle de Versão

## Branches

- `main`: protegida e pronta para produção.
- `develop`: integração dos incrementos concluídos.
- branches curtas de feature/fix/docs: trabalho focado.

## Fluxo

1. criar branch a partir de `develop`;
2. implementar um incremento coerente;
3. atualizar testes e documentação;
4. executar build e testes locais;
5. criar commit focado;
6. abrir PR para `develop` ou, na integração de release, de `develop` para `main`;
7. exigir CI e revisão antes do merge;
8. publicar pelo pipeline existente.

## Regras

- nenhum push direto em `main`;
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
