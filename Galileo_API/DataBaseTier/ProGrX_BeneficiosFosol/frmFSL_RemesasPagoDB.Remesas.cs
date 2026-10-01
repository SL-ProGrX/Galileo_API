using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Registra una nueva remesa de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de la remesa.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto FSL_RemesasPago_Remesa_Registrar(
            int CodEmpresa,
            FslRemesaGuardarRequest request)
        {
            var validacion =
                FSL_RemesasPago_Remesa_Request_Validar(
                    request,
                    validarCodigo: false);

            if (validacion != null)
            {
                return validacion;
            }

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using var transaction =
                        connection.BeginTransaction();

                    const string sqlConsecutivo = """
                        SELECT
                            ISNULL(MAX(TESORERIA_REMESA), 0) + 1
                        FROM FSL_REMESAS_TESORERIA
                            WITH (UPDLOCK, HOLDLOCK);
                        """;

                    var consecutivo =
                        connection.QuerySingle<long>(
                            sqlConsecutivo,
                            transaction: transaction);

                    const string sqlInsertar = """
                        INSERT INTO FSL_REMESAS_TESORERIA
                        (
                            TESORERIA_REMESA,
                            REGISTRO_USUARIO,
                            REGISTRO_FECHA,
                            ESTADO,
                            FECHA_INICIO,
                            FECHA_CORTE,
                            NOTAS
                        )
                        VALUES
                        (
                            @consecutivo,
                            @usuario,
                            GETDATE(),
                            'A',
                            @fecha_inicio,
                            @fecha_corte,
                            @notas
                        );
                        """;

                    connection.Execute(
                        sqlInsertar,
                        new
                        {
                            consecutivo,
                            usuario =
                                request.usuario.Trim(),
                            fecha_inicio =
                                request.fecha_inicio.Date,
                            fecha_corte =
                                request.fecha_corte.Date,
                            notas = request.notas.Trim()
                        },
                        transaction);

                    transaction.Commit();
                    return consecutivo;
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al registrar la remesa.");
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Registra",
                $"Remesa FOSOL Traslado a Tesoreria : {response.Result}");

            return DbHelper.OkResponse(
                "Remesa registrada correctamente.");
        }

        /// <summary>
        /// Actualiza una remesa de tesorería que no se encuentre cerrada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información actualizada.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto FSL_RemesasPago_Remesa_Actualizar(
            int CodEmpresa,
            FslRemesaGuardarRequest request)
        {
            var validacion =
                FSL_RemesasPago_Remesa_Request_Validar(
                    request,
                    validarCodigo: true);

            if (validacion != null)
            {
                return validacion;
            }

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    const string sqlEstado = """
                        SELECT TOP 1
                            LTRIM(RTRIM(ISNULL(ESTADO, '')))
                        FROM FSL_REMESAS_TESORERIA
                        WHERE TESORERIA_REMESA = @cod_remesa;
                        """;

                    var estado =
                        connection.QueryFirstOrDefault<string>(
                            sqlEstado,
                            new
                            {
                                request.cod_remesa
                            });

                    if (estado == null)
                    {
                        return (
                            correcto: false,
                            mensaje: MensajeRemesaNoExiste);
                    }

                    if (estado == "C")
                    {
                        return (
                            correcto: false,
                            mensaje:
                                "No se puede modificar la remesa porque ya fue cerrada.");
                    }

                    const string sqlActualizar = """
                        UPDATE FSL_REMESAS_TESORERIA
                        SET REGISTRO_USUARIO = @usuario,
                            FECHA_INICIO = @fecha_inicio,
                            FECHA_CORTE = @fecha_corte,
                            NOTAS = @notas
                        WHERE TESORERIA_REMESA = @cod_remesa;
                        """;

                    connection.Execute(
                        sqlActualizar,
                        new
                        {
                            usuario =
                                request.usuario.Trim(),
                            fecha_inicio =
                                request.fecha_inicio.Date,
                            fecha_corte =
                                request.fecha_corte.Date,
                            notas = request.notas.Trim(),
                            request.cod_remesa
                        });

                    return (
                        correcto: true,
                        mensaje: string.Empty);
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al actualizar la remesa.");
            }

            if (!response.Result.correcto)
            {
                return DbHelper.ErrorResponse(
                    response.Result.mensaje,
                    CodigoValidacion);
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Modifica",
                $"Remesa FOSOL Traslado a Tesoreria : {request.cod_remesa}");

            return DbHelper.OkResponse(
                "Remesa actualizada correctamente.");
        }

        /// <summary>
        /// Elimina una remesa que permanezca abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto FSL_RemesasPago_Remesa_Eliminar(
            int CodEmpresa,
            long codRemesa,
            string usuario)
        {
            if (codRemesa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    const string sqlEstado = """
                        SELECT TOP 1
                            LTRIM(RTRIM(ISNULL(ESTADO, '')))
                        FROM FSL_REMESAS_TESORERIA
                        WHERE TESORERIA_REMESA = @codRemesa;
                        """;

                    var estado =
                        connection.QueryFirstOrDefault<string>(
                            sqlEstado,
                            new
                            {
                                codRemesa
                            });

                    if (estado == null)
                    {
                        return (
                            correcto: false,
                            mensaje: MensajeRemesaNoExiste);
                    }

                    if (estado != "A")
                    {
                        return (
                            correcto: false,
                            mensaje:
                                "Solo se pueden eliminar remesas abiertas.");
                    }

                    const string sqlEliminar = """
                        DELETE FROM FSL_REMESAS_TESORERIA
                        WHERE TESORERIA_REMESA = @codRemesa
                          AND ESTADO = 'A';
                        """;

                    connection.Execute(
                        sqlEliminar,
                        new
                        {
                            codRemesa
                        });

                    return (
                        correcto: true,
                        mensaje: string.Empty);
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al eliminar la remesa.");
            }

            if (!response.Result.correcto)
            {
                return DbHelper.ErrorResponse(
                    response.Result.mensaje,
                    CodigoValidacion);
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                usuario,
                "Elimina",
                $"Remesa FOSOL Traslado a Tesoreria : {codRemesa}");

            return DbHelper.OkResponse(
                "Remesa eliminada correctamente.");
        }

        /// <summary>
        /// Cierra una remesa abierta para permitir su traslado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Remesa y usuario responsable.</param>
        /// <returns>Resultado del cierre.</returns>
        public ErrorDto FSL_RemesasPago_Remesa_Cerrar(
            int CodEmpresa,
            FslRemesaCerrarRequest request)
        {
            if (request.cod_remesa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            const string sql = """
                UPDATE FSL_REMESAS_TESORERIA
                SET ESTADO = 'C'
                WHERE TESORERIA_REMESA = @cod_remesa
                  AND ESTADO = 'A';
                """;

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        request.cod_remesa
                    });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al cerrar la remesa.");
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La remesa indicada no existe o ya se encuentra cerrada.",
                    CodigoValidacion);
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Aplica",
                $"Cierra Remesa Traslado a Tesoreria : {request.cod_remesa}");

            return DbHelper.OkResponse(
                "Remesa cerrada satisfactoriamente.");
        }

        /// <summary>
        /// Valida los datos requeridos para registrar o actualizar una remesa.
        /// </summary>
        /// <param name="request">Información de la remesa.</param>
        /// <param name="validarCodigo">Indica si debe validarse el código.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto?
            FSL_RemesasPago_Remesa_Request_Validar(
                FslRemesaGuardarRequest request,
                bool validarCodigo)
        {
            if (validarCodigo &&
                request.cod_remesa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            if (request.fecha_inicio == default ||
                request.fecha_corte == default)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la fecha inicial y la fecha de corte.",
                    CodigoValidacion);
            }

            if (request.fecha_inicio.Date >
                request.fecha_corte.Date)
            {
                return DbHelper.ErrorResponse(
                    "La fecha inicial no puede ser mayor que la fecha de corte.",
                    CodigoValidacion);
            }

            return null;
        }
    }
}