using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        /// <summary>
        /// Calcula el inventario lógico de los productos registrados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Toma, bodega, fecha y usuario.</param>
        /// <returns>Detalle con las existencias lógicas.</returns>
        public ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_InventarioLogico_Obtener(
                int CodEmpresa,
                TomaFisicaInventarioRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTomaRequerida,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            string validacion =
                INV_TomaFisica_Inventario_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    INV_TomaFisica_InventarioLogico_Ejecutar(
                        connection,
                        request));

            return INV_TomaFisica_Resultado_Obtener(
                result,
                ErrorInventarioLogico,
                new List<TomaFisicaDetalleDto>());
        }

        /// <summary>
        /// Guarda la toma, incorpora faltantes y compara existencias.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Encabezado, detalle y usuario.</param>
        /// <returns>Detalle actualizado.</returns>
        public ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_Comparar(
                int CodEmpresa,
                TomaFisicaGuardarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTomaRequerida,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            string validacion =
                INV_TomaFisica_Guardar_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            if (request.toma.consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            INV_TomaFisica_Guardar_Normalizar(request);

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    INV_TomaFisica_Comparar_Ejecutar(
                        connection,
                        request));

            var respuesta =
                INV_TomaFisica_Resultado_Obtener(
                    result,
                    ErrorComparar,
                    new List<TomaFisicaDetalleDto>());

            if (respuesta.Code == 0)
            {
                INV_TomaFisica_Bitacora_Registrar(
                    CodEmpresa,
                    request.usuario,
                    "Compara - WEB",
                    $"Toma Fisica Cod: {request.toma.consecutivo}");
            }

            return respuesta;
        }

        /// <summary>
        /// Ejecuta el cálculo del inventario lógico.
        /// </summary>
        /// <param name="connection">Conexión de base de datos.</param>
        /// <param name="request">Información del proceso.</param>
        /// <returns>Detalle calculado.</returns>
        private static ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_InventarioLogico_Ejecutar(
                IDbConnection connection,
                TomaFisicaInventarioRequest request)
        {
            string validacion =
                INV_TomaFisica_Contexto_Validar(
                    connection,
                    null,
                    request.consecutivo,
                    request.cod_bodega);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            INV_TomaFisica_InventarioProceso_Ejecutar(
                connection,
                null,
                request.cod_bodega,
                request.fecha_corte,
                request.usuario);

            var detalle = request.cod_productos is null
                ? INV_TomaFisica_Detalle_Logico_Consultar(
                    connection,
                    request.consecutivo,
                    request.usuario)
                : INV_TomaFisica_Productos_Logico_Consultar(
                    connection,
                    request.cod_bodega,
                    request.usuario,
                    request.cod_productos);

            return DbHelper.CreateOkResponse(detalle);
        }

        /// <summary>
        /// Ejecuta la comparación de la toma física.
        /// </summary>
        /// <param name="connection">Conexión de base de datos.</param>
        /// <param name="request">Información de la toma.</param>
        /// <returns>Detalle actualizado.</returns>
        private static ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_Comparar_Ejecutar(
                IDbConnection connection,
                TomaFisicaGuardarRequest request)
        {
            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                string validacion =
                    INV_TomaFisica_Estado_Validar(
                        connection,
                        transaction,
                        request.toma.consecutivo);

                if (!string.IsNullOrEmpty(validacion))
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse(
                        validacion,
                        CodigoValidacion,
                        new List<TomaFisicaDetalleDto>());
                }

                validacion =
                    INV_TomaFisica_Referencias_Validar(
                        connection,
                        transaction,
                        request);

                if (!string.IsNullOrEmpty(validacion))
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse(
                        validacion,
                        CodigoValidacion,
                        new List<TomaFisicaDetalleDto>());
                }

                INV_TomaFisica_Encabezado_Actualizar(
                    connection,
                    transaction,
                    request);

                INV_TomaFisica_Detalle_Reemplazar(
                    connection,
                    transaction,
                    request.toma.consecutivo,
                    request.toma.cod_bodega,
                    request.detalle);

                INV_TomaFisica_InventarioProceso_Ejecutar(
                    connection,
                    transaction,
                    request.toma.cod_bodega,
                    request.toma.fecha_corte,
                    request.usuario);

                INV_TomaFisica_Faltantes_Insertar(
                    connection,
                    transaction,
                    request.toma.consecutivo,
                    request.toma.cod_bodega,
                    request.usuario);

                INV_TomaFisica_ExistenciaLogica_Actualizar(
                    connection,
                    transaction,
                    request.toma.consecutivo,
                    request.toma.cod_bodega,
                    request.usuario);

                var detalle =
                    INV_TomaFisica_Detalle_Consultar(
                        connection,
                        transaction,
                        request.toma.consecutivo);

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    detalle,
                    "Comparaci&oacute;n realizada satisfactoriamente.");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Ejecuta el proceso general de inventario.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="fechaCorte">Fecha de corte.</param>
        /// <param name="usuario">Usuario del proceso.</param>
        private static void
            INV_TomaFisica_InventarioProceso_Ejecutar(
                IDbConnection connection,
                IDbTransaction? transaction,
                string codBodega,
                DateTime? fechaCorte,
                string usuario)
        {
            const string query = """
                EXEC spINVProceso
                    @CodBodega,
                    0,
                    0,
                    @FechaCorte,
                    @Usuario,
                    '',
                    0,
                    NULL,
                    NULL;
                """;

            connection.Execute(
                query,
                new
                {
                    CodBodega = codBodega,
                    FechaCorte = fechaCorte,
                    Usuario = usuario
                },
                transaction,
                TiempoEsperaInventario);
        }

        /// <summary>
        /// Inserta los productos faltantes.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="usuario">Usuario del proceso.</param>
        private static void
            INV_TomaFisica_Faltantes_Insertar(
                IDbConnection connection,
                IDbTransaction transaction,
                int consecutivo,
                string codBodega,
                string usuario)
        {
            const string query = """
                INSERT INTO pv_invTF_Detalle
                (
                    consecutivo,
                    cod_bodega,
                    cod_producto,
                    existencia_logica,
                    existencia_fisica,
                    ubicacion
                )
                SELECT
                    @Consecutivo,
                    @CodBodega,
                    I.cod_producto,
                    ISNULL(
                        I.existencia_inicial +
                        I.entradas -
                        I.salidas,
                        0
                    ),
                    0,
                    ''
                FROM pv_inventario_proceso I
                INNER JOIN pv_productos P
                    ON I.cod_producto = P.cod_producto
                WHERE I.cod_bodega = @CodBodega
                  AND I.usuario = @Usuario
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM pv_invTF_Detalle D
                      WHERE D.consecutivo = @Consecutivo
                        AND D.cod_producto = I.cod_producto
                  );
                """;

            connection.Execute(
                query,
                new
                {
                    Consecutivo = consecutivo,
                    CodBodega = codBodega,
                    Usuario = usuario
                },
                transaction);
        }

        /// <summary>
        /// Actualiza la existencia lógica del detalle.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="usuario">Usuario del proceso.</param>
        private static void
            INV_TomaFisica_ExistenciaLogica_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                int consecutivo,
                string codBodega,
                string usuario)
        {
            const string query = """
                UPDATE D
                SET D.existencia_logica =
                    ISNULL(
                        I.existencia_inicial +
                        I.entradas -
                        I.salidas,
                        0
                    )
                FROM pv_invTF_Detalle D
                INNER JOIN pv_inventario_proceso I
                    ON D.cod_bodega = I.cod_bodega
                   AND D.cod_producto = I.cod_producto
                WHERE D.consecutivo = @Consecutivo
                  AND D.cod_bodega = @CodBodega
                  AND I.usuario = @Usuario;
                """;

            connection.Execute(
                query,
                new
                {
                    Consecutivo = consecutivo,
                    CodBodega = codBodega,
                    Usuario = usuario
                },
                transaction);
        }

        /// <summary>
        /// Consulta el detalle persistido.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <returns>Detalle persistido.</returns>
        private static List<TomaFisicaDetalleDto>
            INV_TomaFisica_Detalle_Consultar(
                IDbConnection connection,
                IDbTransaction transaction,
                int consecutivo)
        {
            const string query = """
                SELECT
                    D.consecutivo,
                    RTRIM(ISNULL(D.cod_bodega, '')) AS cod_bodega,
                    RTRIM(ISNULL(B.descripcion, '')) AS bodega,
                    RTRIM(ISNULL(D.cod_producto, '')) AS cod_producto,
                    RTRIM(ISNULL(P.descripcion, '')) AS descripcion,
                    RTRIM(ISNULL(P.tipo_producto, '')) AS tipo,
                    RTRIM(ISNULL(D.ubicacion, '')) AS ubicacion,
                    CAST(
                        ISNULL(D.existencia_logica, 0)
                        AS decimal(18, 4)
                    ) AS existencia_logica,
                    CAST(
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS existencia_fisica,
                    CAST(
                        ISNULL(D.existencia_logica, 0) -
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS diferencia
                FROM pv_invTF_Detalle D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN pv_bodegas B
                    ON D.cod_bodega = B.cod_bodega
                WHERE D.consecutivo = @Consecutivo
                ORDER BY D.cod_producto, D.ubicacion;
                """;

            return connection
                .Query<TomaFisicaDetalleDto>(
                    query,
                    new { Consecutivo = consecutivo },
                    transaction)
                .ToList();
        }

        /// <summary>
        /// Consulta el detalle con existencia lógica calculada.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="usuario">Usuario del proceso.</param>
        /// <returns>Detalle calculado.</returns>
        private static List<TomaFisicaDetalleDto>
            INV_TomaFisica_Detalle_Logico_Consultar(
                IDbConnection connection,
                int consecutivo,
                string usuario)
        {
            const string query = """
                SELECT
                    D.consecutivo,
                    RTRIM(ISNULL(D.cod_bodega, '')) AS cod_bodega,
                    RTRIM(ISNULL(B.descripcion, '')) AS bodega,
                    RTRIM(ISNULL(D.cod_producto, '')) AS cod_producto,
                    RTRIM(ISNULL(P.descripcion, '')) AS descripcion,
                    RTRIM(ISNULL(P.tipo_producto, '')) AS tipo,
                    RTRIM(ISNULL(D.ubicacion, '')) AS ubicacion,
                    CAST(
                        ISNULL(
                            I.existencia_inicial +
                            I.entradas -
                            I.salidas,
                            0
                        )
                        AS decimal(18, 4)
                    ) AS existencia_logica,
                    CAST(
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS existencia_fisica,
                    CAST(
                        ISNULL(
                            I.existencia_inicial +
                            I.entradas -
                            I.salidas,
                            0
                        ) -
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS diferencia
                FROM pv_invTF_Detalle D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN pv_bodegas B
                    ON D.cod_bodega = B.cod_bodega
                LEFT JOIN pv_inventario_proceso I
                    ON D.cod_bodega = I.cod_bodega
                   AND D.cod_producto = I.cod_producto
                   AND I.usuario = @Usuario
                WHERE D.consecutivo = @Consecutivo
                ORDER BY D.cod_producto, D.ubicacion;
                """;

            return connection
                .Query<TomaFisicaDetalleDto>(
                    query,
                    new
                    {
                        Consecutivo = consecutivo,
                        Usuario = usuario
                    })
                .ToList();
        }

        /// <summary>
        /// Consulta las existencias lógicas de los productos visibles en la toma.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="usuario">Usuario del proceso.</param>
        /// <param name="codProductos">Códigos de los productos visibles.</param>
        /// <returns>Productos encontrados con su existencia lógica.</returns>
        private static List<TomaFisicaDetalleDto>
            INV_TomaFisica_Productos_Logico_Consultar(
                IDbConnection connection,
                string codBodega,
                string usuario,
                IEnumerable<string> codProductos)
        {
            string[] productos = codProductos
                .Where(codigo => !string.IsNullOrWhiteSpace(codigo))
                .Select(codigo => codigo.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var resultado = new List<TomaFisicaDetalleDto>();

            if (productos.Length == 0)
            {
                return resultado;
            }

            const string query = """
            SELECT
                RTRIM(ISNULL(cod_producto, '')) AS cod_producto,
                CAST(
                    ISNULL(existencia_inicial + entradas - salidas, 0)
                    AS decimal(18, 4)
                ) AS existencia_logica
            FROM pv_inventario_proceso
            WHERE cod_bodega = @CodBodega
              AND usuario = @Usuario
              AND cod_producto IN @Productos;
            """;

            foreach (string[] lote in productos.Chunk(500))
            {
                resultado.AddRange(
                    connection.Query<TomaFisicaDetalleDto>(
                        query,
                        new
                        {
                            CodBodega = codBodega,
                            Usuario = usuario,
                            Productos = lote
                        },
                        commandTimeout: TiempoEsperaInventario));
            }

            return resultado;
        }
    }
}