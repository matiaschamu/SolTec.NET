using Microsoft.Extensions.Logging;

namespace Soltec.NET.Services;

public interface IRegistroDiagnosticoService
{
    void Registrar(
        LogLevel nivel,
        string categoria,
        string mensaje,
        Exception? excepcion = null,
        EventId evento = default);

    void RegistrarErrorFatal(string origen, Exception excepcion, bool finalizaProceso);
    Task<string> CrearPaqueteExportacionAsync();
    Task BorrarRegistrosAsync();
}
