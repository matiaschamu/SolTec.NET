using System.Text.Json;

namespace Soltec.NET.Services
{
    public interface IPreferenciasService
    {
        void GuardarModoOffline(string nombreCarpeta, bool modoOffline);

        bool LeerModoOffline(string nombreCarpeta);

        void GuardarHashArchivos(Dictionary<string, string> hashes);

        Dictionary<string, string> LeerHashArchivos();
    }
    public class PreferenciasService: IPreferenciasService
    {
        public void GuardarModoOffline(string nombreCarpeta, bool modoOffline)
        {
            Preferences.Set($"ModoOffline_{nombreCarpeta}", modoOffline);
        }

        public bool LeerModoOffline(string nombreCarpeta)
        {
            return Preferences.Get($"ModoOffline_{nombreCarpeta}", false);
        }

        public void GuardarHashArchivos(Dictionary<string, string> hashes)
        {
            var json = JsonSerializer.Serialize(hashes);
            Preferences.Set("HashArchivosLocales", json);
        }

        public Dictionary<string, string> LeerHashArchivos()
        {
            var json = Preferences.Get("HashArchivosLocales", "{}");
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
    }
}
