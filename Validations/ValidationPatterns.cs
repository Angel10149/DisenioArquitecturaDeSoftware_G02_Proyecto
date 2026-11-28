namespace AppWeb.Validations
{
    /// <summary>
    /// Fuente única de verdad para expresiones regulares reutilizables en modelos, ViewModels y servicios
    /// </summary>
    public static class ValidationPatterns
    {
        /// <summary>Exactamente 8 dígitos numéricos (DNI peruano)</summary>
        public const string Dni = @"^[0-9]{8}$";

        /// <summary>Exactamente 11 dígitos numéricos (RUC peruano)</summary>
        public const string Ruc = @"^[0-9]{11}$";

        /// <summary>Carnet de extranjería o pasaporte: 6 a 20 caracteres alfanuméricos</summary>
        public const string CeOPasaporte = @"^[A-Za-z0-9]{6,20}$";

        /// <summary>Teléfono con 7 a 15 dígitos, opcional '+' al inicio, espacios o guiones (ej. +51 956521578)</summary>
        public const string Telefono = @"^\+?(?=(?:[^\d]*\d){7,15}[^\d]*$)[0-9\-\s]+$";

        /// <summary>Solo letras (incluyendo acentos y ñ) y espacios</summary>
        public const string SoloLetras = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$";

        /// <summary>Solo letras (incluyendo acentos y ñ), espacios y puntos (ej. Sr. Juan Pérez)</summary>
        public const string SoloLetrasConPunto = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\.]+$";

        /// <summary>Razón social: al menos una letra, permite números, espacios, puntos, guiones y '&amp;'</summary>
        public const string RazonSocial = @"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ])[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\.\-&]+$";

        /// <summary>Nombre de categoría: al menos una letra, permite letras, números, espacios y guiones</summary>
        public const string NombreCategoria = @"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ])[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\-]+$";

        /// <summary>Nombre de producto: exige al menos una letra (permite medidas, comillas, paréntesis, etc.)</summary>
        public const string NombreProducto = @"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ]).+$";

        /// <summary>Código de barras o SKU: letras, números, guiones y guion bajo</summary>
        public const string CodigoBarras = @"^[a-zA-Z0-9\-_]+$";

        /// <summary>Abreviatura de unidad de medida: 1 a 10 caracteres alfanuméricos con al menos una letra (ej. UNI, KG, M2)./summary>
        public const string AbreviaturaUnidad = @"^(?=.*[a-zA-Z])[a-zA-Z0-9]{1,10}$";

        /// <summary>Código de moneda ISO 4217 de 3 letras mayúsculas (ej. PEN, USD)</summary>
        public const string MonedaIso = @"^[A-Z]{3}$";

        /// <summary>Serie de boleta en formato B001</summary>
        public const string SerieBoleta = @"^B[0-9]{3}$";

        /// <summary>Serie de factura en formato F001</summary>
        public const string SerieFactura = @"^F[0-9]{3}$";

        /// <summary>URL absoluta http(s)://... o ruta local en /pictures/...</summary>
        public const string LogoRutaOUrl = @"^(https?://.+|/pictures/.+)$";

        /// <summary>Prioridad permitida en requerimientos de compra</summary>
        public const string PrioridadRequerimiento = @"^(Baja|Media|Alta)$";

        /// <summary>Tipos de documento permitidos para clientes</summary>
        public const string TipoDocumentoCliente = @"^(DNI|RUC|CE|Pasaporte)$";
    }
}
