using Dapper;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        /// <summary>Valida la existencia y el estado de una toma física.</summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Estado_Validar(
            IDbConnection connection,
            IDbTransaction transaction,
            int consecutivo)
        {
            const string query = """
                SELECT TOP 1 RTRIM(ISNULL(estado, ''))
                FROM pv_InvTomaFisica WITH (UPDLOCK, HOLDLOCK)
                WHERE consecutivo = @Consecutivo;
                """;

            string? estado = connection.QueryFirstOrDefault<string>(
                query,
                new { Consecutivo = consecutivo },
                transaction);

            if (estado is null)
            {
                return MensajeTomaNoExiste;
            }

            return string.Equals(estado, EstadoSolicitado, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : MensajeTomaProcesada;
        }

        /// <summary>Valida la toma física y la bodega utilizadas por un proceso.</summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Contexto_Validar(
            IDbConnection connection,
            IDbTransaction? transaction,
            int consecutivo,
            string codBodega)
        {
            const string query = """
                SELECT TOP 1
                    RTRIM(ISNULL(estado, '')) AS estado,
                    RTRIM(ISNULL(cod_bodega, '')) AS cod_bodega
                FROM pv_InvTomaFisica
                WHERE consecutivo = @Consecutivo;
                """;

            var toma = connection.QueryFirstOrDefault<TomaFisicaDto>(
                query,
                new { Consecutivo = consecutivo },
                transaction);

            if (toma is null)
            {
                return MensajeTomaNoExiste;
            }

            if (!string.Equals(toma.estado, EstadoSolicitado, StringComparison.OrdinalIgnoreCase))
            {
                return MensajeTomaProcesada;
            }

            return string.Equals(toma.cod_bodega, codBodega.Trim(), StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : "La bodega no corresponde a la toma f&iacute;sica.";
        }

        /// <summary>Valida que existan la bodega y los productos que deben guardarse.</summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Información que debe validarse.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Referencias_Validar(
            IDbConnection connection,
            IDbTransaction transaction,
            TomaFisicaGuardarRequest request)
        {
            const string queryBodega = """
                SELECT COUNT(1)
                FROM pv_bodegas
                WHERE cod_bodega = @CodBodega;
                """;

            int existeBodega = connection.QueryFirstOrDefault<int>(
                queryBodega,
                new { CodBodega = request.toma.cod_bodega },
                transaction);

            if (existeBodega == 0)
            {
                return MensajeBodegaNoExiste;
            }

            string[] productos = request.detalle
                .Select(item => item.cod_producto.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (productos.Length == 0)
            {
                return string.Empty;
            }

            const string queryProductos = """
                SELECT COUNT(DISTINCT cod_producto)
                FROM pv_productos
                WHERE cod_producto IN @Productos;
                """;

            int productosEncontrados = connection.QueryFirstOrDefault<int>(
                queryProductos,
                new { Productos = productos },
                transaction);

            return productosEncontrados == productos.Length
                ? string.Empty
                : MensajeProductoNoExiste;
        }

        /// <summary>Valida la información requerida para guardar una toma física.</summary>
        /// <param name="request">Información de la toma.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Guardar_Validar(TomaFisicaGuardarRequest request)
        {
            if (request.toma is null)
            {
                return MensajeTomaRequerida;
            }

            if (string.IsNullOrWhiteSpace(request.toma.cod_bodega))
            {
                return MensajeBodegaRequerida;
            }

            if (!request.toma.fecha_inicio.HasValue)
            {
                return MensajeFechaInicioRequerida;
            }

            if (!request.toma.fecha_corte.HasValue)
            {
                return MensajeFechaCorteRequerida;
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return MensajeUsuarioRequerido;
            }

            if (request.detalle is null)
            {
                return "El detalle de la toma f&iacute;sica es requerido.";
            }

            if (request.detalle.Any(item => string.IsNullOrWhiteSpace(item.cod_producto)))
            {
                return MensajeProductoRequerido;
            }

            bool tieneDuplicados = request.detalle
                .GroupBy(item => new
                {
                    producto = item.cod_producto.Trim().ToUpperInvariant(),
                    ubicacion = item.ubicacion.Trim().ToUpperInvariant()
                })
                .Any(grupo => grupo.Count() > 1);

            return tieneDuplicados ? MensajeProductoDuplicado : string.Empty;
        }

        /// <summary>Valida una solicitud de inventario lógico.</summary>
        /// <param name="request">Información del proceso.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Inventario_Validar(TomaFisicaInventarioRequest request)
        {
            if (request.consecutivo <= 0)
            {
                return MensajeConsecutivoRequerido;
            }

            if (string.IsNullOrWhiteSpace(request.cod_bodega))
            {
                return MensajeBodegaRequerida;
            }

            if (!request.fecha_corte.HasValue)
            {
                return MensajeFechaCorteRequerida;
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        /// <summary>Valida los criterios de búsqueda de un producto.</summary>
        /// <param name="request">Criterios recibidos.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Producto_Validar(TomaFisicaProductoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cod_bodega))
            {
                return MensajeBodegaRequerida;
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return MensajeProductoRequerido;
            }

            string tipo = request.tipo.Trim().ToUpperInvariant();

            return tipo == TipoCodigoBarras || tipo == TipoCodigoProducto
                ? string.Empty
                : "El tipo de b&uacute;squeda no es v&aacute;lido.";
        }

        /// <summary>Valida una línea capturada mediante código de barras.</summary>
        /// <param name="linea">Línea recibida.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Barras_Validar(TomaFisicaDetalleDto linea)
        {
            if (linea.consecutivo <= 0)
            {
                return MensajeConsecutivoRequerido;
            }

            return string.IsNullOrWhiteSpace(linea.cod_producto)
                ? MensajeProductoRequerido
                : string.Empty;
        }

        /// <summary>Valida el encabezado asociado con una captura por código de barras.</summary>
        /// <param name="encabezado">Encabezado encontrado.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string INV_TomaFisica_Barras_Encabezado_Validar(TomaFisicaDto encabezado)
        {
            return string.Equals(encabezado.estado, EstadoSolicitado, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : MensajeTomaProcesada;
        }
    }
}