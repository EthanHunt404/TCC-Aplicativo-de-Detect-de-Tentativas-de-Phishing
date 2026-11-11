using DetectorPhishing.Domain;

namespace DetectorPhishing.Testes;

/// <summary>
/// Fake de IConsulta com retornos configuraveis por teste - permite
/// exercitar cada cenario do EmailAnalist sem MongoDB nenhum.
/// </summary>
public class ConsultaFake : IConsulta
{
    public string[] DominiosEncurtamento { get; set; } = Array.Empty<string>();
    public string[] EmpresasIdentificadas { get; set; } = Array.Empty<string>();
    public string DominioOficial { get; set; } = string.Empty;

    public string[] ConsultarDominiosEncurtamento(string hyperlink) => DominiosEncurtamento;

    public string[] IdentificarEmpresa(string conteudo) => EmpresasIdentificadas;

    public string ConsultarTemplateEmpresa(string empresa) => DominioOficial;
}
