using System.ComponentModel;

namespace Soltec.NET.Models

{
    public class ConfiguracionManual : INotifyPropertyChanged
    {
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
                }
            }
        }

        public Dictionary<string, string> HashArchivosLocales { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
