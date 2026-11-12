using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using DetectorPhishing.Application;
using DetectorPhishing.Domain;
using DetectorPhishing.Infrastructure;

namespace DetectorPhishing.Presentation;

public static class Program
{
    private const string NamespaceTestes = "DetectorPhishing.Testes";
    private const string NomeMutexInstancia = "PhishingDetector.SingleInstance";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--testes")
            return await ExecutarTestes(integracao: false);

        if (args.Length == 1 && args[0] == "--testes-integracao")
            return await ExecutarTestes(integracao: true);

        if (args.Length == 1 && args[0] == "--demo-notificacao")
            return await ExecutarAplicativo(demonstrarNotificacao: true, injetarAmostras: false);

        if (args.Length == 1 && args[0] == "--injetar-amostras")
            return await ExecutarAplicativo(demonstrarNotificacao: false, injetarAmostras: true);

        if (args.Length == 0)
            return await ExecutarAplicativo(demonstrarNotificacao: false, injetarAmostras: false);

        Console.Error.WriteLine("Argumento inválido. Use --testes, --testes-integracao, --injetar-amostras ou --demo-notificacao, ou execute sem argumentos.");
        return 2;
    }

    private static async Task<int> ExecutarTestes(bool integracao)
    {
        var testes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(tipo => tipo.Namespace == NamespaceTestes)
            .SelectMany(tipo => tipo.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(metodo => metodo.GetCustomAttributes()
                .Any(atributo => atributo.GetType().FullName == "Xunit.FactAttribute"))
            .Where(metodo => metodo.Name.StartsWith("Integracao_", StringComparison.Ordinal) == integracao)
            .OrderBy(metodo => metodo.MetadataToken)
            .ToList();

        if (testes.Count == 0)
        {
            Console.Error.WriteLine(integracao
                ? "Nenhum teste de integração foi encontrado."
                : "Nenhum teste local foi encontrado.");
            return 1;
        }

        var falhas = 0;

        foreach (var teste in testes)
        {
            try
            {
                var resultado = teste.Invoke(null, null);
                if (resultado is Task tarefa)
                    await tarefa;

                Console.WriteLine($"[OK]    {teste.Name}");
            }
            catch (Exception ex)
            {
                falhas++;
                var erro = ex is TargetInvocationException { InnerException: { } innerException }
                    ? innerException
                    : ex;
                Console.WriteLine($"[FALHA] {teste.Name}");
                Console.WriteLine($"        {erro.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{testes.Count - falhas} de {testes.Count} testes passaram.");

        return falhas == 0 ? 0 : 1;
    }

    private static async Task<int> ExecutarAplicativo(bool demonstrarNotificacao, bool injetarAmostras)
    {
        using var mutex = new Mutex(true, NomeMutexInstancia, out var primeiraInstancia);
        if (!primeiraInstancia)
            return 0;

        using var iconeBandeja = new IconeBandeja();
        if (demonstrarNotificacao)
        {
            var caminhoRelatorio = CriarRelatorioDeDemonstracao();
            iconeBandeja.Notificador.Notificar(caminhoRelatorio);
            System.Windows.Forms.Application.Run();
            return 0;
        }

        var configuracao = ConfiguracaoAplicacao.Carregar();
        var consulta = new MongoDBInterface(configuracao);
        var analista = new EmailAnalist(consulta);
        var gravador = new GravadorRelatorioTxt(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        var canal = Channel.CreateBounded<ConteudoRelevante>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });
        using var cancelamento = new CancellationTokenSource();
        var servico = new ServicoDeAnalise(canal.Reader, analista, gravador, iconeBandeja.Notificador);
        var tarefaServico = Task.Run(() => servico.ExecutarAsync(cancelamento.Token));
        var tarefaInjecao = injetarAmostras
            ? InjetarAmostrasAsync(canal.Writer)
            : Task.CompletedTask;

        try
        {
            System.Windows.Forms.Application.Run();
        }
        finally
        {
            canal.Writer.TryComplete();
            cancelamento.Cancel();
        }

        await tarefaInjecao;
        await tarefaServico;
        return 0;
    }

    private static async Task InjetarAmostrasAsync(ChannelWriter<ConteudoRelevante> escritor)
    {
        var diretorioAmostras = Path.Combine(Directory.GetCurrentDirectory(), "base", "amostras");
        var arquivos = Directory.GetFiles(diretorioAmostras, "S-*.eml")
            .OrderBy(caminho => caminho, StringComparer.Ordinal);

        try
        {
            foreach (var arquivo in arquivos)
            {
                var mensagem = MimeKit.MimeMessage.Load(arquivo);
                var conteudo = ExtratorDeConteudo.Extrair(mensagem);
                if (conteudo != null)
                    await escritor.WriteAsync(conteudo);
            }

            escritor.TryComplete();
        }
        catch (Exception ex)
        {
            escritor.TryComplete(ex);
            throw;
        }
    }

    private static string CriarRelatorioDeDemonstracao()
    {
        var nomeArquivo = $"relatorio_demo_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt";
        var caminhoRelatorio = Path.Combine(Path.GetTempPath(), nomeArquivo);
        var conteudo = string.Join(Environment.NewLine,
            "RELATÓRIO DE INCONSISTÊNCIAS",
            $"Data de geração: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}",
            "Total de inconsistências: 1",
            "1. [Demonstração] Notificação de exemplo para verificar a bandeja.",
            "--- CONTEÚDO ANALISADO ---",
            "Esta é uma mensagem de demonstração.");

        File.WriteAllText(caminhoRelatorio, conteudo, new UTF8Encoding(false));
        return caminhoRelatorio;
    }
}
