using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Transacoes;

public class TransacoesDateRulesTests : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:5000/api/v1";

    private const int TipoDespesa = 0;
    private const int CategoriaDespesa = 0;

    private readonly HttpClient _client = new();
    private readonly List<Guid> _pessoasCriadas = [];

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var pessoaId in _pessoasCriadas)
        {
            await _client.DeleteAsync($"{BaseUrl}/Pessoas/{pessoaId}");
        }

        _client.Dispose();
    }

    [Fact(DisplayName = "Regra de negócio: não deve permitir transação com data futura")]
    public async Task Nao_Deve_Permitir_Transacao_Com_Data_Futura()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-30));
        var categoria = await CriarCategoria(CategoriaDespesa);

        var response = await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            data: DateTime.Today.AddDays(1),
            descricao: "Transação com data futura"
        );

        AssertRegraNegocioBloqueada(response);
    }

    [Fact(DisplayName = "Regra de negócio: não deve permitir transação antes do nascimento da pessoa")]
    public async Task Nao_Deve_Permitir_Transacao_Antes_Do_Nascimento_Da_Pessoa()
    {
        var dataNascimento = DateTime.Today.AddYears(-20);

        var pessoa = await CriarPessoa(dataNascimento);
        var categoria = await CriarCategoria(CategoriaDespesa);

        var response = await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            data: dataNascimento.AddDays(-1),
            descricao: "Transação antes do nascimento da pessoa"
        );

        AssertRegraNegocioBloqueada(response);
    }

    private static void AssertRegraNegocioBloqueada(HttpResponseMessage response)
    {
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError,
            $"Esperado erro de validação, mas retornou {response.StatusCode}"
        );
    }

    private async Task<PessoaResponse> CriarPessoa(DateTime dataNascimento)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Pessoas", new
        {
            nome = $"Pessoa Teste {Guid.NewGuid()}",
            dataNascimento
        });

        response.EnsureSuccessStatusCode();

        var pessoa = (await response.Content.ReadFromJsonAsync<PessoaResponse>())!;

        _pessoasCriadas.Add(pessoa.Id);

        return pessoa;
    }

    private async Task<CategoriaResponse> CriarCategoria(int finalidade)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Categorias", new
        {
            descricao = $"Categoria Teste {Guid.NewGuid()}",
            finalidade
        });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CategoriaResponse>())!;
    }

    private async Task<HttpResponseMessage> CriarTransacao(
        int tipo,
        Guid categoriaId,
        Guid pessoaId,
        DateTime data,
        string descricao)
    {
        return await _client.PostAsJsonAsync($"{BaseUrl}/Transacoes", new
        {
            descricao,
            valor = 100,
            tipo,
            categoriaId,
            pessoaId,
            data
        });
    }

    private record PessoaResponse(Guid Id);

    private record CategoriaResponse(Guid Id);
}