using System.Text.Json;
using System.Text.Json.Serialization;

namespace DetectorPhishing.Infrastructure;

public sealed class ConfiguracaoAplicacao
{
    [JsonPropertyName("mongo_uri")]
    public string MongoUri { get; init; } = string.Empty;

    [JsonPropertyName("mongo_database")]
    public string MongoDatabase { get; init; } = string.Empty;

    public static ConfiguracaoAplicacao Carregar(string? caminho = null)
    {
        caminho ??= Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(caminho);
        var configuracao = JsonSerializer.Deserialize<ConfiguracaoAplicacao>(json)
            ?? throw new InvalidDataException("appsettings.json está vazio ou inválido.");

        if (string.IsNullOrWhiteSpace(configuracao.MongoUri))
            throw new InvalidDataException("A configuração mongo_uri é obrigatória.");

        if (string.IsNullOrWhiteSpace(configuracao.MongoDatabase))
            throw new InvalidDataException("A configuração mongo_database é obrigatória.");

        return configuracao;
    }
}
