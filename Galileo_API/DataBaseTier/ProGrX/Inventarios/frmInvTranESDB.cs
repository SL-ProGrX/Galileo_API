using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvTranEsDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a las transacciones de inventario.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranEsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea el acceso a la base de datos de la empresa.
        /// </summary>
        /// <returns>Instancia de PortalDB.</returns>
        private PortalDB INV_TranES_PortalDB_Crear()
        {
            return new PortalDB(_config);
        }

        /// <summary>
        /// Comprueba que el tipo corresponda a una transacción admitida.
        /// </summary>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <returns>Tipo normalizado o cadena vacía si no es válido.</returns>
        private static string INV_TranES_Tipo_Normalizar(string? tipoTran)
        {
            string tipo = tipoTran?.Trim().ToUpperInvariant() ?? string.Empty;
            return tipo is "E" or "S" or "T" ? tipo : string.Empty;
        }

        /// <summary>
        /// Traduce el estado de la transacción para mostrarlo.
        /// </summary>
        /// <param name="estado">Estado almacenado.</param>
        /// <returns>Descripción del estado.</returns>
        private static string INV_TranES_Estado_Describir(string? estado)
        {
            return estado switch
            {
                "S" => "Solicitada",
                "A" => "Autorizada",
                "P" => "Procesada",
                "R" => "Rechazada",
                _ => estado ?? string.Empty
            };
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo de una transacción.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <returns>Boleta con diez posiciones.</returns>
        private static string INV_TranES_Boleta_Siguiente(
            IDbConnection connection,
            IDbTransaction transaction,
            string tipoTran)
        {
            const string query = """
                SELECT CONVERT(bigint, ISNULL(MAX(boleta), '0')) + 1
                FROM pv_InvTranSac WITH (UPDLOCK, HOLDLOCK)
                WHERE tipo = @TipoTran
                """;

            long consecutivo = connection.QuerySingle<long>(
                query,
                new { TipoTran = tipoTran },
                transaction);

            return consecutivo.ToString("D10", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Elimina el detalle de una boleta dentro de la transacción activa.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codBoleta">Código de la boleta.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        private static void INV_TranES_Lineas_Eliminar(
            IDbConnection connection,
            IDbTransaction transaction,
            string codBoleta,
            string tipoTran)
        {
            const string query = """
                DELETE FROM pv_InvTraDet
                WHERE boleta = @CodBoleta AND tipo = @TipoTran
                """;

            connection.Execute(
                query,
                new { CodBoleta = codBoleta, TipoTran = tipoTran },
                transaction);
        }

        /// <summary>
        /// Inserta una línea de producto en la boleta.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codBoleta">Código de la boleta.</param>
        /// <param name="tipoTran">Tipo de transacción.</param>
        /// <param name="linea">Número de línea.</param>
        /// <param name="item">Datos del producto.</param>
        private static void INV_TranES_Linea_Insertar(
            IDbConnection connection,
            IDbTransaction transaction,
            string codBoleta,
            string tipoTran,
            int linea,
            InvProducLineasInsert item)
        {
            const string queryNormal = """
                INSERT INTO pv_InvTraDet
                    (linea, boleta, tipo, cod_producto, cod_bodega,
                     cantidad, despacho, precio)
                VALUES
                    (@Linea, @CodBoleta, @TipoTran, @CodProducto, @CodBodega,
                     @Cantidad, @Despacho, @Precio)
                """;

            const string queryTraslado = """
                INSERT INTO pv_InvTraDet
                    (linea, boleta, tipo, cod_producto, cod_bodega,
                     cantidad, despacho, precio, cod_bodega_destino)
                VALUES
                    (@Linea, @CodBoleta, @TipoTran, @CodProducto, @CodBodega,
                     @Cantidad, @Despacho, @Precio, @CodBodegaDestino)
                """;

            connection.Execute(
                tipoTran == "T" ? queryTraslado : queryNormal,
                new
                {
                    Linea = linea,
                    CodBoleta = codBoleta,
                    TipoTran = tipoTran,
                    CodProducto = item.cod_producto.Trim(),
                    CodBodega = item.cod_bodega.Trim(),
                    item.cantidad,
                    item.despacho,
                    item.precio,
                    CodBodegaDestino = item.cod_bodega_destino.Trim()
                },
                transaction);
        }

        /// <summary>
        /// Obtiene el encabezado de una transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Encabezado de la transacción.</returns>
        public ErrorDto<TranESData> InvTranES_Obtener(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2,
                    new TranESData());
            }

            if (string.IsNullOrWhiteSpace(CodBoleta))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la boleta es requerido.",
                    -2,
                    new TranESData());
            }

            const string query = """
                SELECT X.*, RTRIM(C.descripcion) AS causa
                FROM PV_INVTRANSAC X
                INNER JOIN pv_entrada_salida C
                    ON X.cod_entsal = C.cod_entsal
                WHERE X.boleta = @CodBoleta
                  AND X.tipo = @TipoTran
                """;

            var result = DbHelper.ExecuteSingleQuery<TranESData>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                query,
                new TranESData(),
                new
                {
                    CodBoleta = CodBoleta.Trim(),
                    TipoTran = tipo
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1),
                    new TranESData());
            }

            TranESData data = result.Result ?? new TranESData();
            data.estado = INV_TranES_Estado_Describir(data.estado);
            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Obtiene las líneas de productos de una transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Líneas de productos.</returns>
        public ErrorDto<List<InvProducLineas>> InvProducLineas_Obtener(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2,
                    new List<InvProducLineas>());
            }

            const string queryNormal = """
                SELECT D.linea, D.cod_producto, P.descripcion,
                       D.cantidad, B.cod_bodega,
                       B.descripcion AS bodega,
                       D.precio,
                       D.cantidad * D.precio AS total,
                       ISNULL(D.despacho, 0) AS despacho
                FROM PV_INVTRADET D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN PV_Bodegas B
                    ON D.cod_bodega = B.cod_bodega
                WHERE D.boleta = @CodBoleta
                  AND D.tipo = @TipoTran
                ORDER BY D.linea
                """;

            const string queryTraslado = """
                SELECT D.linea, D.cod_producto, P.descripcion,
                       D.cantidad, B.cod_bodega,
                       B.descripcion AS bodega,
                       D.cod_bodega_destino,
                       D.precio,
                       D.cantidad * D.precio AS total,
                       ISNULL(D.despacho, 0) AS despacho
                FROM PV_INVTRADET D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN PV_Bodegas B
                    ON D.cod_bodega = B.cod_bodega
                WHERE D.boleta = @CodBoleta
                  AND D.tipo = @TipoTran
                ORDER BY D.linea
                """;

            var result = DbHelper.ExecuteListQuery<InvProducLineas>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                tipo == "T" ? queryTraslado : queryNormal,
                new
                {
                    CodBoleta = CodBoleta?.Trim() ?? string.Empty,
                    TipoTran = tipo
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ?? new List<InvProducLineas>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener las l&iacute;neas de la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1),
                    new List<InvProducLineas>());
        }

        /// <summary>
        /// Busca la boleta anterior o siguiente y devuelve su encabezado completo.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <param name="CodBoleta">Boleta actual.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Encabezado encontrado.</returns>
        public ErrorDto<TranESData> InvTranES_scroll(
            int CodEmpresa,
            int scrollValue,
            string? CodBoleta,
            string TipoTran)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2,
                    new TranESData());
            }

            const string querySiguiente = """
                SELECT TOP 1 boleta
                FROM pv_InvTranSac
                WHERE tipo = @TipoTran
                  AND boleta > @CodBoleta
                ORDER BY boleta ASC
                """;

            const string queryAnterior = """
                SELECT TOP 1 boleta
                FROM pv_InvTranSac
                WHERE tipo = @TipoTran
                  AND boleta < @CodBoleta
                ORDER BY boleta DESC
                """;

            var result = DbHelper.ExecuteSingleQuery<string>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                scrollValue == 1 ? querySiguiente : queryAnterior,
                string.Empty,
                new
                {
                    TipoTran = tipo,
                    CodBoleta = CodBoleta?.Trim() ?? string.Empty
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al desplazar la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1),
                    new TranESData());
            }

            return string.IsNullOrWhiteSpace(result.Result)
                ? DbHelper.CreateOkResponse(new TranESData())
                : InvTranES_Obtener(CodEmpresa, result.Result, tipo);
        }

        /// <summary>
        /// Obtiene las plantillas de transacciones según los filtros indicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="CodBoleta">Boleta opcional.</param>
        /// <param name="GeneraUser">Usuario generador opcional.</param>
        /// <param name="GeneraFecha">Fecha de generación opcional.</param>
        /// <returns>Listado de plantillas.</returns>
        public ErrorDto<List<InvTranPlantilla>> InvTranPlantilla_Obtener(
            int CodEmpresa,
            string TipoTran,
            string? CodBoleta,
            string? GeneraUser,
            string? GeneraFecha)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2,
                    new List<InvTranPlantilla>());
            }

            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;

            if (!string.IsNullOrWhiteSpace(GeneraFecha))
            {
                string[] formatos = ["yyyy-MM-dd", "yyyy/MM/dd"];

                if (!DateTime.TryParseExact(
                        GeneraFecha.Trim(),
                        formatos,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fecha))
                {
                    return DbHelper.CreateErrorResponse(
                        "La fecha debe utilizar el formato yyyy-MM-dd.",
                        -2,
                        new List<InvTranPlantilla>());
                }

                fechaInicio = fecha.Date;
                fechaFin = fecha.Date.AddDays(1);
            }

            const string query = """
                SELECT boleta, genera_user, genera_fecha, documento, notas
                FROM pv_InvTranSac
                WHERE plantilla = 1
                  AND tipo = @TipoTran
                  AND (@CodBoleta IS NULL OR boleta = @CodBoleta)
                  AND (@GeneraUser IS NULL OR genera_user LIKE @GeneraUser)
                  AND (@FechaInicio IS NULL OR genera_fecha >= @FechaInicio)
                  AND (@FechaFin IS NULL OR genera_fecha < @FechaFin)
                ORDER BY boleta DESC
                """;

            var parametros = new DynamicParameters();
            parametros.Add("TipoTran", tipo);
            parametros.Add(
                "CodBoleta",
                string.IsNullOrWhiteSpace(CodBoleta) ? null : CodBoleta.Trim());
            parametros.Add(
                "GeneraUser",
                string.IsNullOrWhiteSpace(GeneraUser)
                    ? null
                    : $"%{GeneraUser.Trim()}%");
            parametros.Add("FechaInicio", fechaInicio, DbType.DateTime);
            parametros.Add("FechaFin", fechaFin, DbType.DateTime);

            var result = DbHelper.ExecuteListQuery<InvTranPlantilla>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                query,
                parametros);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ?? new List<InvTranPlantilla>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener las plantillas.",
                    result.Code.GetValueOrDefault(-1),
                    new List<InvTranPlantilla>());
        }

        /// <summary>
        /// Registra el encabezado de una nueva transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="request">Datos del encabezado.</param>
        /// <returns>Boleta generada en la descripción del resultado.</returns>
        public ErrorDto InvTranES_Insertar(
            int CodEmpresa,
            string TipoTran,
            TranESData request)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0 || request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos de la transacci&oacute;n no son v&aacute;lidos.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del tipo de entrada o salida es requerido.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.genera_user))
            {
                return DbHelper.ErrorResponse(
                    "El usuario que genera la transacci&oacute;n es requerido.",
                    -2);
            }

            const string query = """
                INSERT INTO pv_InvTranSac
                    (boleta, tipo, cod_entsal, genera_fecha, documento,
                     notas, genera_user, estado, plantilla, fecha,
                     fecha_sistema, total)
                VALUES
                    (@Boleta, @TipoTran, @CodEntsal, GETDATE(), @Documento,
                     @Notas, @GeneraUser, 'S', @Plantilla,
                     COALESCE(@Fecha, GETDATE()), GETDATE(), @Total)
                """;

            var result = DbHelper.WithConn<string>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction =
                        connection.BeginTransaction(IsolationLevel.Serializable);

                    string boleta = INV_TranES_Boleta_Siguiente(
                        connection,
                        transaction,
                        tipo);

                    connection.Execute(
                        query,
                        new
                        {
                            Boleta = boleta,
                            TipoTran = tipo,
                            CodEntsal = request.cod_entsal.Trim(),
                            Documento = request.documento,
                            Notas = request.notas,
                            GeneraUser = request.genera_user.Trim(),
                            request.plantilla,
                            Fecha = request.fecha,
                            request.total
                        },
                        transaction);

                    transaction.Commit();
                    return boleta;
                });

            return result.Code == 0
                ? DbHelper.OkResponse(result.Result ?? string.Empty)
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al insertar la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el encabezado de una transacción solicitada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos a actualizar.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto InvTranES_Actualizar(
            int CodEmpresa,
            TranESUpdate request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.boleta) ||
                string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La boleta y el tipo de entrada o salida son requeridos.",
                    -2);
            }

            string tipo = INV_TranES_Tipo_Normalizar(request.tipo);
            if (tipo.Length == 0)
            {
                return DbHelper.ErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2);
            }

            const string query = """
                UPDATE pv_InvTranSac
                SET cod_entsal = @CodEntsal,
                    fecha = COALESCE(@Fecha, GETDATE()),
                    documento = @Documento,
                    notas = @Notas,
                    total = @Total,
                    plantilla = @Plantilla
                WHERE boleta = @Boleta
                  AND tipo = @TipoTran
                  AND estado = 'S'
                """;

            var result = DbHelper.ExecuteNonQueryWithResult(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                query,
                new
                {
                    Boleta = request.boleta.Trim(),
                    TipoTran = tipo,
                    CodEntsal = request.cod_entsal.Trim(),
                    Fecha = request.fecha,
                    Documento = request.documento,
                    Notas = request.notas,
                    request.total,
                    request.plantilla
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse("Registro actualizado correctamente")
                : DbHelper.ErrorResponse(
                    "La boleta no existe o ya no est&aacute; solicitada.",
                    -2);
        }

        /// <summary>
        /// Elimina el encabezado y sus líneas de detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvTranES_Eliminar(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0 || string.IsNullOrWhiteSpace(CodBoleta))
            {
                return DbHelper.ErrorResponse(
                    "La boleta y el tipo de transacci&oacute;n son requeridos.",
                    -2);
            }

            const string query = """
                DELETE FROM pv_InvTranSac
                WHERE boleta = @CodBoleta
                  AND tipo = @TipoTran
                """;

            var result = DbHelper.WithConn<bool>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    INV_TranES_Lineas_Eliminar(
                        connection,
                        transaction,
                        CodBoleta.Trim(),
                        tipo);

                    int filas = connection.Execute(
                        query,
                        new
                        {
                            CodBoleta = CodBoleta.Trim(),
                            TipoTran = tipo
                        },
                        transaction);

                    transaction.Commit();
                    return filas > 0;
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al eliminar la transacci&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse("La boleta no existe.", -2);
        }

        /// <summary>
        /// Reemplaza las líneas de productos de una boleta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="producLineas">Líneas a guardar.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto InvProducLineas_Insertar(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran,
            List<InvProducLineasInsert> producLineas)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0 ||
                string.IsNullOrWhiteSpace(CodBoleta) ||
                producLineas is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del detalle no son v&aacute;lidos.",
                    -2);
            }

            var result = DbHelper.WithConn<bool>(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    INV_TranES_Lineas_Eliminar(
                        connection,
                        transaction,
                        CodBoleta.Trim(),
                        tipo);

                    int posicion = 0;
                    foreach (InvProducLineasInsert item in producLineas)
                    {
                        posicion++;

                        if (item is null ||
                            string.IsNullOrWhiteSpace(item.cod_producto) ||
                            item.cantidad <= 0)
                        {
                            continue;
                        }

                        INV_TranES_Linea_Insertar(
                            connection,
                            transaction,
                            CodBoleta.Trim(),
                            tipo,
                            item.linea > 0 ? item.linea : posicion,
                            item);
                    }

                    transaction.Commit();
                    return true;
                });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Informaci&oacute;n guardada correctamente")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al guardar las l&iacute;neas.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una línea de producto de la boleta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodBoleta">Código de boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <param name="Linea">Número de línea.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvProducLineas_Eliminar(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran,
            int Linea)
        {
            string tipo = INV_TranES_Tipo_Normalizar(TipoTran);
            if (tipo.Length == 0 ||
                string.IsNullOrWhiteSpace(CodBoleta) ||
                Linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Los datos de la l&iacute;nea no son v&aacute;lidos.",
                    -2);
            }

            const string query = """
                DELETE FROM pv_InvTraDet
                WHERE tipo = @TipoTran
                  AND boleta = @CodBoleta
                  AND linea = @Linea
                """;

            var result = DbHelper.ExecuteNonQueryWithResult(
                INV_TranES_PortalDB_Crear(),
                CodEmpresa,
                query,
                new
                {
                    TipoTran = tipo,
                    CodBoleta = CodBoleta.Trim(),
                    Linea
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al eliminar la l&iacute;nea.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse("La l&iacute;nea no existe.", -2);
        }
    }
}