using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRequisitosDB
    {
        private const string MensajeCodigoRequerido =
            "El c&oacute;digo del requisito es requerido.";

        private const string MensajeDescripcionRequerida =
            "La descripci&oacute;n del requisito es requerida.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        /// <summary>
        /// Registra un requisito nuevo.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del requisito.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto FSL_Requisitos_Requisito_Registrar(
            int CodEmpresa,
            FslRequisitoGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_Requisitos_Requisito_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigo = request.cod_requisito
                .Trim()
                .ToUpperInvariant();

            var descripcion = request.descripcion.Trim();
            var usuario = request.usuario.Trim();

            const string sql = """
                INSERT INTO FSL_REQUISITOS
                (
                    COD_REQUISITO,
                    DESCRIPCION,
                    ACTIVO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                SELECT
                    @codigo,
                    @descripcion,
                    @activo,
                    GETDATE(),
                    @usuario
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM FSL_REQUISITOS
                    WHERE COD_REQUISITO = @codigo
                );
                """;

            var operacion = new FslRequisitoOperacion
            {
                Sql = sql,
                Parametros = new
                {
                    codigo,
                    descripcion,
                    activo = request.activo ? 1 : 0,
                    usuario
                },
                Usuario = usuario,
                Movimiento = "Registra",
                Detalle =
                    $"Requisitos (Lista) Id.:{codigo}",
                MensajeExito =
                    "Requisito registrado correctamente.",
                MensajeSinCambios =
                    "El requisito indicado ya existe."
            };

            return FSL_Requisitos_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Actualiza un requisito existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del requisito.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        public ErrorDto FSL_Requisitos_Requisito_Actualizar(
            int CodEmpresa,
            FslRequisitoGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_Requisitos_Requisito_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigo = request.cod_requisito
                .Trim()
                .ToUpperInvariant();

            var descripcion = request.descripcion.Trim();
            var usuario = request.usuario.Trim();

            const string sql = """
                UPDATE FSL_REQUISITOS
                SET
                    DESCRIPCION = @descripcion,
                    ACTIVO = @activo
                WHERE COD_REQUISITO = @codigo;
                """;

            var operacion = new FslRequisitoOperacion
            {
                Sql = sql,
                Parametros = new
                {
                    codigo,
                    descripcion,
                    activo = request.activo ? 1 : 0
                },
                Usuario = usuario,
                Movimiento = "Modifica",
                Detalle =
                    $"Requisitos (Lista) Id.:{codigo}",
                MensajeExito =
                    "Requisito actualizado correctamente.",
                MensajeSinCambios =
                    "No se encontr&oacute; el requisito indicado."
            };

            return FSL_Requisitos_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Elimina un requisito existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codRequisito">
        /// C&oacute;digo del requisito.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        public ErrorDto FSL_Requisitos_Requisito_Eliminar(
            int CodEmpresa,
            string codRequisito,
            string usuario)
        {
            var codigo = codRequisito
                .Trim()
                .ToUpperInvariant();

            var usuarioRegistro = usuario.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(usuarioRegistro))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            const string sql = """
                DELETE FROM FSL_REQUISITOS
                WHERE COD_REQUISITO = @codigo;
                """;

            var operacion = new FslRequisitoOperacion
            {
                Sql = sql,
                Parametros = new { codigo },
                Usuario = usuarioRegistro,
                Movimiento = "Elimina",
                Detalle =
                    $"Requisitos (Lista) Id.:{codigo}",
                MensajeExito =
                    "Requisito eliminado correctamente.",
                MensajeSinCambios =
                    "No se encontr&oacute; el requisito indicado."
            };

            return FSL_Requisitos_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Registra, actualiza o elimina la asignaci&oacute;n
        /// de un requisito a un plan y una causa.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Estado final de la asignaci&oacute;n.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_Requisitos_Asignacion_Actualizar(
                int CodEmpresa,
                FslRequisitoAsignacionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_Requisitos_Asignacion_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var plan = request.cod_plan
                .Trim()
                .ToUpperInvariant();

            var causa = request.cod_causa
                .Trim()
                .ToUpperInvariant();

            var requisito = request.cod_requisito
                .Trim()
                .ToUpperInvariant();

            var usuario = request.usuario.Trim();

            const string sql = """
                DECLARE @movimiento VARCHAR(10) = '';

                IF @asignado = 1
                BEGIN
                    IF EXISTS
                    (
                        SELECT 1
                        FROM FSL_REQUISITOS_CAUSAS
                        WHERE COD_PLAN = @plan
                          AND COD_CAUSA = @causa
                          AND COD_REQUISITO = @requisito
                    )
                    BEGIN
                        UPDATE FSL_REQUISITOS_CAUSAS
                        SET OPCIONAL = @opcional,
                            ASIGNADO = 1
                        WHERE COD_PLAN = @plan
                          AND COD_CAUSA = @causa
                          AND COD_REQUISITO = @requisito;

                        SET @movimiento = 'Modifica';
                    END
                    ELSE
                    BEGIN
                        INSERT INTO FSL_REQUISITOS_CAUSAS
                        (
                            COD_PLAN,
                            COD_CAUSA,
                            COD_REQUISITO,
                            OPCIONAL,
                            ASIGNADO,
                            REGISTRO_FECHA,
                            REGISTRO_USUARIO
                        )
                        VALUES
                        (
                            @plan,
                            @causa,
                            @requisito,
                            @opcional,
                            1,
                            GETDATE(),
                            @usuario
                        );

                        SET @movimiento = 'Registra';
                    END;
                END
                ELSE
                BEGIN
                    DELETE FROM FSL_REQUISITOS_CAUSAS
                    WHERE COD_PLAN = @plan
                      AND COD_CAUSA = @causa
                      AND COD_REQUISITO = @requisito;

                    IF @@ROWCOUNT > 0
                    BEGIN
                        SET @movimiento = 'Borrar';
                    END;
                END;

                SELECT @movimiento;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection.QueryFirstOrDefault<string>(
                        sql,
                        new
                        {
                            plan,
                            causa,
                            requisito,
                            opcional =
                                request.opcional ? 1 : 0,
                            asignado =
                                request.asignado ? 1 : 0,
                            usuario
                        }) ?? string.Empty);

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al actualizar la asignaci&oacute;n.");
            }

            var movimiento = response.Result ?? string.Empty;

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.ErrorResponse(
                    "No se encontr&oacute; la asignaci&oacute;n indicada.",
                    CodigoValidacion);
            }

            FSL_Requisitos_Bitacora_Registrar(
                CodEmpresa,
                usuario,
                movimiento,
                $"Requisito : {requisito} " +
                $"Plan: {plan} Causa: {causa}");

            return DbHelper.OkResponse(
                "Asignaci&oacute;n actualizada correctamente.");
        }

        /// <summary>
        /// Ejecuta una operaci&oacute;n de mantenimiento y
        /// registra el movimiento en bit&aacute;cora.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="operacion">
        /// Datos de la operaci&oacute;n.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        private ErrorDto FSL_Requisitos_Operacion_Ejecutar(
            int CodEmpresa,
            FslRequisitoOperacion operacion)
        {
            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    operacion.Sql,
                    operacion.Parametros);

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al procesar el requisito.");
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    operacion.MensajeSinCambios,
                    CodigoValidacion);
            }

            FSL_Requisitos_Bitacora_Registrar(
                CodEmpresa,
                operacion.Usuario,
                operacion.Movimiento,
                operacion.Detalle);

            return DbHelper.OkResponse(
                operacion.MensajeExito);
        }

        /// <summary>
        /// Valida los datos requeridos para registrar o
        /// actualizar un requisito.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n del requisito.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o una cadena vac&iacute;a.
        /// </returns>
        private static string
            FSL_Requisitos_Requisito_Validar(
                FslRequisitoGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.cod_requisito))
            {
                return MensajeCodigoRequerido;
            }

            if (string.IsNullOrWhiteSpace(
                request.descripcion))
            {
                return MensajeDescripcionRequerida;
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        /// <summary>
        /// Valida los datos requeridos para actualizar una
        /// asignaci&oacute;n.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n de la asignaci&oacute;n.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o una cadena vac&iacute;a.
        /// </returns>
        private static string
            FSL_Requisitos_Asignacion_Validar(
                FslRequisitoAsignacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return MensajePlanRequerido;
            }

            if (string.IsNullOrWhiteSpace(request.cod_causa))
            {
                return MensajeCausaRequerida;
            }

            if (string.IsNullOrWhiteSpace(
                request.cod_requisito))
            {
                return MensajeCodigoRequerido;
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        private sealed class FslRequisitoOperacion
        {
            public string Sql { get; init; } = string.Empty;
            public object Parametros { get; init; } = new();
            public string Usuario { get; init; } = string.Empty;
            public string Movimiento { get; init; } = string.Empty;
            public string Detalle { get; init; } = string.Empty;
            public string MensajeExito { get; init; } = string.Empty;
            public string MensajeSinCambios { get; init; } =
                string.Empty;
        }
    }
}