using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Transacoes;

public class TransacoesBusinessRulesTests : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:5000/api/v1";

    private const int TipoDespesa = 0;
    private const int TipoReceita = 1;

    private const int CategoriaDespesa = 0;
    private const int CategoriaReceita = 1;
    private const int CategoriaAmbos = 2;

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

    [Fact(DisplayName = "Regra de negócio: menor não pode ter receita")]
    public async Task Nao_Deve_Permitir_Receita_Para_Pessoa_Menor_De_Idade()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-16));
        var categoria = await CriarCategoria(CategoriaReceita);

        var response = await CriarTransacao(
            tipo: TipoReceita,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            descricao: "Receita indevida para menor de idade"
        );

        AssertRegraNegocioBloqueada(response);
    }

    [Fact(DisplayName = "Regra de negócio: não deve permitir receita em categoria de despesa")]
    public async Task Nao_Deve_Permitir_Receita_Em_Categoria_De_Despesa()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-30));
        var categoria = await CriarCategoria(CategoriaDespesa);

        var response = await CriarTransacao(
            tipo: TipoReceita,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            descricao: "Receita usando categoria de despesa"
        );

        AssertRegraNegocioBloqueada(response);
    }

    [Fact(DisplayName = "Regra de negócio: não deve permitir despesa em categoria de receita")]
    public async Task Nao_Deve_Permitir_Despesa_Em_Categoria_De_Receita()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-30));
        var categoria = await CriarCategoria(CategoriaReceita);

        var response = await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            descricao: "Despesa usando categoria de receita"
        );

        AssertRegraNegocioBloqueada(response);
    }

    [Fact(DisplayName = "Regra de negócio: deve permitir receita em categoria ambos")]
    public async Task Deve_Permitir_Receita_Em_Categoria_Ambos()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-30));
        var categoria = await CriarCategoria(CategoriaAmbos);

        var response = await CriarTransacao(
            tipo: TipoReceita,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            descricao: "Receita válida em categoria ambos"
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "Regra de negócio: deve permitir despesa em categoria ambos")]
    public async Task Deve_Permitir_Despesa_Em_Categoria_Ambos()
    {
        var pessoa = await CriarPessoa(DateTime.Today.AddYears(-30));
        var categoria = await CriarCategoria(CategoriaAmbos);

        var response = await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoria.Id,
            pessoaId: pessoa.Id,
            descricao: "Despesa válida em categoria ambos"
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
        string descricao)
    {
        return await _client.PostAsJsonAsync($"{BaseUrl}/Transacoes", new
        {
            descricao,
            valor = 100,
            tipo,
            categoriaId,
            pessoaId,
            data = DateTime.Today
        });
    }

    private record PessoaResponse(Guid Id);

    private record CategoriaResponse(Guid Id);
}