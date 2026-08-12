using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace Soltec.NET.ViewModels;

#pragma warning disable MVVMTK0045 // CommunityToolkit 8.4 aún no genera propiedades parciales en la configuración C# actual.
public partial class MotoresViewModel : ObservableObject
{
    private const double RaizDeTres = 1.7320508075688772;
    private const double KwPorHp = 0.746;

    public IReadOnlyList<string> TiposAlimentacion { get; } = ["Trifásico", "Monofásico"];
    public IReadOnlyList<string> Conexiones { get; } = ["Estrella", "Triángulo"];
    public IReadOnlyList<int> CantidadesPolos { get; } = [2, 4, 6, 8, 10, 12];

    [ObservableProperty] private string tipoAlimentacion = "Trifásico";
    [ObservableProperty] private string conexion = "Estrella";
    [ObservableProperty] private double tensionLinea = 380;
    [ObservableProperty] private double potenciaNominalKw = 11.19;
    [ObservableProperty] private double rendimientoPorcentaje = 90;
    [ObservableProperty] private double factorPotencia = 0.85;
    [ObservableProperty] private double cargaPorcentaje = 100;
    [ObservableProperty] private double corrienteMedida = 18;
    [ObservableProperty] private double limiteVariadorPorcentaje = 100;
    [ObservableProperty] private double frecuencia = 50;
    [ObservableProperty] private int polos = 4;
    [ObservableProperty] private double rpmMedidas = 1450;
    [ObservableProperty] private double corrienteFaseA = 18;
    [ObservableProperty] private double corrienteFaseB = 18;
    [ObservableProperty] private double corrienteFaseC = 18;

    [ObservableProperty] private bool esTrifasico = true;
    [ObservableProperty] private string potenciaHp = string.Empty;
    [ObservableProperty] private string corrienteNominal = string.Empty;
    [ObservableProperty] private string corrienteSegunCarga = string.Empty;
    [ObservableProperty] private string tensionDeFase = string.Empty;
    [ObservableProperty] private string corrienteDeFase = string.Empty;
    [ObservableProperty] private string potenciaEstimada = string.Empty;
    [ObservableProperty] private string potenciaEstimadaHp = string.Empty;
    [ObservableProperty] private string cargaEstimada = string.Empty;
    [ObservableProperty] private string potenciaEntrada = string.Empty;
    [ObservableProperty] private string potenciaAparente = string.Empty;
    [ObservableProperty] private string potenciaReactiva = string.Empty;
    [ObservableProperty] private string corrienteMinimaVariador = string.Empty;
    [ObservableProperty] private string limiteCorrienteVariador = string.Empty;
    [ObservableProperty] private string velocidadSincronica = string.Empty;
    [ObservableProperty] private string deslizamiento = string.Empty;
    [ObservableProperty] private string parNominal = string.Empty;
    [ObservableProperty] private string desequilibrioCorriente = string.Empty;
    [ObservableProperty] private string estadoDesequilibrio = string.Empty;
    [ObservableProperty] private Color colorDesequilibrio = Colors.Green;
    [ObservableProperty] private string advertencia = string.Empty;

    public MotoresViewModel() => Recalcular();

    partial void OnTipoAlimentacionChanged(string value)
    {
        EsTrifasico = value == "Trifásico";
        Recalcular();
    }

    partial void OnConexionChanged(string value) => Recalcular();
    partial void OnTensionLineaChanged(double value) => Recalcular();
    partial void OnPotenciaNominalKwChanged(double value) => Recalcular();
    partial void OnRendimientoPorcentajeChanged(double value) => Recalcular();
    partial void OnFactorPotenciaChanged(double value) => Recalcular();
    partial void OnCargaPorcentajeChanged(double value) => Recalcular();
    partial void OnCorrienteMedidaChanged(double value) => Recalcular();
    partial void OnLimiteVariadorPorcentajeChanged(double value) => Recalcular();
    partial void OnFrecuenciaChanged(double value) => Recalcular();
    partial void OnPolosChanged(int value) => Recalcular();
    partial void OnRpmMedidasChanged(double value) => Recalcular();
    partial void OnCorrienteFaseAChanged(double value) => Recalcular();
    partial void OnCorrienteFaseBChanged(double value) => Recalcular();
    partial void OnCorrienteFaseCChanged(double value) => Recalcular();

    [RelayCommand]
    private void RestaurarValores()
    {
        TipoAlimentacion = "Trifásico";
        Conexion = "Estrella";
        TensionLinea = 380;
        PotenciaNominalKw = 11.19;
        RendimientoPorcentaje = 90;
        FactorPotencia = 0.85;
        CargaPorcentaje = 100;
        CorrienteMedida = 18;
        LimiteVariadorPorcentaje = 100;
        Frecuencia = 50;
        Polos = 4;
        RpmMedidas = 1450;
        CorrienteFaseA = CorrienteFaseB = CorrienteFaseC = 18;
        Recalcular();
    }

    private void Recalcular()
    {
        double tension = Positivo(TensionLinea);
        double kw = Positivo(PotenciaNominalKw);
        double rendimiento = Limitar(RendimientoPorcentaje / 100, 0.01, 1);
        double fp = Limitar(FactorPotencia, 0.01, 1);
        double carga = Limitar(CargaPorcentaje / 100, 0, 2);
        double factorSistema = EsTrifasico ? RaizDeTres : 1;
        double divisorCorriente = factorSistema * tension * fp * rendimiento;
        double corrientePlenaCarga = divisorCorriente > 0 ? kw * 1000 / divisorCorriente : 0;
        double corrienteCarga = corrientePlenaCarga * carga;
        double corriente = Positivo(CorrienteMedida);
        double kwDesdeCorriente = factorSistema * tension * corriente * fp * rendimiento / 1000;
        double cargaDesdeCorriente = corrientePlenaCarga > 0 ? corriente / corrientePlenaCarga * 100 : 0;
        double kwEntrada = rendimiento > 0 ? kw / rendimiento : 0;
        double kva = fp > 0 ? kwEntrada / fp : 0;
        double kvar = kwEntrada * Math.Tan(Math.Acos(fp));
        double rpmSincronicas = Polos > 0 ? 120 * Positivo(Frecuencia) / Polos : 0;
        double slip = rpmSincronicas > 0 ? (rpmSincronicas - Positivo(RpmMedidas)) / rpmSincronicas * 100 : 0;
        double torque = RpmMedidas > 0 ? 9550 * kw / RpmMedidas : 0;

        PotenciaHp = $"{Numero(kw / KwPorHp)} HP";
        CorrienteNominal = $"{Numero(corrientePlenaCarga)} A";
        CorrienteSegunCarga = $"{Numero(corrienteCarga)} A al {Numero(carga * 100, 0)} %";
        PotenciaEstimada = $"{Numero(kwDesdeCorriente)} kW";
        PotenciaEstimadaHp = $"{Numero(kwDesdeCorriente / KwPorHp)} HP";
        CargaEstimada = $"{Numero(cargaDesdeCorriente, 0)} % de la nominal";
        PotenciaEntrada = $"{Numero(kwEntrada)} kW eléctricos";
        PotenciaAparente = $"{Numero(kva)} kVA";
        PotenciaReactiva = $"{Numero(kvar)} kvar";
        CorrienteMinimaVariador = $"≥ {Numero(corrientePlenaCarga)} A de salida";
        LimiteCorrienteVariador = $"{Numero(corrientePlenaCarga * Math.Max(LimiteVariadorPorcentaje, 0) / 100)} A";
        VelocidadSincronica = $"{Numero(rpmSincronicas, 0)} rpm";
        Deslizamiento = $"{Numero(slip)} %";
        ParNominal = $"{Numero(torque)} N·m";

        if (EsTrifasico)
        {
            bool estrella = Conexion == "Estrella";
            TensionDeFase = $"{Numero(estrella ? tension / RaizDeTres : tension)} V por bobinado";
            CorrienteDeFase = $"{Numero(estrella ? corrienteCarga : corrienteCarga / RaizDeTres)} A por bobinado";
        }
        else
        {
            TensionDeFase = $"{Numero(tension)} V";
            CorrienteDeFase = $"{Numero(corrienteCarga)} A";
        }

        CalcularDesequilibrio();
        Advertencia = CrearAdvertencia(cargaDesdeCorriente, slip);
    }

    private void CalcularDesequilibrio()
    {
        double a = Positivo(CorrienteFaseA);
        double b = Positivo(CorrienteFaseB);
        double c = Positivo(CorrienteFaseC);
        double promedio = (a + b + c) / 3;
        double mayorDesvio = Math.Max(Math.Abs(a - promedio), Math.Max(Math.Abs(b - promedio), Math.Abs(c - promedio)));
        double porcentaje = promedio > 0 ? mayorDesvio / promedio * 100 : 0;

        DesequilibrioCorriente = $"{Numero(porcentaje)} % (promedio {Numero(promedio)} A)";
        if (porcentaje <= 10)
        {
            EstadoDesequilibrio = "Dentro de la referencia máxima del 10 %";
            ColorDesequilibrio = Colors.Green;
        }
        else
        {
            EstadoDesequilibrio = "Supera el 10 %: revisar tensiones, conexiones y bobinados";
            ColorDesequilibrio = Colors.Red;
        }
    }

    private string CrearAdvertencia(double cargaCalculada, double slip)
    {
        List<string> avisos = [];
        if (RendimientoPorcentaje <= 0 || FactorPotencia <= 0 || TensionLinea <= 0)
            avisos.Add("Completá tensión, rendimiento y factor de potencia con valores válidos.");
        if (cargaCalculada > 110)
            avisos.Add("La corriente medida supera el 110 % de la nominal calculada: verificar sobrecarga, tensión y placa.");
        if (slip < 0)
            avisos.Add("Las rpm medidas superan la velocidad sincrónica; revisá frecuencia, polos o medición.");
        else if (slip > 8)
            avisos.Add("El deslizamiento calculado es alto; puede indicar sobrecarga o un dato de placa incorrecto.");

        return avisos.Count == 0
            ? "Son estimaciones basadas en los datos ingresados. Para protecciones y variadores manda la corriente de placa y el manual del fabricante."
            : string.Join(" ", avisos);
    }

    private static double Positivo(double valor) => double.IsFinite(valor) ? Math.Max(valor, 0) : 0;
    private static double Limitar(double valor, double minimo, double maximo) => Math.Clamp(double.IsFinite(valor) ? valor : minimo, minimo, maximo);
    private static string Numero(double valor, int decimales = 2) => valor.ToString($"N{decimales}", CultureInfo.CurrentCulture);
}
