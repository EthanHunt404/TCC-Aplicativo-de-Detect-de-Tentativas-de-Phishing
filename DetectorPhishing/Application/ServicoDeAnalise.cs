using System.Diagnostics;
using System.Threading.Channels;
using DetectorPhishing.Domain;

namespace DetectorPhishing.Application;

public sealed class ServicoDeAnalise
{
    private readonly ChannelReader<ConteudoRelevante> _canal;
    private readonly EmailAnalist _analista;
    private readonly IGravadorRelatorio _gravador;
    private readonly INotificador _notificador;

    public ServicoDeAnalise(
        ChannelReader<ConteudoRelevante> canal,
        EmailAnalist analista,
        IGravadorRelatorio gravador,
        INotificador notificador)
    {
        _canal = canal ?? throw new ArgumentNullException(nameof(canal));
        _analista = analista ?? throw new ArgumentNullException(nameof(analista));
        _gravador = gravador ?? throw new ArgumentNullException(nameof(gravador));
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
    }

    public async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        await foreach (var conteudo in _canal.ReadAllAsync(cancellationToken))
        {
            var cronometro = Stopwatch.StartNew();
            var etapa = "análise";
            var resultado = "sem_inconsistencias";

            try
            {
                var relatorio = _analista.Analisar(conteudo);
                if (relatorio != null)
                {
                    resultado = "suspeito";
                    etapa = "gravar_relatorio";
                    var caminhoRelatorio = await _gravador.GravarAsync(relatorio);
                    etapa = "notificar";
                    _notificador.Notificar(caminhoRelatorio);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                resultado = "falha";
            }
            finally
            {
                cronometro.Stop();
                RegistroAplicacao.RegistrarEvento("-", etapa, cronometro.ElapsedMilliseconds, resultado);
            }
        }
    }
}
