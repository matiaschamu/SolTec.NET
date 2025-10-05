using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Models
{
    public class Manual : INotifyPropertyChanged
    {
        public string Nombre { get; set; }
        public string Url { get; set; }

        private bool _estaOffline;
        public bool EstaOffline
        {
            get => _estaOffline;
            set
            {
                if (_estaOffline != value)
                {
                    _estaOffline = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NombreConIcono));
                }
            }
        }

        public string NombreConIcono => EstaOffline
            ? $"📂 {Nombre} (Offline)"
            : $"🌐 {Nombre} (Online)";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
