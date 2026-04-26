# BUG-003 - API permite transação com data anterior ao nascimento da pessoa

## Descrição
A API permite cadastrar uma transação financeira com data anterior à data de nascimento da pessoa vinculada.

Esse comportamento representa uma inconsistência lógica, pois uma pessoa não deveria possuir registros financeiros antes de sua própria data de nascimento.

## Regra esperada
Uma transação vinculada a uma pessoa não deveria possuir data anterior à data de nascimento dessa pessoa.

## Comportamento atual
Ao enviar uma requisição `POST /api/v1/Transacoes` com uma data anterior ao nascimento da pessoa, a API retorna:

- Status: `201 Created`

## Comportamento esperado
A API deveria rejeitar a requisição, retornando erro de validação, por exemplo:

- Status: `400 Bad Request`
- Mensagem indicando que a data da transação não pode ser anterior à data de nascimento da pessoa

## Passos para reproduzir
1. Criar uma pessoa com uma data de nascimento válida
2. Criar uma categoria válida
3. Enviar uma requisição `POST /api/v1/Transacoes`
4. Informar uma data da transação anterior à data de nascimento da pessoa
5. Observar que a API cria a transação com sucesso

## Evidência automatizada
Teste relacionado:

```text
TransacoesDateRulesTests.Nao_Deve_Permitir_Transacao_Antes_Do_Nascimento_Da_Pessoa
```

## Impacto
- Esse comportamento pode comprometer a integridade cronológica dos dados financeiros
- Gerar relatórios inconsistentes por pessoa, período ou histórico

## Severidade
Média