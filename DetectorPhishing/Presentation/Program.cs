using System.Reflection;
using System.Text;
using System.Threading;

namespace DetectorPhishing.Presentation;

public static class Program
{
    private const string NamespaceTestes = "DetectorPhishing.Testes";
    private const string NomeMutexInstancia = "PhishingDetector.SingleInstance";

    public static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--testes")
            return ExecutarTestes();

        if (args.Length == 1 && args[0] == "--demo-notificacao")
            return ExecutarAplicativo(demonstrarNotificacao: true);

        if (args.Length == 0)
            return ExecutarAplicativo(demonstrarNotificacao: false);

        Console.Error.WriteLine("Argumento inválido. Use --testes ou --demo-notificacao, ou execute sem argumentos.");
        return 2;
    }

    private static int ExecutarTestes()
    {
        var testes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(tipo => tipo.Namespace == NamespaceTestes)
            .SelectMany(tipo => tipo.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(metodo => metodo.GetCustomAttributes()
                .Any(atributo => atributo.GetType().FullName == "Xunit.FactAttribute"))
            .OrderBy(metodo => metodo.MetadataToken)
            .ToList();

        var falhas = 0;

        foreach (var teste in testes)
        {
            try
            {
                teste.Invoke(null, null);
                Console.WriteLine($"[OK]    {teste.Name}");
            }
            catch (TargetInvocationException ex)
            {
                falhas++;
                Console.WriteLine($"[FALHA] {teste.Name}");
                Console.WriteLine($"        {ex.InnerException?.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{testes.Count - falhas} de {testes.Count} testes passaram.");

        return falhas == 0 ? 0 : 1;
    }

    private static int ExecutarAplicativo(bool demonstrarNotificacao)
    {
        using var mutex = new Mutex(true, NomeMutexInstancia, out var primeiraInstancia);
        if (!primeiraInstancia)
            return 0;

        using var iconeBandeja = new IconeBandeja();
        if (demonstrarNotificacao)
        {
            var caminhoRelatorio = CriarRelatorioDeDemonstracao();
            iconeBandeja.Notificador.Notificar(caminhoRelatorio);
        }

        System.Windows.Forms.Application.Run();
        return 0;
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
