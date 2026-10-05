using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Obtiene las remesas cerradas disponibles para traslado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Remesas cerradas.</returns>
        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Traslados_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    R.TESORERIA_REMESA AS tesoreria_remesa,
                    LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, '')))
                        AS registro_usuario,
                    R.REGISTRO_FECHA AS registro_fecha,
                    R.FECHA_INICIO AS fecha_inicio,
                    R.FECHA_CORTE AS fecha_corte,
                    LTRIM(RTRIM(ISNULL(R.NOTAS, ''))) AS notas,
                    LTRIM(RTRIM(ISNULL(R.ESTADO, ''))) AS estado,
                    'Remesa Cerrada' AS estado_descripcion,
                    CONCAT(
                        RIGHT(
                            '0000' +
                            CONVERT(VARCHAR(20), R.TESORERIA_REMESA),
                            4
                        ),
                        '...',
                        LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, ''))),
                        '...',
                        CONVERT(VARCHAR(19), R.REGISTRO_FECHA, 120),
                        ' I:',
                        CONVERT(VARCHAR(10), R.FECHA_INICIO, 103),
                        ' C:',
                        CONVERT(VARCHAR(10), R.FECHA_CORTE, 103)
                    ) AS descripcion
                FROM FSL_REMESAS_TESORERIA R
                WHERE R.ESTADO = 'C'
                ORDER BY R.REGISTRO_FECHA DESC;
                """;

            return DbHelper.ExecuteListQuery<FslRemesaDto>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los expedientes pendientes de traslado de una remesa.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <returns>Expedientes pendientes de traslado.</returns>
        public ErrorDto<List<FslExpedienteRemesaDto>>
            FSL_RemesasPago_TrasladoLista_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            if (codRemesa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion,
                    new List<FslExpedienteRemesaDto>());
            }

            const string sql = """
                SELECT
                    CONVERT(VARCHAR(30), E.COD_EXPEDIENTE)
                        AS cod_expediente,
                    LTRIM(RTRIM(ISNULL(E.CEDULA, ''))) AS cedula,
                    LTRIM(RTRIM(ISNULL(S.NOMBRE, ''))) AS nombre,
                    CAST(ISNULL(E.TOTAL_SOBRANTE, 0) AS DECIMAL(18, 2))
                        AS total_sobrante,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_CEDULA, '')))
                        AS presenta_cedula,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_NOMBRE, '')))
                        AS presenta_nombre
                FROM FSL_EXPEDIENTES E
                INNER JOIN SOCIOS S
                    ON E.CEDULA = S.CEDULA
                INNER JOIN FSL_REMESAS_TESORERIA R
                    ON R.TESORERIA_REMESA =
                        E.TESORERIA_REMESA
                WHERE R.TESORERIA_REMESA = @codRemesa
                  AND R.ESTADO = 'C'
                  AND E.RESOLUCION_FECHA >= R.FECHA_INICIO
                  AND E.RESOLUCION_FECHA <
                      DATEADD(DAY, 1, R.FECHA_CORTE)
                  AND E.TIPO_DESEMBOLSO = 'T'
                  AND E.ESTADO = 'X'
                  AND E.TOTAL_SOBRANTE > 0
                  AND ISNULL(E.TESORERIA_SOLICITUD, 0) = 0
                ORDER BY E.CEDULA, S.NOMBRE;
                """;

            return DbHelper.ExecuteListQuery<
                FslExpedienteRemesaDto>(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    new
                    {
                        codRemesa
                    });
        }

        /// <summary>
        /// Traslada los expedientes seleccionados a tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Remesa, usuario y expedientes.</param>
        /// <returns>Resultado del traslado.</returns>
        public ErrorDto FSL_RemesasPago_Traslado_Aplicar(
            int CodEmpresa,
            FslRemesaAplicarRequest request)
        {
            var validacion =
                FSL_RemesasPago_Aplicar_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            var codigos = request.casos
                .Select(item =>
                    item.cod_expediente.Trim())
                .Where(codigo =>
                    !string.IsNullOrWhiteSpace(codigo))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (codigos.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos un expediente.",
                    CodigoValidacion);
            }

            var configuracion =
                FSL_RemesasPago_Traslado_Configuracion_Obtener(
                    CodEmpresa,
                    request.usuario);

            if (!configuracion.esValida)
            {
                return DbHelper.ErrorResponse(
                    configuracion.mensaje,
                    CodigoValidacion);
            }

            List<FslExpedienteRemesaDto> expedientes;

            using var connection =
                DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                if (!FSL_RemesasPago_Remesa_Cerrada_Existe(
                    connection,
                    transaction,
                    request.cod_remesa))
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "La remesa indicada no se encuentra disponible para trasladar.",
                        CodigoValidacion);
                }

                expedientes =
                    FSL_RemesasPago_Traslado_Casos_Obtener(
                        connection,
                        transaction,
                        request.cod_remesa,
                        codigos);

                if (expedientes.Count != codigos.Count)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "Uno o m&aacute;s expedientes ya no se encuentran disponibles para trasladar.",
                        CodigoValidacion);
                }

                foreach (var expediente in expedientes)
                {
                    FSL_RemesasPago_Traslado_Caso_Procesar(
                        connection,
                        transaction,
                        request,
                        expediente,
                        configuracion);
                }

                const string sqlRemesa = """
                    UPDATE FSL_REMESAS_TESORERIA
                    SET ESTADO = 'T'
                    WHERE TESORERIA_REMESA = @cod_remesa
                      AND ESTADO = 'C';
                    """;

                connection.Execute(
                    sqlRemesa,
                    new
                    {
                        request.cod_remesa
                    },
                    transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }

            foreach (var expediente in expedientes)
            {
                FSL_RemesasPago_Bitacora_Registrar(
                    CodEmpresa,
                    request.usuario,
                    "Registra",
                    $"Traspaso a Tesoreria - Expediente:{expediente.cod_expediente}");
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Aplica",
                $"Carga Remesa Traslado a Tesoreria : {request.cod_remesa}");

            return DbHelper.OkResponse(
                "Traslado a Tesorer&iacute;a realizado satisfactoriamente.");
        }

        /// <summary>
        /// Obtiene y valida la configuración necesaria para el traslado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <returns>Configuración del traslado.</returns>
        private FslTrasladoConfiguracion
            FSL_RemesasPago_Traslado_Configuracion_Obtener(
                int CodEmpresa,
                string usuario)
        {
            var configuracion =
                new FslTrasladoConfiguracion
                {
                    cuenta =
                        _mBeneficiosDb.fxFSL_Parametros(
                            CodEmpresa,
                            "01"),
                    banco_cheque =
                        _mBeneficiosDb.fxFSL_Parametros(
                            CodEmpresa,
                            "04"),
                    concepto =
                        _mBeneficiosDb.fxFSL_Parametros(
                            CodEmpresa,
                            "05"),
                    unidad =
                        _mBeneficiosDb.fxFSL_Parametros(
                            CodEmpresa,
                            "07")
                };

            const string sqlToken = """
                SELECT TOP 1
                    ID_TOKEN
                FROM TES_TOKENS
                WHERE ESTADO = 'A'
                ORDER BY REGISTRO_FECHA;
                """;

            var tokenResponse =
                DbHelper.ExecuteSingleQuery<string>(
                    _portalDb,
                    CodEmpresa,
                    sqlToken,
                    defaultValue: string.Empty);

            configuracion.token =
                tokenResponse.Result ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                configuracion.token))
            {
                configuracion.token =
                    _mTesFuncionesDb.fxTesToken(
                        CodEmpresa,
                        usuario);
            }

            configuracion.mensaje =
                FSL_RemesasPago_Traslado_Configuracion_Mensaje(
                    configuracion);

            configuracion.esValida =
                string.IsNullOrWhiteSpace(
                    configuracion.mensaje);

            return configuracion;
        }

        /// <summary>
        /// Obtiene el mensaje de validación de la configuración.
        /// </summary>
        /// <param name="configuracion">Configuración del traslado.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string
            FSL_RemesasPago_Traslado_Configuracion_Mensaje(
                FslTrasladoConfiguracion configuracion)
        {
            if (string.IsNullOrWhiteSpace(
                configuracion.cuenta))
            {
                return "No se encuentra configurada la cuenta contable del traslado.";
            }

            if (string.IsNullOrWhiteSpace(
                configuracion.banco_cheque) ||
                !int.TryParse(
                    configuracion.banco_cheque,
                    out _))
            {
                return "No se encuentra configurado el banco para pagos mediante cheque.";
            }

            if (string.IsNullOrWhiteSpace(
                configuracion.concepto))
            {
                return "No se encuentra configurado el concepto de tesorer&iacute;a.";
            }

            if (string.IsNullOrWhiteSpace(
                configuracion.unidad))
            {
                return "No se encuentra configurada la unidad de negocio.";
            }

            if (string.IsNullOrWhiteSpace(
                configuracion.token))
            {
                return "No fue posible obtener el token de tesorer&iacute;a.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Valida que la remesa permanezca cerrada.
        /// </summary>
        private static bool
            FSL_RemesasPago_Remesa_Cerrada_Existe(
                SqlConnection connection,
                SqlTransaction transaction,
                long codRemesa)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM FSL_REMESAS_TESORERIA
                WHERE TESORERIA_REMESA = @codRemesa
                  AND ESTADO = 'C';
                """;

            return connection.QuerySingle<int>(
                sql,
                new
                {
                    codRemesa
                },
                transaction) > 0;
        }

        /// <summary>
        /// Obtiene nuevamente los expedientes seleccionados desde la base de datos.
        /// </summary>
        private static List<FslExpedienteRemesaDto>
            FSL_RemesasPago_Traslado_Casos_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                long codRemesa,
                List<string> codigos)
        {
            const string sql = """
                SELECT
                    CONVERT(VARCHAR(30), E.COD_EXPEDIENTE)
                        AS cod_expediente,
                    LTRIM(RTRIM(ISNULL(E.CEDULA, ''))) AS cedula,
                    LTRIM(RTRIM(ISNULL(S.NOMBRE, ''))) AS nombre,
                    CAST(ISNULL(E.TOTAL_SOBRANTE, 0) AS DECIMAL(18, 2))
                        AS total_sobrante,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_CEDULA, '')))
                        AS presenta_cedula,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_NOMBRE, '')))
                        AS presenta_nombre
                FROM FSL_EXPEDIENTES E
                INNER JOIN SOCIOS S
                    ON E.CEDULA = S.CEDULA
                INNER JOIN FSL_REMESAS_TESORERIA R
                    ON R.TESORERIA_REMESA =
                        E.TESORERIA_REMESA
                WHERE R.TESORERIA_REMESA = @codRemesa
                  AND R.ESTADO = 'C'
                  AND E.COD_EXPEDIENTE IN @codigos
                  AND E.RESOLUCION_FECHA >= R.FECHA_INICIO
                  AND E.RESOLUCION_FECHA <
                      DATEADD(DAY, 1, R.FECHA_CORTE)
                  AND E.TIPO_DESEMBOLSO = 'T'
                  AND E.ESTADO = 'X'
                  AND E.TOTAL_SOBRANTE > 0
                  AND ISNULL(E.TESORERIA_SOLICITUD, 0) = 0;
                """;

            return connection
                .Query<FslExpedienteRemesaDto>(
                    sql,
                    new
                    {
                        codRemesa,
                        codigos
                    },
                    transaction)
                .ToList();
        }

        /// <summary>
        /// Procesa el traslado a tesorería de un expediente.
        /// </summary>
        private void FSL_RemesasPago_Traslado_Caso_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            FslRemesaAplicarRequest request,
            FslExpedienteRemesaDto expediente,
            FslTrasladoConfiguracion configuracion)
        {
            var medioPago =
                FSL_RemesasPago_MedioPago_Obtener(
                    connection,
                    transaction,
                    expediente.cedula,
                    configuracion.banco_cheque);

            var solicitud =
                FSL_RemesasPago_Tesoreria_Maestro_Crear(
                    connection,
                    transaction,
                    new FslTesoreriaCrear
                    {
                        tipo_documento =
                            medioPago.tipo_documento,
                        banco = medioPago.banco,
                        monto =
                            expediente.total_sobrante,
                        codigo =
                            expediente.cedula,
                        beneficiario =
                            expediente.nombre,
                        detalle1 = "FOSOL",
                        detalle2 =
                            $"Exp.: {expediente.cod_expediente}",
                        cuenta = medioPago.cuenta,
                        unidad = configuracion.unidad,
                        token = configuracion.token,
                        usuario = request.usuario,
                        cod_remesa =
                            request.cod_remesa,
                        concepto =
                            configuracion.concepto
                    });

            var cuentaBanco =
                FSL_RemesasPago_Banco_Cuenta_Obtener(
                    connection,
                    transaction,
                    medioPago.banco);

            FSL_RemesasPago_Tesoreria_Detalle_Crear(
                connection,
                transaction,
                new FslTesoreriaDetalleCrear
                {
                    solicitud = solicitud,
                    cuenta = cuentaBanco,
                    monto = expediente.total_sobrante,
                    debe_haber = "H",
                    linea = 1,
                    unidad = configuracion.unidad
                });

            FSL_RemesasPago_Tesoreria_Detalle_Crear(
                connection,
                transaction,
                new FslTesoreriaDetalleCrear
                {
                    solicitud = solicitud,
                    cuenta = configuracion.cuenta,
                    monto = expediente.total_sobrante,
                    debe_haber = "D",
                    linea = 2,
                    unidad = configuracion.unidad
                });

            const string sqlExpediente = """
                UPDATE FSL_EXPEDIENTES
                SET TESORERIA_SOLICITUD = @solicitud,
                    TESORERIA_FECHA = GETDATE(),
                    TESORERIA_USUARIO = @usuario
                WHERE TESORERIA_REMESA = @cod_remesa
                  AND COD_EXPEDIENTE = @cod_expediente
                  AND ISNULL(TESORERIA_SOLICITUD, 0) = 0;
                """;

            var afectados = connection.Execute(
                sqlExpediente,
                new
                {
                    solicitud,
                    usuario = request.usuario,
                    request.cod_remesa,
                    expediente.cod_expediente
                },
                transaction);

            if (afectados <= 0)
            {
                throw new InvalidOperationException(
                    $"No fue posible actualizar el expediente {expediente.cod_expediente}.");
            }
        }

        /// <summary>
        /// Obtiene el medio de pago del beneficiario.
        /// </summary>
        private static FslMedioPago
            FSL_RemesasPago_MedioPago_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                string cedula,
                string bancoCheque)
        {
            const string sql = """
                SELECT TOP 1
                    LTRIM(RTRIM(ISNULL(CUENTA, ''))) AS cuenta,
                    ID_BANCO AS id_banco
                FROM CUENTAS_AHORROS
                WHERE TIPO = 1
                  AND CEDULA = @cedula
                ORDER BY PRIORIDAD;
                """;

            var cuenta =
                connection.QueryFirstOrDefault<
                    FslCuentaAhorro>(
                        sql,
                        new
                        {
                            cedula
                        },
                        transaction);

            if (cuenta != null)
            {
                return new FslMedioPago
                {
                    tipo_documento = "TE",
                    banco = cuenta.id_banco,
                    cuenta = cuenta.cuenta
                };
            }

            return new FslMedioPago
            {
                tipo_documento = "CK",
                banco = int.Parse(
                    bancoCheque,
                    System.Globalization.CultureInfo.InvariantCulture),
                cuenta = string.Empty
            };
        }

        /// <summary>
        /// Registra el maestro de una solicitud de tesorería.
        /// </summary>
        private static long
            FSL_RemesasPago_Tesoreria_Maestro_Crear(
                SqlConnection connection,
                SqlTransaction transaction,
                FslTesoreriaCrear request)
        {
            var autoriza =
                request.tipo_documento == "CK"
                    ? "S"
                    : "N";

            var usuarioAutoriza =
                request.tipo_documento == "CK"
                    ? request.usuario
                    : null;

            const string sql = """
                INSERT INTO TES_TRANSACCIONES
                (
                    COD_CONCEPTO,
                    COD_UNIDAD,
                    ID_BANCO,
                    TIPO,
                    CODIGO,
                    BENEFICIARIO,
                    MONTO,
                    FECHA_SOLICITUD,
                    ESTADO,
                    ESTADOI,
                    MODULO,
                    SUBMODULO,
                    CTA_AHORROS,
                    DETALLE1,
                    DETALLE2,
                    REFERENCIA,
                    OP,
                    GENERA,
                    ACTUALIZA,
                    USER_SOLICITA,
                    AUTORIZA,
                    USER_AUTORIZA,
                    FECHA_AUTORIZACION,
                    ID_TOKEN,
                    REMESA_TIPO,
                    REMESA_ID
                )
                OUTPUT INSERTED.NSOLICITUD
                VALUES
                (
                    @concepto,
                    @unidad,
                    @banco,
                    @tipo_documento,
                    @codigo,
                    @beneficiario,
                    @monto,
                    GETDATE(),
                    'P',
                    'P',
                    'CC',
                    'C',
                    @cuenta,
                    @detalle1,
                    @detalle2,
                    0,
                    0,
                    'S',
                    'S',
                    @usuario,
                    @autoriza,
                    @usuarioAutoriza,
                    CASE
                        WHEN @autoriza = 'S'
                        THEN GETDATE()
                        ELSE NULL
                    END,
                    @token,
                    'FSL',
                    @cod_remesa
                );
                """;

            return connection.QuerySingle<long>(
                sql,
                new
                {
                    request.concepto,
                    request.unidad,
                    request.banco,
                    request.tipo_documento,
                    request.codigo,
                    request.beneficiario,
                    request.monto,
                    request.cuenta,
                    request.detalle1,
                    request.detalle2,
                    request.usuario,
                    autoriza,
                    usuarioAutoriza,
                    request.token,
                    request.cod_remesa
                },
                transaction);
        }

        /// <summary>
        /// Registra una l&#237;nea del detalle contable de tesorer&#237;a.
        /// </summary>
        /// <param name="connection">Conexi&#243;n activa.</param>
        /// <param name="transaction">Transacci&#243;n vigente.</param>
        /// <param name="request">Informaci&#243;n del detalle contable.</param>
        private static void
            FSL_RemesasPago_Tesoreria_Detalle_Crear(
                SqlConnection connection,
                SqlTransaction transaction,
                FslTesoreriaDetalleCrear request)
        {
            const string sql = """
            INSERT INTO TES_TRANS_ASIENTO
            (
                NSOLICITUD,
                CUENTA_CONTABLE,
                MONTO,
                DEBEHABER,
                LINEA,
                COD_UNIDAD
            )
            VALUES
            (
                @solicitud,
                @cuenta,
                @monto,
                @debe_haber,
                @linea,
                @unidad
            );
            """;

            connection.Execute(
                sql,
                new
                {
                    request.solicitud,
                    cuenta = request.cuenta.Trim(),
                    request.monto,
                    request.debe_haber,
                    request.linea,
                    request.unidad
                },
                transaction);
        }

        /// <summary>
        /// Obtiene la cuenta contable configurada para un banco.
        /// </summary>
        private static string
            FSL_RemesasPago_Banco_Cuenta_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                int banco)
        {
            const string sql = """
                SELECT TOP 1
                    LTRIM(RTRIM(ISNULL(CTACONTA, '')))
                FROM TES_BANCOS
                WHERE ID_BANCO = @banco;
                """;

            return connection.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    banco
                },
                transaction) ?? "0";
        }

        private sealed class FslTrasladoConfiguracion
        {
            public string cuenta { get; set; } = string.Empty;
            public string banco_cheque { get; set; } = string.Empty;
            public string concepto { get; set; } = string.Empty;
            public string unidad { get; set; } = string.Empty;
            public string token { get; set; } = string.Empty;
            public bool esValida { get; set; } = false;
            public string mensaje { get; set; } = string.Empty;
        }

        private sealed class FslCuentaAhorro
        {
            public string cuenta { get; set; } = string.Empty;
            public int id_banco { get; set; } = 0;
        }

        private sealed class FslMedioPago
        {
            public string tipo_documento { get; set; } = string.Empty;
            public int banco { get; set; } = 0;
            public string cuenta { get; set; } = string.Empty;
        }

        private sealed class FslTesoreriaCrear
        {
            public string tipo_documento { get; set; } = string.Empty;
            public int banco { get; set; } = 0;
            public decimal monto { get; set; } = 0;
            public string codigo { get; set; } = string.Empty;
            public string beneficiario { get; set; } = string.Empty;
            public string detalle1 { get; set; } = string.Empty;
            public string detalle2 { get; set; } = string.Empty;
            public string cuenta { get; set; } = string.Empty;
            public string unidad { get; set; } = string.Empty;
            public string token { get; set; } = string.Empty;
            public string usuario { get; set; } = string.Empty;
            public long cod_remesa { get; set; } = 0;
            public string concepto { get; set; } = string.Empty;
        }

        private sealed class FslTesoreriaDetalleCrear
        {
            public long solicitud { get; set; } = 0;
            public string cuenta { get; set; } = string.Empty;
            public decimal monto { get; set; } = 0;
            public string debe_haber { get; set; } = string.Empty;
            public int linea { get; set; } = 0;
            public string unidad { get; set; } = string.Empty;
        }
    }
}