import { Page, expect } from '@playwright/test';

export class BasePage {
  protected readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async abrirAplicacao() {
    await this.page.goto('/');
    await expect(this.page.getByText('Minhas Finanças')).toBeVisible();
  }

  async irParaPessoas() {
    await this.page.goto('/pessoas');
    await expect(this.page).toHaveURL(/\/pessoas/);
  }

  async irParaCategorias() {
    await this.page.goto('/categorias');
    await expect(this.page).toHaveURL(/\/categorias/);
  }

  async irParaTransacoes() {
    await this.page.goto('/transacoes');
    await expect(this.page).toHaveURL(/\/transacoes/);
  }

  async irParaRelatorios() {
    await this.page.goto('/totais');
    await expect(this.page).toHaveURL(/\/totais/);
  }
}