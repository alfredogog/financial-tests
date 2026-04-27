import { expect, Page } from '@playwright/test';
import { BasePage } from './BasePage';

export class TransacoesPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async criarReceita(
    descricao: string,
    valor: string,
    pessoaNome: string,
    categoriaDescricao: string
  ) {
    await this.criarTransacao({
      descricao,
      valor,
      tipo: 'Receita',
      pessoaNome,
      categoriaDescricao,
    });
  }

  async criarDespesa(
    descricao: string,
    valor: string,
    pessoaNome: string,
    categoriaDescricao: string
  ) {
    await this.criarTransacao({
      descricao,
      valor,
      tipo: 'Despesa',
      pessoaNome,
      categoriaDescricao,
    });
  }

  private async criarTransacao(dados: {
    descricao: string;
    valor: string;
    tipo: 'Receita' | 'Despesa';
    pessoaNome: string;
    categoriaDescricao: string;
  }) {
    await this.irParaTransacoes();

    await expect(this.page).toHaveURL(/\/transacoes/);

    await this.page
      .getByRole('button', { name: /adicionar transação|nova transação|criar transação/i })
      .click();

    await this.page.getByLabel(/descrição|descricao/i).fill(dados.descricao);
    await this.page.getByLabel(/valor/i).fill(dados.valor);

    await this.preencherDataAtual();

    await this.page.getByLabel(/tipo/i).selectOption({ label: dados.tipo });

    await this.selecionarPessoa(dados.pessoaNome);
    await this.selecionarCategoria(dados.categoriaDescricao);

    await this.page.getByRole('button', { name: /salvar|cadastrar|criar/i }).click();

    await expect(this.page.getByText(dados.descricao)).toBeVisible();
  }

  private async preencherDataAtual() {
    const hoje = new Date();

    const ano = hoje.getFullYear();
    const mes = String(hoje.getMonth() + 1).padStart(2, '0');
    const dia = String(hoje.getDate()).padStart(2, '0');

    await this.page.getByLabel(/data/i).fill(`${ano}-${mes}-${dia}`);
  }

  private async selecionarPessoa(nome: string) {
    await this.page.locator('#pessoa-select').fill(nome);

    await this.page
      .locator('#pessoa-select-options')
      .getByText(nome, { exact: true })
      .click();
  }

  private async selecionarCategoria(descricao: string) {
    await this.page.locator('#categoria-select').fill(descricao);

    await this.page
      .locator('#categoria-select-options')
      .getByText(descricao, { exact: true })
      .click();
  }
}