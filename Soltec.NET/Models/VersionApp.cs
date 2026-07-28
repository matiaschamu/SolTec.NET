namespace Soltec.NET.Models
{
    /// <summary>
    /// Última versión publicada de la app, tal como se describe en
    /// Extras/app-version.json (publicado por GitHub Pages).
    /// </summary>
    public class VersionApp
    {
        /// <summary>
        /// Equivale a ApplicationVersion del .csproj (el versionCode del manifest de Android).
        /// Es el número que se compara para decidir si hay actualización.
        /// </summary>
        public int VersionCode { get; set; }

        /// <summary>
        /// Equivale a ApplicationDisplayVersion del .csproj (ej: "1.0.97"). Solo se muestra.
        /// </summary>
        public string VersionName { get; set; } = string.Empty;

        /// <summary>
        /// Texto que se le muestra al técnico en el aviso. Es genérico e invita a
        /// actualizar: no hace falta reescribirlo en cada versión.
        /// </summary>
        public string Notas { get; set; } = string.Empty;
    }
}
