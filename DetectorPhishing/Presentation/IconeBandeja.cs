using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DetectorPhishing.Application;

namespace DetectorPhishing.Presentation;

public sealed class IconeBandeja : IDisposable
{
    private readonly ContextMenuStrip _menu;
    private readonly NotifyIcon _icone;
    private readonly ToolStripMenuItem _itemStatus;
    private readonly NotificadorBandeja _notificador;

    public IconeBandeja()
    {
        _itemStatus = new ToolStripMenuItem("Status: Sem autorização")
        {
            Enabled = false
        };

        var itemAutorizar = new ToolStripMenuItem("Autorizar conta de e-mail");
        itemAutorizar.Click += (_, _) => MostrarAviso("Função ainda não ligada");

        var itemAbrirRelatorios = new ToolStripMenuItem("Abrir pasta de relatórios");
        itemAbrirRelatorios.Click += (_, _) => AbrirPastaRelatorios();

        var itemSair = new ToolStripMenuItem("Sair");
        itemSair.Click += (_, _) => System.Windows.Forms.Application.ExitThread();

        _menu = new ContextMenuStrip();
        _menu.Items.AddRange(new ToolStripItem[]
        {
            _itemStatus,
            itemAutorizar,
            itemAbrirRelatorios,
            itemSair
        });

        _icone = new NotifyIcon
        {
            ContextMenuStrip = _menu,
            Icon = SystemIcons.Shield,
            Text = "Phishing Detector - Sem autorização",
            Visible = true
        };

        _notificador = new NotificadorBandeja(_icone);
    }

    public INotificador Notificador => _notificador;

    public void AtualizarStatus(StatusMonitoramento status)
    {
        var textoStatus = status switch
        {
            StatusMonitoramento.SemAutorizacao => "Sem autorização",
            StatusMonitoramento.Conectando => "Conectando",
            StatusMonitoramento.Monitorando => "Monitorando",
            StatusMonitoramento.Reconectando => "Reconectando",
            StatusMonitoramento.BaseIndisponivel => "Base indisponível",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status de monitoramento inválido.")
        };

        _itemStatus.Text = $"Status: {textoStatus}";
        _icone.Text = $"Phishing Detector - {textoStatus}";
    }

    public void Dispose()
    {
        _notificador.Dispose();
        _icone.Visible = false;
        _icone.Dispose();
        _menu.Dispose();
    }

    private void AbrirPastaRelatorios()
    {
        var documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documentos))
            throw new InvalidOperationException("A pasta de documentos do usuário não está disponível.");

        var pastaRelatorios = Path.Combine(documentos, "Phishing Reports");
        if (!Directory.Exists(pastaRelatorios))
        {
            MostrarAviso("Nenhum relatório gerado ainda");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = pastaRelatorios,
            UseShellExecute = true
        });
    }

    private void MostrarAviso(string mensagem)
    {
        _icone.ShowBalloonTip(3000, "Phishing Detector", mensagem, ToolTipIcon.Info);
    }
}
