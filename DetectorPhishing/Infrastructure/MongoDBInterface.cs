using System.Text.RegularExpressions;
using DetectorPhishing.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DetectorPhishing.Infrastructure;

public sealed class MongoDBInterface : IConsulta
{
    private static readonly Regex Separadores = new("[^\\p{L}\\p{Nd}]+", RegexOptions.Compiled);

    private readonly HashSet<string> _dominiosEncurtamento = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _padroesEncurtamento = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _empresas = new(StringComparer.OrdinalIgnoreCase);
    private readonly IMongoClient _cliente;
    private readonly IMongoCollection<BsonDocument> _templates;

    public MongoDBInterface(ConfiguracaoAplicacao configuracao)
    {
        ArgumentNullException.ThrowIfNull(configuracao);
        _cliente = new MongoClient(configuracao.MongoUri);
        var banco = _cliente.GetDatabase(configuracao.MongoDatabase);
        var colecaoEncurtadores = banco.GetCollection<BsonDocument>("shorteners");
        var colecaoEmpresas = banco.GetCollection<BsonDocument>("companies");
        _templates = banco.GetCollection<BsonDocument>("templates");

        CarregarEncurtadores(colecaoEncurtadores);
        CarregarEmpresas(colecaoEmpresas);
    }

    public string[] ConsultarDominiosEncurtamento(string hyperlink)
    {
        if (!Uri.TryCreate(hyperlink, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Array.Empty<string>();
        }

        return _dominiosEncurtamento.Contains(uri.Host)
            ? _padroesEncurtamento[uri.Host]
            : Array.Empty<string>();
    }

    public string[] IdentificarEmpresa(string conteudo)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var palavras = Separadores.Split(conteudo)
            .Where(palavra => palavra.Length > 0)
            .ToArray();

        for (var inicio = 0; inicio < palavras.Length; inicio++)
        {
            for (var tamanho = 1; tamanho <= 3 && inicio + tamanho <= palavras.Length; tamanho++)
            {
                var candidato = string.Join(" ", palavras, inicio, tamanho);
                if (_empresas.TryGetValue(candidato, out var grafiaIndex))
                    return new[] { grafiaIndex };
            }
        }

        return new[] { "nenhum" };
    }

    public string ConsultarTemplateEmpresa(string empresa)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(empresa);
        var template = _templates.Find(Builders<BsonDocument>.Filter.Eq("company", empresa)).FirstOrDefault();
        if (template == null)
            return string.Empty;

        if (!template.TryGetValue("official_domains", out var dominios)
            || !dominios.IsBsonArray
            || dominios.AsBsonArray.Count == 0)
        {
            throw new InvalidDataException($"Template da empresa '{empresa}' não possui domínios oficiais válidos.");
        }

        var dominioPrincipal = dominios.AsBsonArray[0];
        if (!dominioPrincipal.IsString)
            throw new InvalidDataException($"Template da empresa '{empresa}' contém domínio oficial inválido.");

        return dominioPrincipal.AsString;
    }

    private void CarregarEncurtadores(IMongoCollection<BsonDocument> colecao)
    {
        var documento = colecao.Find(Builders<BsonDocument>.Filter.Empty).FirstOrDefault()
            ?? throw new InvalidDataException("A coleção shorteners não contém o documento esperado.");

        if (!documento.TryGetValue("list", out var lista) || !lista.IsBsonArray)
            throw new InvalidDataException("O documento shorteners não contém uma lista válida.");

        foreach (var item in lista.AsBsonArray)
        {
            if (!item.IsBsonDocument)
                throw new InvalidDataException("A lista shorteners contém um registro inválido.");

            var registro = item.AsBsonDocument;
            var dominio = LerTextoObrigatorio(registro, "domain");
            if (!registro.TryGetValue("patterns", out var padroes) || !padroes.IsBsonArray)
                throw new InvalidDataException($"O domínio '{dominio}' não possui patterns válidos.");

            var padroesTexto = padroes.AsBsonArray
                .Select(padrao => padrao.IsString
                    ? padrao.AsString
                    : throw new InvalidDataException($"O domínio '{dominio}' contém pattern inválido."))
                .ToArray();

            if (!_dominiosEncurtamento.Add(dominio))
                throw new InvalidDataException($"Domínio de encurtamento duplicado: '{dominio}'.");

            _padroesEncurtamento.Add(dominio, padroesTexto);
        }
    }

    private void CarregarEmpresas(IMongoCollection<BsonDocument> colecao)
    {
        var documento = colecao.Find(Builders<BsonDocument>.Filter.Empty).FirstOrDefault()
            ?? throw new InvalidDataException("A coleção companies não contém o documento esperado.");

        if (!documento.TryGetValue("companies", out var empresas) || !empresas.IsBsonArray)
            throw new InvalidDataException("O documento companies não contém uma lista válida.");

        foreach (var empresa in empresas.AsBsonArray)
        {
            if (!empresa.IsString || string.IsNullOrWhiteSpace(empresa.AsString))
                throw new InvalidDataException("O index de empresas contém um nome inválido.");

            if (!_empresas.TryAdd(empresa.AsString, empresa.AsString))
                throw new InvalidDataException($"Empresa duplicada no index: '{empresa.AsString}'.");
        }
    }

    private static string LerTextoObrigatorio(BsonDocument documento, string campo)
    {
        if (!documento.TryGetValue(campo, out var valor) || !valor.IsString || string.IsNullOrWhiteSpace(valor.AsString))
            throw new InvalidDataException($"O registro não contém o campo '{campo}' válido.");

        return valor.AsString;
    }
}
