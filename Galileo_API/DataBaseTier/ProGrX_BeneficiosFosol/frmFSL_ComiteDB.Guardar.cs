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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        cod_comite = codComite,
                        descripcion =
                            request.descripcion.Trim(),
                        request.numero_resolutores,
                        request.activo,
                        usuario =
                            request.usuario.Trim()
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al registrar el comit&eacute;.",
                    "El c&oacute;digo del comit&eacute; ya existe.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Registra",
                $"Comité de FOSOL Id.:{codComite}");

            return DbHelper.OkResponse(
                "Comit&eacute; registrado correctamente.");
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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        cod_comite = codComite,
                        descripcion =
                            request.descripcion.Trim(),
                        request.numero_resolutores,
                        request.activo
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al actualizar el comit&eacute;.",
                    "El comit&eacute; indicado no existe.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Modifica",
                $"Comité de FOSOL Id.:{codComite}");

            return DbHelper.OkResponse(
                "Comit&eacute; actualizado correctamente.");
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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        codComite = codigo
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al eliminar el comit&eacute;.",
                    "El comit&eacute; indicado no existe.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                usuario,
                "Elimina",
                $"Comité de FOSOL Id.:{codigo}");

            return DbHelper.OkResponse(
                "Comit&eacute; eliminado correctamente.");
        }

        /// <summary>
        /// Registra un nuevo miembro en un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto
            FSL_Comite_Miembro_Registrar(
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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        cod_comite = codComite,
                        cedula,
                        nombre =
                            request.nombre.Trim(),
                        usuario_vinculado =
                            request.usuario_vinculado
                                .Trim(),
                        request.activo,
                        usuario =
                            request.usuario.Trim()
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al registrar el miembro.",
                    "El miembro ya existe o el comit&eacute; indicado no est&aacute; disponible.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Registra",
                $"Comité Miembro: {codComite}.. Id.:{cedula}");

            return DbHelper.OkResponse(
                "Miembro registrado correctamente.");
        }

        /// <summary>
        /// Actualiza un miembro existente de un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <returns>Resultado de la actualizaci&oacute;n.</returns>
        public ErrorDto
            FSL_Comite_Miembro_Actualizar(
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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        cod_comite = codComite,
                        cedula,
                        nombre =
                            request.nombre.Trim(),
                        usuario_vinculado =
                            request.usuario_vinculado
                                .Trim(),
                        request.activo,
                        usuario =
                            request.usuario.Trim()
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al actualizar el miembro.",
                    "El miembro indicado no existe en el comit&eacute;.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Modifica",
                $"Comité Miembro: {codComite}.. Id.:{cedula}");

            return DbHelper.OkResponse(
                "Miembro actualizado correctamente.");
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

            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        codComite = codigo,
                        cedula = identificacion
                    });

            var error =
                FSL_Comite_Operacion_Resultado_Validar(
                    response,
                    "Ocurri&oacute; un error al eliminar el miembro.",
                    "El miembro indicado no existe en el comit&eacute;.");

            if (error != null)
            {
                return error;
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                usuario,
                "Elimina",
                $"Comité Miembro: {codigo} .. Id.:{identificacion}");

            return DbHelper.OkResponse(
                "Miembro eliminado correctamente.");
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

            if (string.IsNullOrWhiteSpace(
                request.cod_comite))
            {
                return DbHelper.ErrorResponse(
                    MensajeComiteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
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

            if (string.IsNullOrWhiteSpace(
                request.cod_comite))
            {
                return DbHelper.ErrorResponse(
                    MensajeComiteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.cedula))
            {
                return DbHelper.ErrorResponse(
                    MensajeCedulaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
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
            if (string.IsNullOrWhiteSpace(
                codComite))
            {
                return DbHelper.ErrorResponse(
                    MensajeComiteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
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
            var validacion =
                FSL_Comite_Eliminar_Validar(
                    codComite,
                    usuario);

            if (validacion != null)
            {
                return validacion;
            }

            if (string.IsNullOrWhiteSpace(
                cedula))
            {
                return DbHelper.ErrorResponse(
                    MensajeCedulaRequerida,
                    CodigoValidacion);
            }

            return null;
        }

        /// <summary>
        /// Convierte el resultado de una operaci&oacute;n SQL en
        /// una respuesta de error cuando corresponda.
        /// </summary>
        /// <param name="response">Resultado de la operaci&oacute;n.</param>
        /// <param name="mensajeError">Mensaje para errores del API.</param>
        /// <param name="mensajeSinCambios">Mensaje cuando no hubo cambios.</param>
        /// <returns>Error o null cuando la operaci&oacute;n fue correcta.</returns>
        private static ErrorDto?
            FSL_Comite_Operacion_Resultado_Validar(
                ErrorDto<int> response,
                string mensajeError,
                string mensajeSinCambios)
        {
            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    mensajeError);
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    mensajeSinCambios,
                    CodigoValidacion);
            }

            return null;
        }
    }
}