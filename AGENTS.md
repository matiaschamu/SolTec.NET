# AGENTS.md — SolTec.NET

> Guía de contexto para agentes de IA (Claude Code y similares) y para cualquier
> persona que trabaje en este repo. Define **qué es la app, para qué existe, cómo
> se ve y cómo se debe trabajar en ella**. Si una decisión de diseño o de código
> contradice este documento, resolverlo o consultarlo antes de avanzar.

---

## 1. Qué es SolTec.NET

**SolTec** es una **empresa industrial**. Esta app es una **herramienta interna**
para su propio personal técnico: centraliza los **manuales técnicos** y utilidades
de trabajo del día a día (cálculo de motores, gestión de **pañol**/herramientas,
consulta de contenidos) y permite usarlos **online y offline**.

- **Usuario objetivo:** técnicos y personal de campo de SolTec. **No** es un producto
  para clientes externos ni un portfolio: las decisiones se toman pensando en
  utilidad real en planta/campo, muchas veces **sin conexión**.
- **Problema que resuelve:** que el técnico tenga siempre a mano la documentación
  correcta y actualizada, con o sin internet, sin depender de buscar PDFs sueltos.

### Metadatos actuales de la app
| Dato | Valor |
|---|---|
| Nombre visible | **Soltec 4.0** |
| Application ID | `com.companyname.soltecpep` |
| Versión (display / build) | `1.0.98` / `98` |
| Framework | .NET 9 · MAUI (MVVM con CommunityToolkit.Mvvm) |
| Plataformas | Android · iOS · Mac Catalyst · Windows |
| Fuente | **Open Sans** (Regular / Semibold) |
| Contenido público | GitHub Pages: `https://matiaschamu.github.io/SolTec.NET/Extras/...` |

---

## 2. Objetivos y principios rectores

En orden de prioridad. Cuando haya que elegir, gana el objetivo más arriba.

1. **Fiabilidad offline primero.** El técnico puede estar sin señal. Descarga por
   carpeta, caché local validado al iniciar, e **integridad por hash SHA-256** en
   cada descarga. Nunca romper el modo offline por una mejora "online".
2. **Contenido correcto y accesible.** Cero 404. El pipeline de PDFs → `content.json`
   → GitHub Pages debe quedar consistente (ver §6). Un manual que no abre es un bug
   grave, no cosmético.
3. **Minimalismo y modernidad en la UI.** Ver §4. La interfaz debe sentirse limpia,
   moderna y sin ruido. **La estética importa tanto como la función.**
4. **Simplicidad de mantenimiento.** Un solo desarrollador mantiene esto. Preferir
   soluciones claras y convencionales (MVVM estándar, servicios pequeños) antes que
   abstracciones ingeniosas.

---

## 3. Ideas clave del producto

- **Catálogo navegable por carpetas**: árbol `FolderInfo → PdfInfo` que refleja la
  organización real de la documentación.
- **Sincronización granular**: se sincroniza/borra **por carpeta**, no todo o nada,
  para controlar el espacio en dispositivos de campo.
- **Estado siempre visible**: el usuario ve conectividad, progreso de descarga y
  estado de archivos. Nada de operaciones silenciosas.
- **Utilidades de trabajo** integradas junto a los manuales: **Cálculo de Motores**
  y **Pañol** (gestión de herramientas). La app es un "banco de trabajo digital",
  no solo un visor de PDFs.
- **Separación app / contenido**: la app es liviana; el contenido vive en `Extras/`
  y se publica aparte. Se puede actualizar documentación sin tocar la app.

---

## 4. Diseño visual — minimalista y moderno

> **Regla madre:** el color exacto es secundario; lo innegociable es que se vea
> **minimalista, moderno y limpio**.

### 4.1 Principios
- **Menos es más.** Espacios en blanco generosos, jerarquía clara por tamaño/peso,
  sin bordes ni sombras innecesarias, sin colores decorativos.
- **Superficies tipo tarjeta**: contenido en `Frame`/tarjetas blancas con esquinas
  redondeadas (`CornerRadius` ~15), sombra sutil, sobre un fondo gris muy claro.
- **Paleta contenida y neutra**: base gris/blanco + **un** acento. El color se usa
  con intención (acento = acción; rojo = destructivo), nunca para adornar.
- **Tipografía**: Open Sans. Títulos en Semibold/Bold, cuerpo Regular 14. Dejar
  respirar el texto.
- **Feedback claro**: indicadores de progreso y estados con color semántico, no
  ambiguo.

### 4.2 Paleta oficial (centralizada en `Colors.xaml`)
Todos los colores viven en `Resources/Styles/Colors.xaml`, sección **"SolTec —
Paleta de marca"**. **Ninguna vista debe hardcodear colores**: se referencian con
`{StaticResource <Key>}`.

### 4.3 Estado y deuda pendiente
- ✅ **Colores centralizados**: ya no hay literales de color inline en las vistas;
  todo pasa por `Colors.xaml`. Al crear vistas nuevas, **referenciar keys**; si
  falta un color, **agregarlo a `Colors.xaml`**, nunca inline.
- ✅ **Acento nativo alineado a la marca**: las keys de plantilla (`Primary`,
  `PrimaryDark`, `Secondary`, `SecondaryDarkText`, `Tertiary`) se repuntaron del
  violeta al **azul de marca** (`Primary` = `#1565C0` = `AccentBlue`). Así los
  controles nativos (Button, Slider, Switch, ProgressBar, TabBar...) siguen la
  estética. Se conservan los **nombres** de las keys porque `Styles.xaml` los usa;
  la `Magenta` de plantilla se eliminó (TabBar ahora usa `Primary`).
- ✅ **Converter con fuente única**: `BoolToColorConverter` ahora resuelve
  `ManualOffline` / `ManualOnline` / `ManualEstadoFallback` desde `Colors.xaml`
  (con fallback en código por seguridad). El estado de un manual se recolorea
  editando `Colors.xaml`, no el `.cs`.
- 🅿️ **`MainPage` conserva sus colores por tile a propósito** (keys `Tile*` en
  `Colors.xaml`, hoy 15). Decisión del dueño: punto de partida para futuros cambios
  de color; un rediseño se hace tocando solo esas keys. Layout y tipografía de los
  tiles: ver §4.4.

### 4.4 Menú principal (MainPage), ícono y splash
- **Tiles uniformes:** todos los botones del menú tienen la **misma** altura (80),
  radio de esquina (10) y estilo de texto. Ninguno resalta sobre otro — todas las
  categorías valen igual; solo cambian el **ancho** (100% para los de fila completa
  como Pañol / Intercambiadores, 48% para los de a pares) y el **color** (`Tile*`).
- **Perilla única de tipografía:** el estilo `TileLabel` (en `MainPage.xaml` →
  `ContentPage.Resources`) centraliza fuente, tamaño y alineación de los 15 tiles.
  Para cambiar el tamaño de texto de todos se toca **solo** su `FontSize`. **No**
  poner `FontSize`/`TextColor`/alineación inline en los labels de los tiles.
- **Ícono de la app** (`Resources/AppIcon/soltec_icon.svg`): el arte debe quedar a
  ~**62% centrado con fondo blanco a sangre** (padding). Así la máscara del adaptive
  icon de Android no recorta el logo (antes cortaba el texto "SOLTEC"). `MauiIcon`
  usa `Color="#FEFEFE"`. Si se reemplaza el SVG, **mantener ese padding** o el ícono
  se recorta de nuevo.
- **Splash** (`Resources/Splash/splash.svg`): fondo **azul de marca `#1565C0`** +
  tarjeta blanca redondeada con el logo centrado. El `MauiSplashScreen` usa
  `Color="#1565C0"` (debe coincidir con el fondo del SVG para que no se vea borde).
  El `splash.svg` commiteado se mantiene **limpio (sin versión)** — es el _template_.
- **Versión en el splash (automática):** el target MSBuild `GenerarSplashConVersion`
  (en el `.csproj`, `BeforeTargets="ResizetizeCollectItems"`) inyecta
  `v$(ApplicationDisplayVersion)` como `<text>` en una copia del splash dentro de
  `obj/` y apunta el `MauiSplashScreen` a esa copia. La versión sale **sola** del
  `.csproj` en cada build; **no** editar el número a mano ni escribirlo en
  `splash.svg`. Para cambiar formato/posición del texto, tocar `_SplashTextoVersion`
  en el target.

---

## 5. Arquitectura y convenciones de código

Solución `Soltec.NET.sln` = **3 proyectos**:

| Proyecto | Tipo | Rol |
|---|---|---|
| `Soltec.NET` | App .NET MAUI | App principal (Android/iOS/MacCatalyst/Windows) |
| `Actualizar Json Soltec` | Consola .NET 9 | Genera `content.json` desde los PDFs de `Extras/` |
| `Validar Json Soltec` | Consola .NET 9 | Valida `content.json` vs archivos locales y URLs (anti-404) |

### App (`Soltec.NET/`) — patrón MVVM
```
Models/      Entidades de dominio (Manual, PdfInfo, FolderInfo, Contenidos, ...)
ViewModels/  Lógica de presentación (CommunityToolkit.Mvvm)
Views/       Páginas XAML (Configuración, Cálculo de Motores, Detalle, Pañol)
Services/    Datos y lógica de negocio
Platforms/   Código por plataforma
Resources/   Íconos, fuentes, imágenes, splash, estilos
```

**Servicios** (mantenerlos chicos y con una responsabilidad):
`ActualizacionService` (aviso de nueva versión, §6.1) · `ArchivoService` (I/O local) ·
`BuscarCarpetasOnline` · `ConexionService` (conectividad) · `ContenidoJsonService`
(parseo del catálogo) · `ConverterService` (converters XAML) · `PreferenciasService`
(prefs persistentes) · `SincronizacionService` (descarga/sync offline).

**Convenciones:**
- **MVVM estricto**: la lógica va en ViewModels/Services, no en el code-behind.
  Usar `[ObservableProperty]` / `[RelayCommand]` de CommunityToolkit.Mvvm.
- **Nombres en español** (código, carpetas y UI están en español). Mantener la
  coherencia: no mezclar inglés salvo términos técnicos establecidos.
- **Nullability**: el proyecto ya limpió warnings de nullability; no reintroducirlos.
- Persistencia local con **SQLite** (`sqlite-net-pcl`); prefs con `PreferenciasService`.
- Verificar **SHA-256** en descargas; no saltear la validación de integridad.

### Navegación de contenido (data-driven)
Las categorías de documentación **no** tienen pantalla ni lógica propia: son datos.
- **Una sola pantalla genérica**: `ContenidoDetallePage?Ruta=Content/<Categoria>`
  lista las subcarpetas y sus PDFs. Manuales, Planos e **Intercambiadores** usan la
  misma página; solo cambia la `Ruta`.
- **Configuración se autollena**: `ConfiguracionViewModel` lista **toda** subcarpeta
  de `Content` como tarjeta sincronizable (`ObtenerCarpetasInicialesAsync`). No hay
  que programar sync por categoría.
- **Agregar una categoría nueva** = (1) un tile en `MainPage` que navegue a su
  `Ruta`, y (2) crear la carpeta con PDFs en `Extras/Content/<Categoria>` + regenerar
  el JSON (§6). El nombre del tile puede diferir del de la carpeta (ej. botón
  "Intercambiadores y Hornos" → carpeta `Content/Intercambiadores`), pero el
  **título de la pantalla sale del nombre de la carpeta**.

---

## 6. Flujo de contenido (crítico — no romper)

El contenido y la app se actualizan por caminos separados. Para publicar contenido:

```
1. Agregar / editar / renombrar PDFs en Extras/
2. Ejecutar "Actualizar Json Soltec"   → regenera Extras/content.json
                                          (normaliza nombres, calcula URL, hash, tamaño)
3. git commit + push                    → GitHub Pages publica el contenido
4. Ejecutar "Validar Json Soltec"       → confirma que no falten archivos ni haya URLs 404
```

- **Regla:** todo cambio en `Extras/` **obliga** a regenerar `content.json`. Un JSON
  desactualizado = archivos que la app no ve o 404 en el técnico.
- Ojo con **case-sensitivity** de URLs (GitHub Pages es sensible a mayúsculas) y con
  acentos/`ñ`/caracteres especiales en nombres: el generador los normaliza, respetarlo.

### 6.1 Aviso de nueva versión de la app

La app se distribuye por **Google Play (canal interno)**, así que Play se encarga de
instalar la actualización. Lo que la app agrega es el **aviso**: al abrir el menú
consulta `Extras/app-version.json` y, si hay una versión más nueva que la instalada,
ofrece abrir la ficha de Play.

```
1. Subir ApplicationDisplayVersion / ApplicationVersion en el .csproj
2. Actualizar Extras/app-version.json con el MISMO VersionCode + notas
3. git commit + push        → GitHub Pages publica el aviso
4. Subir el AAB a Play (canal interno)
```

- **Regla:** `VersionCode` del JSON debe ser **igual** a `ApplicationVersion` del
  `.csproj`. Si el JSON queda adelantado, se avisa de una versión que Play todavía no
  tiene; si queda atrasado, nadie se entera.
- **Publicar el JSON recién cuando el AAB ya esté disponible en Play**, por el mismo
  motivo.
- El chequeo corre **una vez por sesión**, con timeout de 5 s, y **falla en silencio**:
  sin conexión no molesta ni demora el arranque (principio §2.1).
- **Se avisa en cada inicio hasta que el técnico actualice.** Es deliberado: no hay
  "no volver a mostrar". El aviso desaparece solo cuando el `versionCode` instalado
  alcanza al publicado. Por eso el §6.1 exige que el AAB ya esté en Play antes de
  pushear el JSON: si no, el aviso queda trabado y no hay forma de sacarlo.

---

## 7. Build, ejecución y CI

```bash
dotnet restore Soltec.NET.sln
# App en Windows, por ejemplo:
dotnet build Soltec.NET/Soltec.NET.csproj -f net9.0-windows10.0.19041.0
# Herramientas de consola:
cd "Actualizar Json Soltec" && dotnet run
cd "Validar Json Soltec"    && dotnet run
```
- iOS/Mac Catalyst requieren macOS + Xcode. Las consolas corren en cualquier SO con SDK .NET 9.
- **CI**: `.github/workflows/ios-build.yml` compila la app para el Simulador de iOS en
  cada push a `main`.

---

## 8. Cómo trabajar en este repo (para agentes)

- **Antes de tocar UI**, releer §4. Diseño minimalista/moderno; usar la paleta de §4.2
  y, si es posible, mejorar la deuda (colores a `Colors.xaml`) en vez de sumar literales.
- **Antes de tocar contenido/`Extras/`**, recordar el flujo de §6: regenerar y validar el JSON.
- **Idioma**: responder y nombrar en **español**.
- **Cambios de app vs contenido**: no mezclar en el mismo commit un cambio de código
  con una recarga masiva de PDFs, salvo que sea intencional.
- **No romper offline ni la validación de hash** por conveniencia.
- Preferir cambios chicos, legibles y en el estilo del código vecino (un solo mantenedor).
```

<!--
Mantener este archivo vivo: si cambian la marca/paleta, el ApplicationId, el flujo
de contenido o la arquitectura, actualizar la sección correspondiente aquí.
-->
