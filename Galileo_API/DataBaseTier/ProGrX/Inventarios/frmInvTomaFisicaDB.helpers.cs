using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        /// <summary>
        /// Genera el siguiente consecutivo.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <returns>Siguiente consecutivo.</returns>
        private static int
            INV_TomaFisica_Consecutivo_Generar(
                IDbConnection connection,
                IDbTransaction transaction)
        {
            const string query = """
                SELECT ISNULL(MAX(consecutivo), 0) + 1
                FROM pv_InvTomaFisica WITH
                    (UPDLOCK, HOLDLOCK);
                """;

            return connection.QueryFirstOrDefault<int>(
                query,
                transaction: transaction);
        }

        /// <summary>
        /// Normaliza los datos que deben guardarse.
        /// </summary>
        /// <param name="request">Información recibida.</param>
        private static void
            INV_TomaFisica_Guardar_Normalizar(
                TomaFisicaGuardarRequest request)
        {
            request.toma.cod_bodega =
                request.toma.cod_bodega.Trim();

            request.toma.notas =
                request.toma.notas.Trim();

            request.usuario =
                request.usuario.Trim();

            foreach (var linea in request.detalle)
            {
                linea.cod_bodega =
                    request.toma.cod_bodega;

                linea.cod_producto =
                    linea.cod_producto.Trim();

                linea.ubicacion =
                    linea.ubicacion.Trim();
            }
        }

        /// <summary>
        /// Obtiene el tamaño de página permitido.
        /// </summary>
        /// <param name="paginacion">Tamaño solicitado.</param>
        /// <returns>Tamaño de página normalizado.</returns>
        private static int
            INV_TomaFisica_Paginacion_Obtener(
                int paginacion)
        {
            return paginacion <= 0
                ? PaginacionPredeterminada
                : Math.Min(
                    paginacion,
                    PaginacionMaxima);
        }

        /// <summary>
        /// Normaliza el campo de ordenamiento.
        /// </summary>
        /// <param name="sortField">Campo recibido.</param>
        /// <returns>Campo permitido.</returns>
        private static string
            INV_TomaFisica_Orden_Campo_Obtener(
                string? sortField)
        {
            return sortField?
                .Trim()
                .ToLowerInvariant() switch
            {
                "cod_bodega" => "cod_bodega",
                "fecha_inicio" => "fecha_inicio",
                "fecha_corte" => "fecha_corte",
                _ => "consecutivo"
            };
        }

        /// <summary>
        /// Normaliza la dirección de ordenamiento.
        /// </summary>
        /// <param name="sortOrder">Dirección recibida.</param>
        /// <returns>Uno o menos uno.</returns>
        private static int
            INV_TomaFisica_Orden_Direccion_Obtener(
                int sortOrder)
        {
            return sortOrder == 1 ? 1 : -1;
        }

        /// <summary>
        /// Escapa los caracteres especiales utilizados por LIKE.
        /// </summary>
        /// <param name="filtro">Filtro recibido.</param>
        /// <returns>Filtro escapado.</returns>
        private static string
            INV_TomaFisica_Filtro_Escapar(
                string filtro)
        {
            return filtro
                .Replace(
                    "\\",
                    "\\\\",
                    StringComparison.Ordinal)
                .Replace(
                    "%",
                    "\\%",
                    StringComparison.Ordinal)
                .Replace(
                    "_",
                    "\\_",
                    StringComparison.Ordinal)
                .Replace(
                    "[",
                    "\\[",
                    StringComparison.Ordinal);
        }

        /// <summary>
        /// Obtiene el resultado final de una operación.
        /// </summary>
        /// <param name="result">Resultado interno.</param>
        /// <param name="mensajeError">Mensaje predeterminado.</param>
        /// <returns>Resultado final.</returns>
        private static ErrorDto
            INV_TomaFisica_Resultado_Obtener(
                ErrorDto<ErrorDto> result,
                string mensajeError)
        {
            return result.Code == 0 &&
                   result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el resultado final de una operación genérica.
        /// </summary>
        /// <typeparam name="T">Tipo de respuesta.</typeparam>
        /// <param name="result">Resultado interno.</param>
        /// <param name="mensajeError">Mensaje predeterminado.</param>
        /// <param name="resultadoVacio">Resultado en caso de error.</param>
        /// <returns>Resultado final.</returns>
        private static ErrorDto<T>
            INV_TomaFisica_Resultado_Obtener<T>(
                ErrorDto<ErrorDto<T>> result,
                string mensajeError,
                T resultadoVacio)
        {
            return result.Code == 0 &&
                   result.Result is not null
                ? result.Result
                : DbHelper.CreateErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1),
                    resultadoVacio);
        }

        /// <summary>
        /// Registra una operación en la bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecutó la operación.</param>
        /// <param name="movimiento">Movimiento realizado.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        private void
            INV_TomaFisica_Bitacora_Registrar(
                int CodEmpresa,
                string usuario,
                string movimiento,
                string detalle)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle,
                    Modulo = ModuloInventarios
                });
        }
    }
}