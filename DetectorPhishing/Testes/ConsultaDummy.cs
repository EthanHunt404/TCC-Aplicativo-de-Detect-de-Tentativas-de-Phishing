using DetectorPhishing.Domain;

namespace DetectorPhishing.Testes;

/// <summary>
/// Implementacao provisoria de IConsulta com dados fixos, so para
/// exercitar o EmailAnalist enquanto o Capturador e o MongoDBInterface
/// reais nao existem.
/// </summary>
public class ConsultaDummy : IConsulta
{
    private static readonly string[] DominiosEncurtamentoConhecidos = { "bit.ly", "tinyurl.com", "goo.gl" };

    public string[] ConsultarDominiosEncurtamento(string hyperlink)
    {
        if (Uri.TryCreate(hyperlink, UriKind.Absolute, out var uri))
        {
            return DominiosEncurtamentoConhecidos
                .Where(d => uri.Host.Equals(d, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        return Array.Empty<string>();
    }

    public string[] IdentificarEmpresa(string conteudo)
    {
        if (conteudo.Contains("Banco Exemplo", StringComparison.OrdinalIgnoreCase))
            return new[] { "Banco Exemplo" };

        return Array.Empty<string>();
    }

    public string ConsultarTemplateEmpresa(string empresa)
    {
        return empresa switch
        {
            "Banco Exemplo" => "banco-exemplo.com.br",
            _ => string.Empty
        };
    }
}
