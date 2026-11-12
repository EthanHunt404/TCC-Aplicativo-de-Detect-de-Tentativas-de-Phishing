using System.Text.RegularExpressions;
using DetectorPhishing.Domain;
using HtmlAgilityPack;
using MimeKit;

namespace DetectorPhishing.Infrastructure;

public static class ExtratorDeConteudo
{
    private static readonly Regex UrlNoTexto = new(
        "https?://[^\\s<>\\\"']+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly char[] PontuacaoFinal = { '.', ',', ';', ':', '!', '?', ')', ']', '}' };

    public static ConteudoRelevante? Extrair(MimeMessage mensagem)
    {
        ArgumentNullException.ThrowIfNull(mensagem);

        var conteudoHtml = mensagem.HtmlBody;
        var conteudoTexto = mensagem.TextBody;
        var hyperlinks = new List<string>();
        var hyperlinksVistos = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrEmpty(conteudoHtml))
        {
            var documentoHtml = new HtmlAgilityPack.HtmlDocument();
            documentoHtml.LoadHtml(conteudoHtml);

            foreach (var ancora in documentoHtml.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
            {
                var href = ancora.GetAttributeValue("href", string.Empty);
                AdicionarUrlAbsoluta(href, hyperlinks, hyperlinksVistos);
            }
        }

        if (!string.IsNullOrEmpty(conteudoTexto))
        {
            foreach (Match correspondencia in UrlNoTexto.Matches(conteudoTexto))
            {
                var hyperlink = correspondencia.Value.TrimEnd(PontuacaoFinal);
                AdicionarUrlAbsoluta(hyperlink, hyperlinks, hyperlinksVistos);
            }
        }

        if (hyperlinks.Count == 0)
            return null;

        var remetente = mensagem.From.Mailboxes.FirstOrDefault()?.Address
            ?? throw new InvalidDataException("A mensagem com hyperlink não contém endereço de remetente.");
        var conteudoBruto = !string.IsNullOrEmpty(conteudoHtml) ? conteudoHtml : conteudoTexto ?? string.Empty;

        return new ConteudoRelevante(conteudoBruto, remetente, string.Empty, hyperlinks);
    }

    private static void AdicionarUrlAbsoluta(string valor, ICollection<string> hyperlinks, ISet<string> vistos)
    {
        if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        if (vistos.Add(valor))
            hyperlinks.Add(valor);
    }
}
