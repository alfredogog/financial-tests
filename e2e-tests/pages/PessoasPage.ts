import { expect, Page } from '@playwright/test';
import { BasePage } from './BasePage';

export class PessoasPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async criarPessoa(nome: string, dataNascimento: string) {
    await this.irParaPessoas();

    await expect(this.page).toHaveURL(/\/pessoas/);

    await this.page.getByRole('button', { name: /nova pessoa|adicionar pessoa|criar pessoa/i }).click();

    await this.page.getByLabel(/nome/i).fill(nome);
    await this.page.getByLabel(/data de nascimento|nascimento/i).fill(dataNascimento);

    await this.page.getByRole('button', { name: /salvar|cadastrar|criar/i }).click();

    await expect(this.page.getByText(nome)).toBeVisible();
  }
}