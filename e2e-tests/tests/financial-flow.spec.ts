import { test } from '@playwright/test';
import { testData } from '../fixtures/testData';
import { BasePage } from '../pages/BasePage';
import { PessoasPage } from '../pages/PessoasPage';
import { CategoriasPage } from '../pages/CategoriasPage';
import { TransacoesPage } from '../pages/TransacoesPage';
import { RelatoriosPage } from '../pages/RelatoriosPage';

test.describe('Fluxo financeiro E2E', () => {
  test('E2E-001 - Deve criar dados financeiros e validar totais por pessoa', async ({ page }) => {
    const basePage = new BasePage(page);
    const pessoasPage = new PessoasPage(page);
    const categoriasPage = new CategoriasPage(page);
    const transacoesPage = new TransacoesPage(page);
    const relatoriosPage = new RelatoriosPage(page);

    await basePage.abrirAplicacao();

    await pessoasPage.criarPessoa(
      testData.pessoa.nome,
      testData.pessoa.dataNascimento
    );

    await categoriasPage.criarCategoriaReceita(
      testData.categorias.receita.descricao
    );

    await categoriasPage.criarCategoriaDespesa(
      testData.categorias.despesa.descricao
    );

    await transacoesPage.criarReceita(
      testData.transacoes.receita.descricao,
      testData.transacoes.receita.valor,
      testData.pessoa.nome,
      testData.categorias.receita.descricao
    );

    await transacoesPage.criarDespesa(
      testData.transacoes.despesa.descricao,
      testData.transacoes.despesa.valor,
      testData.pessoa.nome,
      testData.categorias.despesa.descricao
    );

    await relatoriosPage.validarTotaisPorPessoa({
      pessoaNome: testData.pessoa.nome,
      totalReceitas: testData.totais.totalReceitas,
      totalDespesas: testData.totais.totalDespesas,
      saldo: testData.totais.saldo,
    });
  });
});