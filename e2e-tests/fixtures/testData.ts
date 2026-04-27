export const testData = {
  pessoa: {
    nome: `Pessoa E2E ${Date.now()}`,
    dataNascimento: '1996-04-26',
  },

  categorias: {
    receita: {
      descricao: `Categoria Receita E2E ${Date.now()}`,
    },
    despesa: {
      descricao: `Categoria Despesa E2E ${Date.now()}`,
    },
  },

  transacoes: {
    receita: {
      descricao: `Receita E2E ${Date.now()}`,
      valor: '500',
    },
    despesa: {
      descricao: `Despesa E2E ${Date.now()}`,
      valor: '200',
    },
  },

  totais: {
    totalReceitas: 'R$ 500.00',
    totalDespesas: 'R$ 200.00',
    saldo: 'R$ 300.00',
  },
};