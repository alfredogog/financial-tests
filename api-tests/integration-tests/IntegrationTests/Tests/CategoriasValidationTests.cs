using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Config;
using Xunit;

namespace IntegrationTests.Categorias;

public class CategoriasValidationTests
{
    [Fact(DisplayName = "Regra de validação: não deve permitir finalidade de categoria inválida")]
    public async Task Nao_Deve_Permitir_Finalidade_De_Categoria_Invalida()
    {
        using var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            $"{TestConfig.ApiBaseUrl}/Categorias",
            new
            {
                descricao = $"Categoria Finalidade Inválida {Guid.NewGuid()}",
                finalidade = 3
            });

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError,
            $"Esperado erro de validação para finalidade inválida, mas retornou {response.StatusCode}"
        );
    }
}