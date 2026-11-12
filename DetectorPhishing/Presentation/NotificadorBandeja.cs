using System.Collections.Concurrent;
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
    private readonly ConcurrentQueue<string> _relatoriosPendentes = new();
    private readonly System.Windows.Forms.Timer _temporizador;
    private string? _caminhoRelatorioAtual;
    private bool _notificacaoVisivel;

    internal NotificadorBandeja(NotifyIcon icone)
    {
        _icone = icone ?? throw new ArgumentNullException(nameof(icone));
        _icone.BalloonTipClicked += AoClicarNaNotificacao;
        _icone.BalloonTipClosed += AoFecharNotificacao;

        _temporizador = new System.Windows.Forms.Timer { Interval = 250 };
        _temporizador.Tick += AoDecorrerTemporizador;
        _temporizador.Start();
    }

    public void Notificar(string caminhoRelatorio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminhoRelatorio);
        if (!File.Exists(caminhoRelatorio))
            throw new FileNotFoundException("O relatório da notificação não foi encontrado.", caminhoRelatorio);

        _relatoriosPendentes.Enqueue(caminhoRelatorio);
    }

    public void Dispose()
    {
        _temporizador.Stop();
        _temporizador.Tick -= AoDecorrerTemporizador;
        _temporizador.Dispose();
        _icone.BalloonTipClicked -= AoClicarNaNotificacao;
        _icone.BalloonTipClosed -= AoFecharNotificacao;
    }

    private void AoDecorrerTemporizador(object? sender, EventArgs e)
    {
        if (_notificacaoVisivel || !_relatoriosPendentes.TryDequeue(out var caminhoRelatorio))
            return;

        _caminhoRelatorioAtual = caminhoRelatorio;
        _notificacaoVisivel = true;
        _icone.ShowBalloonTip(5000, TituloNotificacao, TextoNotificacao, ToolTipIcon.Warning);
    }

    private void AoFecharNotificacao(object? sender, EventArgs e)
    {
        _notificacaoVisivel = false;
        _caminhoRelatorioAtual = null;
    }

    private void AoClicarNaNotificacao(object? sender, EventArgs e)
    {
        if (_caminhoRelatorioAtual == null)
            throw new InvalidOperationException("Não há relatório associado à notificação.");

        Process.Start(new ProcessStartInfo
        {
            FileName = _caminhoRelatorioAtual,
            UseShellExecute = true
        });
    }
}
