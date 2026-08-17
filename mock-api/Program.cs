using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var pessoas = new ConcurrentDictionary<Guid, Pessoa>();
var categorias = new ConcurrentDictionary<Guid, Categoria>();
var transacoes = new ConcurrentDictionary<Guid, Transacao>();

const int TipoDespesa = 0;
const int TipoReceita = 1;

const int CategoriaDespesa = 0;
const int CategoriaReceita = 1;
const int CategoriaAmbos = 2;


// ======================================================
// HEALTH CHECK
// ======================================================

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok"
    });
});


// ======================================================
// PESSOAS
// ======================================================

app.MapPost("/api/v1/Pessoas", (PessoaRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(new
        {
            message = "Nome é obrigatório."
        });
    }

    if (request.DataNascimento.Date > DateTime.Today)
    {
        return Results.BadRequest(new
        {
            message = "Data de nascimento não pode ser futura."
        });
    }

    var pessoa = new Pessoa(
        Guid.NewGuid(),
        request.Nome,
        request.DataNascimento.Date
    );

    pessoas[pessoa.Id] = pessoa;

    return Results.Created(
        $"/api/v1/Pessoas/{pessoa.Id}",
        pessoa
    );
});


app.MapDelete("/api/v1/Pessoas/{id:guid}", (Guid id) =>
{
    if (!pessoas.TryRemove(id, out _))
    {
        return Results.NotFound();
    }

    var transacoesDaPessoa = transacoes.Values
        .Where(t => t.PessoaId == id)
        .Select(t => t.Id)
        .ToList();

    foreach (var transacaoId in transacoesDaPessoa)
    {
        transacoes.TryRemove(transacaoId, out _);
    }

    return Results.NoContent();
});


// ======================================================
// CATEGORIAS
// ======================================================

app.MapPost("/api/v1/Categorias", (CategoriaRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Descricao))
    {
        return Results.BadRequest(new
        {
            message = "Descrição é obrigatória."
        });
    }

    if (
        request.Finalidade != CategoriaDespesa &&
        request.Finalidade != CategoriaReceita &&
        request.Finalidade != CategoriaAmbos
    )
    {
        return Results.BadRequest(new
        {
            message = "Finalidade da categoria inválida."
        });
    }

    var categoria = new Categoria(
        Guid.NewGuid(),
        request.Descricao,
        request.Finalidade
    );

    categorias[categoria.Id] = categoria;

    return Results.Created(
        $"/api/v1/Categorias/{categoria.Id}",
        categoria
    );
});


// ======================================================
// TRANSAÇÕES
// ======================================================

app.MapPost("/api/v1/Transacoes", (TransacaoRequest request) =>
{
    if (!pessoas.TryGetValue(request.PessoaId, out var pessoa))
    {
        return Results.BadRequest(new
        {
            message = "Pessoa não encontrada."
        });
    }

    if (!categorias.TryGetValue(request.CategoriaId, out var categoria))
    {
        return Results.BadRequest(new
        {
            message = "Categoria não encontrada."
        });
    }

    if (
        request.Tipo != TipoDespesa &&
        request.Tipo != TipoReceita
    )
    {
        return Results.BadRequest(new
        {
            message = "Tipo de transação inválido."
        });
    }


    // --------------------------------------------------
    // Regra: não permitir transações futuras
    // --------------------------------------------------

    if (request.Data.Date > DateTime.Today)
    {
        return Results.BadRequest(new
        {
            message = "Não é permitido registrar transação com data futura."
        });
    }


    // --------------------------------------------------
    // Regra: transação não pode ser anterior
    // ao nascimento da pessoa
    // --------------------------------------------------

    if (request.Data.Date < pessoa.DataNascimento.Date)
    {
        return Results.BadRequest(new
        {
            message = "A transação não pode ocorrer antes do nascimento da pessoa."
        });
    }


    // --------------------------------------------------
    // Regra: menor de idade não pode ter receita
    // --------------------------------------------------

    var idadeNaDataDaTransacao = CalcularIdade(
        pessoa.DataNascimento,
        request.Data
    );

    if (
        request.Tipo == TipoReceita &&
        idadeNaDataDaTransacao < 18
    )
    {
        return Results.BadRequest(new
        {
            message = "Pessoa menor de idade não pode possuir receita."
        });
    }


    // --------------------------------------------------
    // Regra: categoria de despesa não aceita receita
    // --------------------------------------------------

    if (
        request.Tipo == TipoReceita &&
        categoria.Finalidade == CategoriaDespesa
    )
    {
        return Results.BadRequest(new
        {
            message = "Categoria de despesa não permite receita."
        });
    }


    // --------------------------------------------------
    // Regra: categoria de receita não aceita despesa
    // --------------------------------------------------

    if (
        request.Tipo == TipoDespesa &&
        categoria.Finalidade == CategoriaReceita
    )
    {
        return Results.BadRequest(new
        {
            message = "Categoria de receita não permite despesa."
        });
    }


    var transacao = new Transacao(
        Guid.NewGuid(),
        request.Descricao,
        request.Valor,
        request.Tipo,
        request.CategoriaId,
        request.PessoaId,
        request.Data.Date
    );

    transacoes[transacao.Id] = transacao;

    return Results.Created(
        $"/api/v1/Transacoes/{transacao.Id}",
        transacao
    );
});


app.MapGet("/api/v1/Transacoes/{id:guid}", (Guid id) =>
{
    if (!transacoes.TryGetValue(id, out var transacao))
    {
        return Results.NotFound();
    }

    return Results.Ok(transacao);
});


// ======================================================
// TOTAIS POR PESSOA
// ======================================================

app.MapGet(
    "/api/v1/Totais/pessoas",
    (int page = 1, int pageSize = 100) =>
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 100;
        }

        var totais = pessoas.Values
            .Select(pessoa =>
            {
                var transacoesPessoa = transacoes.Values
                    .Where(t => t.PessoaId == pessoa.Id)
                    .ToList();

                var totalReceitas = transacoesPessoa
                    .Where(t => t.Tipo == TipoReceita)
                    .Sum(t => t.Valor);

                var totalDespesas = transacoesPessoa
                    .Where(t => t.Tipo == TipoDespesa)
                    .Sum(t => t.Valor);

                return new TotaisPorPessoaResponse(
                    pessoa.Id,
                    pessoa.Nome,
                    totalReceitas,
                    totalDespesas,
                    totalReceitas - totalDespesas
                );
            })
            .OrderBy(p => p.Nome)
            .ToList();

        var totalCount = totais.Count;

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize
            );

        var items = totais
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Results.Ok(
            new PaginacaoResponse<TotaisPorPessoaResponse>(
                items,
                totalCount,
                page,
                pageSize,
                totalPages
            )
        );
    }
);


// ======================================================
// EXECUÇÃO
// ======================================================

app.Run();


// ======================================================
// MÉTODOS AUXILIARES
// ======================================================

static int CalcularIdade(
    DateTime dataNascimento,
    DateTime dataReferencia
)
{
    var idade =
        dataReferencia.Year -
        dataNascimento.Year;

    if (
        dataNascimento.Date >
        dataReferencia.Date.AddYears(-idade)
    )
    {
        idade--;
    }

    return idade;
}


// ======================================================
// MODELOS
// ======================================================

record PessoaRequest(
    string Nome,
    DateTime DataNascimento
);

record Pessoa(
    Guid Id,
    string Nome,
    DateTime DataNascimento
);


record CategoriaRequest(
    string Descricao,
    int Finalidade
);

record Categoria(
    Guid Id,
    string Descricao,
    int Finalidade
);


record TransacaoRequest(
    string Descricao,
    decimal Valor,
    int Tipo,
    Guid CategoriaId,
    Guid PessoaId,
    DateTime Data
);

record Transacao(
    Guid Id,
    string Descricao,
    decimal Valor,
    int Tipo,
    Guid CategoriaId,
    Guid PessoaId,
    DateTime Data
);


record TotaisPorPessoaResponse(
    Guid PessoaId,
    string Nome,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);


record PaginacaoResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);