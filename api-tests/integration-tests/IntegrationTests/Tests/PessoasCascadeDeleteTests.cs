using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Pessoas;

public class PessoasCascadeDeleteTests
{
    private const string BaseUrl = "http://localhost:5000/api/v1";

    private const int TipoDespesa = 0;
    private const int CategoriaDespesa = 0;

    private readonly HttpClient _client = new();
    private readonly ITestOutputHelper _output;

    public PessoasCascadeDeleteTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "Pessoa: deve excluir transações em cascata ao remover pessoa")]
    public async Task Deve_Excluir_Transacoes_Em_Cascata_Ao_Remover_Pessoa()
    {
        var pessoa = await CriarPessoa();
        var categoria = await CriarCategoria();
        var transacao = await CriarTransacao(pessoa.Id, categoria.Id);

        var transacaoAntesDaExclusao = await BuscarTransacaoPorId(transacao.Id);

        Assert.NotNull(transacaoAntesDaExclusao);
        Assert.Equal(transacao.Id, transacaoAntesDaExclusao!.Id);

        var deletePessoaResponse = await _client.DeleteAsync($"{BaseUrl}/Pessoas/{pessoa.Id}");

        _output.WriteLine($"DELETE /Pessoas/{pessoa.Id}: {deletePessoaResponse.StatusCode}");

        Assert.Equal(HttpStatusCode.NoContent, deletePessoaResponse.StatusCode);

        var transacaoDepoisDaExclusaoResponse = await _client.GetAsync($"{BaseUrl}/Transacoes/{transacao.Id}");

        _output.WriteLine($"GET /Transacoes/{transacao.Id} após deletar pessoa: {transacaoDepoisDaExclusaoResponse.StatusCode}");

        Assert.True(
            transacaoDepoisDaExclusaoResponse.StatusCode == HttpStatusCode.NotFound ||
            transacaoDepoisDaExclusaoResponse.StatusCode == HttpStatusCode.NoContent,
            $"Esperado que a transação vinculada à pessoa excluída não existisse mais, mas retornou {transacaoDepoisDaExclusaoResponse.StatusCode}"
        );
    }

    private async Task<PessoaResponse> CriarPessoa()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Pessoas", new
        {
            nome = $"Pessoa Cascade {Guid.NewGuid()}",
            dataNascimento = DateTime.Today.AddYears(-30)
        });

        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Pessoas =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(body);
        _output.WriteLine("=============================");

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PessoaResponse>(body, JsonOptions())!;
    }

    private async Task<CategoriaResponse> CriarCategoria()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Categorias", new
        {
            descricao = $"Categoria Cascade {Guid.NewGuid()}",
            finalidade = CategoriaDespesa
        });

        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Categorias =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(body);
        _output.WriteLine("================================");

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<CategoriaResponse>(body, JsonOptions())!;
    }

    private async Task<TransacaoResponse> CriarTransacao(Guid pessoaId, Guid categoriaId)
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/Transacoes", new
        {
            descricao = $"Despesa Cascade {Guid.NewGuid()}",
            valor = 150,
            tipo = TipoDespesa,
            categoriaId,
            pessoaId,
            data = DateTime.Today
        });

        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine("===== RESPONSE /Transacoes =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(body);
        _output.WriteLine("================================");

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TransacaoResponse>(body, JsonOptions())!;
    }

    private async Task<TransacaoResponse?> BuscarTransacaoPorId(Guid transacaoId)
    {
        var response = await _client.GetAsync($"{BaseUrl}/Transacoes/{transacaoId}");

        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"===== RESPONSE GET /Transacoes/{transacaoId} =====");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(body);
        _output.WriteLine("==================================================");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return JsonSerializer.Deserialize<TransacaoResponse>(body, JsonOptions());
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

    private record TransacaoResponse(Guid Id);
}