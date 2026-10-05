using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Registra un expediente y genera requisitos y operaciones.
        /// </summary>
        public ErrorDto<FslExpedienteGuardarResultado>
            FSL_Expediente_Insertar(
                int CodEmpresa,
                FslExpedienteGuardarRequest request)
        {
            var validacion = FSL_Expediente_Datos_Validar(request);
            if (validacion != null)
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new FslExpedienteGuardarResultado());
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (!FSL_Expediente_Registro_Valido(
                        connection,
                        transaction,
                        request.cedula,
                        request.cod_plan,
                        request.cod_causa,
                        0))
                {
                    return DbHelper.CreateErrorResponse(
                        "El caso ya fue presentado anteriormente, verifique.",
                        CodigoValidacion,
                        new FslExpedienteGuardarResultado());
                }

                var tipoDesembolso =
                    FSL_Expediente_TipoDesembolso_Obtener(
                        connection,
                        transaction,
                        request.cod_plan);

                const string sql = """
                    INSERT INTO FSL_EXPEDIENTES
                    (
                        COD_EXPEDIENTE,
                        CEDULA,
                        COD_PLAN,
                        COD_CAUSA,
                        COD_COMITE,
                        COD_ENFERMEDAD,
                        ESTADO,
                        RESOLUCION_ESTADO,
                        PRESENTA_CEDULA,
                        PRESENTA_NOMBRE,
                        PRESENTA_NOTAS,
                        REFERENCIA_DOCUMENTO,
                        REFERENCIA_NUMERO,
                        ENFERMEDAD_FECHA,
                        ENFERMEDAD_USUARIO,
                        ENFERMEDAD_NOTAS,
                        FECHA_ESTABLECE_CAUSA,
                        NOTAS,
                        TOTAL_DISPONIBLE,
                        TOTAL_APLICADO,
                        TOTAL_SOBRANTE,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO,
                        TIPO_DESEMBOLSO
                    )
                    OUTPUT INSERTED.COD_EXPEDIENTE
                    VALUES
                    (
                        dbo.fxFSL_ExpedienteConsecutivo(),
                        @cedula,
                        @cod_plan,
                        @cod_causa,
                        @cod_comite,
                        @cod_enfermedad,
                        'P',
                        'P',
                        @presenta_cedula,
                        @presenta_nombre,
                        @presenta_notas,
                        @referencia_documento,
                        @referencia_numero,
                        @enfermedad_fecha,
                        @registro_usuario,
                        @enfermedad_notas,
                        @fecha_establece_causa,
                        @notas,
                        0,
                        0,
                        0,
                        GETDATE(),
                        @registro_usuario,
                        @tipo_desembolso
                    );
                    """;

                var codExpediente = connection.QuerySingle<long>(
                    sql,
                    new
                    {
                        cedula = request.cedula.Trim(),
                        cod_plan = request.cod_plan.Trim(),
                        cod_causa = request.cod_causa.Trim(),
                        cod_comite = request.cod_comite.Trim(),
                        cod_enfermedad = request.cod_enfermedad.Trim(),
                        presenta_cedula = request.presenta_cedula.Trim(),
                        presenta_nombre = request.presenta_nombre.Trim(),
                        presenta_notas = request.presenta_notas.Trim(),
                        referencia_documento =
                            request.referencia_documento.Trim(),
                        referencia_numero =
                            request.referencia_numero.Trim(),
                        request.enfermedad_fecha,
                        enfermedad_notas =
                            request.enfermedad_notas.Trim(),
                        request.fecha_establece_causa,
                        notas = request.notas.Trim(),
                        registro_usuario =
                            request.registro_usuario.Trim(),
                        tipo_desembolso = tipoDesembolso
                    },
                    transaction);

                FSL_Expediente_Procesos_Ejecutar(
                    connection,
                    transaction,
                    codExpediente,
                    request.registro_usuario);

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    new FslExpedienteGuardarResultado
                    {
                        cod_expediente = codExpediente
                    },
                    "Expediente registrado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FslExpedienteGuardarResultado());
            }
        }

        /// <summary>
        /// Actualiza un expediente pendiente.
        /// </summary>
        public ErrorDto
            FSL_Expediente_Actualizar(
                int CodEmpresa,
                FslExpedienteGuardarRequest request)
        {
            if (request.cod_expediente <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion);
            }

            var validacion = FSL_Expediente_Datos_Validar(request);
            if (validacion != null)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var estado = connection.QueryFirstOrDefault<string>(
                    """
                    SELECT ESTADO
                    FROM FSL_EXPEDIENTES WITH (UPDLOCK)
                    WHERE COD_EXPEDIENTE = @cod_expediente;
                    """,
                    new { request.cod_expediente },
                    transaction);

                if (string.IsNullOrWhiteSpace(estado))
                {
                    return DbHelper.ErrorResponse(
                        MensajeExpedienteNoExiste,
                        CodigoValidacion);
                }

                if (!string.Equals(
                        estado.Trim(),
                        EstadoPendiente,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.ErrorResponse(
                        MensajeExpedienteNoPendiente,
                        CodigoValidacion);
                }

                var tipoDesembolso =
                    FSL_Expediente_TipoDesembolso_Obtener(
                        connection,
                        transaction,
                        request.cod_plan);

                const string sql = """
                    UPDATE FSL_EXPEDIENTES
                    SET
                        COD_PLAN = @cod_plan,
                        COD_CAUSA = @cod_causa,
                        COD_COMITE = @cod_comite,
                        COD_ENFERMEDAD = @cod_enfermedad,
                        NOTAS = @notas,
                        PRESENTA_CEDULA = @presenta_cedula,
                        PRESENTA_NOMBRE = @presenta_nombre,
                        PRESENTA_NOTAS = @presenta_notas,
                        REFERENCIA_DOCUMENTO =
                            @referencia_documento,
                        REFERENCIA_NUMERO = @referencia_numero,
                        FECHA_ESTABLECE_CAUSA =
                            @fecha_establece_causa,
                        ENFERMEDAD_FECHA = @enfermedad_fecha,
                        ENFERMEDAD_NOTAS = @enfermedad_notas,
                        MODIFICA_USUARIO = @modifica_usuario,
                        MODIFICA_FECHA = GETDATE(),
                        TIPO_DESEMBOLSO = @tipo_desembolso
                    WHERE COD_EXPEDIENTE = @cod_expediente;
                    """;

                connection.Execute(
                    sql,
                    new
                    {
                        cod_plan = request.cod_plan.Trim(),
                        cod_causa = request.cod_causa.Trim(),
                        cod_comite = request.cod_comite.Trim(),
                        cod_enfermedad = request.cod_enfermedad.Trim(),
                        notas = request.notas.Trim(),
                        presenta_cedula = request.presenta_cedula.Trim(),
                        presenta_nombre = request.presenta_nombre.Trim(),
                        presenta_notas = request.presenta_notas.Trim(),
                        referencia_documento =
                            request.referencia_documento.Trim(),
                        referencia_numero =
                            request.referencia_numero.Trim(),
                        request.fecha_establece_causa,
                        request.enfermedad_fecha,
                        enfermedad_notas =
                            request.enfermedad_notas.Trim(),
                        modifica_usuario =
                            request.modifica_usuario.Trim(),
                        tipo_desembolso = tipoDesembolso,
                        request.cod_expediente
                    },
                    transaction);

                FSL_Expediente_Procesos_Ejecutar(
                    connection,
                    transaction,
                    request.cod_expediente,
                    request.modifica_usuario);

                transaction.Commit();

                return DbHelper.OkResponse(
                    "Expediente actualizado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el cumplimiento de un requisito.
        /// </summary>
        public ErrorDto
            FSL_Expediente_Requisito_Actualizar(
                int CodEmpresa,
                FslExpedienteRequisitoActualizarRequest request)
        {
            if (request.cod_expediente <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.cod_requisito))
            {
                return DbHelper.ErrorResponse(
                    "El requisito es requerido.",
                    CodigoValidacion);
            }

            const string sql = """
                UPDATE R
                SET
                    R.ESTADO = @estado,
                    R.REGISTRO_FECHA = GETDATE(),
                    R.REGISTRO_USUARIO = @registro_usuario
                FROM FSL_EXPEDIENTES_REQUISITOS R
                INNER JOIN FSL_EXPEDIENTES E
                    ON R.COD_EXPEDIENTE = E.COD_EXPEDIENTE
                WHERE R.COD_EXPEDIENTE = @cod_expediente
                  AND R.COD_REQUISITO = @cod_requisito
                  AND E.ESTADO = 'P';
                """;

            var respuesta = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                sql,
                new
                {
                    estado = request.estado ? 1 : 0,
                    registro_usuario =
                        request.registro_usuario.Trim(),
                    request.cod_expediente,
                    cod_requisito = request.cod_requisito.Trim()
                });

            if (respuesta.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    respuesta.Description ?? "Error al actualizar el requisito.");
            }

            return respuesta.Result > 0
                ? DbHelper.OkResponse(
                    "Requisito actualizado satisfactoriamente.")
                : DbHelper.ErrorResponse(
                    "El expediente no est&aacute; pendiente o el requisito no existe.",
                    CodigoValidacion);
        }

        /// <summary>
        /// Guarda la resoluci&#243;n y sus miembros.
        /// </summary>
        public ErrorDto
            FSL_Expediente_Resolucion_Guardar(
                int CodEmpresa,
                FslExpedienteResolucionGuardarRequest request)
        {
            var validacion =
                FSL_Expediente_ResolucionRequest_Validar(request);

            if (validacion != null)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var contexto =
                    FSL_Expediente_ResolucionContexto_Obtener(
                        connection,
                        transaction,
                        request);

                var error =
                    FSL_Expediente_ResolucionContexto_Validar(
                        contexto,
                        request);

                if (error != null)
                {
                    return DbHelper.ErrorResponse(
                        error,
                        CodigoValidacion);
                }

                const string sqlActualizar = """
                    UPDATE FSL_EXPEDIENTES
                    SET
                        RESOLUCION_NOTAS = @resolucion_notas,
                        RESOLUCION_ESTADO = @resolucion_estado,
                        RESOLUCION_FECHA = GETDATE(),
                        RESOLUCION_USUARIO = @resolucion_usuario,
                        ESTADO = @resolucion_estado
                    WHERE COD_EXPEDIENTE = @cod_expediente;
                    """;

                connection.Execute(
                    sqlActualizar,
                    new
                    {
                        resolucion_notas =
                            request.resolucion_notas.Trim(),
                        resolucion_estado =
                            request.resolucion_estado.Trim(),
                        resolucion_usuario =
                            request.resolucion_usuario.Trim(),
                        request.cod_expediente
                    },
                    transaction);

                connection.Execute(
                    """
                    DELETE FROM FSL_EXPEDIENTE_COMITE
                    WHERE COD_EXPEDIENTE = @cod_expediente;
                    """,
                    new { request.cod_expediente },
                    transaction);

                const string sqlInsertar = """
                    INSERT INTO FSL_EXPEDIENTE_COMITE
                    (
                        COD_EXPEDIENTE,
                        COD_COMITE,
                        CEDULA,
                        ASIGNA_FECHA,
                        ASIGNA_USUARIO,
                        RESOLUCION_ESTADO
                    )
                    VALUES
                    (
                        @cod_expediente,
                        @cod_comite,
                        @cedula,
                        GETDATE(),
                        @resolucion_usuario,
                        @resolucion_estado
                    );
                    """;

                var miembros = request.miembros
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(item.cedula))
                    .Select(item => item.cedula.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(cedula => new
                    {
                        request.cod_expediente,
                        cod_comite = request.cod_comite.Trim(),
                        cedula,
                        resolucion_usuario =
                            request.resolucion_usuario.Trim(),
                        resolucion_estado =
                            request.resolucion_estado.Trim()
                    })
                    .ToList();

                connection.Execute(
                    sqlInsertar,
                    miembros,
                    transaction);

                transaction.Commit();

                return DbHelper.OkResponse(
                    "Expediente actualizado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string
            FSL_Expediente_TipoDesembolso_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                string codPlan)
        {
            const string sql = """
                SELECT RTRIM(ISNULL(TIPO_DESEMBOLSO, ''))
                FROM FSL_PLANES
                WHERE COD_PLAN = @cod_plan;
                """;

            return connection.QueryFirstOrDefault<string>(
                       sql,
                       new { cod_plan = codPlan.Trim() },
                       transaction) ??
                   string.Empty;
        }

        private static void
            FSL_Expediente_Procesos_Ejecutar(
                SqlConnection connection,
                SqlTransaction transaction,
                long codExpediente,
                string usuario)
        {
            var parametros = new
            {
                Expediente = codExpediente,
                Usuario = usuario.Trim()
            };

            connection.Execute(
                "spFSL_ExpedienteRequisitos",
                parametros,
                transaction,
                commandType: CommandType.StoredProcedure);

            connection.Execute(
                "spFSL_ExpedienteOperaciones",
                parametros,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static FslResolucionContexto
            FSL_Expediente_ResolucionContexto_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                FslExpedienteResolucionGuardarRequest request)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(Ex.ESTADO, '')) AS estado,
                    ISNULL(Co.NUMERO_RESOLUTORES, 0)
                        AS numero_resolutores,
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaRequisitos(
                            Ex.COD_EXPEDIENTE))
                        AS cumple_requisitos,
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaTiempoPresentacion(
                            Ex.COD_EXPEDIENTE))
                        AS cumple_tiempo,
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaRegistro(
                            Ex.CEDULA,
                            Ex.COD_PLAN,
                            Ex.COD_CAUSA,
                            Ex.COD_EXPEDIENTE))
                        AS cumple_registro
                FROM FSL_EXPEDIENTES Ex WITH (UPDLOCK)
                INNER JOIN FSL_COMITES Co
                    ON Ex.COD_COMITE = Co.COD_COMITE
                WHERE Ex.COD_EXPEDIENTE = @cod_expediente
                  AND Ex.COD_COMITE = @cod_comite;
                """;

            return connection.QueryFirstOrDefault<FslResolucionContexto>(
                       sql,
                       new
                       {
                           request.cod_expediente,
                           cod_comite = request.cod_comite.Trim()
                       },
                       transaction) ??
                   new FslResolucionContexto();
        }
    }
}