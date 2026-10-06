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
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&#243;n del requisito.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto FSL_Requisitos_Requisito_Registrar(
            int CodEmpresa,
            FslRequisitoGuardarRequest request)
        {
            return FSL_Requisitos_Requisito_Guardar(
                CodEmpresa,
                request,
                true);
        }

        /// <summary>
        /// Actualiza un requisito existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&#243;n del requisito.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&#243;n.
        /// </returns>
        public ErrorDto FSL_Requisitos_Requisito_Actualizar(
            int CodEmpresa,
            FslRequisitoGuardarRequest request)
        {
            return FSL_Requisitos_Requisito_Guardar(
                CodEmpresa,
                request,
                false);
        }

        /// <summary>
        /// Elimina un requisito existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="codRequisito">
        /// C&#243;digo del requisito.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&#243;n.
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

            var validacion =
                FSL_Requisitos_CodigoUsuario_Validar(
                    codigo,
                    usuarioRegistro);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            const string sql = """
                DELETE FROM FSL_REQUISITOS
                WHERE COD_REQUISITO = @codigo;
                """;

            return FSL_Requisitos_Operacion_Ejecutar(
                CodEmpresa,
                new FslRequisitoOperacion
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
                });
        }

        /// <summary>
        /// Registra, actualiza o elimina la asignaci&#243;n
        /// de un requisito a un plan y una causa.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Estado final de la asignaci&#243;n.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&#243;n.
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

            var movimiento =
                response.Result ?? string.Empty;

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
        /// Ejecuta el registro o la actualizaci&#243;n de un
        /// requisito.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&#243;n del requisito.
        /// </param>
        /// <param name="registrar">
        /// Indica si corresponde registrar o actualizar.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&#243;n.
        /// </returns>
        private ErrorDto FSL_Requisitos_Requisito_Guardar(
            int CodEmpresa,
            FslRequisitoGuardarRequest request,
            bool registrar)
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

            var datos =
                FSL_Requisitos_Requisito_Datos_Obtener(
                    request);

            const string sql = """
                IF @registrar = 1
                BEGIN
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
                END
                ELSE
                BEGIN
                    UPDATE FSL_REQUISITOS
                    SET DESCRIPCION = @descripcion,
                        ACTIVO = @activo
                    WHERE COD_REQUISITO = @codigo;
                END;
                """;

            var movimiento =
                registrar ? "Registra" : "Modifica";

            var mensajeExito = registrar
                ? "Requisito registrado correctamente."
                : "Requisito actualizado correctamente.";

            var mensajeSinCambios = registrar
                ? "El requisito indicado ya existe."
                : "No se encontr&oacute; el requisito indicado.";

            return FSL_Requisitos_Operacion_Ejecutar(
                CodEmpresa,
                new FslRequisitoOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        registrar = registrar ? 1 : 0,
                        codigo = datos.Codigo,
                        descripcion = datos.Descripcion,
                        activo = datos.Activo ? 1 : 0,
                        usuario = datos.Usuario
                    },
                    Usuario = datos.Usuario,
                    Movimiento = movimiento,
                    Detalle =
                        "Requisitos (Lista) Id.:" +
                        datos.Codigo,
                    MensajeExito = mensajeExito,
                    MensajeSinCambios =
                        mensajeSinCambios
                });
        }

        /// <summary>
        /// Ejecuta una operaci&#243;n de mantenimiento y
        /// registra el movimiento en bit&#225;cora.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="operacion">
        /// Datos de la operaci&#243;n.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&#243;n.
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
        /// Obtiene los datos normalizados de un requisito.
        /// </summary>
        /// <param name="request">
        /// Informaci&#243;n original.
        /// </param>
        /// <returns>
        /// Informaci&#243;n preparada para persistencia.
        /// </returns>
        private static FslRequisitoDatos
            FSL_Requisitos_Requisito_Datos_Obtener(
                FslRequisitoGuardarRequest request)
        {
            return new FslRequisitoDatos
            {
                Codigo = request.cod_requisito
                    .Trim()
                    .ToUpperInvariant(),
                Descripcion =
                    request.descripcion.Trim(),
                Activo = request.activo,
                Usuario = request.usuario.Trim()
            };
        }

        /// <summary>
        /// Valida los datos requeridos para registrar o
        /// actualizar un requisito.
        /// </summary>
        /// <param name="request">
        /// Informaci&#243;n del requisito.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&#243;n o una cadena vac&#237;a.
        /// </returns>
        private static string
            FSL_Requisitos_Requisito_Validar(
                FslRequisitoGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.descripcion))
            {
                return MensajeDescripcionRequerida;
            }

            return FSL_Requisitos_CodigoUsuario_Validar(
                request.cod_requisito,
                request.usuario);
        }

        /// <summary>
        /// Valida los datos requeridos para actualizar una
        /// asignaci&#243;n.
        /// </summary>
        /// <param name="request">
        /// Informaci&#243;n de la asignaci&#243;n.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&#243;n o una cadena vac&#237;a.
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

            return FSL_Requisitos_CodigoUsuario_Validar(
                request.cod_requisito,
                request.usuario);
        }

        /// <summary>
        /// Valida el c&#243;digo del requisito y el usuario.
        /// </summary>
        /// <param name="codigo">
        /// C&#243;digo del requisito.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&#243;n o una cadena vac&#237;a.
        /// </returns>
        private static string
            FSL_Requisitos_CodigoUsuario_Validar(
                string codigo,
                string usuario)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return MensajeCodigoRequerido;
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        private sealed class FslRequisitoDatos
        {
            public string Codigo { get; init; } =
                string.Empty;

            public string Descripcion { get; init; } =
                string.Empty;

            public bool Activo { get; init; } = false;

            public string Usuario { get; init; } =
                string.Empty;
        }

        private sealed class FslRequisitoOperacion
        {
            public string Sql { get; init; } =
                string.Empty;

            public object Parametros { get; init; } =
                new();

            public string Usuario { get; init; } =
                string.Empty;

            public string Movimiento { get; init; } =
                string.Empty;

            public string Detalle { get; init; } =
                string.Empty;

            public string MensajeExito { get; init; } =
                string.Empty;

            public string MensajeSinCambios { get; init; } =
                string.Empty;
        }
    }
}