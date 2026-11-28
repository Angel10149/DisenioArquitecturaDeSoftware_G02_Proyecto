using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AppWeb.Validations
{
    /// <summary>
    /// Resultado inmutable de una validación centralizada
    /// </summary>
    public sealed record ValidationOutcome(bool EsValido, IReadOnlyList<string> Errores)
    {
        public string PrimerError => Errores.Count > 0 ? Errores[0] : string.Empty;
    }

    /// <summary>
    /// Utilidad estática para ejecutar validaciones de DataAnnotations y IValidatableObject
    /// desde la capa de servicios o desde pruebas automatizadas
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida todas las propiedades y reglas IValidatableObject de la instancia indicada
        /// </summary>
        public static ValidationOutcome Validar(object instancia)
        {
            ArgumentNullException.ThrowIfNull(instancia);

            var resultados = new List<ValidationResult>();
            var contexto = new ValidationContext(instancia);
            bool esValido = Validator.TryValidateObject(instancia, contexto, resultados, validateAllProperties: true);

            var errores = resultados
                .Select(r => r.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Cast<string>()
                .Distinct()
                .ToList();

            return new ValidationOutcome(esValido, errores);
        }

        /// <summary>
        /// Valida un documento de identidad peruano según su tipo (DNI, RUC, CE, Pasaporte)
        /// </summary>
        public static bool EsDocumentoValido(string? tipoDocumento, string? numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
                return false;

            var doc = numeroDocumento.Trim();
            return tipoDocumento.Trim() switch
            {
                "DNI" => Regex.IsMatch(doc, ValidationPatterns.Dni),
                "RUC" => Regex.IsMatch(doc, ValidationPatterns.Ruc),
                "CE" or "Pasaporte" => Regex.IsMatch(doc, ValidationPatterns.CeOPasaporte),
                _ => false
            };
        }
    }
}
