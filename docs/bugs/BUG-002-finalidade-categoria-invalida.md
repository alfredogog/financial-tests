# BUG-002 - API permite cadastro de categoria com finalidade inválida

## Descrição
A API permite cadastrar uma categoria informando uma finalidade inexistente.

Durante os testes, foi identificado que os valores válidos para finalidade de categoria são:

- `0` = Despesa
- `1` = Receita
- `2` = Ambas

Ao enviar `finalidade = 3`, a API retornou sucesso na criação do registro.

## Regra esperada
A API deveria validar se a finalidade informada pertence ao conjunto de valores permitidos.

## Comportamento atual
Ao enviar uma requisição `POST /api/v1/Categorias` com `finalidade = 3`, a API retorna:

- Status: `201 Created`

## Comportamento esperado
A API deveria rejeitar a requisição, retornando erro de validação, por exemplo:

- Status: `400 Bad Request`
- Mensagem indicando que a finalidade informada é inválida

## Passos para reproduzir
1. Enviar uma requisição `POST /api/v1/Categorias`
2. Informar uma descrição válida
3. Informar `finalidade = 3`
4. Observar que a API cria a categoria com sucesso

## Evidência automatizada
Teste relacionado:

```text
CategoriasValidationTests.Nao_Deve_Permitir_Finalidade_De_Categoria_Invalida
```

## Impacto
- Permitir valores inválidos para finalidade pode gerar inconsistência nos dados
- Comportamento inesperado no frontend
- Falhas em regras de negócio que dependem da classificação correta da categoria

## Severidade
Média