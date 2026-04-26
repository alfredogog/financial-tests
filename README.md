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