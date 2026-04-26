# BUG-001 - API permite erro interno ao cadastrar receita para menor de idade

## Descrição
Ao tentar cadastrar uma transação do tipo receita para uma pessoa menor de idade, a API retorna erro interno (500), ao invés de validar a regra de negócio.

## Regra de negócio esperada
Menores de idade não podem possuir receitas.

## Comportamento atual
A API retorna:
- Status: 500 Internal Server Error

## Comportamento esperado
A API deveria retornar:
- Status: 400 BadRequest
- Mensagem informando violação de regra de negócio

## Passos para reproduzir
1. Criar pessoa menor de idade
2. Criar categoria do tipo receita
3. Tentar cadastrar transação de receita para essa pessoa

## Impacto
- Falha na validação de regra de negócio crítica
- Possível inconsistência de dados
- Experiência ruim para o consumidor da API

## Severidade
Alta