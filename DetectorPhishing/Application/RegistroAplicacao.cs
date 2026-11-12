using System.Globalization;

namespace DetectorPhishing.Application;

public static class RegistroAplicacao
{
    public static void RegistrarEvento(string uid, string etapa, long tempoMilissegundos, string resultado)
    {
        var diretorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PhishingDetector",
            "logs");
        Directory.CreateDirectory(diretorio);

        var caminho = Path.Combine(diretorio, "app.log");
        var linha = string.Join(" | ",
            $"UID: {uid}",
            $"etapa: {etapa}",
            $"tempo_ms: {tempoMilissegundos.ToString(CultureInfo.InvariantCulture)}",
            $"resultado: {resultado}");

        File.AppendAllText(caminho, linha + Environment.NewLine);
    }
}
