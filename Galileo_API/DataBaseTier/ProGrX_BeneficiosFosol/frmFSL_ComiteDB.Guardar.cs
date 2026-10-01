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
            return FSL_Comite_Comite_Guardar(
                CodEmpresa,
                request,
                FslComiteOperacionTipo.RegistrarComite);
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
            return FSL_Comite_Comite_Guardar(
                CodEmpresa,
                request,
                FslComiteOperacionTipo.ActualizarComite);
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
            return FSL_Comite_Eliminar_Ejecutar(
                CodEmpresa,
                codComite,
                usuario);
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
            return FSL_Comite_Miembro_Guardar(
                CodEmpresa,
                request,
                FslComiteOperacionTipo.RegistrarMiembro);
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
            return FSL_Comite_Miembro_Guardar(
                CodEmpresa,
                request,
                FslComiteOperacionTipo.ActualizarMiembro);
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
            return FSL_Comite_Eliminar_Ejecutar(
                CodEmpresa,
                codComite,
                cedula,
                usuario);
        }

        /// <summary>
        /// Registra o actualiza un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del comit&eacute;.</param>
        /// <param name="tipo">Tipo de operaci&oacute;n.</param>
        /// <returns>Resultado de la operaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Comite_Guardar(
            int CodEmpresa,
            FslComiteGuardarRequest request,
            FslComiteOperacionTipo tipo)
        {
            var validacion =
                FSL_Comite_Comite_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sqlRegistrar = """
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

            const string sqlActualizar = """
                UPDATE FSL_COMITES
                SET DESCRIPCION = @descripcion,
                    NUMERO_RESOLUTORES =
                        @numero_resolutores,
                    ACTIVO = @activo
                WHERE COD_COMITE = @cod_comite;
                """;

            var codComite =
                request.cod_comite.Trim();

            var operacion =
                new FslComiteOperacion
                {
                    Tipo = tipo,
                    Sql =
                        tipo ==
                        FslComiteOperacionTipo
                            .RegistrarComite
                            ? sqlRegistrar
                            : sqlActualizar,
                    Parametros = new
                    {
                        cod_comite = codComite,
                        descripcion =
                            request.descripcion.Trim(),
                        request.numero_resolutores,
                        request.activo,
                        usuario =
                            request.usuario.Trim()
                    },
                    Usuario = request.usuario,
                    Detalle =
                        $"Comité de FOSOL Id.:{codComite}"
                };

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Registra o actualiza un miembro de un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n del miembro.</param>
        /// <param name="tipo">Tipo de operaci&oacute;n.</param>
        /// <returns>Resultado de la operaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Miembro_Guardar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request,
            FslComiteOperacionTipo tipo)
        {
            var validacion =
                FSL_Comite_Miembro_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sqlRegistrar = """
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

            const string sqlActualizar = """
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

            var operacion =
                new FslComiteOperacion
                {
                    Tipo = tipo,
                    Sql =
                        tipo ==
                        FslComiteOperacionTipo
                            .RegistrarMiembro
                            ? sqlRegistrar
                            : sqlActualizar,
                    Parametros = new
                    {
                        cod_comite = codComite,
                        cedula,
                        nombre = request.nombre.Trim(),
                        usuario_vinculado =
                            request.usuario_vinculado.Trim(),
                        request.activo,
                        usuario =
                            request.usuario.Trim()
                    },
                    Usuario = request.usuario,
                    Detalle =
                        $"Comité Miembro: {codComite}.. Id.:{cedula}"
                };

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Elimina un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Resultado de la eliminaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Eliminar_Ejecutar(
            int CodEmpresa,
            string codComite,
            string usuario)
        {
            var request =
                new FslComiteEliminarOperacion
                {
                    CodComite = codComite,
                    Usuario = usuario,
                    Tipo =
                        FslComiteOperacionTipo
                            .EliminarComite
                };

            return FSL_Comite_Eliminar_Ejecutar(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Elimina un miembro de un comit&eacute;.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="codComite">C&oacute;digo del comit&eacute;.</param>
        /// <param name="cedula">C&eacute;dula del miembro.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Resultado de la eliminaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Eliminar_Ejecutar(
            int CodEmpresa,
            string codComite,
            string cedula,
            string usuario)
        {
            var request =
                new FslComiteEliminarOperacion
                {
                    CodComite = codComite,
                    Cedula = cedula,
                    Usuario = usuario,
                    Tipo =
                        FslComiteOperacionTipo
                            .EliminarMiembro
                };

            return FSL_Comite_Eliminar_Ejecutar(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Ejecuta la eliminaci&oacute;n solicitada.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="request">Informaci&oacute;n de la eliminaci&oacute;n.</param>
        /// <returns>Resultado de la eliminaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Eliminar_Ejecutar(
            int CodEmpresa,
            FslComiteEliminarOperacion request)
        {
            var validacion =
                FSL_Comite_Eliminar_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            const string sqlComite = """
                DELETE FROM FSL_COMITES
                WHERE COD_COMITE = @codComite;
                """;

            const string sqlMiembro = """
                DELETE FROM FSL_COMITES_MIEMBROS
                WHERE COD_COMITE = @codComite
                  AND CEDULA = @cedula;
                """;

            var codigo =
                request.CodComite.Trim();

            var identificacion =
                request.Cedula.Trim();

            var esMiembro =
                request.Tipo ==
                FslComiteOperacionTipo.EliminarMiembro;

            var operacion =
                new FslComiteOperacion
                {
                    Tipo = request.Tipo,
                    Sql =
                        esMiembro
                            ? sqlMiembro
                            : sqlComite,
                    Parametros = new
                    {
                        codComite = codigo,
                        cedula = identificacion
                    },
                    Usuario = request.Usuario,
                    Detalle =
                        esMiembro
                            ? $"Comité Miembro: {codigo} .. Id.:{identificacion}"
                            : $"Comité de FOSOL Id.:{codigo}"
                };

            return FSL_Comite_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Ejecuta una operaci&oacute;n de mantenimiento y registra
        /// el movimiento realizado en la bit&aacute;cora.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="operacion">Informaci&oacute;n de la operaci&oacute;n.</param>
        /// <returns>Resultado de la operaci&oacute;n.</returns>
        private ErrorDto FSL_Comite_Operacion_Ejecutar(
            int CodEmpresa,
            FslComiteOperacion operacion)
        {
            var mensajes =
                FSL_Comite_Operacion_Mensajes_Obtener(
                    operacion.Tipo);

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
                    mensajes.Error);
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    mensajes.SinCambios,
                    CodigoValidacion);
            }

            FSL_Comite_Bitacora_Registrar(
                CodEmpresa,
                operacion.Usuario,
                mensajes.Movimiento,
                operacion.Detalle);

            return DbHelper.OkResponse(
                mensajes.Exito);
        }

        /// <summary>
        /// Obtiene los mensajes asociados con una operaci&oacute;n.
        /// </summary>
        /// <param name="tipo">Tipo de operaci&oacute;n.</param>
        /// <returns>Mensajes y movimiento de la operaci&oacute;n.</returns>
        private static FslComiteOperacionMensajes
            FSL_Comite_Operacion_Mensajes_Obtener(
                FslComiteOperacionTipo tipo)
        {
            return tipo switch
            {
                FslComiteOperacionTipo.RegistrarComite =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Registra",
                        Error =
                            "Ocurri&oacute; un error al registrar el comit&eacute;.",
                        SinCambios =
                            "El c&oacute;digo del comit&eacute; ya existe.",
                        Exito =
                            "Comit&eacute; registrado correctamente."
                    },

                FslComiteOperacionTipo.ActualizarComite =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Modifica",
                        Error =
                            "Ocurri&oacute; un error al actualizar el comit&eacute;.",
                        SinCambios =
                            "El comit&eacute; indicado no existe.",
                        Exito =
                            "Comit&eacute; actualizado correctamente."
                    },

                FslComiteOperacionTipo.EliminarComite =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Elimina",
                        Error =
                            "Ocurri&oacute; un error al eliminar el comit&eacute;.",
                        SinCambios =
                            "El comit&eacute; indicado no existe.",
                        Exito =
                            "Comit&eacute; eliminado correctamente."
                    },

                FslComiteOperacionTipo.RegistrarMiembro =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Registra",
                        Error =
                            "Ocurri&oacute; un error al registrar el miembro.",
                        SinCambios =
                            "El miembro ya existe o el comit&eacute; indicado no est&aacute; disponible.",
                        Exito =
                            "Miembro registrado correctamente."
                    },

                FslComiteOperacionTipo.ActualizarMiembro =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Modifica",
                        Error =
                            "Ocurri&oacute; un error al actualizar el miembro.",
                        SinCambios =
                            "El miembro indicado no existe en el comit&eacute;.",
                        Exito =
                            "Miembro actualizado correctamente."
                    },

                FslComiteOperacionTipo.EliminarMiembro =>
                    new FslComiteOperacionMensajes
                    {
                        Movimiento = "Elimina",
                        Error =
                            "Ocurri&oacute; un error al eliminar el miembro.",
                        SinCambios =
                            "El miembro indicado no existe en el comit&eacute;.",
                        Exito =
                            "Miembro eliminado correctamente."
                    },

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(tipo),
                        tipo,
                        "El tipo de operaci&oacute;n no es v&aacute;lido.")
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
        /// Valida los datos requeridos de una eliminaci&oacute;n.
        /// </summary>
        /// <param name="request">Informaci&oacute;n de la eliminaci&oacute;n.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Eliminar_Request_Validar(
                FslComiteEliminarOperacion request)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            var campos =
                new List<(
                    string Valor,
                    string Mensaje)>
                {
                    (
                        request.CodComite,
                        MensajeComiteRequerido
                    ),
                    (
                        request.Usuario,
                        MensajeUsuarioRequerido
                    )
                };

            if (
                request.Tipo ==
                FslComiteOperacionTipo.EliminarMiembro)
            {
                campos.Add(
                    (
                        request.Cedula,
                        MensajeCedulaRequerida
                    ));
            }

            return FSL_Comite_Campos_Requeridos_Validar(
                campos);
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
            return FSL_Comite_Campos_Requeridos_Validar(
                (IEnumerable<(
                    string Valor,
                    string Mensaje)>)campos);
        }

        /// <summary>
        /// Valida una colecci&oacute;n de campos requeridos.
        /// </summary>
        /// <param name="campos">Valores y mensajes de validaci&oacute;n.</param>
        /// <returns>Error de validaci&oacute;n o null.</returns>
        private static ErrorDto?
            FSL_Comite_Campos_Requeridos_Validar(
                IEnumerable<(
                    string Valor,
                    string Mensaje)> campos)
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

        private enum FslComiteOperacionTipo
        {
            RegistrarComite,
            ActualizarComite,
            EliminarComite,
            RegistrarMiembro,
            ActualizarMiembro,
            EliminarMiembro
        }

        private sealed class FslComiteOperacion
        {
            public FslComiteOperacionTipo Tipo
            {
                get;
                init;
            }

            public string Sql { get; init; } =
                string.Empty;

            public object Parametros { get; init; } =
                new();

            public string Usuario { get; init; } =
                string.Empty;

            public string Detalle { get; init; } =
                string.Empty;
        }

        private sealed class
            FslComiteEliminarOperacion
        {
            public string CodComite { get; init; } =
                string.Empty;

            public string Cedula { get; init; } =
                string.Empty;

            public string Usuario { get; init; } =
                string.Empty;

            public FslComiteOperacionTipo Tipo
            {
                get;
                init;
            }
        }

        private sealed class
            FslComiteOperacionMensajes
        {
            public string Movimiento { get; init; } =
                string.Empty;

            public string Error { get; init; } =
                string.Empty;

            public string SinCambios { get; init; } =
                string.Empty;

            public string Exito { get; init; } =
                string.Empty;
        }
    }
}