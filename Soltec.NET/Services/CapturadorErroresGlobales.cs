namespace Soltec.NET.Services;

public static class CapturadorErroresGlobales
{
    private static int _inicializado;

    public static void Inicializar(IRegistroDiagnosticoService registro)
    {
        if (Interlocked.Exchange(ref _inicializado, 1) != 0)
            return;

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception excepcion)
                registro.RegistrarErrorFatal(nameof(AppDomain.UnhandledException), excepcion, args.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            registro.RegistrarErrorFatal(nameof(TaskScheduler.UnobservedTaskException), args.Exception, false);
            args.SetObserved();
        };

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
        {
            registro.RegistrarErrorFatal("AndroidEnvironment.UnhandledExceptionRaiser", args.Exception, true);
        };
#endif
    }
}
