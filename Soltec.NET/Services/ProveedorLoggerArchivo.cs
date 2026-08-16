using Microsoft.Extensions.Logging;

namespace Soltec.NET.Services;

public sealed class ProveedorLoggerArchivo : ILoggerProvider
{
    private readonly IRegistroDiagnosticoService _registro;

    public ProveedorLoggerArchivo(IRegistroDiagnosticoService registro)
    {
        _registro = registro;
    }

    public ILogger CreateLogger(string categoryName) => new LoggerArchivo(categoryName, _registro);

    public void Dispose()
    {
    }

    private sealed class LoggerArchivo : ILogger
    {
        private readonly string _categoria;
        private readonly IRegistroDiagnosticoService _registro;

        public LoggerArchivo(string categoria, IRegistroDiagnosticoService registro)
        {
            _categoria = categoria;
            _registro = registro;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // Evita llenar el archivo con telemetría interna del framework. La app
            // conserva Information; Microsoft/System solo se guardan desde Warning.
            LogLevel mínimo = _categoria.StartsWith("Soltec.NET", StringComparison.Ordinal)
                ? LogLevel.Information
                : LogLevel.Warning;

            return logLevel >= mínimo;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _registro.Registrar(logLevel, _categoria, formatter(state, exception), exception, eventId);
        }
    }
}
