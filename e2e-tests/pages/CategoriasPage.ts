import { expect, Page } from '@playwright/test';
import { BasePage } from './BasePage';

export class CategoriasPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async criarCategoriaReceita(descricao: string) {
    await this.criarCategoria(descricao, 'Receita');
  }

  async criarCategoriaDespesa(descricao: string) {
    await this.criarCategoria(descricao, 'Despesa');
  }

  private async criarCategoria(descricao: string, finalidade: 'Receita' | 'Despesa') {
    await this.irParaCategorias();

    await expect(this.page).toHaveURL(/\/categorias/);

    await this.page
      .getByRole('button', { name: /nova categoria|adicionar categoria|criar categoria/i })
      .click();

    await this.page.getByLabel(/descrição|descricao|nome/i).fill(descricao);
    await this.page.getByLabel(/finalidade|tipo/i).selectOption({ label: finalidade });

    await this.page.getByRole('button', { name: /salvar|cadastrar|criar/i }).click();

    await this.page.waitForLoadState('networkidle').catch(() => {});
    await this.page.reload();

    await expect(this.page).toHaveURL(/\/categorias/);
  }
}