using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public static class DbExceptionHelper
    {
        /// <summary>
        /// Traduce excepciones de EF Core y SQL Server a mensajes amigables en español.
        /// </summary>
        public static string Traducir(Exception ex)
        {
            var sqlEx = BuscarSqlException(ex);
            if (sqlEx != null)
            {
                return sqlEx.Number switch
                {
                    2601 or 2627 => "Ya existe un registro con los mismos datos únicos (valor duplicado).",
                    547 => "No se puede completar la operación porque el registro está relacionado con otros datos o falta una referencia válida.",
                    2628 or 8152 => "Uno de los textos ingresados supera la longitud máxima permitida en la base de datos.",
                    515 => "Falta un campo obligatorio requerido por la base de datos.",
                    -1 or -2 or 2 or 53 or 4060 or 10054 or 10060 or 18456 =>
                        "No se pudo conectar con la base de datos. Verifique la conexión e intente nuevamente.",
                    _ => "Ocurrió un error en la base de datos al procesar la operación."
                };
            }

            if (ex is DbUpdateConcurrencyException)
            {
                return "El registro fue modificado o eliminado por otro usuario. Recargue la página e intente nuevamente.";
            }

            if (ex is DbUpdateException)
            {
                return "No se pudieron guardar los cambios en la base de datos. Verifique los datos ingresados.";
            }

            if (ex is TimeoutException)
            {
                return "La operación excedió el tiempo de espera. Intente nuevamente.";
            }

            return "Ocurrió un error inesperado al procesar la operación. Intente nuevamente.";
        }

        private static SqlException? BuscarSqlException(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is SqlException sqlEx)
                {
                    return sqlEx;
                }
                ex = ex.InnerException;
            }
            return null;
        }
    }
}
