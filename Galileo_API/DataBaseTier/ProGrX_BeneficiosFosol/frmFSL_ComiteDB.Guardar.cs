using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslComiteDB
    {
        /// <summary>
        /// Registra un nuevo comit&eacute; de FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del comit&eacute;.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto FSL_Comite_Comite_Registrar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            var validacion =
                FSL_Comite_Comite_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                INSERT INTO FSL_COMITES
                (
                    COD_COMITE,
                    DESCRIPCION,
                    NUMERO_RESOLUTORES,
                    ACTIVO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                SELECT
                    @cod_comite,
                    @descripcion,
                    @numero_resolutores,
                    @activo,
                    GETDATE(),
                    @usuario
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM FSL_COMITES
                    WHERE COD_COMITE = @cod_comite
                );
                """;

            var codComite =
                request.cod_comite.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros =
                        FSL_Comite_Comite_Parametros_Crear(
                            request,
                            codComite),
                    Usuario = request.usuario,
                    Movimiento = "Registra",
                    Detalle =
                        $"Comité de FOSOL Id.:{codComite}",
                    MensajeError =
                        "Ocurri&oacute; un error al registrar el comit&eacute;.",
                    MensajeSinCambios =
                        "El c&oacute;digo del comit&eacute; ya existe.",
                    MensajeExito =
                        "Comit&eacute; registrado correctamente."
                });
        }

        /// <summary>
        /// Actualiza un comit&eacute; existente.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del comit&eacute;.</param>
        /// <returns>Resultado de la actualizaci&oacute;n.</returns>
        public ErrorDto FSL_Comite_Comite_Actualizar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            var validacion =
                FSL_Comite_Comite_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                UPDATE FSL_COMITES
                SET DESCRIPCION = @descripcion,
                    NUMERO_RESOLUTORES =
                        @numero_resolutores,
                    ACTIVO = @activo
                WHERE COD_COMITE = @cod_comite;
                """;

            var codComite =
                request.cod_comite.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros =
                        FSL_Comite_Comite_Parametros_Crear(
                            request,
                            codComite),
                    Usuario = request.usuario,
                    Movimiento = "Modifica",
                    Detalle =
                        $"Comité de FOSOL Id.:{codComite}",
                    MensajeError =
                        "Ocurri&oacute; un error al actualizar el comit&eacute;.",
                    MensajeSinCambios =
                        "El comit&eacute; indicado no existe.",
                    MensajeExito =
                        "Comit&eacute; actualizado correctamente."
                });
        }

        /// <summary>
        /// Elimina un comit&eacute; de FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Resultado de la eliminaci&oacute;n.</returns>
        public ErrorDto FSL_Comite_Comite_Eliminar(
            int CodEmpresa,
            string codComite,
            string usuario)
        {
            var validacion =
                FSL_Comite_Eliminar_Validar(
                    codComite,
                    usuario);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM FSL_COMITES
                WHERE COD_COMITE = @codComite;
                """;

            var codigo = codComite.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        codComite = codigo
                    },
                    Usuario = usuario,
                    Movimiento = "Elimina",
                    Detalle =
                        $"Comité de FOSOL Id.:{codigo}",
                    MensajeError =
                        "Ocurri&oacute; un error al eliminar el comit&eacute;.",
                    MensajeSinCambios =
                        "El comit&eacute; indicado no existe.",
                    MensajeExito =
                        "Comit&eacute; eliminado correctamente."
                });
        }

        /// <summary>
        /// Registra un nuevo miembro en un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto FSL_Comite_Miembro_Registrar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            var validacion =
                FSL_Comite_Miembro_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                INSERT INTO FSL_COMITES_MIEMBROS
                (
                    CEDULA,
                    COD_COMITE,
                    NOMBRE,
                    USUARIO_VINCULADO,
                    ACTIVO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                SELECT
                    @cedula,
                    @cod_comite,
                    @nombre,
                    @usuario_vinculado,
                    @activo,
                    GETDATE(),
                    @usuario
                WHERE EXISTS
                (
                    SELECT 1
                    FROM FSL_COMITES
                    WHERE COD_COMITE = @cod_comite
                )
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM FSL_COMITES_MIEMBROS
                    WHERE COD_COMITE = @cod_comite
                      AND CEDULA = @cedula
                );
                """;

            var codComite =
                request.cod_comite.Trim();

            var cedula =
                request.cedula.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros =
                        FSL_Comite_Miembro_Parametros_Crear(
                            request,
                            codComite,
                            cedula),
                    Usuario = request.usuario,
                    Movimiento = "Registra",
                    Detalle =
                        $"Comité Miembro: {codComite}.. Id.:{cedula}",
                    MensajeError =
                        "Ocurri&oacute; un error al registrar el miembro.",
                    MensajeSinCambios =
                        "El miembro ya existe o el comit&eacute; indicado no est&aacute; disponible.",
                    MensajeExito =
                        "Miembro registrado correctamente."
                });
        }

        /// <summary>
        /// Actualiza un miembro existente de un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <returns>Resultado de la actualizaci&oacute;n.</returns>
        public ErrorDto FSL_Comite_Miembro_Actualizar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            var validacion =
                FSL_Comite_Miembro_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                UPDATE FSL_COMITES_MIEMBROS
                SET NOMBRE = @nombre,
                    USUARIO_VINCULADO =
                        @usuario_vinculado,
                    ACTIVO = @activo,
                    SALIDA_FECHA =
                        CASE
                            WHEN @activo = 0
                            THEN GETDATE()
                            ELSE SALIDA_FECHA
                        END,
                    SALIDA_USUARIO =
                        CASE
                            WHEN @activo = 0
                            THEN @usuario
                            ELSE SALIDA_USUARIO
                        END
                WHERE COD_COMITE = @cod_comite
                  AND CEDULA = @cedula;
                """;

            var codComite =
                request.cod_comite.Trim();

            var cedula =
                request.cedula.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros =
                        FSL_Comite_Miembro_Parametros_Crear(
                            request,
                            codComite,
                            cedula),
                    Usuario = request.usuario,
                    Movimiento = "Modifica",
                    Detalle =
                        $"Comité Miembro: {codComite}.. Id.:{cedula}",
                    MensajeError =
                        "Ocurri&oacute; un error al actualizar el miembro.",
                    MensajeSinCambios =
                        "El miembro indicado no existe en el comit&eacute;.",
                    MensajeExito =
                        "Miembro actualizado correctamente."
                });
        }

        /// <summary>
        /// Elimina un miembro de un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="cedula">C&eacute;dula del miembro.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Resultado de la eliminaci&oacute;n.</returns>
        public ErrorDto FSL_Comite_Miembro_Eliminar(
            int CodEmpresa,
            string codComite,
            string cedula,
            string usuario)
        {
            var validacion =
                FSL_Comite_Miembro_Eliminar_Validar(
                    codComite,
                    cedula,
                    usuario);

            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM FSL_COMITES_MIEMBROS
                WHERE COD_COMITE = @codComite
                  AND CEDULA = @cedula;
                """;

            var codigo = codComite.Trim();
            var identificacion = cedula.Trim();

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                new FslComiteOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        codComite = codigo,
                        cedula = identificacion
                    },
                    Usuario = usuario,
                    Movimiento = "Elimina",
                    Detalle =
                        $"Comité Miembro: {codigo} .. Id.:{identificacion}",
                    MensajeError =
                        "Ocurri&oacute; un error al eliminar el miembro.",
                    MensajeSinCambios =
                        "El miembro indicado no existe en el comit&eacute;.",
                    MensajeExito =
                        "Miembro eliminado correctamente."
                });
        }

        /// <summary>
        /// Ejecuta una operaci&oacute;n de mantenimiento y registra
        /// el movimiento realizado en la bit&aacute;cora.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="operacion">Configuraci&oacute;n de la operaci&oacute;n.</param>
        /// <returns>Resultado de la operaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Operacion_Ejecutar(
            int CodEmpresa,
            FslComiteOperacion operacion)
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
                    operacion.MensajeError);
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    operacion.MensajeSinCambios,
                    CodigoValidacion);
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                operacion.Usuario,
                operacion.Movimiento,
                operacion.Detalle);

            return DbHelper.OkResponse(
                operacion.MensajeExito);
        }

        /// <summary>
        /// Crea los par&aacute;metros SQL de un comit&eacute;.
        /// </summary>
        /// <param name="request">Informaci&oacute;n del comit&eacute;.</param>
        /// <param name="codComite">C&oacute;digo normalizado.</param>
        /// <returns>Par&aacute;metros de la operaci&oacute;n.</returns>
        private static object
            FSL_Comite_Comite_Parametros_Crear(
                FslComiteGuardarRequest request,
                string codComite)
        {
            return new
            {
                cod_comite = codComite,
                descripcion =
                    request.descripcion.Trim(),
                request.numero_resolutores,
                request.activo,
                usuario =
                    request.usuario.Trim()
            };
        }

        /// <summary>
        /// Crea los par&aacute;metros SQL de un miembro.
        /// </summary>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <param name="codComite">C&oacute;digo del comit&eacute; normalizado.</param>
        /// <param name="cedula">C&eacute;dula normalizada.</param>
        /// <returns>Par&aacute;metros de la operaci&oacute;n.</returns>
        private static object
            FSL_Comite_Miembro_Parametros_Crear(
                FslComiteMiembroGuardarRequest request,
                string codComite,
                string cedula)
        {
            return new
            {
                cod_comite = codComite,
                cedula,
                nombre =
                    request.nombre.Trim(),
                usuario_vinculado =
                    request.usuario_vinculado.Trim(),
                request.activo,
                usuario =
                    request.usuario.Trim()
            };
        }

        /// <summary>
        /// Valida la informaci&oacute;n requerida de un comit&eacute;.
        /// </summary>
        /// <param name="request">Informaci&oacute;n recibida.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Comite_Request_Validar(
                FslComiteGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            return FSL_Comite_Campos_Requeridos_Validar(
                (
                    request.cod_comite,
                    MensajeComiteRequerido
                ),
                (
                    request.usuario,
                    MensajeUsuarioRequerido
                ));
        }

        /// <summary>
        /// Valida la informaci&oacute;n requerida de un miembro.
        /// </summary>
        /// <param name="request">Informaci&oacute;n recibida.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Miembro_Request_Validar(
                FslComiteMiembroGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            return FSL_Comite_Campos_Requeridos_Validar(
                (
                    request.cod_comite,
                    MensajeComiteRequerido
                ),
                (
                    request.cedula,
                    MensajeCedulaRequerida
                ),
                (
                    request.usuario,
                    MensajeUsuarioRequerido
                ));
        }

        /// <summary>
        /// Valida los datos requeridos para eliminar un comit&eacute;.
        /// </summary>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Eliminar_Validar(
                string codComite,
                string usuario)
        {
            return FSL_Comite_Campos_Requeridos_Validar(
                (
                    codComite,
                    MensajeComiteRequerido
                ),
                (
                    usuario,
                    MensajeUsuarioRequerido
                ));
        }

        /// <summary>
        /// Valida los datos requeridos para eliminar un miembro.
        /// </summary>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="cedula">C&eacute;dula del miembro.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Miembro_Eliminar_Validar(
                string codComite,
                string cedula,
                string usuario)
        {
            return FSL_Comite_Campos_Requeridos_Validar(
                (
                    codComite,
                    MensajeComiteRequerido
                ),
                (
                    cedula,
                    MensajeCedulaRequerida
                ),
                (
                    usuario,
                    MensajeUsuarioRequerido
                ));
        }

        /// <summary>
        /// Valida una colecci&oacute;n de campos requeridos.
        /// </summary>
        /// <param name="campos">Valores y mensajes de validaci&oacute;n.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Campos_Requeridos_Validar(
                params (
                    string Valor,
                    string Mensaje
                )[] campos)
        {
            foreach (var campo in campos)
            {
                if (string.IsNullOrWhiteSpace(
                    campo.Valor))
                {
                    return DbHelper.ErrorResponse(
                        campo.Mensaje,
                        CodigoValidacion);
                }
            }

            return null;
        }

        private sealed class FslComiteOperacion
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

            public string MensajeError { get; init; } =
                string.Empty;

            public string MensajeSinCambios { get; init; } =
                string.Empty;

            public string MensajeExito { get; init; } =
                string.Empty;
        }
    }
}