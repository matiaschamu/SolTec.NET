# SolTec.NET

[![Build iOS](https://github.com/matiaschamu/SolTec.NET/actions/workflows/ios-build.yml/badge.svg)](https://github.com/matiaschamu/SolTec.NET/actions/workflows/ios-build.yml)

**SolTec.NET** es una solución de .NET 9 compuesta por una aplicación móvil multiplataforma
y dos herramientas de consola de soporte. La app centraliza manuales técnicos y utilidades
de trabajo, permitiendo consultarlos **online y offline**; las herramientas de consola
generan y validan el catálogo de contenido (`content.json`) que la app consume.

---

## 📁 Estructura de la solución

La solución [`Soltec.NET.sln`](Soltec.NET.sln) agrupa tres proyectos:

| Proyecto | Tipo | Framework | Descripción |
|---|---|---|---|
| [Soltec.NET](Soltec.NET) | App .NET MAUI | `net9.0-android` / `net9.0-ios` / `net9.0-maccatalyst` / `net9.0-windows` | Aplicación móvil/escritorio principal |
| [Actualizar Json Soltec](Actualizar%20Json%20Soltec) | Consola | `net9.0` | Genera el `content.json` a partir de los PDFs locales |
| [Validar Json Soltec](Validar%20Json%20Soltec) | Consola | `net9.0` | Valida que el `content.json` y las URLs publicadas sean correctos |

Carpetas auxiliares:

- **`Extras/`** — Contenido (PDFs organizados en carpetas) y el `content.json` generado. Se publica en GitHub Pages.
- **`Recursos/`** — Recursos de diseño y material de apoyo del proyecto.

---

## 1️⃣ Soltec.NET — Aplicación .NET MAUI

Aplicación multiplataforma (Android, iOS, Mac Catalyst y Windows) construida con **.NET MAUI**
y patrón **MVVM** apoyado en [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet).

### Características
- Catálogo de manuales técnicos navegable por carpetas.
- Modo **offline**: descarga y sincronización de archivos por carpeta para consultarlos sin conexión.
- Detección de conectividad y validación de caché local de `content.json` al iniciar.
- Verificación de integridad de descargas mediante **hash SHA-256**.
- Cálculo de motores y otras utilidades de trabajo.
- Persistencia de preferencias y almacenamiento local con **SQLite**.

### Arquitectura
```
Soltec.NET/
├── Models/        # Entidades de dominio (Manual, PdfInfo, FolderInfo, Contenidos, ...)
├── ViewModels/    # Lógica de presentación (ConfiguracionViewModel, ContenidoDetalleViewModel)
├── Views/         # Páginas XAML (Configuración, Cálculo de motores, Detalle de contenido, Pañol)
├── Services/      # Acceso a datos y lógica de negocio
│   ├── ArchivoService.cs          # Lectura/escritura de archivos locales
│   ├── BuscarCarpetasOnline.cs    # Búsqueda de carpetas en el servidor
│   ├── ConexionService.cs         # Estado de conectividad
│   ├── ContenidoJsonService.cs    # Parseo del catálogo content.json
│   ├── ConverterService.cs        # Convertidores de bindings XAML
│   ├── PreferenciasService.cs     # Preferencias persistentes
│   └── SincronizacionService.cs   # Sincronización/descarga offline
├── Platforms/     # Código específico por plataforma (Android, iOS, MacCatalyst, Windows, Tizen)
└── Resources/     # Íconos, fuentes, imágenes, splash y estilos
```

### Dependencias principales
- `CommunityToolkit.Maui` y `CommunityToolkit.Mvvm`
- `Microsoft.Maui.Controls`
- `Microsoft.Extensions.Http`
- `sqlite-net-pcl` + `SQLitePCLRaw.bundle_green`

---

## 2️⃣ Actualizar Json Soltec — Generador del catálogo

Herramienta de consola que recorre la carpeta **`Extras/`**, detecta los archivos PDF y
genera el archivo **`content.json`** que describe toda la estructura de carpetas y archivos.

### Qué hace
1. **Normaliza nombres problemáticos**: detecta PDFs con acentos, `ñ` o caracteres especiales
   (`#%&{}<>*?/$!'":@+\`|=`) que rompen las URLs, sugiere un nombre limpio y permite renombrarlos
   de forma interactiva.
2. **Recorre recursivamente** las carpetas construyendo un árbol `FolderInfo` → `PdfInfo`.
3. Para cada PDF calcula:
   - La **URL pública** (`https://matiaschamu.github.io/SolTec.NET/Extras/...`).
   - El **hash SHA-256** (para detectar cambios y validar descargas).
   - El **tamaño en bytes**.
4. Serializa el árbol a `Extras/content.json` con formato indentado.

### Uso
```bash
cd "Actualizar Json Soltec"
dotnet run
```
> Ejecutar después de **agregar, quitar o renombrar** PDFs en `Extras/`. Una vez generado el
> `content.json`, hacer commit y push para que GitHub Pages publique los cambios.

---

## 3️⃣ Validar Json Soltec — Validador del catálogo

Herramienta de consola que verifica la **consistencia** entre los archivos locales, el
`content.json` publicado y las URLs reales en el servidor. Sirve para evitar errores 404 en la app.

### Qué hace
- **Fase 1 — Cruce local vs servidor:**
  - Detecta archivos locales que **no figuran** en el `content.json` (¿olvidaste regenerarlo?).
  - Detecta archivos "fantasma" definidos en el JSON que **no existen** localmente.
- **Fase 2 — Validación de URLs (anti-404):**
  - Hace una petición `HEAD` a cada URL del catálogo y reporta las que devuelven error
    HTTP o fallan por red (útil para detectar problemas de *case-sensitivity* o pushes olvidados).

### Uso
```bash
cd "Validar Json Soltec"
dotnet run
```
> Ejecutar antes de publicar para confirmar que todo el contenido es accesible online.

### Flujo de trabajo recomendado
```
1. Agregar/editar PDFs en Extras/
2. Actualizar Json Soltec   → genera content.json
3. git commit + push        → GitHub Pages publica el contenido
4. Validar Json Soltec      → confirma que no hay archivos faltantes ni URLs rotas
```

---

## 📦 Requisitos

- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- [Visual Studio 2022/2026](https://visualstudio.microsoft.com/) con el workload
  **.NET Multi-platform App UI development (MAUI)**.
- Para compilar cada plataforma de la app:
  - **Android**: Android SDK y emulador o dispositivo físico.
  - **iOS / Mac Catalyst**: macOS con Xcode (compilación nativa).
  - **Windows**: Windows 10 versión 1809 o superior.

> Las dos herramientas de consola solo requieren el SDK de .NET 9 y se ejecutan en cualquier sistema.

---

## 🔧 Instalación y ejecución

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/matiaschamu/SolTec.NET.git
   cd SolTec.NET
   ```
2. Restaurar dependencias:
   ```bash
   dotnet restore Soltec.NET.sln
   ```
3. Ejecutar la app en una plataforma concreta, por ejemplo Windows:
   ```bash
   dotnet build Soltec.NET/Soltec.NET.csproj -f net9.0-windows10.0.19041.0
   ```
   O abrir `Soltec.NET.sln` en Visual Studio y seleccionar el destino deseado.

---

## 🤖 Integración continua

El workflow [`.github/workflows/ios-build.yml`](.github/workflows/ios-build.yml) compila
automáticamente la app para el **Simulador de iOS** en cada push a `main`, usando runners
de macOS. El estado se refleja en el badge al inicio de este README.
