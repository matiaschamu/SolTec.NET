using System.Text.Json;

namespace Soltec.NET.Service
{
    public class PreferenciasService
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
