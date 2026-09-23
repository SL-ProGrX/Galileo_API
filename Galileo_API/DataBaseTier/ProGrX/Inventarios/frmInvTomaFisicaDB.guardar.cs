using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        /// <summary>
        /// Registra o actualiza una toma física y su detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Encabezado, detalle y usuario.</param>
        /// <returns>Consecutivo guardado.</returns>
        public ErrorDto<int> INV_TomaFisica_Guardar(
            int CodEmpresa,
            TomaFisicaGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTomaRequerida,
                    CodigoValidacion,
                    0);
            }

            string validacion =
                INV_TomaFisica_Guardar_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    0);
            }

            INV_TomaFisica_Guardar_Normalizar(request);

            bool nuevo = request.toma.consecutivo <= 0;

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    INV_TomaFisica_Guardar_Ejecutar(
                        connection,
                        request,
                        nuevo));

            var respuesta =
                INV_TomaFisica_Resultado_Obtener(
                    result,
                    ErrorGuardar,
                    0);

            if (respuesta.Code == 0)
            {
                INV_TomaFisica_Bitacora_Registrar(
                    CodEmpresa,
                    request.usuario,
                    nuevo
                        ? "Registra - WEB"
                        : "Modifica - WEB",
                    $"Toma Fisica Cod: {respuesta.Result}");
            }

            return respuesta;
        }

        /// <summary>
        /// Elimina una toma física solicitada y su detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="usuario">Usuario que elimina el registro.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_TomaFisica_Eliminar(
            int CodEmpresa,
            int consecutivo,
            string? usuario)
        {
            if (consecutivo <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeConsecutivoRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    INV_TomaFisica_Eliminar_Ejecutar(
                        connection,
                        consecutivo));

            var respuesta =
                INV_TomaFisica_Resultado_Obtener(
                    result,
                    ErrorEliminar);

            if (respuesta.Code == 0)
            {
                INV_TomaFisica_Bitacora_Registrar(
                    CodEmpresa,
                    usuario.Trim(),
                    "Elimina - WEB",
                    $"Toma Fisica Cod: {consecutivo}");
            }

            return respuesta;
        }

        /// <summary>
        /// Guarda una línea capturada mediante código de barras.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="linea">Información de la línea.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_TomaFisica_Barras_Guardar(
            int CodEmpresa,
            TomaFisicaDetalleDto linea)
        {
            if (linea is null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n del producto es requerida.",
                    CodigoValidacion);
            }

            string validacion =
                INV_TomaFisica_Barras_Validar(linea);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            linea.cod_bodega = linea.cod_bodega.Trim();
            linea.cod_producto =
                linea.cod_producto.Trim();
            linea.ubicacion = linea.ubicacion.Trim();

            var result = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    INV_TomaFisica_Barras_Guardar_Ejecutar(
                        connection,
                        linea));

            return INV_TomaFisica_Resultado_Obtener(
                result,
                ErrorGuardarBarras);
        }

        /// <summary>
        /// Ejecuta el guardado transaccional.
        /// </summary>
        /// <param name="connection">Conexión de base de datos.</param>
        /// <param name="request">Información que debe guardarse.</param>
        /// <param name="nuevo">Indica si es una toma nueva.</param>
        /// <returns>Consecutivo guardado.</returns>
        private static ErrorDto<int>
            INV_TomaFisica_Guardar_Ejecutar(
                IDbConnection connection,
                TomaFisicaGuardarRequest request,
                bool nuevo)
        {
            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                string validacion =
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
                        0);
                }

                int consecutivo = nuevo
                    ? INV_TomaFisica_Consecutivo_Generar(
                        connection,
                        transaction)
                    : request.toma.consecutivo;

                if (nuevo)
                {
                    INV_TomaFisica_Encabezado_Insertar(
                        connection,
                        transaction,
                        consecutivo,
                        request);
                }
                else
                {
                    validacion =
                        INV_TomaFisica_Estado_Validar(
                            connection,
                            transaction,
                            consecutivo);

                    if (!string.IsNullOrEmpty(validacion))
                    {
                        transaction.Rollback();

                        return DbHelper.CreateErrorResponse(
                            validacion,
                            CodigoValidacion,
                            0);
                    }

                    INV_TomaFisica_Encabezado_Actualizar(
                        connection,
                        transaction,
                        request);
                }

                INV_TomaFisica_Detalle_Reemplazar(
                    connection,
                    transaction,
                    consecutivo,
                    request.toma.cod_bodega,
                    request.detalle);

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    consecutivo,
                    "Informaci&oacute;n guardada satisfactoriamente.");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Ejecuta la eliminación transaccional.
        /// </summary>
        /// <param name="connection">Conexión de base de datos.</param>
        /// <param name="consecutivo">Consecutivo que debe eliminarse.</param>
        /// <returns>Resultado de la eliminación.</returns>
        private static ErrorDto
            INV_TomaFisica_Eliminar_Ejecutar(
                IDbConnection connection,
                int consecutivo)
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
                        consecutivo);

                if (!string.IsNullOrEmpty(validacion))
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        validacion,
                        CodigoValidacion);
                }

                const string queryDetalle = """
                    DELETE FROM pv_invTF_Detalle
                    WHERE consecutivo = @Consecutivo;
                    """;

                const string queryEncabezado = """
                    DELETE FROM pv_InvTomaFisica
                    WHERE consecutivo = @Consecutivo;
                    """;

                var parametros = new
                {
                    Consecutivo = consecutivo
                };

                connection.Execute(
                    queryDetalle,
                    parametros,
                    transaction);

                int registros = connection.Execute(
                    queryEncabezado,
                    parametros,
                    transaction);

                if (registros == 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        MensajeTomaNoExiste,
                        CodigoValidacion);
                }

                transaction.Commit();

                return DbHelper.OkResponse(
                    "Toma f&iacute;sica eliminada satisfactoriamente.");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Guarda una línea capturada por código de barras.
        /// </summary>
        /// <param name="connection">Conexión de base de datos.</param>
        /// <param name="linea">Línea que debe insertarse.</param>
        /// <returns>Resultado del guardado.</returns>
        private static ErrorDto
            INV_TomaFisica_Barras_Guardar_Ejecutar(
                IDbConnection connection,
                TomaFisicaDetalleDto linea)
        {
            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                const string queryEncabezado = """
                    SELECT TOP 1
                        RTRIM(ISNULL(estado, '')) AS estado,
                        RTRIM(ISNULL(cod_bodega, ''))
                            AS cod_bodega
                    FROM pv_InvTomaFisica WITH
                        (UPDLOCK, HOLDLOCK)
                    WHERE consecutivo = @Consecutivo;
                    """;

                var encabezado =
                    connection.QueryFirstOrDefault<
                        TomaFisicaDto>(
                            queryEncabezado,
                            new
                            {
                                Consecutivo =
                                    linea.consecutivo
                            },
                            transaction);

                if (encabezado is null)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        MensajeTomaNoExiste,
                        CodigoValidacion);
                }

                string validacion =
                    INV_TomaFisica_Barras_Encabezado_Validar(
                        encabezado);

                if (!string.IsNullOrEmpty(validacion))
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        validacion,
                        CodigoValidacion);
                }

                const string queryExiste = """
                    SELECT COUNT(1)
                    FROM pv_invTF_Detalle
                    WHERE consecutivo = @Consecutivo
                      AND cod_producto = @CodProducto
                      AND ISNULL(ubicacion, '') = @Ubicacion;
                    """;

                int existe =
                    connection.QueryFirstOrDefault<int>(
                        queryExiste,
                        new
                        {
                            Consecutivo =
                                linea.consecutivo,
                            CodProducto =
                                linea.cod_producto,
                            Ubicacion =
                                linea.ubicacion
                        },
                        transaction);

                if (existe > 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        MensajeProductoDuplicado,
                        CodigoValidacion);
                }

                const string queryProducto = """
                    SELECT COUNT(1)
                    FROM pv_productos
                    WHERE cod_producto = @CodProducto;
                    """;

                int existeProducto =
                    connection.QueryFirstOrDefault<int>(
                        queryProducto,
                        new
                        {
                            CodProducto =
                                linea.cod_producto
                        },
                        transaction);

                if (existeProducto == 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        MensajeProductoNoExiste,
                        CodigoValidacion);
                }

                INV_TomaFisica_Barras_Insertar(
                    connection,
                    transaction,
                    encabezado.cod_bodega,
                    linea);

                transaction.Commit();

                return DbHelper.OkResponse("Ok");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserta el encabezado de una toma física.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo generado.</param>
        /// <param name="request">Información que debe insertarse.</param>
        private static void
            INV_TomaFisica_Encabezado_Insertar(
                IDbConnection connection,
                IDbTransaction transaction,
                int consecutivo,
                TomaFisicaGuardarRequest request)
        {
            const string query = """
                INSERT INTO pv_InvTomaFisica
                (
                    consecutivo,
                    cod_bodega,
                    fecha_inicio,
                    fecha_corte,
                    estado,
                    fecha_crea,
                    user_crea,
                    notas
                )
                VALUES
                (
                    @Consecutivo,
                    @CodBodega,
                    @FechaInicio,
                    @FechaCorte,
                    'S',
                    Getdate(),
                    @Usuario,
                    @Notas
                );
                """;

            connection.Execute(
                query,
                new
                {
                    Consecutivo = consecutivo,
                    CodBodega =
                        request.toma.cod_bodega,
                    FechaInicio =
                        request.toma.fecha_inicio,
                    FechaCorte =
                        request.toma.fecha_corte,
                    Usuario = request.usuario,
                    Notas = request.toma.notas
                },
                transaction);
        }

        /// <summary>
        /// Actualiza el encabezado de una toma física.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Información que debe actualizarse.</param>
        private static void
            INV_TomaFisica_Encabezado_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                TomaFisicaGuardarRequest request)
        {
            const string query = """
                UPDATE pv_InvTomaFisica
                SET
                    notas = @Notas,
                    fecha_inicio = @FechaInicio,
                    fecha_corte = @FechaCorte,
                    cod_bodega = @CodBodega
                WHERE consecutivo = @Consecutivo;
                """;

            connection.Execute(
                query,
                new
                {
                    Notas = request.toma.notas,
                    FechaInicio =
                        request.toma.fecha_inicio,
                    FechaCorte =
                        request.toma.fecha_corte,
                    CodBodega =
                        request.toma.cod_bodega,
                    Consecutivo =
                        request.toma.consecutivo
                },
                transaction);
        }

        /// <summary>
        /// Reemplaza completamente el detalle.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="detalle">Líneas que deben guardarse.</param>
        private static void
            INV_TomaFisica_Detalle_Reemplazar(
                IDbConnection connection,
                IDbTransaction transaction,
                int consecutivo,
                string codBodega,
                IEnumerable<TomaFisicaDetalleDto> detalle)
        {
            const string queryEliminar = """
                DELETE FROM pv_invTF_Detalle
                WHERE consecutivo = @Consecutivo;
                """;

            connection.Execute(
                queryEliminar,
                new { Consecutivo = consecutivo },
                transaction);

            var parametros = detalle
                .Select(item => new
                {
                    Consecutivo = consecutivo,
                    CodBodega = codBodega,
                    CodProducto =
                        item.cod_producto.Trim(),
                    Ubicacion =
                        item.ubicacion.Trim(),
                    ExistenciaLogica =
                        item.existencia_logica,
                    ExistenciaFisica =
                        item.existencia_fisica
                })
                .ToList();

            if (parametros.Count == 0)
            {
                return;
            }

            const string queryInsertar = """
                INSERT INTO pv_invTF_Detalle
                (
                    consecutivo,
                    cod_bodega,
                    cod_producto,
                    ubicacion,
                    existencia_logica,
                    existencia_fisica
                )
                VALUES
                (
                    @Consecutivo,
                    @CodBodega,
                    @CodProducto,
                    @Ubicacion,
                    @ExistenciaLogica,
                    @ExistenciaFisica
                );
                """;

            connection.Execute(
                queryInsertar,
                parametros,
                transaction);
        }

        /// <summary>
        /// Inserta una línea capturada por código de barras.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <param name="linea">Línea capturada.</param>
        private static void
            INV_TomaFisica_Barras_Insertar(
                IDbConnection connection,
                IDbTransaction transaction,
                string codBodega,
                TomaFisicaDetalleDto linea)
        {
            const string query = """
                INSERT INTO pv_invTF_Detalle
                (
                    consecutivo,
                    cod_bodega,
                    cod_producto,
                    ubicacion,
                    existencia_logica,
                    existencia_fisica
                )
                VALUES
                (
                    @Consecutivo,
                    @CodBodega,
                    @CodProducto,
                    @Ubicacion,
                    @ExistenciaLogica,
                    @ExistenciaFisica
                );
                """;

            connection.Execute(
                query,
                new
                {
                    Consecutivo = linea.consecutivo,
                    CodBodega = codBodega,
                    CodProducto = linea.cod_producto,
                    Ubicacion = linea.ubicacion,
                    ExistenciaLogica =
                        linea.existencia_logica,
                    ExistenciaFisica =
                        linea.existencia_fisica
                },
                transaction);
        }
    }
}