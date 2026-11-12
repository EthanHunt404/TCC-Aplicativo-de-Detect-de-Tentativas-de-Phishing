using DetectorPhishing.Domain;

namespace DetectorPhishing.Application;

public interface IGravadorRelatorio
{
    Task<string> GravarAsync(RelatorioInconsistencias relatorio);
}
