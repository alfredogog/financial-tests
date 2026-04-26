using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Totais;

public class TotaisPorPessoaTests : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:5000/api/v1";

    private const int TipoDespesa = 0;
    private const int TipoReceita = 1;

    private const int CategoriaDespesa = 0;
    private const int CategoriaReceita = 1;
    private const int CategoriaAmbos = 2;

    private readonly HttpClient _client = new();
    private readonly List<Guid> _pessoasCriadas = [];
    private readonly ITestOutputHelper _output;

    public TotaisPorPessoaTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var pessoaId in _pessoasCriadas)
        {
            var response = await _client.DeleteAsync($"{BaseUrl}/Pessoas/{pessoaId}");
            _output.WriteLine($"Cleanup pessoa {pessoaId}: {response.StatusCode}");
        }

        _client.Dispose();
    }

    [Fact(DisplayName = "Relatório: deve isolar pessoa com receita, pessoa com despesa e pessoa com ambos")]
    public async Task Deve_Isolar_Pessoa_Com_Receita_Pessoa_Com_Despesa_E_Pessoa_Com_Ambos()
    {
        var pessoaReceita = await CriarPessoa(DateTime.Today.AddYears(-30), "Pessoa Receita");
        var pessoaDespesa = await CriarPessoa(DateTime.Today.AddYears(-35), "Pessoa Despesa");
        var pessoaAmbos = await CriarPessoa(DateTime.Today.AddYears(-40), "Pessoa Ambos");

        var categoriaReceita = await CriarCategoria(CategoriaReceita, "Categoria Receita");
        var categoriaDespesa = await CriarCategoria(CategoriaDespesa, "Categoria Despesa");

        await CriarTransacao(
            tipo: TipoReceita,
            categoriaId: categoriaReceita.Id,
            pessoaId: pessoaReceita.Id,
            valor: 500,
            descricao: "Receita única da pessoa"
        );

        await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoriaDespesa.Id,
            pessoaId: pessoaDespesa.Id,
            valor: 200,
            descricao: "Despesa única da pessoa"
        );

        await CriarTransacao(
            tipo: TipoReceita,
            categoriaId: categoriaReceita.Id,
            pessoaId: pessoaAmbos.Id,
            valor: 1000,
            descricao: "Receita da pessoa com ambos"
        );

        await CriarTransacao(
            tipo: TipoDespesa,
            categoriaId: categoriaDespesa.Id,
            pessoaId: pessoaAmbos.Id,
            valor: 250,
            descricao: "Despesa da pessoa com ambos"
        );

        var totais = await ObterTotaisPorPessoa();

        var totalPessoaReceita = totais.FirstOrDefault(t => t.PessoaId == pessoaReceita.Id);
        var totalPessoaDespesa = totais.FirstOrDefault(t => t.PessoaId == pessoaDespesa.Id);
        var totalPessoaAmbos = totais.FirstOrDefault(t => t.PessoaId == pessoaAmbos.Id);

        _output.WriteLine($"Pessoa receita criada: {pessoaReceita.Id}");
        _output.WriteLine($"Pessoa despesa criada: {pessoaDespesa.Id}");
        _output.WriteLine($"Pessoa ambos criada: {pessoaAmbos.Id}");

        _output.WriteLine($"Pessoa receita encontrada no relatório: {totalPessoaReceita is not null}");
        _output.WriteLine($"Pessoa despesa encontrada no relatório: {totalPessoaDespesa is not null}");
        _output.WriteLine($"Pessoa ambos encontrada no relatório: {totalPessoaAmbos is not null}");

        Assert.NotNull(totalPessoaReceita);
        Assert.NotNull(totalPessoaDespesa);
        Assert.NotNull(totalPessoaAmbos);

        Assert.Equal(500, totalPessoaReceita!.TotalReceitas);
        Assert.Equal(0, totalPessoaReceita.TotalDespesas);
        Assert.Equal(500, totalPessoaReceita.Saldo);

        Assert.Equal(0, totalPessoaDespesa!.TotalReceitas);
        Assert.Equal(200, totalPessoaDespesa.TotalDespesas);
        Assert.Equal(-200, totalPessoaDespesa.Saldo);

        Assert.Equal(1000, totalPessoaAmbos!.TotalReceitas);
        Assert.Equal(250, totalPessoaAmbos.TotalDespesas);
        Assert.Equal(750, totalPessoaAmbos.Saldo);
    }

    private async Task<List<TotaisPorPessoaResponse>> ObterTotaisPorPessoa()
    {
        var response = await _client.GetAsync($"{BaseUrl}/Totais/pessoas?page=1&pageSize=1000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Totais/pessoas =====");
        _output.WriteLine(json);
        _output.WriteLine("====================================");

        var resultado = JsonSerializer.Deserialize<PaginacaoResponse<TotaisPorPessoaResponse>>(
            json,
            JsonOptions()
        );

        Assert.NotNull(resultado);
        Assert.NotNull(resultado!.Items);

        return resultado.Items;
    }

    private async Task<PessoaResponse> CriarPessoa(DateTime dataNascimento, string prefixoNome)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Pessoas", new
        {
            nome = $"{prefixoNome} {Guid.NewGuid()}",
            dataNascimento
        });

        var responseBody = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Pessoas =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(responseBody);
        _output.WriteLine("=============================");

        response.EnsureSuccessStatusCode();

        var pessoa = JsonSerializer.Deserialize<PessoaResponse>(
            responseBody,
            JsonOptions()
        )!;

        _pessoasCriadas.Add(pessoa.Id);

        return pessoa;
    }

    private async Task<CategoriaResponse> CriarCategoria(int finalidade, string prefixoDescricao)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Categorias", new
        {
            descricao = $"{prefixoDescricao} {Guid.NewGuid()}",
            finalidade
        });

        var responseBody = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Categorias =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(responseBody);
        _output.WriteLine("================================");

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<CategoriaResponse>(
            responseBody,
            JsonOptions()
        )!;
    }

    private async Task CriarTransacao(
        int tipo,
        Guid categoriaId,
        Guid pessoaId,
        decimal valor,
        string descricao)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Transacoes", new
        {
            descricao,
            valor,
            tipo,
            categoriaId,
            pessoaId,
            data = DateTime.Today
        });

        var responseBody = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Transacoes =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(responseBody);
        _output.WriteLine("================================");

        response.EnsureSuccessStatusCode();
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private record PessoaResponse(Guid Id);

    private record CategoriaResponse(Guid Id);

    private record PaginacaoResponse<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    private record TotaisPorPessoaResponse(
        Guid PessoaId,
        string Nome,
        decimal TotalReceitas,
        decimal TotalDespesas,
        decimal Saldo
    );
}