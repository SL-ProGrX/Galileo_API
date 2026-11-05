using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatReservasDb : InversionesCatalogoDbBase
    {
        public FrmIvrCatReservasDb(IConfiguration config)
            : base(
                config,
                "El código del tipo de reserva es requerido.",
                "Tipos de Reservas:  ")
        {
        }

        /// <summary>
        /// Obtiene el catálogo de reservas ordenado por código.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa cuyas reservas se consultan.
        /// </param>
        /// <returns>
        /// Lista de reservas o el detalle del error de consulta.
        /// </returns>
        public ErrorDto<List<IvrCatReservaDto>> IVR_Cat_Reservas_Lista_Obtener(
            int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_RESERVA) AS Cod_Reserva,
                    RTRIM(ISNULL(DESCRIPCION, '')) AS Descripcion,
                    CAST(ISNULL(ACTIVA, 0) AS bit) AS Activa
                FROM IVR_RESERVAS
                ORDER BY COD_RESERVA;
                """;

            return DbHelper.ExecuteListQuery<IvrCatReservaDto>(
                _portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Registra un tipo de reserva nuevo y su movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se registra el tipo de reserva.
        /// </param>
        /// <param name="request">
        /// Código, descripción, estado y usuario que registra la reserva.
        /// </param>
        /// <returns>
        /// Resultado del registro, validación de código duplicado o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Reservas_Registrar(
            int codEmpresa,
            IvrCatReservaGuardarRequest? request)
        {
            const string sql = """
                INSERT INTO IVR_RESERVAS
                (
                    COD_RESERVA, DESCRIPCION, ACTIVA,
                    REGISTRO_USUARIO, REGISTRO_FECHA
                )
                SELECT
                    @cod_reserva, @descripcion, @activa,
                    @usuario, dbo.MyGetdate()
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM IVR_RESERVAS
                    WHERE COD_RESERVA = @cod_reserva
                );
                """;

            return Guardar(codEmpresa, request, sql, true);
        }

        /// <summary>
        /// Actualiza la descripción y estado del tipo de reserva, conservando su código y datos de registro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se actualiza el tipo de reserva.
        /// </param>
        /// <param name="request">
        /// Código del tipo de reserva, datos que deben actualizarse y usuario del movimiento.
        /// </param>
        /// <returns>
        /// Resultado de la actualización, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Reservas_Actualizar(
            int codEmpresa,
            IvrCatReservaGuardarRequest? request)
        {
            const string sql = """
                UPDATE IVR_RESERVAS
                SET DESCRIPCION = @descripcion,
                    ACTIVA = @activa
                WHERE COD_RESERVA = @cod_reserva;
                """;

            return Guardar(codEmpresa, request, sql, false);
        }

        /// <summary>
        /// Elimina el tipo de reserva indicado y registra el movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa a la que pertenece el tipo de reserva.
        /// </param>
        /// <param name="codReserva">
        /// Código del tipo de reserva que debe eliminarse.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la eliminación y se registra en la bitácora.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Reservas_Eliminar(
            int codEmpresa,
            string? codReserva,
            string? usuario)
        {
            var validacion = ValidarIdentificacion(
                codReserva, usuario);
            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM IVR_RESERVAS
                WHERE COD_RESERVA = @cod_reserva;
                """;

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new { cod_reserva = codReserva!.Trim() });

            return ResultadoMovimiento(
                codEmpresa, resultado, usuario!, "Elimina", codReserva,
                "El tipo de reserva indicado no existe.");
        }

        private ErrorDto Guardar(
            int codEmpresa,
            IvrCatReservaGuardarRequest? request,
            string sql,
            bool nuevo)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del tipo de reserva son requeridos.", CodigoMensajeControlado);
            }

            var req = request;
            var validacion = ValidarIdentificacion(
                req.Cod_Reserva, req.Usuario);
            if (validacion != null)
            {
                return validacion;
            }

            var codigo = nuevo
                ? req.Cod_Reserva.Trim().ToUpperInvariant()
                : req.Cod_Reserva.Trim();

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new
                {
                    cod_reserva = codigo,
                    descripcion = req.Descripcion ?? string.Empty,
                    activa = req.Activa,
                    usuario = req.Usuario.Trim()
                });

            return ResultadoMovimiento(
                codEmpresa, resultado, req.Usuario,
                nuevo ? "Registra" : "Modifica", codigo,
                nuevo ? "El tipo de reserva indicado ya existe."
                    : "El tipo de reserva indicado no existe.");
        }
        /// <summary>
        /// Consulta las cuentas vinculadas a la reserva con su descripción contable.
        /// </summary>
        /// <param name="codEmpresa">Empresa cuyos vínculos se consultan.</param>
        /// <param name="codReserva">Código de la reserva seleccionada.</param>
        /// <returns>Cuentas vinculadas o el detalle del error.</returns>
        public ErrorDto<List<IvrCatReservaCuentaDto>> IVR_Cat_Reservas_Cuentas_Obtener(
            int codEmpresa,
            string? codReserva)
        {
            if (string.IsNullOrWhiteSpace(codReserva))
            {
                return DbHelper.CreateErrorResponse<List<IvrCatReservaCuentaDto>>(
                    "El código de la reserva es requerido.", CodigoMensajeControlado);
            }

            const string sql = """
                SELECT
                    RTRIM(Rc.COD_CUENTA) AS Cod_Cuenta,
                    RTRIM(Cta.COD_CUENTA_MASK) AS Cod_Cuenta_Mask,
                    RTRIM(ISNULL(Cta.DESCRIPCION, '')) AS Descripcion
                FROM IVR_RESERVAS_CUENTAS Rc
                INNER JOIN vCNTX_CUENTAS_LOCAL Cta ON Rc.COD_CUENTA = Cta.COD_CUENTA
                WHERE Rc.COD_RESERVA = @cod_reserva
                ORDER BY Rc.COD_CUENTA;
                """;
            return DbHelper.ExecuteListQuery<IvrCatReservaCuentaDto>(
                _portalDb, codEmpresa, sql, new { cod_reserva = codReserva.Trim() });
        }

        /// <summary>
        /// Vincula una cuenta usando el procedimiento original de VB6.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se registra el vínculo.</param>
        /// <param name="request">Reserva, cuenta con máscara y usuario del registro.</param>
        /// <returns>Resultado de la vinculación o el detalle del error.</returns>
        public ErrorDto IVR_Cat_Reservas_Cuenta_Registrar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return GuardarCuenta(codEmpresa, request, false, false);
        }

        /// <summary>
        /// Sustituye la cuenta vinculada mediante el SP original sin transacción externa.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se modifica el vínculo.</param>
        /// <param name="request">Reserva, cuenta anterior, nueva cuenta y usuario del cambio.</param>
        /// <returns>Resultado de la sustitución o aviso de cambio parcial si falla.</returns>
        public ErrorDto IVR_Cat_Reservas_Cuenta_Actualizar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return GuardarCuenta(codEmpresa, request, false, true);
        }

        /// <summary>
        /// Desvincula una cuenta mediante el procedimiento original, sin borrar el catálogo contable.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se elimina el vínculo.</param>
        /// <param name="request">Reserva, cuenta vinculada y usuario que elimina la asignación.</param>
        /// <returns>Resultado de la desvinculación o el detalle del error.</returns>
        public ErrorDto IVR_Cat_Reservas_Cuenta_Eliminar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return GuardarCuenta(codEmpresa, request, true, false);
        }

        /// <summary>
        /// Valida y ejecuta directamente el cambio de vínculo, sin transacción externa.
        /// </summary>
        private ErrorDto GuardarCuenta(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request,
            bool eliminar,
            bool reemplazar)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Los datos del vínculo son requeridos.", CodigoMensajeControlado);

            var validacion = ValidarIdentificacion(request.Cod_Reserva, request.Usuario);
            if (validacion != null) return validacion;

            if (string.IsNullOrWhiteSpace(request.Cod_Cuenta) ||
                (reemplazar && string.IsNullOrWhiteSpace(request.Cuenta_Anterior)))
                return DbHelper.ErrorResponse(
                    "Debe indicar la cuenta y, al modificar, la cuenta anterior.", CodigoMensajeControlado);

            if (request.Cod_Reserva.Trim().Length > 10 || request.Cod_Cuenta.Trim().Length > 60 ||
                (reemplazar && request.Cuenta_Anterior.Trim().Length > 60) || request.Usuario.Trim().Length > 30)
                return DbHelper.ErrorResponse("Los datos del vínculo superan la longitud permitida.", CodigoMensajeControlado);

            var escrituraIniciada = false;
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var reserva = request.Cod_Reserva.Trim();
                const string sqlReserva = "SELECT COUNT(*) FROM IVR_RESERVAS WHERE COD_RESERVA = @reserva;";
                if (connection.ExecuteScalar<int>(sqlReserva, new { reserva }) == 0)
                    return DbHelper.ErrorResponse("La reserva indicada no existe.", CodigoMensajeControlado);

                var cuenta = ConsultarCuenta(connection, request.Cod_Cuenta.Trim());
                if (cuenta == null)
                    return DbHelper.ErrorResponse("La cuenta contable indicada no existe.", CodigoMensajeControlado);

                return CambiarVinculo(connection, reserva, cuenta, request,
                    eliminar, reemplazar, ref escrituraIniciada);
            }
            catch (Exception)
            {
                if (!escrituraIniciada)
                    return DbHelper.ErrorResponse("No se pudo validar o iniciar el cambio de vínculo.", -1);

                return DbHelper.ErrorResponse(
                    reemplazar
                        ? "La sustitución quedó inconclusa. Pueden estar vinculadas la cuenta anterior, la nueva o ambas. Revise los vínculos antes de reintentar."
                        : "El procedimiento pudo cambiar el vínculo, aunque no se confirmó el resultado. Revise las cuentas de la reserva antes de reintentar.",
                    -1);
            }
        }

        /// <summary>Aplica una vinculación, desvinculación o sustitución validada.</summary>
        private static ErrorDto CambiarVinculo(
            IDbConnection connection,
            string reserva,
            IvrCatReservaCuentaDto cuenta,
            IvrCatReservaCuentaRequest request,
            bool eliminar,
            bool reemplazar,
            ref bool escrituraIniciada)
        {
            if (reemplazar)
                return ReemplazarVinculo(connection, reserva, cuenta, request, ref escrituraIniciada);

            var existe = TieneVinculo(connection, reserva, cuenta.Cod_Cuenta);
            if (eliminar && !existe)
                return DbHelper.ErrorResponse("La cuenta no está vinculada a esta reserva.", CodigoMensajeControlado);
            if (!eliminar && existe)
                return DbHelper.ErrorResponse("La cuenta ya está vinculada a esta reserva.", CodigoMensajeControlado);

            ProcesarVinculo(connection, reserva, cuenta, !eliminar, request.Usuario, ref escrituraIniciada);
            return DbHelper.OkResponse("Proceso concluido satisfactoriamente.");
        }

        /// <summary>
        /// Vincula la nueva cuenta antes de desvincular la anterior para conservar
        /// el vínculo previo si falla la primera llamada al procedimiento.
        /// </summary>
        private static ErrorDto ReemplazarVinculo(
            IDbConnection connection,
            string reserva,
            IvrCatReservaCuentaDto cuenta,
            IvrCatReservaCuentaRequest request,
            ref bool escrituraIniciada)
        {
            var anterior = ConsultarCuenta(connection, request.Cuenta_Anterior.Trim());
            if (anterior == null || !TieneVinculo(connection, reserva, anterior.Cod_Cuenta))
                return DbHelper.ErrorResponse("La cuenta anterior ya no está vinculada a la reserva.", CodigoMensajeControlado);

            if (anterior.Cod_Cuenta == cuenta.Cod_Cuenta)
                return DbHelper.OkResponse("La cuenta seleccionada conserva el vínculo actual.");

            if (TieneVinculo(connection, reserva, cuenta.Cod_Cuenta))
                return DbHelper.ErrorResponse("La cuenta ya está vinculada a esta reserva.", CodigoMensajeControlado);

            ProcesarVinculo(connection, reserva, cuenta, true, request.Usuario, ref escrituraIniciada);
            ProcesarVinculo(connection, reserva, anterior, false, request.Usuario, ref escrituraIniciada);
            return DbHelper.OkResponse("Proceso concluido satisfactoriamente.");
        }

        /// <summary>Resuelve la cuenta contable y su máscara.</summary>
        private static IvrCatReservaCuentaDto? ConsultarCuenta(IDbConnection connection, string cuenta)
        {
            const string sql = """
                SELECT TOP (1)
                    RTRIM(COD_CUENTA) AS Cod_Cuenta,
                    RTRIM(COD_CUENTA_MASK) AS Cod_Cuenta_Mask,
                    RTRIM(ISNULL(DESCRIPCION, '')) AS Descripcion
                FROM vCNTX_CUENTAS_LOCAL
                WHERE COD_CUENTA = @cuenta OR COD_CUENTA_MASK = @cuenta;
                """;
            return connection.QueryFirstOrDefault<IvrCatReservaCuentaDto>(sql, new { cuenta });
        }

        /// <summary>Comprueba la vinculación actual de la cuenta.</summary>
        private static bool TieneVinculo(IDbConnection connection, string reserva, string cuenta)
        {
            const string sql = """
                SELECT COUNT(*) FROM IVR_RESERVAS_CUENTAS
                WHERE COD_RESERVA = @reserva AND COD_CUENTA = @cuenta;
                """;
            return connection.ExecuteScalar<int>(sql, new { reserva, cuenta }) > 0;
        }

        /// <summary>Ejecuta y comprueba el cambio solicitado mediante el SP existente.</summary>
        private static void ProcesarVinculo(
            IDbConnection connection,
            string reserva,
            IvrCatReservaCuentaDto cuenta,
            bool vincular,
            string usuario,
            ref bool escrituraIniciada)
        {
            // El procedimiento recibe la cuenta con máscara y cuatro argumentos por posición.
            const string sql = "EXEC spIVR_RESERVAS_CUENTAS_REGISTRO @reserva, @cuenta, @vincula, @usuario;";
            escrituraIniciada = true;
            connection.Execute(sql, new
            {
                reserva,
                cuenta = cuenta.Cod_Cuenta_Mask,
                vincula = vincular ? 1 : 0,
                usuario = usuario.Trim()
            });
            if (TieneVinculo(connection, reserva, cuenta.Cod_Cuenta) != vincular)
                throw new InvalidOperationException("El procedimiento no completó el cambio del vínculo solicitado.");
        }
    }
}
