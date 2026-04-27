import { expect, Page } from '@playwright/test';
import { BasePage } from './BasePage';

export class RelatoriosPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async validarTotaisPorPessoa(dados: {
    pessoaNome: string;
    totalReceitas: string;
    totalDespesas: string;
    saldo: string;
  }) {
    await this.irParaRelatorios();

    await expect(this.page).toHaveURL(/\/totais/);
    await expect(
      this.page.getByRole('heading', { name: /totais por pessoa/i })
    ).toBeVisible();

    const linhaPessoa = this.page
      .locator('tr')
      .filter({ hasText: dados.pessoaNome });

    await expect(linhaPessoa).toBeVisible();

    await expect(linhaPessoa).toContainText(dados.totalReceitas);
    await expect(linhaPessoa).toContainText(dados.totalDespesas);
    await expect(linhaPessoa).toContainText(dados.saldo);
  }
}