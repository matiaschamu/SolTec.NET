using System.ComponentModel;

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

        public Color BotonColor => ModoOffline ? Colors.Green : Colors.LightGray;
        public Color TextoBotonColor => ModoOffline ? Colors.White : Colors.DarkGray;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
