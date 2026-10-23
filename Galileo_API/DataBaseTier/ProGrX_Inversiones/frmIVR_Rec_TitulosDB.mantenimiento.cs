using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrRecTitulosDb
    {
        /// <summary>
        /// Registra una inversión nueva conservando el proceso utilizado por ProGrX.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la inversión.</param>
        /// <returns>Identificador generado y mensaje del proceso.</returns>
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Registrar(
            int codEmpresa,
            IvrRecTitulosGuardarRequest? request)
        {
            if (request != null)
            {
                request.titulo_id = 0;
            }
            return IVR_Rec_Titulos_Guardar(codEmpresa, request, false);
        }

        /// <summary>
        /// Actualiza una inversión existente en estado de solicitud.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos actualizados de la inversión.</param>
        /// <returns>Identificador y mensaje del proceso.</returns>
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Actualizar(
            int codEmpresa,
            IvrRecTitulosGuardarRequest? request) =>
            IVR_Rec_Titulos_Guardar(codEmpresa, request, true);

        /// <summary>
        /// Activa una inversión en solicitud cuando su adquisición está completa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Inversión y usuario que ejecuta la activación.</param>
        /// <returns>Resultado de la activación.</returns>
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Activar(
            int codEmpresa,
            IvrRecTitulosAccionRequest? request)
        {
            var error = IVR_Rec_Titulos_Accion_Validar(request);
            if (error != null)
            {
                return error;
            }

            const string sql = """
                IF NOT EXISTS (
                    SELECT 1 FROM vIVR_INVERSIONES
                    WHERE TITULO_ID = @tituloId AND ESTADO = 'S')
                BEGIN
                    SELECT CONVERT(bigint, 0) AS titulo_id,
                           'Solo se puede activar una inversión en estado de solicitud.' AS mensaje;
                    RETURN;
                END;

                DECLARE @importe decimal(28, 8) =
                    ISNULL((SELECT SUM(IMPORTE)
                            FROM vIVR_TRANSACCIONES
                            WHERE REF_TIPO = 'T'
                              AND REF_CODIGO = CONVERT(varchar(30), @tituloId)
                              AND REF_CONCEPTO = 'T'), 0);
                DECLARE @liquidacion decimal(28, 8) =
                    ISNULL((SELECT TOP 1 VALOR_TRANSADO + COMISION_PUESTO + COMISION_BNV + IVA
                            FROM vIVR_INVERSIONES
                            WHERE TITULO_ID = @tituloId), 0);

                IF ABS(@liquidacion - @importe) > 0.01
                BEGIN
                    SELECT CONVERT(bigint, 0) AS titulo_id,
                           'El monto de adquisición se encuentra pendiente.' AS mensaje;
                    RETURN;
                END;

                EXEC spIVR_TITULOS_ACTIVA @tituloId, @usuario;
                SELECT @tituloId AS titulo_id,
                       'Inversión activada satisfactoriamente.' AS mensaje;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.QueryFirst<IvrRecTitulosGuardarData>(
                    sql,
                    new
                    {
                        tituloId = request!.titulo_id,
                        usuario = IVR_Rec_Titulos_Texto_Normalizar(request.usuario, 30),
                    }));
        }

        /// <summary>
        /// Elimina una adquisición perteneciente a una inversión en solicitud.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Inversión y transacción que se eliminarán.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto IVR_Rec_Titulos_Adquisicion_Eliminar(
            int codEmpresa,
            IvrRecTitulosTransaccionEliminarRequest? request)
        {
            if (request == null ||
                !IVR_Rec_Titulos_Id_Valido(request.titulo_id) ||
                request.transac_id <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La adquisición indicada no es válida.",
                    CodigoValidacion);
            }

            const string sql = """
                DELETE T
                FROM IVR_TRANSACCIONES T
                INNER JOIN vIVR_INVERSIONES I
                    ON I.TITULO_ID = @tituloId
                   AND I.ESTADO = 'S'
                WHERE T.TRANSAC_ID = @transacId
                  AND T.REF_TIPO = 'T'
                  AND T.REF_CODIGO = CONVERT(varchar(30), @tituloId)
                  AND T.REF_CONCEPTO = 'T';
                SELECT @@ROWCOUNT;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        tituloId = request.titulo_id,
                        transacId = request.transac_id,
                    }));

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ?? "No fue posible eliminar la adquisición.");
            }

            if (response.Result == 0)
            {
                return DbHelper.ErrorResponse(
                    "No se localizó una adquisición eliminable.",
                    CodigoValidacion);
            }

            return DbHelper.OkResponse("Adquisición eliminada satisfactoriamente.");
        }

        /// <summary>
        /// Recalcula la tabla de primas y descuentos con la tasa efectiva indicada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Inversión, tasa efectiva y usuario.</param>
        /// <returns>Resultado del recálculo.</returns>
        public ErrorDto IVR_Rec_Titulos_Pyd_Recalcular(
            int codEmpresa,
            IvrRecTitulosAccionRequest? request)
        {
            var error = IVR_Rec_Titulos_Accion_Validar(request);
            if (error != null)
            {
                return DbHelper.ErrorResponse(
                    error.Description ?? "Solicitud no válida.",
                    error.Code ?? -1);
            }
            if (request!.tasa_efectiva < 0 || request.tasa_efectiva > 1000)
            {
                return DbHelper.ErrorResponse("La tasa efectiva no es válida.", CodigoValidacion);
            }

            const string sql = "EXEC spIvr_Titulo_PyD_Recalculo_TEA @tituloId, @tasa, @usuario;";
            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Execute(
                    sql,
                    new
                    {
                        tituloId = request.titulo_id,
                        tasa = request.tasa_efectiva,
                        usuario = IVR_Rec_Titulos_Texto_Normalizar(request.usuario, 30),
                    }));
            return response.Code == 0
                ? DbHelper.OkResponse("Flujos de primas y descuentos actualizados satisfactoriamente.")
                : DbHelper.ErrorResponse(response.Description ?? "No fue posible recalcular los flujos.");
        }

        private ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Guardar(
            int codEmpresa,
            IvrRecTitulosGuardarRequest? request,
            bool actualizacion)
        {
            var validation = IVR_Rec_Titulos_Guardar_Validar(request, actualizacion);
            if (validation != null)
            {
                return validation;
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    if (actualizacion)
                    {
                        var estado = connection.QueryFirstOrDefault<string>(
                            "SELECT ESTADO FROM vIVR_INVERSIONES WHERE TITULO_ID = @tituloId;",
                            new { tituloId = request!.titulo_id });
                        if (!string.Equals(estado, "S", StringComparison.OrdinalIgnoreCase))
                        {
                            return new IvrRecTitulosGuardarData
                            {
                                titulo_id = 0,
                                mensaje = "Solo se puede guardar en estado de solicitud.",
                            };
                        }
                    }

                    return string.Equals(request!.tipo_instrumento, "F", StringComparison.OrdinalIgnoreCase)
                        ? IVR_Rec_Titulos_Fondo_Guardar(connection, request)
                        : IVR_Rec_Titulos_Titulo_Guardar(connection, request);
                });
        }

        private static IvrRecTitulosGuardarData IVR_Rec_Titulos_Titulo_Guardar(
            System.Data.IDbConnection connection,
            IvrRecTitulosGuardarRequest request)
        {
            const string sql = """
                EXEC spIVR_TITULOS_T_REGISTRA
                     @titulo_id, @cod_instrumento, @cod_categoria, @cod_emisor,
                     @cod_administrador, @cod_portafolio, @cod_recurso, @cod_reserva,
                     @cod_divisa, @cod_periodicidad, @base_intereses, @operacion, @serie,
                     @isin, @cusip, @ejecutivo_cuenta, @detalle, @fecha_compra,
                     @fecha_vencimiento, @tipo_cambio, @tasa_inicial, @precio,
                     @valor_facial, @valor_transado, @comision_puesto, @comision_bnv,
                     @iva, @ib_documento, @usuario, 0, 0, 0, @rend_neto,
                     @rend_nominal, @tir, @tasa_efectiva, @isr_monto;
                """;
            return connection.QueryFirst<IvrRecTitulosGuardarData>(sql, request);
        }

        private static IvrRecTitulosGuardarData IVR_Rec_Titulos_Fondo_Guardar(
            System.Data.IDbConnection connection,
            IvrRecTitulosGuardarRequest request)
        {
            const string sql = """
                EXEC spIVR_TITULOS_F_REGISTRA
                     @titulo_id, @cod_instrumento, @cod_categoria, @cod_emisor,
                     @cod_administrador, @cod_portafolio, @cod_recurso, @cod_reserva,
                     @cod_divisa, @operacion, @serie, @isin, @cusip,
                     @ejecutivo_cuenta, @detalle, @fecha_compra, @tipo_cambio,
                     @valor_facial, @participacion_valor, @participacion_numero,
                     @calcula_intereses, @ib_documento, @usuario;
                """;
            return connection.QueryFirst<IvrRecTitulosGuardarData>(sql, request);
        }

        private static ErrorDto<IvrRecTitulosGuardarData>? IVR_Rec_Titulos_Guardar_Validar(
            IvrRecTitulosGuardarRequest? request,
            bool actualizacion)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "No se recibieron los datos de la inversión.",
                    CodigoValidacion);
            }
            if (actualizacion && !IVR_Rec_Titulos_Id_Valido(request.titulo_id))
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "El número de inversión no es válido.",
                    CodigoValidacion);
            }
            if (string.IsNullOrWhiteSpace(request.cod_instrumento) ||
                string.IsNullOrWhiteSpace(request.operacion) ||
                string.IsNullOrWhiteSpace(request.serie) ||
                string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "Instrumento, operación, serie y usuario son requeridos.",
                    CodigoValidacion);
            }
            if (request.fecha_compra == default || request.tipo_cambio <= 0)
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "La fecha de compra o el tipo de cambio no son válidos.",
                    CodigoValidacion);
            }
            if (!string.Equals(request.tipo_instrumento, "T", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.tipo_instrumento, "F", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "El tipo de instrumento todavía no posee un proceso de registro en ProGrX.",
                    CodigoValidacion);
            }
            return null;
        }

        private static ErrorDto<IvrRecTitulosGuardarData>? IVR_Rec_Titulos_Accion_Validar(
            IvrRecTitulosAccionRequest? request)
        {
            if (request == null ||
                !IVR_Rec_Titulos_Id_Valido(request.titulo_id) ||
                string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosGuardarData>(
                    "La inversión y el usuario son requeridos.",
                    CodigoValidacion);
            }
            return null;
        }
    }
}
