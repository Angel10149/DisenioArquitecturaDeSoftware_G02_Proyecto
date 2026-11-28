# Sistema Centralizado de Validaciones (`AppWeb.Validations`)

Este módulo centraliza las reglas de validación reutilizables del proyecto sobre **DataAnnotations nativo de .NET** (`System.ComponentModel.DataAnnotations`), sin dependencias externas y compatible con la arquitectura en N capas (`Controllers` → `Services` → `Repositories` → `Data`).

## Estructura

- **`ValidationPatterns.cs`**: Única fuente de verdad para todas las expresiones regulares del sistema (DNI, RUC, CE/Pasaporte, teléfono, solo letras, razón social, código de barras, series `B001`/`F001`, logo URL/ruta, etc.).
- **`ValidationAttributes.cs`**: Atributos reutilizables (`[...]` ) listos para usar en cualquier `Model` o `ViewModel`. Se integran automáticamente con `ModelState.IsValid` y con los Tag Helpers de Razor.
- **`ValidationHelper.cs`**: Utilidad estática para ejecutar validaciones desde la capa `Services/` o desde pruebas automatizadas (`AppWeb.Tests`).

---

## Catálogo de atributos disponibles

| Atributo | Qué valida | Ejemplo de uso |
|---|---|---|
| `[CampoObligatorio]` | Campo requerido con mensaje estándar en español | `[CampoObligatorio]` |
| `[DniPeruano]` | Exactamente 8 dígitos numéricos | `[DniPeruano]` |
| `[RucPeruano]` | Exactamente 11 dígitos numéricos | `[RucPeruano]` |
| `[TelefonoPeruano]` | 7 a 15 dígitos (permite `+`, espacios y guiones) | `[TelefonoPeruano]` |
| `[CorreoElectronico]` | Formato de correo electrónico (permite nulo/vacío si es opcional) | `[CorreoElectronico]` |
| `[SoloLetras]` | Solo letras y espacios (`permitirPunto: true` habilita `.`) | `[SoloLetras(permitirPunto: true)]` |
| `[RazonSocial]` | Alfanumérico con al menos una letra (`.`, `-`, `&`) | `[RazonSocial]` |
| `[LogoRutaOUrl]` | URL `http(s)://...` o ruta local `/pictures/...` | `[LogoRutaOUrl]` |
| `[NumeroNoNegativo]` | Valor `>= 0` para `int`, `long`, `decimal`, `double`, `float` | `[NumeroNoNegativo]` |
| `[NumeroPositivo]` | Valor `> 0` para `int`, `long`, `decimal`, `double`, `float` | `[NumeroPositivo]` |
| `[FechaRango]` | `DateTime` o `DateOnly` con opciones `NoPasada`, `NoFutura`, `AnioMinimo`, `AnioMaximo` | `[FechaRango(NoPasada = true)]` |

---

## 1. Cómo usar el sistema en un nuevo modelo o ViewModel

1. Agrega `using AppWeb.Validations;` al inicio del archivo.
2. Decora las propiedades con los atributos necesarios (puedes sobreescribir `ErrorMessage` si quieres un texto específico):

```csharp
using System.ComponentModel.DataAnnotations;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class Transportista
    {
        [CampoObligatorio, StringLength(11), RucPeruano]
        public string ruc { get; set; } = string.Empty;

        [CampoObligatorio, StringLength(150), RazonSocial]
        public string razonSocial { get; set; } = string.Empty;

        [CampoObligatorio, StringLength(100), SoloLetras(permitirPunto: true)]
        public string chofer { get; set; } = string.Empty;

        [CampoObligatorio, StringLength(20), TelefonoPeruano]
        public string telefono { get; set; } = string.Empty;

        [StringLength(100), CorreoElectronico]
        public string? correo { get; set; }

        [CampoObligatorio, NumeroPositivo]
        public decimal tarifaFlete { get; set; }

        [CampoObligatorio, FechaRango(NoPasada = true)]
        public DateOnly fechaDespacho { get; set; }
    }
}
```

> **Nota:** Agregar o cambiar atributos de validación en C# **no modifica el esquema de SQL Server** ni requiere nuevas migraciones de EF Core.

---

## 2. Cómo validar desde la capa de servicios (`Services/`)

Cuando necesites validar un objeto directamente en un servicio (sin pasar por `ModelState` de un controlador):

```csharp
var resultado = ValidationHelper.Validar(entidad);
if (!resultado.EsValido)
{
    return (false, resultado.PrimerError);
}
```

Para validar un documento peruano según su tipo (`DNI`, `RUC`, `CE`, `Pasaporte`):

```csharp
bool docValido = ValidationHelper.EsDocumentoValido(cliente.tipoDocumento, cliente.numeroDocumento);
```

---

## 3. Cómo agregar una nueva validación al sistema centralizado

1. **Agrega el patrón** en `ValidationPatterns.cs` (si usa expresión regular):
   ```csharp
   public const string PlacaVehiculo = @"^[A-Z0-9]{3}-[0-9]{3}$";
   ```
2. **Crea el atributo** en `ValidationAttributes.cs`:
   ```csharp
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
   public sealed class PlacaVehiculoAttribute : RegularExpressionAttribute
   {
       public PlacaVehiculoAttribute() : base(ValidationPatterns.PlacaVehiculo)
       {
           ErrorMessage = "La placa debe tener formato ABC-123";
       }
   }
   ```
3. **Agrega pruebas** (casos válidos e inválidos) en `AppWeb.Tests/ValidacionesTests.cs` y ejecuta `dotnet test`.

---

## 4. Qué reglas permanecen en `Services/`

Las reglas que dependen de **base de datos, transacciones o estados del negocio** deben seguir en la capa `Services/`:
- Unicidad de correo o número de documento en BD (`UsuarioService`, `VentaService`).
- Disponibilidad y descuento/devolución de stock (`VentaService`, `CompraService`).
- Estados de flujo (`Cotizacion: Aprobada → Convertida`, `Venta: Pendiente → Pagada/Anulada`).
- Métodos de pago permitidos, tipos de comprobante y correlativo concurrente (`CajaService`).

---

## 5. Ejecutar las pruebas automatizadas

Desde la raíz del repositorio:

```powershell
dotnet test
```
