using System.ComponentModel.DataAnnotations;

namespace AppWeb.Validations
{
    /// <summary>
    /// Atributo reutilizable para campos obligatorios con mensaje estándar en español
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class CampoObligatorioAttribute : RequiredAttribute
    {
        public CampoObligatorioAttribute()
        {
            ErrorMessage = "El campo {0} es obligatorio";
        }
    }

    /// <summary>
    /// Valida un DNI peruano de exactamente 8 dígitos numéricos
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class DniPeruanoAttribute : RegularExpressionAttribute
    {
        public DniPeruanoAttribute() : base(ValidationPatterns.Dni)
        {
            ErrorMessage = "El DNI debe contener exactamente 8 dígitos numéricos";
        }
    }

    /// <summary>
    /// Valida un RUC peruano de exactamente 11 dígitos numéricos
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class RucPeruanoAttribute : RegularExpressionAttribute
    {
        public RucPeruanoAttribute() : base(ValidationPatterns.Ruc)
        {
            ErrorMessage = "El RUC debe contener exactamente 11 dígitos";
        }
    }

    /// <summary>
    /// Valida un número de teléfono entre 7 y 15 dígitos (permite '+', espacios y guiones)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class TelefonoPeruanoAttribute : RegularExpressionAttribute
    {
        public TelefonoPeruanoAttribute() : base(ValidationPatterns.Telefono)
        {
            ErrorMessage = "Ingrese un teléfono válido (7 a 15 dígitos, ej. +51 956521578)";
        }
    }

    /// <summary>
    /// Valida el formato de un correo electrónico (permite nulo o vacío cuando el campo es opcional)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class CorreoElectronicoAttribute : ValidationAttribute
    {
        private static readonly EmailAddressAttribute _emailValidator = new();

        public CorreoElectronicoAttribute()
        {
            ErrorMessage = "El formato del correo electrónico no es válido";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            if (value is string texto && string.IsNullOrWhiteSpace(texto)) return true;
            return _emailValidator.IsValid(value);
        }
    }

    /// <summary>
    /// Valida textos de solo letras y espacios (opcionalmente permite puntos)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class SoloLetrasAttribute : RegularExpressionAttribute
    {
        public SoloLetrasAttribute(bool permitirPunto = false)
            : base(permitirPunto ? ValidationPatterns.SoloLetrasConPunto : ValidationPatterns.SoloLetras)
        {
            ErrorMessage = "Solo se permiten letras y espacios";
        }
    }

    /// <summary>
    /// Valida una razón social exigiendo al menos una letra.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class RazonSocialAttribute : RegularExpressionAttribute
    {
        public RazonSocialAttribute() : base(ValidationPatterns.RazonSocial)
        {
            ErrorMessage = "La razón social debe contener letras";
        }
    }

    /// <summary>
    /// Valida una URL http(s)://... o una ruta local dentro de /pictures/...
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class LogoRutaOUrlAttribute : RegularExpressionAttribute
    {
        public LogoRutaOUrlAttribute() : base(ValidationPatterns.LogoRutaOUrl)
        {
            ErrorMessage = "Ingrese una URL válida (http/https) o una ruta local en /pictures/";
        }
    }

    /// <summary>
    /// Valida que un valor numérico (int, long, decimal, double) no sea negativo (&gt;= 0)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NumeroNoNegativoAttribute : ValidationAttribute
    {
        public NumeroNoNegativoAttribute()
        {
            ErrorMessage = "El campo {0} no puede ser negativo";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            return value switch
            {
                int i => i >= 0,
                long l => l >= 0,
                decimal d => d >= 0m,
                double db => !double.IsNaN(db) && !double.IsInfinity(db) && db >= 0d,
                float f => !float.IsNaN(f) && !float.IsInfinity(f) && f >= 0f,
                _ => false
            };
        }
    }

    /// <summary>
    /// Valida que un valor numérico sea estrictamente mayor a cero (&gt; 0)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NumeroPositivoAttribute : ValidationAttribute
    {
        public NumeroPositivoAttribute()
        {
            ErrorMessage = "El campo {0} debe ser mayor a 0";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            return value switch
            {
                int i => i > 0,
                long l => l > 0,
                decimal d => d > 0m,
                double db => !double.IsNaN(db) && !double.IsInfinity(db) && db > 0d,
                float f => !float.IsNaN(f) && !float.IsInfinity(f) && f > 0f,
                _ => false
            };
        }
    }

    /// <summary>
    /// Valida fechas (DateTime o DateOnly): impide fechas pasadas/futuras o fuera de un rango de años
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class FechaRangoAttribute : ValidationAttribute
    {
        public bool NoPasada { get; set; }
        public bool NoFutura { get; set; }
        public int AnioMinimo { get; set; } = 2000;
        public int AnioMaximo { get; set; } = 2100;

        public FechaRangoAttribute()
        {
            ErrorMessage = "La fecha ingresada en {0} no está dentro del rango permitido";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            DateOnly fecha;
            if (value is DateTime dt)
            {
                if (dt == default) return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                fecha = DateOnly.FromDateTime(dt);
            }
            else if (value is DateOnly dOnly)
            {
                if (dOnly == default) return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                fecha = dOnly;
            }
            else
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            if (fecha.Year < AnioMinimo || fecha.Year > AnioMaximo)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"La fecha debe estar entre los años {AnioMinimo} y {AnioMaximo}");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            if (NoPasada && fecha < hoy)
            {
                return new ValidationResult(
                    ErrorMessage ?? "La fecha no puede ser anterior a hoy");
            }

            if (NoFutura && fecha > hoy)
            {
                return new ValidationResult(
                    ErrorMessage ?? "La fecha no puede ser posterior a hoy");
            }

            return ValidationResult.Success;
        }
    }
}
