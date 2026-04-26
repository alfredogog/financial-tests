# financial-tests
Automação de testes para uma apldicação de controle financeiro, focado em validação de regras de negócio e estratégia de testes.

## Observação
O código da aplicação não foi incluído, conforme solicitado no teste técnico.

Artefatos de testes foram adicionados ao gitignore para manter o repositório limpo e focado nos códigos de teste.

## Como rodar a aplicação

## Como rodar os testes [ extensões e comandos ]

### Mapeamento de enums utilizado nos testes

Durante a análise da API, foi identificado que os valores numéricos esperados são:

- Tipo da transação:
  - 0 = Despesa
  - 1 = Receita

- Finalidade da categoria:
  - 0 = Despesa
  - 1 = Receita
  - 2 = Ambas

### Validação de enums

Durante a análise da API, foi identificado que a finalidade da categoria aceita os seguintes valores válidos:

- 0 = Despesa
- 1 = Receita
- 2 = Ambas

Foi criado um teste para verificar o comportamento da API ao receber `finalidade = 3`, valor que não representa nenhuma finalidade conhecida. A expectativa adotada é que a API rejeite esse payload com erro de validação, preservando a integridade dos dados.

### Hipótese adotada para validação de datas

Como o sistema foi descrito como uma aplicação de controle de gastos residenciais, foi adotada a hipótese de que as transações representam eventos financeiros já realizados ou ocorridos até a data atual.

Com base nessa interpretação, foram criados testes exploratórios para validar se a API impediria:

- transações com data futura;
- transações com data anterior à data de nascimento da pessoa vinculada.

Durante a execução, a API permitiu ambos os cenários, retornando `201 Created`.

Como a regra de data futura não está explicitamente descrita no escopo funcional, esse comportamento foi documentado como ponto de atenção e não como bug crítico. Já a transação anterior ao nascimento da pessoa foi considerada uma inconsistência lógica, pois uma pessoa não deveria possuir registros financeiros anteriores à sua existência.

## Resumo dos testes implementados

### Testes de Integração

#### `TransacoesBusinessRulesTests.cs`

Valida regras de negócio relacionadas ao cadastro de transações.

Cenários cobertos:

- não permitir receita para pessoa menor de idade;
- não permitir receita em categoria de despesa;
- não permitir despesa em categoria de receita;
- permitir receita em categoria com finalidade "Ambas";
- permitir despesa em categoria com finalidade "Ambas".

Objetivo: garantir que as transações respeitem as regras de idade da pessoa e finalidade da categoria.

---

#### `TransacoesDateRulesTests.cs`

Valida regras e hipóteses relacionadas às datas das transações.

Cenários cobertos:

- não permitir transação com data futura;
- não permitir transação com data anterior ao nascimento da pessoa.

Objetivo: verificar consistência cronológica das transações em relação ao contexto de controle de gastos residenciais.

Observação: o cenário de data futura foi tratado como ponto de atenção, pois a regra não está explicitamente descrita no escopo funcional.

---

#### `CategoriasValidationTests.cs`

Valida a aceitação de valores de finalidade para categorias.

Cenário coberto:

- não permitir cadastro de categoria com finalidade inválida (`finalidade = 3`).

Objetivo: garantir que a API aceite apenas valores conhecidos para finalidade da categoria:

- `0` = Despesa
- `1` = Receita
- `2` = Ambas

---

#### `TotaisPorPessoaTests.cs`

Valida o relatório de totais por pessoa.

Cenário coberto:

- criar uma pessoa com receita;
- criar uma pessoa com despesa;
- criar uma pessoa com receita e despesa;
- consultar o endpoint de totais por pessoa;
- validar total de receitas, total de despesas e saldo de cada pessoa;
- validar isolamento dos cálculos entre pessoas diferentes.

Objetivo: garantir que o relatório `/api/v1/Totais/pessoas` calcule corretamente receitas, despesas e saldo por pessoa.

---

#### `PessoasCascadeDeleteTests.cs`

Valida a regra de exclusão em cascata ao remover uma pessoa.

Cenário coberto:

- criar uma pessoa;
- criar uma categoria;
- criar uma transação vinculada à pessoa;
- confirmar que a transação existe;
- excluir a pessoa;
- validar que a transação vinculada não está mais acessível.

Objetivo: garantir que, ao excluir uma pessoa, suas transações vinculadas também sejam removidas.

## Resultados esperados dos testes de integração

Durante a execução dos testes de integração, alguns cenários podem falhar porque representam comportamentos divergentes das regras de negócio esperadas.

Essas falhas foram mantidas intencionalmente como evidência automatizada dos bugs encontrados.

## Observação sobre testes com falha

Alguns testes de integração falham intencionalmente, pois representam comportamentos divergentes das regras esperadas e foram mantidos como evidência automatizada dos bugs documentados.

Falhas esperadas:
- BUG-002 - API permite cadastro de categoria com finalidade inválida
- BUG-003 - API permite transação anterior ao nascimento da pessoa
- OBS-001 - API permite transação com data futura

### Bugs/observações relacionados

- BUG-002 - API permite cadastro de categoria com finalidade inválida
- BUG-003 - API permite transação com data anterior ao nascimento da pessoa
- OBS-001 - API permite cadastro de transação com data futura

Os detalhes estão documentados em `/docs/bugs`.