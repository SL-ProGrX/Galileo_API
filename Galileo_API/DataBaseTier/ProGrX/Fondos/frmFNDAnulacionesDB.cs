using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier;
using Galileo.Models.GEN;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndAnulacionesDb
    {
        private readonly IConfiguration _config;
        private readonly MFndFuncionesDb _mFNDFunciones;
        private readonly MSecurityMainDb _mSecurity;
        private readonly int vModulo = 18;
        private const string SpAutorizaDatos = "spFnd_Autoriza_Datos";
        private const string SpGestionRegistro = "spFnd_Gestion_Registro";
        private const string SpGestionEstado = "spFnd_Gestion_Estado";
        private const string SpSeguridadAnulacion = "dbo.spFndSeguridad_ApAnul";

        public FrmFndAnulacionesDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mFNDFunciones = new MFndFuncionesDb(_config);
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener información de anulaciones
        /// </summary>
        public ErrorDto<FndAnulacionesDto> FND_Anulaciones_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new FndAnulacionesDto());
            }
            const string query = @"
                    SELECT
                        C.cedula,
                        S.nombre,
                        P.descripcion AS PlanX,
                        O.descripcion AS OperadoraX,
                        C.cod_plan,
                        C.cod_contrato,
                        C.cod_operadora,
                        C.estado,
                        C.fecha_Inicio,
                        ISNULL(P.cuenta_Maestra,0) AS CuentaMaestra,
                        P.Tipo_CDP,
                        P.permite_mov_cajas
                    FROM dbo.fnd_contratos C
                    INNER JOIN dbo.Socios S ON C.cedula = S.cedula
                    INNER JOIN dbo.fnd_planes P ON C.cod_plan = P.cod_plan AND C.cod_operadora = P.cod_operadora
                    INNER JOIN dbo.fnd_operadoras O ON C.cod_operadora = O.cod_operadora
                    WHERE C.cod_operadora = @Operadora
                      AND C.cod_plan = @Plan
                      AND C.cod_Contrato = @Contrato;";

            var result = DbHelper.ExecuteSingleQuery<FndAnulacionesDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new FndAnulacionesDto(),
                CrearParametrosContrato(Params));

            return new ErrorDto<FndAnulacionesDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesDto()
            };
        }

        /// <summary>
        /// Obtener subcuentas asociadas 
        /// </summary>
        public ErrorDto<List<FndAnulacionesSubCuentasDto>> FND_Anulaciones_SubCuentas_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de subcuentas son requeridos.",
                    -2,
                    new List<FndAnulacionesSubCuentasDto>());
            }
            const string query = @"
                    SELECT
                        IDx,
                        Cedula,
                        Nombre,
                        aportes,
                        rendimiento
                    FROM dbo.fnd_subCuentas
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND estado = 'A';";

            return DbHelper.ExecuteListQuery<FndAnulacionesSubCuentasDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                CrearParametrosContrato(Params));
        }

        /// <summary>
        /// Obtener autorizadores
        /// </summary>
        public ErrorDto<FndAutorizaDto> FND_Anulaciones_Autoriza_Obtener(int CodEmpresa, string Plan, string Usuario)
        {
            if (_mFNDFunciones.fxFndParametro(CodEmpresa, "01.2") != "S")
            {
                return DbHelper.CreateOkResponse<FndAutorizaDto>(null);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndAutorizaDto>(
                    SpAutorizaDatos,
                    new
                    {
                        Plan = NormalizarTexto(Plan),
                        TipoMov = "N",
                        Usuario = NormalizarTexto(Usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndAutorizaDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAutorizaDto()
            };
        }

        /// <summary>
        /// Solicitar autorización de anulaciones
        /// </summary>
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_SolicitaAutorizacion_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de autorización son requeridos.",
                    -2,
                    new FndAnulacionesEstadoGestionDto());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var limite = FND_Anulaciones_Limite_Consultar(connection, Params)
                    ?? throw new InvalidOperationException(
                        "No se encontró el límite de autorización del usuario y plan.");
                Params.autoriza_monto = limite.monto;
                return connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                    SpGestionRegistro,
                    CrearParametrosSolicitudAutorizacion(Params),
                    commandType: System.Data.CommandType.StoredProcedure);
            });

            return new ErrorDto<FndAnulacionesEstadoGestionDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesEstadoGestionDto()
            };
        }

        /// <summary>
        /// Refrescar estado de autorización
        /// </summary>
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_AutorizacionRefresh_Obtener(int CodEmpresa, int GestionId)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                    SpGestionEstado,
                    new { GestionId = GestionId },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndAnulacionesEstadoGestionDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesEstadoGestionDto()
            };
        }

        /// <summary>
        /// Procesar anulacion
        /// </summary>
        public ErrorDto<object> FND_Anulaciones_Anular(int CodEmpresa, FndAnulacionesParams Params, string Accion, string Notas)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse<object>("Los datos de anulación son requeridos.", -2, null);
            }

            var resultado = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                EjecutarAnulacion(CodEmpresa, Params, NormalizarTexto(Accion).ToUpper(), NormalizarTexto(Notas), connection));

            return resultado.Code == 0
                ? resultado.Result ?? DbHelper.CreateErrorResponse<object>("No se obtuvo resultado de anulación.", -1, null)
                : DbHelper.CreateErrorResponse<object>(resultado.Description ?? "Error al procesar anulación.", resultado.Code ?? -1, null);
        }

        /// <summary>
        /// Paridad VB6 frmFNDAnulaciones.cmdAnular: valida, ejecuta spFondos_Anula_Aporte (documento FNC en
        /// SIF_TRANSACCIONES, asiento y saldo a favor en cajas), aplica subcuentas e imprime el recibo.
        /// </summary>
        private ErrorDto<object> EjecutarAnulacion(int codEmpresa, FndAnulacionesParams parametros, string accion, string notas, SqlConnection connection)
        {
            if (notas.Length < 30)
            {
                return DbHelper.CreateErrorResponse<object>("Indique una nota v&aacute;lida para justificar el movimiento!", -2, null);
            }

            if (accion != AccionCuentaContable && accion != AccionSaldoFavor)
            {
                return DbHelper.CreateErrorResponse<object>("Indique la acci&oacute;n a procesar (Cuenta Contable o Saldo a Favor).", -2, null);
            }

            var contrato = ObtenerContratoAnulacion(connection, parametros);
            if (contrato is null)
            {
                return DbHelper.CreateErrorResponse<object>("No se encontr&oacute; el contrato...", -2, null);
            }

            if (contrato.tipo_cdp == 1 || !contrato.permite_mov_cajas)
            {
                return DbHelper.CreateErrorResponse<object>(
                    "Este plan no permite movimientos en cajas o corresponde a un CDP.",
                    -2, null);
            }

            // La configuración persistida decide si corresponde aplicar subcuentas.
            parametros.aporteLocked = contrato.cuentamaestra == 1;
            var validacion = ValidarAnulacion(codEmpresa, parametros, contrato, connection);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(validacion.Description ?? "Error al validar anulación.", validacion.Code ?? -1, null);
            }

            var validacionSubCuentas = ValidarSubCuentas(codEmpresa, parametros);
            if (validacionSubCuentas.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(validacionSubCuentas.Description ?? "Error al validar subcuentas.", validacionSubCuentas.Code ?? -1, null);
            }

            var salidaResultado = FND_Anulaciones_Salida_Obtener(
                codEmpresa, parametros, accion);

            if (salidaResultado.Code != 0 || salidaResultado.Result is null)
            {
                return DbHelper.CreateErrorResponse<object>(
                    salidaResultado.Description ?? "No fue posible resolver la salida de la anulación.",
                    salidaResultado.Code == -2 ? -2 : -1,
                    null);
            }

            var salida = salidaResultado.Result;

            var distribucion = CalcularDistribucion(parametros.aporte ?? 0m, contrato.aportes, contrato.rendimiento);
            var aplica = connection.QueryFirstOrDefault<FndAnulacionesAplicaResultDto>(
                SpAnulaAporte,
                new
                {
                    Operadora = parametros.operadora,
                    Plan = NormalizarTexto(parametros.plan),
                    Contrato = parametros.contrato,
                    TipoDoc = TipoDocumentoAnulacion,
                    Aportes = distribucion.Aporte,
                    Rendimiento = distribucion.Rendimiento,
                    Usuario = NormalizarTexto(parametros.usuario),
                    Notas = notas,
                    AccionTipo = accion,
                    Cuenta = salida.Cuenta,
                    SF_Codigo = salida.FormaPago,
                    Documento = salida.Documento,
                    Deposito = salida.Deposito,
                    GestionId = parametros.gestion_id ?? 0
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (aplica is null || aplica.Pass != 1)
            {
                return DbHelper.CreateErrorResponse<object>(aplica?.Mensaje ?? "No se pudo aplicar la anulaci&oacute;n.", -2, null);
            }

            var numDoc = NormalizarTexto(aplica.NumDoc);
            RegistrarBitacoraAnulacion(codEmpresa, parametros, aplica);
            AplicarSubCuentasSiCorresponde(connection, parametros, aplica, numDoc);

            return ImprimirResultadoAnulacion(codEmpresa, parametros, numDoc);
        }

        private static ContratoAnulacion? ObtenerContratoAnulacion(SqlConnection connection, FndAnulacionesParams parametros)
        {
            const string query = @"
                    SELECT C.aportes, C.rendimiento,
                           ISNULL(P.cuenta_Maestra, 0) AS cuentamaestra,
                           P.Tipo_CDP AS tipo_cdp,
                           P.permite_mov_cajas
                    FROM dbo.fnd_contratos C
                    INNER JOIN dbo.fnd_planes P
                      ON P.cod_operadora = C.cod_operadora
                     AND P.cod_plan = C.cod_plan
                    WHERE C.cod_operadora = @Operadora
                      AND C.cod_plan = @Plan
                      AND C.cod_contrato = @Contrato;";

            return connection.QueryFirstOrDefault<ContratoAnulacion>(query, CrearParametrosContrato(parametros));
        }

        private ErrorDto<bool> ValidarAnulacion(int codEmpresa, FndAnulacionesParams parametros, FndAnulacionesSubCuentasDto contrato, SqlConnection connection)
        {
            if ((parametros.aporte ?? 0m) <= 0m)
            {
                return DbHelper.CreateErrorResponse("No se especific&oacute; el contrato o el monto", -2, false);
            }

            if (parametros.aporte > contrato.aportes + contrato.rendimiento)
            {
                return DbHelper.CreateErrorResponse("La Anulaci&oacute;n es mayor que el total de los aportes y rendimientos del contrato...", -2, false);
            }

            if (_mFNDFunciones.fxFndParametro(codEmpresa, "01.2") != "S")
            {
                return DbHelper.CreateOkResponse(false);
            }

            return ValidarSeguridadAnulacion(parametros, connection);
        }

        private static ErrorDto<bool> ValidarSeguridadAnulacion(FndAnulacionesParams parametros, SqlConnection connection)
        {
            var autoriza = connection.QueryFirstOrDefault<int>(
                SpSeguridadAnulacion,
                new
                {
                    Operadora = parametros.operadora,
                    Plan = parametros.plan,
                    Usuario = parametros.usuario
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (autoriza == 0)
            {
                return DbHelper.CreateErrorResponse("El Usuario no tiene nivel de Autorizaci&oacute;n para realizar este movimiento!", -2, true);
            }

            var limite = FND_Anulaciones_Limite_Consultar(connection, parametros);
            if (limite is null || limite.autorizado != 1)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró un límite de autorización válido para este usuario y plan.",
                    -2, false);
            }

            // El límite se obtiene del servidor, nunca del request.
            if (parametros.aporte > limite.monto && !GestionAutorizada(connection, parametros.gestion_id))
            {
                return DbHelper.CreateErrorResponse("- Este movimiento requiere AUTORIZACI&Oacute;N, verifique el estado de la misma y/o solicite una!", -2, true);
            }

            return DbHelper.CreateOkResponse(true);
        }

        private static bool GestionAutorizada(SqlConnection connection, int? gestionId)
        {
            if ((gestionId ?? 0) <= 0)
            {
                return false;
            }

            var estado = connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                SpGestionEstado,
                new { GestionId = gestionId },
                commandType: System.Data.CommandType.StoredProcedure);

            return NormalizarTexto(estado?.gestion_estado).StartsWith('A');
        }

        /// <summary>Consulta el límite de autorización vigente en el servidor.</summary>
        private static FndAutorizaDto? FND_Anulaciones_Limite_Consultar(
            SqlConnection connection, FndAnulacionesParams parametros)
        {
            return connection.QueryFirstOrDefault<FndAutorizaDto>(
                SpAutorizaDatos,
                new
                {
                    Plan = NormalizarTexto(parametros.plan),
                    TipoMov = TipoGestionAnulacion,
                    Usuario = NormalizarTexto(parametros.usuario)
                },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        /// <summary>Valida el desglose con la configuración y los saldos persistidos.</summary>
        private ErrorDto<bool> ValidarSubCuentas(
            int codEmpresa, FndAnulacionesParams parametros)
        {
            var montos = parametros.subcuentas ?? new List<FndAnulacionesSubCuentaMontoDto>();
            if (parametros.aporteLocked != true)
            {
                return montos.Count == 0
                    ? DbHelper.CreateOkResponse(true)
                    : DbHelper.CreateErrorResponse(
                        "El contrato no admite un desglose por subcuentas.", -2, false);
            }

            if (montos.Any(x => x.anulacion < 0m) ||
                montos.Select(x => x.idx).Distinct().Count() != montos.Count)
            {
                return DbHelper.CreateErrorResponse(
                    "El desglose contiene montos negativos o subcuentas repetidas.", -2, false);
            }

            if (montos.Count == 0 || montos.Sum(x => x.anulacion) != parametros.aporte)
            {
                return DbHelper.CreateErrorResponse(
                    "El desglose de subcuentas debe coincidir con el monto a anular.", -2, false);
            }

            var consulta = FND_Anulaciones_SubCuentas_Obtener(codEmpresa, parametros);
            if (consulta.Code != 0 || consulta.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    consulta.Description ?? "No se pudieron consultar las subcuentas.", -1, false);
            }

            var actuales = consulta.Result.ToDictionary(x => x.idx);
            foreach (var monto in montos)
            {
                if (!actuales.TryGetValue(monto.idx, out var actual) ||
                    monto.anulacion > actual.aportes + actual.rendimiento)
                {
                    return DbHelper.CreateErrorResponse(
                        $"La subcuenta {monto.idx} no está disponible o no tiene saldo suficiente.",
                        -2, false);
                }
            }

            return DbHelper.CreateOkResponse(true);
        }

        /// <summary>
        /// Resuelve la cuenta de salida y los datos del documento.
        /// Saldo a favor conserva su forma de pago configurada.
        /// </summary>
        private ErrorDto<SalidaAnulacion> FND_Anulaciones_Salida_Obtener(
            int codEmpresa,
            FndAnulacionesParams parametros,
            string accion)
        {
            if (accion != AccionSaldoFavor)
            {
                return FND_Anulaciones_Documento_Obtener(codEmpresa, parametros);
            }

            var resultado = DbHelper.WithConn(
                new PortalDB(_config),
                codEmpresa,
                connection => connection.QueryFirstOrDefault<SalidaAnulacion>(@"
                    SELECT RTRIM(COD_CUENTA) AS Cuenta,
                           RTRIM(COD_FORMA_PAGO) AS FormaPago
                    FROM SIF_FORMAS_PAGO
                    WHERE TIPO = 'S' AND Activa = 1;"));

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    resultado.Description ?? "Error al consultar la forma de pago.",
                    -1,
                    null);
            }

            if (resultado.Result is null ||
                string.IsNullOrWhiteSpace(resultado.Result.Cuenta) ||
                string.IsNullOrWhiteSpace(resultado.Result.FormaPago))
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "No existe una forma de pago de saldo a favor activa con cuenta configurada.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse(resultado.Result);
        }

        /// <summary>
        /// Consulta FNC y exige los datos del diálogo cuando corresponde.
        /// </summary>
        private ErrorDto<SalidaAnulacion> FND_Anulaciones_Documento_Obtener(
            int codEmpresa,
            FndAnulacionesParams parametros)
        {
            var enlace = new MProGrxMain(_config)
                .EmpresaEnlaceObtener()
                ?.FirstOrDefault();

            if (enlace is null)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "No se pudo obtener la configuración documental de la empresa.",
                    -1,
                    null);
            }

            // El VB6 revisado no define FNC en la rama documental v1.
            if (enlace.SysDocVersion == 1)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "La anulación a cuenta contable requiere definir la configuración "
                    + "del documento FNC para la versión documental 1.",
                    -2,
                    null);
            }

            var configuracion = new FrmCcDocCuentasDb(_config)
                .CC_DocCuentas_Documento_Verificar(codEmpresa, TipoDocumentoAnulacion);

            if (configuracion.Code != 0 || configuracion.Result is null)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    configuracion.Description ?? "Error al consultar el documento FNC.",
                    configuracion.Code == -2 ? -2 : -1,
                    null);
            }

            if (configuracion.Result.verificar)
            {
                return FND_Anulaciones_Documento_Validar(
                    codEmpresa, parametros.documento);
            }

            return FND_Anulaciones_CuentaConfigurada_Validar(
                codEmpresa, configuracion.Result.cuenta);
        }

        /// <summary>
        /// Usa la cuenta configurada sin máscara cuando FNC no requiere captura.
        /// </summary>
        private ErrorDto<SalidaAnulacion> FND_Anulaciones_CuentaConfigurada_Validar(
            int codEmpresa,
            string cuentaConfigurada)
        {
            var cuentasDb = new MCntLinkDB(_config);
            var cuenta = cuentasDb.fxgCntCuentaFormato(
                codEmpresa, false, cuentaConfigurada, 0);

            if (string.IsNullOrWhiteSpace(cuenta) ||
                !cuentasDb.fxgCntCuentaValida(codEmpresa, cuenta))
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "El documento FNC no tiene una cuenta contable válida configurada.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse(new SalidaAnulacion
            {
                Cuenta = cuenta
            });
        }

        /// <summary>
        /// Revalida los datos mediante el validador compartido.
        /// La bandera valido del cliente no sustituye esta validación.
        /// </summary>
        private ErrorDto<SalidaAnulacion> FND_Anulaciones_Documento_Validar(
            int codEmpresa,
            CcDocCuentasResultadoData? documento)
        {
            if (documento is null)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "Debe indicar la cuenta contable y el detalle del documento.",
                    -2,
                    null);
            }

            var resultado = new FrmCcDocCuentasDb(_config)
                .CC_DocCuentas_Documento_Validar(
                    codEmpresa,
                    new CcDocCuentasValidarRequest
                    {
                        cuenta = NormalizarTexto(documento.cuenta),
                        referencia = NormalizarTexto(documento.deposito),
                        detalle = NormalizarTexto(documento.detalle)
                    });

            if (resultado.Code != 0 || resultado.Result is null)
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    resultado.Description ?? "No fue posible validar el documento.",
                    resultado.Code == -2 ? -2 : -1,
                    null);
            }

            if (!resultado.Result.valido ||
                string.IsNullOrWhiteSpace(resultado.Result.cuenta))
            {
                return DbHelper.CreateErrorResponse<SalidaAnulacion>(
                    "Debe indicar un documento contable válido.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse(new SalidaAnulacion
            {
                Cuenta = resultado.Result.cuenta,
                Documento = resultado.Result.detalle,
                Deposito = resultado.Result.deposito
            });
        }

        private static AnulacionDistribucion CalcularDistribucion(decimal monto, decimal aporteActual, decimal rendimientoActual)
        {
            var sobrante = monto;
            var aplicadoRendimiento = Math.Min(sobrante, rendimientoActual);
            sobrante -= aplicadoRendimiento;
            var aplicadoAporte = Math.Min(sobrante, aporteActual);

            return new AnulacionDistribucion
            {
                Aporte = aplicadoAporte,
                Rendimiento = aplicadoRendimiento
            };
        }

        /// <summary>VB6: detalle y rebajo por subcuenta con Fecha/Proceso/NumDoc devueltos por el SP, en una transaccion.</summary>
        private static void AplicarSubCuentasSiCorresponde(
            SqlConnection connection,
            FndAnulacionesParams parametros,
            FndAnulacionesAplicaResultDto aplica,
            string numDoc)
        {
            if (parametros.aporteLocked != true)
            {
                return;
            }

            var montos = (parametros.subcuentas ?? new List<FndAnulacionesSubCuentaMontoDto>())
                .Where(x => x.anulacion > 0m)
                .ToList();
            if (montos.Count == 0)
            {
                return;
            }

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            foreach (var monto in montos)
            {
                AplicarSubCuenta(connection, transaction, parametros, monto, aplica, numDoc);
            }
            transaction.Commit();
        }

        private static void AplicarSubCuenta(
            SqlConnection connection,
            SqlTransaction transaction,
            FndAnulacionesParams parametros,
            FndAnulacionesSubCuentaMontoDto monto,
            FndAnulacionesAplicaResultDto aplica,
            string numDoc)
        {
            var llave = new
            {
                Id = monto.idx,
                Operadora = parametros.operadora,
                Plan = NormalizarTexto(parametros.plan),
                Contrato = parametros.contrato
            };

            var actual = connection.QuerySingle<FndAnulacionesSubCuentasDto>(@"
                    SELECT IdX AS idx, ISNULL(Aportes, 0) AS aportes, ISNULL(rendimiento, 0) AS rendimiento
                    FROM dbo.Fnd_subCuentas WITH (UPDLOCK)
                    WHERE cod_operadora = @Operadora AND cod_plan = @Plan AND cod_contrato = @Contrato AND IdX = @Id;",
                llave, transaction);

            if (monto.anulacion > actual.aportes + actual.rendimiento)
            {
                throw new InvalidOperationException(
                    $"El saldo de la subcuenta {monto.idx} cambió antes de aplicar el desglose.");
            }

            connection.Execute(@"
                    INSERT INTO dbo.fnd_SubCuentas_detalle (Idx, Cod_operadora, Cod_plan, Cod_Contrato, Fecha, Monto, Fecha_Proceso, Tcon, Ncon)
                    VALUES (@Id, @Operadora, @Plan, @Contrato, @Fecha, @Monto * -1, @Proceso, @TipoDoc, @NumDoc);",
                new
                {
                    llave.Id,
                    llave.Operadora,
                    llave.Plan,
                    llave.Contrato,
                    Fecha = aplica.Fecha ?? DateTime.Now,
                    Monto = monto.anulacion,
                    aplica.Proceso,
                    TipoDoc = TipoDocumentoAnulacion,
                    NumDoc = numDoc
                },
                transaction);

            var distribucion = CalcularDistribucionSubCuenta(monto.anulacion, actual.aportes, actual.rendimiento);
            connection.Execute(@"
                    UPDATE dbo.Fnd_subCuentas
                    SET Aportes = Aportes - @Aporte,
                        rendimiento = rendimiento - @Rendimiento
                    WHERE cod_operadora = @Operadora AND cod_plan = @Plan AND cod_contrato = @Contrato AND IdX = @Id;",
                new
                {
                    llave.Id,
                    llave.Operadora,
                    llave.Plan,
                    llave.Contrato,
                    distribucion.Aporte,
                    distribucion.Rendimiento
                },
                transaction);
        }

        /// <summary>VB6 subcuentas: primero aportes, luego rendimiento.</summary>
        private static AnulacionDistribucion CalcularDistribucionSubCuenta(decimal monto, decimal aporteActual, decimal rendimientoActual)
        {
            var sobrante = monto;
            var aporte = Math.Min(sobrante, aporteActual);
            sobrante -= aporte;
            var rendimiento = Math.Min(sobrante, rendimientoActual);

            return new AnulacionDistribucion
            {
                Aporte = aporte,
                Rendimiento = rendimiento
            };
        }

        private ErrorDto<object> ImprimirResultadoAnulacion(int codEmpresa, FndAnulacionesParams parametros, string numDoc)
        {
            var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
            var sysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;
            var result = sysDocVersion == 1 && long.TryParse(numDoc, out var recibo)
                ? _mFNDFunciones.sbgFNDImprimeRecibo(codEmpresa, recibo, TipoDocumentoAnulacion, parametros.operadora)
                : new MRecibos(_config).sbImprimeRecibo(
                    codEmpresa,
                    numDoc,
                    TipoDocumentoAnulacion,
                    parametros.usuario ?? string.Empty);

            if (result.Code == 0)
            {
                result.Description = $"Anulaci&oacute;n aplicada, con Nota de Cr&eacute;dito # {numDoc}";
            }
            else
            {
                result.Code = -1;
                result.Description = $"Anulaci&oacute;n aplicada, con Nota de Cr&eacute;dito # {numDoc}, pero no se pudo generar el recibo. No repita la anulación. Detalle técnico: {result.Description}";
            }

            return result;
        }

        /// <summary>VB6: Call Bitacora(rs!Movimiento, rs!Mensaje).</summary>
        private void RegistrarBitacoraAnulacion(int codEmpresa, FndAnulacionesParams parametros, FndAnulacionesAplicaResultDto aplica)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(parametros.usuario).ToUpper(),
                DetalleMovimiento = NormalizarTexto(aplica.Mensaje),
                Movimiento = string.IsNullOrWhiteSpace(aplica.Movimiento) ? "Registra - WEB" : NormalizarTexto(aplica.Movimiento),
                Modulo = vModulo
            });
        }

        private static object CrearParametrosContrato(FndAnulacionesParams parametros)
        {
            return new
            {
                Operadora = parametros?.operadora,
                Plan = NormalizarTexto(parametros?.plan),
                Contrato = parametros?.contrato
            };
        }

        private static object CrearParametrosSolicitudAutorizacion(FndAnulacionesParams parametros)
        {
            return new
            {
                // VB6: exec spFnd_Gestion_Registro Cedula, 'N', Operadora, Plan, Contrato, mAutorizaMonto (MntCal), txtAporte (MntSol), Usuario, Nota
                Cedula = NormalizarTexto(parametros?.cedula),
                Tipo = TipoGestionAnulacion,
                Operadora = parametros?.operadora,
                Plan = NormalizarTexto(parametros?.plan),
                Contrato = parametros?.contrato,
                MntCal = parametros?.autoriza_monto ?? 0,
                MntSol = parametros?.aporte ?? 0,
                Usuario = NormalizarTexto(parametros?.usuario),
                Nota = NormalizarTexto(parametros?.nota)
            };
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private const string TipoDocumentoAnulacion = "FNC";
        private const string AccionCuentaContable = "C";
        private const string AccionSaldoFavor = "S";
        private const string SpAnulaAporte = "dbo.spFondos_Anula_Aporte";
        private const string TipoGestionAnulacion = "N";

        /// <summary>Extiende los saldos existentes con la configuración del plan.</summary>
        private sealed class ContratoAnulacion : FndAnulacionesSubCuentasDto
        {
            public int cuentamaestra { get; set; }
            public int tipo_cdp { get; set; }
            public bool permite_mov_cajas { get; set; }
        }

        private sealed class AnulacionDistribucion
        {
            public decimal Aporte { get; init; }
            public decimal Rendimiento { get; init; }
        }

        private sealed class SalidaAnulacion
        {
            public string Cuenta { get; init; } = string.Empty;
            public string FormaPago { get; init; } = string.Empty;
            public string Documento { get; init; } = string.Empty;
            public string Deposito { get; init; } = string.Empty;
        }
    }
}