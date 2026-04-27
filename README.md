# financial-tests
Automação de testes para uma aplicação de controle financeiro, com foco em validação de regras de negócio, testes de integração, testes E2E e documentação de falhas encontradas.

## Observação
O código da aplicação não foi incluído, conforme solicitado no teste técnico.

Artefatos de execução, dependências locais e arquivos de build foram adicionados ao `.gitignore` para manter o repositório limpo e focado nos códigos de teste.

## Atendimento aos critérios do desafio

Este README foi estruturado para explicar:

- Como rodar os testes de integração e E2E;
- Como a pirâmide de testes foi aplicada;
- Quais bugs, observações e achados exploratórios foram encontrados;
- Quais decisões técnicas foram tomadas durante a construção da estratégia de testes.

## Estrutura da pirâmide de testes

A estratégia de testes foi organizada com base na pirâmide de testes, priorizando cenários de maior valor para o domínio da aplicação.

### Base da pirâmide - Testes unitários

Os testes unitários não foram implementados neste repositório porque o código-fonte da aplicação não deveria ser alterado nem versionado junto com a solução do desafio.

Como os testes foram construídos de forma externa à aplicação, não havia acesso direto às classes internas de domínio, serviços, validadores, componentes ou funções utilitárias para testá-las de maneira isolada sem copiar parte do código da aplicação para este repositório.

Por esse motivo, a estratégia priorizou:

- testes de integração via API, validando regras de negócio com a aplicação em execução;
- testes E2E com Playwright, validando o fluxo principal do usuário pela interface web.

Testes unitários de frontend com Vitest também não foram implementados, pois exigiriam acesso ao código-fonte do frontend, como componentes, hooks ou funções internas.

Essa decisão evita duplicação ou cópia indevida do código da aplicação original e mantém o repositório restrito aos artefatos de teste solicitados.

### Meio da pirâmide - Testes de integração

Os testes de integração foram priorizados porque permitem validar as principais regras de negócio da API de forma direta, rápida e com maior estabilidade do que testes exclusivamente pela interface.

Eles validam a comunicação entre:

- endpoints HTTP;
- regras de negócio;
- persistência de dados;
- respostas retornadas pela API.

Essa camada concentrou a maior parte dos cenários automatizados.

### Topo da pirâmide - Testes E2E

Os testes E2E foram implementados com Playwright e TypeScript para validar o fluxo principal do usuário pela interface web.

Como testes E2E são mais custosos, lentos e sensíveis a alterações visuais, foi implementado um fluxo principal de ponta a ponta, cobrindo:

- criação de pessoa;
- criação de categorias;
- criação de transações;
- validação do relatório de totais por pessoa.

Esse teste valida a integração completa entre frontend, API, banco de dados e retorno visual para o usuário.

## Como rodar a aplicação

Antes de executar os testes, é necessário subir a aplicação original fornecida no desafio.

Na raiz do projeto original da aplicação, execute:

```bash
docker compose up --build
```

A API deve ficar disponível em: `http://localhost:5000`

Swagger: `http://localhost:5000/swagger`

Frontend: `http://localhost:5173`

## Estrutura do projeto

```text
financial-tests/
├── api-tests/
│   └── integration-tests/
├── e2e-tests/
│   ├── fixtures/
│   ├── pages/
│   ├── tests/
│   └── utils/
├── docs/
│   ├── bugs/
│   ├── exploratory/
│   └── images/
├── .gitignore
└── README.md
```

## Como rodar os testes

### Pré-requisitos

Para executar os testes, é necessário ter instalado:

- Docker Desktop;
- .NET SDK 8;
- Node.js;
- npm;
- Playwright.

---

### Testes de integração

Na raiz deste repositório, execute:

```bash
dotnet test
```

Para rodar apenas os testes de integração:

```bash
cd api-tests/integration-tests/IntegrationTests
dotnet test
```

### Testes E2E

Acesse a pasta do projeto E2E:

```bash
cd e2e-tests
```

Instale as dependências:

```bash
npm install
```

Para rodar todos os testes E2E:

```bash
npx playwright test
```

Para rodar o fluxo principal com navegador visível:

```bash
npx playwright test tests/financial-flow.spec.ts --headed
```

Para abrir o relatório HTML do Playwright:

```bash
npx playwright show-report
```

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

---

### Testes E2E

Os testes E2E foram implementados com Playwright e TypeScript, utilizando o padrão Page Object Model.

A estrutura foi organizada em:

- `tests/` - cenários E2E;
- `pages/` - ações e seletores por tela;
- `fixtures/` - massa de dados;
- `utils/` - funções auxiliares.

#### `financial-flow.spec.ts`

Valida o fluxo principal da aplicação pela interface web.

Cenário coberto:

- acessar a aplicação;
- criar pessoa;
- criar categoria de receita;
- criar categoria de despesa;
- criar transação de receita;
- criar transação de despesa;
- acessar a tela de relatórios/totais;
- validar o relatório de totais por pessoa.

Esse teste valida o fluxo completo:

```text
Frontend → API → Banco de dados → Relatório na interface
```

#### Limpeza de dados no E2E

Após a execução do teste E2E, a pessoa criada é removida via API no `afterEach`.

Como a exclusão de pessoa remove suas transações vinculadas em cascata, as transações criadas no E2E também são removidas.

As categorias criadas no teste podem permanecer no banco, pois não foi identificado endpoint para exclusão de categorias.

## Justificativa das escolhas de testes

A maior parte dos testes foi concentrada na camada de integração porque as principais regras de negócio da aplicação estão expostas pela API e podem ser validadas de forma objetiva por meio dos endpoints.

Essa abordagem permite testar regras críticas com menor custo de manutenção do que testes E2E, além de facilitar a identificação de falhas específicas em status code, payloads e persistência de dados.

O teste E2E foi utilizado para validar o fluxo principal do usuário pela interface, garantindo que a aplicação funcione de ponta a ponta. A quantidade de testes E2E foi mantida reduzida de forma intencional, pois esse tipo de teste tende a ser mais lento, mais sensível a mudanças visuais e mais custoso para manutenção.

Os testes unitários não foram implementados porque o código-fonte da aplicação não foi incluído neste repositório. Criar testes unitários artificiais sobre funções auxiliares do próprio projeto de testes teria baixo valor para o desafio, pois não validaria unidades reais da aplicação.

Dessa forma, a estratégia priorizou testes com maior relevância para o comportamento real do sistema, mantendo rastreabilidade entre regras de negócio, evidências automatizadas e documentação dos problemas encontrados.

Também foram realizados testes manuais exploratórios no frontend para complementar a automação, especialmente em pontos de usabilidade, responsividade, consistência visual e validações de formulário. Esses cenários foram documentados separadamente em arquivos `.md`, com evidências visuais, por se tratarem de achados mais adequados à análise exploratória do que à automação.

## Testes manuais exploratórios

Além dos testes automatizados, foram realizados testes manuais exploratórios na interface web, com foco em usabilidade, responsividade, consistência visual e validações de formulário.

Esses achados foram documentados separadamente porque nem todos os problemas visuais ou de experiência do usuário são melhor representados por testes automatizados.

Os documentos estão localizados em:

```text
docs/exploratory/
```

As evidências visuais estão localizadas em: `docs/images/`

Achados documentados:

- `FRONT-001` - Combobox de Pessoa e Categoria exige seleção manual da opção após digitação;
- `FRONT-002` - Caractere estranho exibido no gráfico de resumo mensal;
- `FRONT-003` - Gráfico de resumo mensal perde legibilidade em resolução reduzida;
- `FRONT-004` - Cards principais do dashboard perdem legibilidade em resolução reduzida;
- `FRONT-005` - Dashboard exibe categorias no resumo mensal mesmo sem transações;
- `FRONT-006` - Inconsistência entre menu "Relatórios" e breadcrumb "Totais".

## GitHub Actions

A configuração de CI com GitHub Actions não foi adicionada porque este repositório contém apenas os testes automatizados e a documentação, conforme solicitado no desafio.

Os testes de integração e E2E dependem da aplicação original em execução, incluindo API, frontend e banco de dados. Como o código-fonte da aplicação não deve ser versionado nem enviado junto com este repositório, o pipeline não teria acesso aos arquivos necessários para subir a aplicação via Docker Compose.

Tecnicamente, o CI poderia ser viabilizado de algumas formas:

- incluir o código da aplicação no mesmo repositório dos testes;
- disponibilizar a aplicação em outro repositório acessível pelo workflow;
- utilizar uma imagem Docker publicada da aplicação;
- disponibilizar o pacote da aplicação como artefato externo para ser baixado durante o pipeline.

Essas alternativas não foram adotadas porque poderiam ferir a regra do desafio de manter este repositório restrito apenas aos testes e à documentação, ou exigiriam um artefato externo da aplicação que não foi fornecido.

Por esse motivo, a execução dos testes foi documentada para ambiente local, garantindo aderência às regras do desafio e evitando a criação de um pipeline que falharia por ausência da aplicação.

## Observação sobre testes com falha

Alguns testes de integração falham intencionalmente, pois representam comportamentos divergentes das regras esperadas e foram mantidos como evidência automatizada dos bugs documentados.

Falhas esperadas:
- BUG-002 - API permite cadastro de categoria com finalidade inválida
- BUG-003 - API permite transação anterior ao nascimento da pessoa
- OBS-001 - API permite transação com data futura

## Bugs encontrados e regras impactadas

### `BUG-001` - Receita para menor de idade retorna erro interno

Regra esperada: uma pessoa menor de idade não deve possuir transação do tipo receita.

Comportamento encontrado: a API bloqueia a operação, porém retorna `500 Internal Server Error` em vez de retornar um erro de validação adequado, como `400 Bad Request`.

Impacto: a regra de negócio é aplicada, mas a resposta HTTP não comunica corretamente o erro ao consumidor da API.

---

### `BUG-002` - Categoria aceita finalidade inválida

Regra esperada: a API deve aceitar apenas finalidades conhecidas para categoria.

Valores válidos identificados:

- `0` = Despesa;
- `1` = Receita;
- `2` = Ambas.

Comportamento encontrado: a API permite cadastrar categoria com `finalidade = 3`, retornando `201 Created`.

Impacto: permite persistência de dados inválidos e pode comprometer regras futuras relacionadas à classificação de categorias.

---

### `BUG-003` - Transação anterior ao nascimento da pessoa

Regra esperada: uma pessoa não deveria possuir registros financeiros anteriores à sua data de nascimento.

Comportamento encontrado: a API permite cadastrar transação com data anterior ao nascimento da pessoa, retornando `201 Created`.

Impacto: gera inconsistência cronológica nos dados financeiros.

---

### `OBS-001` - Transação com data futura

Hipótese avaliada: como o sistema foi interpretado como controle de gastos residenciais, foi considerado que as transações representariam eventos financeiros já realizados.

Comportamento encontrado: a API permite cadastrar transação com data futura, retornando `201 Created`.

Classificação: foi documentado como observação, e não como bug crítico, porque a regra de data futura não está explicitamente descrita no escopo funcional. Dependendo da proposta do produto, transações futuras poderiam representar previsão, planejamento ou agendamento financeiro.

---

## Observações finais

A estratégia de testes foi construída com foco nas regras de negócio principais do sistema de controle financeiro, priorizando cenários de maior impacto para o domínio da aplicação.

Os testes de integração foram concentrados na API por permitirem validar diretamente regras como menor de idade sem receita, compatibilidade entre categoria e tipo de transação, totais por pessoa e exclusão em cascata.

O teste E2E foi utilizado para validar o fluxo principal pela interface web, garantindo que as ações realizadas pelo usuário reflitam corretamente no relatório de totais por pessoa.

Os testes unitários e a configuração de CI com GitHub Actions não foram implementados por limitações relacionadas ao escopo do desafio: o código-fonte da aplicação original não deveria ser alterado nem versionado neste repositório, e os testes dependem da aplicação em execução localmente.

Além dos testes automatizados, foram documentados achados manuais exploratórios relacionados à usabilidade, responsividade, consistência visual e validações de formulário.

As falhas encontradas foram documentadas em arquivos `.md`, sem qualquer alteração no código da aplicação, mantendo o repositório restrito aos artefatos de teste e documentação.