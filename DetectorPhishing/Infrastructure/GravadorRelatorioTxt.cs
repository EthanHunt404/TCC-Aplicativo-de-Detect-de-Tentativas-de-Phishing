using System.Globalization;
using System.Text;
using DetectorPhishing.Application;
using DetectorPhishing.Domain;

namespace DetectorPhishing.Infrastructure;

public sealed class GravadorRelatorioTxt : IGravadorRelatorio
{
    private readonly string _diretorioDocumentos;

    public GravadorRelatorioTxt(string diretorioDocumentos)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diretorioDocumentos);
        _diretorioDocumentos = diretorioDocumentos;
    }

    public async Task<string> GravarAsync(RelatorioInconsistencias relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);

        var diretorioRelatorios = Path.Combine(_diretorioDocumentos, "Phishing Reports");
        Directory.CreateDirectory(diretorioRelatorios);

        var conteudo = Formatar(relatorio);
        var dataNomeArquivo = relatorio.DataGeracao;
        while (true)
        {
            var nomeArquivo = $"relatorio_{dataNomeArquivo.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture)}.txt";
            var caminho = Path.Combine(diretorioRelatorios, nomeArquivo);

            try
            {
                await using var arquivo = new FileStream(
                    caminho,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);
                await using var escritor = new StreamWriter(arquivo, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                await escritor.WriteAsync(conteudo);
                await escritor.FlushAsync();
                return caminho;
            }
            catch (IOException) when (File.Exists(caminho))
            {
                dataNomeArquivo = dataNomeArquivo.AddMilliseconds(1);
            }
        }
    }

    private static string Formatar(RelatorioInconsistencias relatorio)
    {
        var linhas = new List<string>
        {
            "RELATÓRIO DE INCONSISTÊNCIAS",
            $"Data de geração: {relatorio.DataGeracao.ToString("O", CultureInfo.InvariantCulture)}",
            $"Total de inconsistências: {relatorio.Inconsistencias.Count}"
        };

        for (var indice = 0; indice < relatorio.Inconsistencias.Count; indice++)
        {
            var inconsistencia = relatorio.Inconsistencias[indice];
            linhas.Add($"{indice + 1}. [{inconsistencia.Tipo}] {inconsistencia.Descricao}");
        }

        linhas.Add("--- CONTEÚDO ANALISADO ---");
        linhas.Add(relatorio.Conteudo);
        return string.Join(Environment.NewLine, linhas);
    }
}
