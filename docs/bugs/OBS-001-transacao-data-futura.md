# OBS-001 - API permite cadastro de transação com data futura

## Descrição
Durante os testes exploratórios e automatizados, foi identificado que a API permite cadastrar transações com data futura.

Como o sistema foi descrito como uma aplicação de controle de gastos residenciais, foi adotada a hipótese de que as transações representam eventos financeiros já realizados ou ocorridos até a data atual.

No entanto, a regra de negócio sobre datas futuras não está explicitamente descrita no escopo funcional. Por esse motivo, este comportamento foi documentado como observação/ponto de atenção, e não como bug confirmado.

## Hipótese adotada
Considerando o contexto de controle de gastos residenciais, foi assumido que transações futuras não deveriam ser permitidas, pois poderiam distorcer o saldo atual, receitas do mês e despesas do mês.

## Comportamento atual
Ao enviar uma requisição `POST /api/v1/Transacoes` com data futura, a API retorna:

- Status: `201 Created`

## Comportamento esperado sob a hipótese adotada
Caso o sistema tenha como objetivo registrar apenas movimentações financeiras já realizadas, a API deveria rejeitar a requisição, retornando erro de validação, por exemplo:

- Status: `400 Bad Request`
- Mensagem indicando que a data da transação não pode ser futura

## Passos para reproduzir
1. Criar uma pessoa válida
2. Criar uma categoria válida
3. Enviar uma requisição `POST /api/v1/Transacoes`
4. Informar uma data futura para a transação
5. Observar que a API cria a transação com sucesso

## Evidência automatizada
Teste relacionado:

```text
TransacoesDateRulesTests.Nao_Deve_Permitir_Transacao_Com_Data_Futura
```

## Impacto
- Caso o produto trabalhe com valores realizados, transações futuras podem impactar incorretamente cálculos como saldo atual, receitas do mês, despesas do mês e relatórios por pessoa
- Caso o produto tenha como objetivo também registrar previsões financeiras ou lançamentos agendados, o comportamento pode ser aceitável, desde que documentado na regra de negócio.

## Severidade
Baixa

## Classificação
Observação / Regra ambígua