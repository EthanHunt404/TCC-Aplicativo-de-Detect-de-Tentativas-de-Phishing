namespace DetectorPhishing.Domain;

/// <summary>
/// Implementa o algoritmo de analise descrito no Documento de Arquitetura,
/// secao 2. Depende apenas de IConsulta - nunca da implementacao solida
/// (MongoDBInterface).
/// </summary>
public class EmailAnalist
{
    private readonly IConsulta _consulta;

    public EmailAnalist(IConsulta consulta)
    {
        _consulta = consulta ?? throw new ArgumentNullException(nameof(consulta));
    }

    /// <summary>
    /// Retorna o relatorio de inconsistencias caso o e-mail seja
    /// considerado suspeito, ou null caso nao haja nada a reportar
    /// (conteudo nulo, sem hyperlinks, ou nenhuma inconsistencia encontrada).
    /// </summary>
    public RelatorioInconsistencias? Analisar(ConteudoRelevante conteudo)
    {
        if (conteudo == null || !conteudo.ContemHyperlink())
            return null;

        var inconsistencias = new List<Inconsistencia>();

        VerificarEncurtamento(conteudo, inconsistencias);
        VerificarDominiosOficiais(conteudo, inconsistencias);

        return inconsistencias.Count > 0
            ? new RelatorioInconsistencias(conteudo.ConteudoBruto, inconsistencias, DateTime.Now)
            : null;
    }

    private void VerificarEncurtamento(ConteudoRelevante conteudo, List<Inconsistencia> inconsistencias)
    {
        foreach (var hyperlink in conteudo.Hyperlinks)
        {
            var padroes = _consulta.ConsultarDominiosEncurtamento(hyperlink);
            if (padroes.Length > 0)
            {
                inconsistencias.Add(new Inconsistencia(
                    "LinkEncurtado",
                    $"Hyperlink '{hyperlink}' corresponde a um padrao de encurtamento conhecido ({string.Join(", ", padroes)})."));
            }
        }
    }

    private void VerificarDominiosOficiais(ConteudoRelevante conteudo, List<Inconsistencia> inconsistencias)
    {
        var empresa = _consulta.IdentificarEmpresa(conteudo.ConteudoBruto).FirstOrDefault();
        if (empresa == null)
            return; // empresa nao identificada: nao ha template para comparar

        var dominioOficial = _consulta.ConsultarTemplateEmpresa(empresa);
        if (string.IsNullOrWhiteSpace(dominioOficial))
            return; // empresa identificada, mas sem template registrado

        var dominioRemetente = ExtrairDominio(conteudo.EnderecoRemetente);
        if (!DominioCorresponde(dominioRemetente, dominioOficial))
        {
            inconsistencias.Add(new Inconsistencia(
                "EnderecoRemetenteDivergente",
                $"Endereco do remetente '{conteudo.EnderecoRemetente}' nao pertence ao dominio oficial '{dominioOficial}'."));
        }

        foreach (var hyperlink in conteudo.Hyperlinks)
        {
            var dominioLink = ExtrairDominio(hyperlink);
            if (!DominioCorresponde(dominioLink, dominioOficial))
            {
                inconsistencias.Add(new Inconsistencia(
                    "HyperlinkDivergente",
                    $"Hyperlink '{hyperlink}' nao pertence ao dominio oficial '{dominioOficial}'."));
            }
        }
    }

    private static string ExtrairDominio(string enderecoOuLink)
    {
        if (string.IsNullOrWhiteSpace(enderecoOuLink))
            return string.Empty;

        if (enderecoOuLink.Contains('@'))
            return enderecoOuLink.Split('@').Last().Trim().ToLowerInvariant();

        if (Uri.TryCreate(enderecoOuLink, UriKind.Absolute, out var uri))
            return uri.Host.ToLowerInvariant();

        return enderecoOuLink.Trim().ToLowerInvariant();
    }

    private static bool DominioCorresponde(string dominio, string dominioOficial)
    {
        if (string.IsNullOrWhiteSpace(dominio) || string.IsNullOrWhiteSpace(dominioOficial))
            return false;

        var oficial = dominioOficial.Trim().ToLowerInvariant();
        return dominio == oficial || dominio.EndsWith("." + oficial);
    }
}
