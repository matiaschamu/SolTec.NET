# Guía de publicación — SolTec.NET

> Chuleta operativa: qué tocar, en qué orden y qué **no** hay que olvidarse.
> El detalle y el "por qué" de cada regla está en [AGENTS.md](AGENTS.md) §6.

---

## 0. Los dos ciclos son independientes

| Querés… | Tocás | ¿Nueva versión en Play? |
|---|---|---|
| Subir/cambiar un manual (PDF) | `Extras/` + `content.json` | **No** |
| Cambiar código o pantallas de la app | `Soltec.NET/` + versión | **Sí** |

La app es liviana y el contenido vive aparte. **Publicar manuales nuevos no requiere
sacar una versión nueva de la app.**

---

## 1. Agregar o cambiar un manual (PDF)

```
1. Copiar el PDF en Extras/Content/<Categoría>/<Subcarpeta>/
2. cd "Actualizar Json Soltec" && dotnet run     → regenera Extras/content.json
3. git commit + push                             → GitHub Pages lo publica
4. cd "Validar Json Soltec" && dotnet run        → confirma que no haya 404 ni faltantes
```

- ✅ **Todo cambio en `Extras/` obliga a regenerar `content.json`.** Si te lo saltás, el
  técnico no ve el archivo nuevo o le da 404.
- El generador **normaliza acentos, `ñ` y caracteres raros** en los nombres y te lo
  pregunta antes de renombrar. Aceptalo: esos caracteres rompen las URLs.
- Las URLs de GitHub Pages son **sensibles a mayúsculas**.
- El paso 4 va **después** del push (valida contra el servidor, no contra tu disco).

---

## 2. Agregar una categoría nueva

No hace falta programar una pantalla: las categorías son datos.

```
1. Crear la carpeta Extras/Content/<Categoría>/ con sus PDFs
2. Agregar el tile en MainPage.xaml (estilo TileLabel + un color Tile* de Colors.xaml)
3. Agregar su handler en MainPage.xaml.cs, copiando uno existente:
      await Shell.Current.GoToAsync(
          $"{nameof(ContenidoDetallePage)}?Ruta={"Content/<Categoría>"}");
4. Seguir el ciclo del punto 1 (regenerar JSON, push, validar)
```

- La pantalla de **Configuración se autollena**: la categoría aparece sola como
  sincronizable, no hay que tocar nada.
- El nombre del tile puede diferir del de la carpeta (ej. tile "Intercambiadores y
  Hornos" → carpeta `Intercambiadores`), pero **el título de la pantalla sale del
  nombre de la carpeta**.
- Esto **sí** requiere versión nueva de la app (tocaste `MainPage.xaml`).

---

## 3. Lanzar versión a PRUEBA INTERNA (Día 0)

```
1. En Soltec.NET.csproj subir:
      ApplicationVersion          (ej: 100 → 101)
      ApplicationDisplayVersion   (ej: 1.0.100 → 1.0.101)

   ⚠️ NO tocar VersionEnAlpha / VersionEnAlphaNombre

2. Compilar Android  → se regenera Extras/app-version.json solo
                       (sigue mostrando la versión vieja: ES CORRECTO)

3. Generar el AAB firmado y subirlo al canal de PRUEBA INTERNA en Play

4. git commit + push  → seguro: el aviso no cambió, nadie se entera todavía
```

- La versión del **splash sale sola** del `.csproj`. No la escribas a mano en el SVG.
- El AAB se genera desde **Visual Studio → Publicar/Archivar**. El keystore **no está en
  el repo** (`AndroidKeyStore=False`): la firma la maneja ese flujo + Play App Signing.

---

## 4. Promover a ALPHA (Día 3) — recién acá se avisa a los técnicos

```
1. En Play: promover el AAB de prueba interna → alpha
2. Esperar a que Play lo muestre disponible para los testers de alpha
3. En Soltec.NET.csproj subir:
      VersionEnAlpha          (ej: 100 → 101)
      VersionEnAlphaNombre    (ej: 1.0.100 → 1.0.101)
4. Compilar Android  → se regenera Extras/app-version.json con la versión nueva
5. git commit + push → AHORA sí, los técnicos ven el aviso al abrir la app
```

**Por qué está separado en dos fases:** la app no puede saber de qué canal de Play fue
instalada. Si el JSON anunciara la versión de prueba interna, los técnicos de alpha
verían el aviso **en cada inicio durante 3 días** sin tener cómo actualizar.

---

## 5. Reglas de oro (las que duelen si te las olvidás)

| Regla | Qué pasa si la rompés |
|---|---|
| **Nunca editar `app-version.json` a mano** | El próximo build lo pisa |
| **Nunca anunciar una versión que no esté en alpha** | Aviso trabado en cada inicio, sin salida |
| **Todo cambio en `Extras/` → regenerar `content.json`** | Manuales invisibles o 404 |
| **Primero Play, después el push del JSON** | Avisás algo que nadie puede instalar |
| **No hardcodear colores en las vistas** | Van en `Resources/Styles/Colors.xaml` |
| **No romper el modo offline ni la validación por hash** | El técnico sin señal queda a pie |

---

## 6. Dónde está cada cosa

| Qué | Dónde |
|---|---|
| Versión de la app | `Soltec.NET/Soltec.NET.csproj` → `ApplicationVersion` |
| Versión que se anuncia | `Soltec.NET/Soltec.NET.csproj` → `VersionEnAlpha` |
| Texto del aviso de update | `Soltec.NET/Soltec.NET.csproj` → `NotasActualizacion` |
| Application ID | `com.companyname.soltecpep` |
| Target / min de Android | API 36 (Android 16) / API 21 (Android 5.0) |
| Contenido publicado | `https://matiaschamu.github.io/SolTec.NET/Extras/` |
| Colores de marca | `Soltec.NET/Resources/Styles/Colors.xaml` |
| Tamaño de texto de los tiles | Estilo `TileLabel` en `MainPage.xaml` |

---

## 7. Si algo sale mal

| Síntoma | Qué mirar |
|---|---|
| Un manual da 404 o no aparece | Correr **Validar Json Soltec**. Casi siempre es un `content.json` sin regenerar o un problema de mayúsculas |
| El aviso de actualización no aparece | `VersionEnAlpha` tiene que ser **mayor** que la instalada, y el JSON tiene que estar pusheado |
| El aviso aparece y no se va | `VersionEnAlpha` quedó adelantada respecto de lo que hay en alpha. Corregila, compilá y pusheá |
| El ícono se ve recortado en Android | El SVG perdió el padding interno (~62% centrado) |
| El splash muestra la versión vieja | Es automático desde el `.csproj`; rebuild. No editar `splash.svg` |
