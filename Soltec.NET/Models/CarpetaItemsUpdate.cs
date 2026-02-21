using System.ComponentModel;
using System.Threading;

namespace Soltec.NET.Models
{
    public class CarpetaItemsUpdate : INotifyPropertyChanged
    {
        public string Nombre { get; set; }

        private bool _modoOffline;
        public bool ModoOffline
        {
            get => _modoOffline;
            set
            {
                if (_modoOffline != value)
                {
                    _modoOffline = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BotonColor));
                    OnPropertyChanged(nameof(TextoBotonColor));
                    OnPropertyChanged(nameof(CanSincronizar));

                    // Guardar en preferencias
                    Preferences.Set($"ModoOffline_{Nombre}", value);
                }
            }
        }

        private string _estadoArchivos = "";
        public string EstadoArchivos
        {
            get => _estadoArchivos;
            set { _estadoArchivos = value; OnPropertyChanged(); }
        }

        private string _progresoDescarga = "";
        public string ProgresoDescarga
        {
            get => _progresoDescarga;
            set { _progresoDescarga = value; OnPropertyChanged(); }
        }

        private bool _isSincronizando;
        public bool IsSincronizando
        {
            get => _isSincronizando;
            set
            {
                _isSincronizando = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSincronizar));
            }
        }

        public bool CanSincronizar => !IsSincronizando;

        public Color BotonColor => CanSincronizar ? Colors.Green : Colors.LightGray;
        public Color TextoBotonColor => CanSincronizar ? Colors.White : Colors.DarkGray;

        private CancellationTokenSource _cts;

        public CancellationToken IniciarSincronizacion()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            IsSincronizando = true;
            return _cts.Token;
        }

        public void DetenerSincronizacion()
        {
            _cts?.Cancel();
            IsSincronizando = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
