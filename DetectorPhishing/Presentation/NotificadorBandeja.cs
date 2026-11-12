using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DetectorPhishing.Application;

namespace DetectorPhishing.Presentation;

public sealed class NotificadorBandeja : INotificador, IDisposable
{
    private const string TituloNotificacao = "E-mail suspeito detectado";
    private const string TextoNotificacao = "Um e-mail foi marcado como suspeito. Clique para abrir o relatório.";

    private readonly NotifyIcon _icone;
    private string? _caminhoRelatorio;

    internal NotificadorBandeja(NotifyIcon icone)
    {
        _icone = icone ?? throw new ArgumentNullException(nameof(icone));
        _icone.BalloonTipClicked += AoClicarNaNotificacao;
    }

    public void Notificar(string caminhoRelatorio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminhoRelatorio);
        if (!File.Exists(caminhoRelatorio))
            throw new FileNotFoundException("O relatório da notificação não foi encontrado.", caminhoRelatorio);

        _caminhoRelatorio = caminhoRelatorio;
        _icone.ShowBalloonTip(5000, TituloNotificacao, TextoNotificacao, ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        _icone.BalloonTipClicked -= AoClicarNaNotificacao;
    }

    private void AoClicarNaNotificacao(object? sender, EventArgs e)
    {
        if (_caminhoRelatorio == null)
            throw new InvalidOperationException("Não há relatório associado à notificação.");

        Process.Start(new ProcessStartInfo
        {
            FileName = _caminhoRelatorio,
            UseShellExecute = true
        });
    }
}
